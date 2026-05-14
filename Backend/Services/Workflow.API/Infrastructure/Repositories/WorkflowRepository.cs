using Microsoft.EntityFrameworkCore;
using Workflow.API.Application.Interfaces.Repositories;
using Workflow.API.Domain.Entities;
using Workflow.API.Infrastructure.Data;

namespace Workflow.API.Infrastructure.Repositories
{
    /// <summary>
    /// Reads and writes workflow records from the workflow database.
    /// </summary>
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly WorkflowDbContext _context;

        /// <summary>
        /// Creates the workflow repository for the current workflow database context.
        /// </summary>
        public WorkflowRepository(WorkflowDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<Price?> GetPricingByProductIdAsync(Guid productId)
        {
            return await _context.Prices.FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        /// <inheritdoc />
        /// <remarks>Uses EF Core entity state to support the same save path for create and update.</remarks>
        public async Task SavePricingAsync(Price price)
        {
            // Service methods use the same save path for create and update, so attach only brand-new rows.
            if (_context.Entry(price).State == EntityState.Detached)
            {
                await _context.Prices.AddAsync(price);
            }
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<Inventory?> GetInventoryByProductIdAsync(Guid productId)
        {
            return await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
        }

        /// <inheritdoc />
        /// <remarks>Uses EF Core entity state to support the same save path for create and update.</remarks>
        public async Task SaveInventoryAsync(Inventory inventory)
        {
            // Detached means EF has not seen this inventory row yet; tracked rows are already update-ready.
            if (_context.Entry(inventory).State == EntityState.Detached)
            {
                await _context.Inventories.AddAsync(inventory);
            }
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<Approval?> GetCurrentApprovalStatusAsync(Guid productId)
        {
            return await _context.Approvals.FirstOrDefaultAsync(a => a.ProductId == productId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Approval>> GetPendingApprovalsAsync()
        {
            return await _context.Approvals
                .Where(a => a.Status == Domain.Enums.ApprovalStatus.Pending)
                .ToListAsync();
        }

        /// <inheritdoc />
        /// <remarks>Uses EF Core entity state to upsert the current approval row for a product.</remarks>
        public async Task SaveApprovalAsync(Approval approval)
        {
            // Approval is keyed by product, so callers can upsert the current review state without branching.
            if (_context.Entry(approval).State == EntityState.Detached)
            {
                await _context.Approvals.AddAsync(approval);
            }
            await _context.SaveChangesAsync();
        }
    }
}
