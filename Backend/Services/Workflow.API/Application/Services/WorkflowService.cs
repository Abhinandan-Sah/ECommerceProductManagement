using Workflow.API.Application.DTOs;
using Workflow.API.Application.Interfaces.Repositories;
using Workflow.API.Application.Interfaces.Services;
using Workflow.API.Domain.Entities;
using Workflow.API.Domain.Enums;
using Workflow.API.Domain.Exceptions;
using MassTransit;
using Shared.Messaging;

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

        public async Task<bool> UpdatePricingAsync(Guid productId, UpdatePricingRequestDto request)
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

            if (price == null)
            {
                price = new Price { ProductId = productId };
            }

            price.MRP = request.MRP;
            price.SalePrice = request.SalePrice;
            price.GSTPercent = request.GSTPercent;

            await _repository.SavePricingAsync(price);

            _logger.LogInformation("Pricing updated for product {ProductId}", productId);
            return true;
        }

        public async Task<bool> UpdateInventoryAsync(Guid productId, UpdateInventoryRequestDto request)
        {
            _logger.LogInformation("Updating inventory for product {ProductId}", productId);

            var inventory = await _repository.GetInventoryByProductIdAsync(productId);

            if (inventory == null)
            {
                inventory = new Inventory { ProductId = productId };
            }

            inventory.WarehouseLocation = request.WarehouseLocation;
            inventory.AvailableQty = request.AvailableQty;
            inventory.ReorderLevel = request.ReorderLevel;

            await _repository.SaveInventoryAsync(inventory);

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

            approval.Status = ApprovalStatus.ReadyForReview;
            approval.SubmittedByUserId = submittedByUserId; 

            await _repository.SaveApprovalAsync(approval);

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
    }
}
