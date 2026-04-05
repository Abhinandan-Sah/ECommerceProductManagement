using Microsoft.EntityFrameworkCore;
using Workflow.API.Application.DTOs;
using Workflow.API.Application.Interfaces;
using Workflow.API.Domain.Entities;
using Workflow.API.Domain.Enums;
using Workflow.API.Infrastructure.Data;

namespace Workflow.API.Infrastructure.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly WorkflowDbContext _context;

        public WorkflowRepository(WorkflowDbContext context)
        {
            _context = context;
        }

        // ─── PRICING ────────────────────────────────────────────────────────
        public async Task<Price?> GetPricingByProductIdAsync(Guid productId)
        {
            return await _context.Prices.FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<bool> UpdatePricingAsync(Guid productId, UpdatePricingRequestDto request)
        {
            var price = await _context.Prices.FirstOrDefaultAsync(p => p.ProductId == productId);

            // Upsert Pattern
            if (price == null)
            {
                price = new Price { ProductId = productId };
                await _context.Prices.AddAsync(price);
            }

            price.MRP = request.MRP;
            price.SalePrice = request.SalePrice;
            price.GSTPercent = request.GSTPercent;

            return await _context.SaveChangesAsync() > 0;
        }

        // ─── INVENTORY ──────────────────────────────────────────────────────
        public async Task<Inventory?> GetInventoryByProductIdAsync(Guid productId)
        {
            return await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
        }

        public async Task<bool> UpdateInventoryAsync(Guid productId, UpdateInventoryRequestDto request)
        {
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);

            // Upsert Pattern
            if (inventory == null)
            {
                inventory = new Inventory { ProductId = productId };
                await _context.Inventories.AddAsync(inventory);
            }

            inventory.WarehouseLocation = request.WarehouseLocation;
            inventory.AvailableQty = request.AvailableQty;
            inventory.ReorderLevel = request.ReorderLevel;

            return await _context.SaveChangesAsync() > 0;
        }

        // ─── APPROVAL WORKFLOW ──────────────────────────────────────────────
        public async Task<Approval?> GetCurrentApprovalStatusAsync(Guid productId)
        {
            return await _context.Approvals.FirstOrDefaultAsync(a => a.ProductId == productId);
        }

        public async Task<bool> SubmitForReviewAsync(Guid productId, Guid submittedByUserId)
        {
            var approval = await _context.Approvals.FirstOrDefaultAsync(a => a.ProductId == productId);

            if (approval == null)
            {
                approval = new Approval
                {
                    ProductId = productId,
                    SubmittedByUserId = submittedByUserId
                };
                await _context.Approvals.AddAsync(approval);
            }

            // Move the status forward
            approval.Status = ApprovalStatus.ReadyForReview;
            approval.SubmittedByUserId = submittedByUserId; // Update in case someone else resubmits

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStatusAsync(Guid productId, UpdateStatusRequestDto request, Guid actionByUserId)
        {
            var approval = await _context.Approvals.FirstOrDefaultAsync(a => a.ProductId == productId);

            if (approval == null)
            {
                // You can't approve a product that hasn't been submitted yet!
                return false;
            }

            approval.Status = request.NewStatus;
            approval.Remarks = request.Remarks;
            approval.ApprovedByUserId = actionByUserId;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}