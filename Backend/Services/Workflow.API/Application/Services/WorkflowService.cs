using Workflow.API.Application.DTOs;
using Workflow.API.Application.Interfaces.Repositories;
using Workflow.API.Application.Interfaces.Services;
using Workflow.API.Domain.Entities;
using Workflow.API.Domain.Enums;
using Workflow.API.Domain.Exceptions;
using MassTransit;
using Shared.Messaging;
using System.Text.Json;

namespace Workflow.API.Application.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _repository;
        private readonly ILogger<WorkflowService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public WorkflowService(IWorkflowRepository repository, ILogger<WorkflowService> logger, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Price?> GetPricingAsync(Guid productId, string? role)
        {
            var price = await _repository.GetPricingByProductIdAsync(productId);
            if (price == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(role) || string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                var approval = await _repository.GetCurrentApprovalStatusAsync(productId);
                if (approval == null || approval.Status != ApprovalStatus.Approved)
                {
                    return null;
                }
            }

            return price;
        }

        public async Task<bool> UpdatePricingAsync(Guid productId, UpdatePricingRequestDto request, Guid actionByUserId)
        {
            _logger.LogInformation("Updating pricing for product {ProductId}", productId);

            // Business rule validation: SalePrice must be less than or equal to MRP
            if (request.SalePrice > request.MRP)
            {
                _logger.LogWarning("Sale price {SalePrice} cannot be greater than MRP {MRP} for product {ProductId}",
                    request.SalePrice, request.MRP, productId);
                throw new BadRequestException("Sale price cannot be greater than MRP.");
            }

            var price = await _repository.GetPricingByProductIdAsync(productId);
            var oldValues = price == null
                ? null
                : JsonSerializer.Serialize(new
                {
                    price.MRP,
                    price.SalePrice,
                    price.GSTPercent
                });

            if (price == null)
            {
                price = new Price { ProductId = productId };
            }

            price.MRP = request.MRP;
            price.SalePrice = request.SalePrice;
            price.GSTPercent = request.GSTPercent;

            await _repository.SavePricingAsync(price);
            await PublishAuditLogAsync(
                productId,
                "PricingUpdated",
                actionByUserId,
                oldValues,
                JsonSerializer.Serialize(new
                {
                    price.MRP,
                    price.SalePrice,
                    price.GSTPercent
                }));

            _logger.LogInformation("Pricing updated for product {ProductId}", productId);
            return true;
        }

        public async Task<Inventory?> GetInventoryAsync(Guid productId)
        {
            _logger.LogInformation("Getting inventory for product {ProductId}", productId);
            return await _repository.GetInventoryByProductIdAsync(productId);
        }

        public async Task<bool> UpdateInventoryAsync(Guid productId, UpdateInventoryRequestDto request, Guid actionByUserId)
        {
            _logger.LogInformation("Updating inventory for product {ProductId}", productId);

            var inventory = await _repository.GetInventoryByProductIdAsync(productId);
            var oldValues = inventory == null
                ? null
                : JsonSerializer.Serialize(new
                {
                    inventory.WarehouseLocation,
                    inventory.AvailableQty,
                    inventory.ReservedQty,
                    inventory.ReorderLevel
                });

            if (inventory == null)
            {
                inventory = new Inventory { ProductId = productId };
            }

            inventory.WarehouseLocation = request.WarehouseLocation;
            inventory.AvailableQty = request.AvailableQty;
            inventory.ReorderLevel = request.ReorderLevel;

            await _repository.SaveInventoryAsync(inventory);
            await PublishAuditLogAsync(
                productId,
                "InventoryUpdated",
                actionByUserId,
                oldValues,
                JsonSerializer.Serialize(new
                {
                    inventory.WarehouseLocation,
                    inventory.AvailableQty,
                    inventory.ReservedQty,
                    inventory.ReorderLevel
                }));

            _logger.LogInformation("Inventory updated for product {ProductId}", productId);
            return true;
        }

        public async Task<bool> SubmitForReviewAsync(Guid productId, Guid submittedByUserId)
        {
            _logger.LogInformation("Submitting product {ProductId} for review by user {UserId}", productId, submittedByUserId);

            var approval = await _repository.GetCurrentApprovalStatusAsync(productId);

            if (approval == null)
            {
                approval = new Approval
                {
                    ProductId = productId,
                    SubmittedByUserId = submittedByUserId
                };
            }

            approval.Status = ApprovalStatus.Pending;
            approval.SubmittedByUserId = submittedByUserId; 

            await _repository.SaveApprovalAsync(approval);
            await PublishAuditLogAsync(
                productId,
                "SubmittedForReview",
                submittedByUserId,
                null,
                JsonSerializer.Serialize(new { Status = approval.Status.ToString() }));

            _logger.LogInformation("Product {ProductId} submitted for review", productId);
            return true;
        }

        public async Task<bool> UpdateStatusAsync(Guid productId, UpdateStatusRequestDto request, Guid actionByUserId)
        {
            _logger.LogInformation("Updating status for product {ProductId} to {NewStatus} by user {UserId}",
                productId, request.NewStatus, actionByUserId);

            var approval = await _repository.GetCurrentApprovalStatusAsync(productId);

            if (approval == null)
            {
                _logger.LogWarning("Cannot update status — no approval record found for product {ProductId}", productId);
                throw new BadRequestException("Product must be submitted before it can be approved/rejected.");
            }

            var oldStatus = approval.Status.ToString();

            approval.Status = request.NewStatus;
            approval.Remarks = request.Remarks;
            approval.ApprovedByUserId = actionByUserId;

            await _repository.SaveApprovalAsync(approval);

            // Publish status-change event so Reporting service can write audit logs.
            await _publishEndpoint.Publish(new ProductStatusChangedEvent
            {
                ProductId = productId,
                OldStatus = oldStatus,
                NewStatus = approval.Status.ToString(),
                ChangedByUserId = actionByUserId
            });

            _logger.LogInformation("Product {ProductId} status updated to {NewStatus}", productId, request.NewStatus);
            return true;
        }

        private Task PublishAuditLogAsync(Guid productId, string action, Guid byUserId, string? oldValues, string? newValues)
        {
            return _publishEndpoint.Publish(new AuditLogCreatedEvent
            {
                EntityName = "Product",
                EntityId = productId,
                Action = action,
                ByUserId = byUserId,
                OldValues = oldValues,
                NewValues = newValues
            });
        }

        public async Task<ApprovalStatusResponseDto?> GetApprovalStatusAsync(Guid productId)
        {
            _logger.LogInformation("Getting approval status for product {ProductId}", productId);

            var approval = await _repository.GetCurrentApprovalStatusAsync(productId);

            if (approval == null)
            {
                return null;
            }

            return new ApprovalStatusResponseDto
            {
                ProductId = approval.ProductId,
                Status = approval.Status,
                IsSubmitted = true,
                ApprovedByUserId = approval.ApprovedByUserId,
                Remarks = approval.Remarks
            };
        }

        public async Task<IEnumerable<ApprovalStatusResponseDto>> GetPendingApprovalsAsync()
        {
            _logger.LogInformation("Getting all pending approvals");
            var approvals = await _repository.GetPendingApprovalsAsync();
            return approvals.Select(a => new ApprovalStatusResponseDto
            {
                ProductId = a.ProductId,
                Status = a.Status,
                IsSubmitted = true,
                ApprovedByUserId = a.ApprovedByUserId,
                Remarks = a.Remarks
            });
        }
    }
}
