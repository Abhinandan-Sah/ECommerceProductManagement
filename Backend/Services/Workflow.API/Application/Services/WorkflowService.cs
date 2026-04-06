using Workflow.API.Application.DTOs;
using Workflow.API.Application.Interfaces.Repositories;
using Workflow.API.Application.Interfaces.Services;
using Workflow.API.Domain.Entities;
using Workflow.API.Domain.Enums;

namespace Workflow.API.Application.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _repository;

        public WorkflowService(IWorkflowRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> UpdatePricingAsync(Guid productId, UpdatePricingRequestDto request)
        {
            var price = await _repository.GetPricingByProductIdAsync(productId);

            if (price == null)
            {
                price = new Price { ProductId = productId };
            }

            price.MRP = request.MRP;
            price.SalePrice = request.SalePrice;
            price.GSTPercent = request.GSTPercent;

            await _repository.SavePricingAsync(price);
            return true;
        }

        public async Task<bool> UpdateInventoryAsync(Guid productId, UpdateInventoryRequestDto request)
        {
            var inventory = await _repository.GetInventoryByProductIdAsync(productId);

            if (inventory == null)
            {
                inventory = new Inventory { ProductId = productId };
            }

            inventory.WarehouseLocation = request.WarehouseLocation;
            inventory.AvailableQty = request.AvailableQty;
            inventory.ReorderLevel = request.ReorderLevel;

            await _repository.SaveInventoryAsync(inventory);
            return true;
        }

        public async Task<bool> SubmitForReviewAsync(Guid productId, Guid submittedByUserId)
        {
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
            return true;
        }

        public async Task<bool> UpdateStatusAsync(Guid productId, UpdateStatusRequestDto request, Guid actionByUserId)
        {
            var approval = await _repository.GetCurrentApprovalStatusAsync(productId);

            if (approval == null)
            {
                return false;
            }

            approval.Status = request.NewStatus;
            approval.Remarks = request.Remarks;
            approval.ApprovedByUserId = actionByUserId;

            await _repository.SaveApprovalAsync(approval);
            return true;
        }
    }
}
