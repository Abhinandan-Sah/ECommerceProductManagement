using Microsoft.EntityFrameworkCore;
using Workflow.API.Application.Interfaces.Repositories;
using Workflow.API.Domain.Entities;
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

        public async Task<Price?> GetPricingByProductIdAsync(Guid productId)
        {
            return await _context.Prices.FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task SavePricingAsync(Price price)
        {
            if (_context.Entry(price).State == EntityState.Detached)
            {
                await _context.Prices.AddAsync(price);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Inventory?> GetInventoryByProductIdAsync(Guid productId)
        {
            return await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
        }

        public async Task SaveInventoryAsync(Inventory inventory)
        {
            if (_context.Entry(inventory).State == EntityState.Detached)
            {
                await _context.Inventories.AddAsync(inventory);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Approval?> GetCurrentApprovalStatusAsync(Guid productId)
        {
            return await _context.Approvals.FirstOrDefaultAsync(a => a.ProductId == productId);
        }

        public async Task SaveApprovalAsync(Approval approval)
        {
            if (_context.Entry(approval).State == EntityState.Detached)
            {
                await _context.Approvals.AddAsync(approval);
            }
            await _context.SaveChangesAsync();
        }
    }
}