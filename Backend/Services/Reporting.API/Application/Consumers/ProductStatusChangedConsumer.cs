using System;
using System.Threading.Tasks;
using MassTransit;
using Shared.Messaging;
using Reporting.API.Application.Interfaces.Repositories;
using Reporting.API.Domain.Entities;
using System.Text.Json;

namespace Reporting.API.Application.Consumers
{
    // This consumer listens specifically for the ProductStatusChangedEvent arriving from Workflow.API or Catalog.API
    public class ProductStatusChangedConsumer : IConsumer<ProductStatusChangedEvent>
    {
        private readonly IAuditRepository _auditRepository;
        
        public ProductStatusChangedConsumer(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task Consume(ConsumeContext<ProductStatusChangedEvent> context)
        {
            var msg = context.Message;

            // Instantly transform the incoming message to a saved Audit Log
            var auditLog = new AuditLog
            {
                EntityName = "Product",
                EntityId = msg.ProductId,
                Action = "StatusChanged",
                ByUserId = msg.ChangedByUserId,
                OldValues = JsonSerializer.Serialize(new { Status = msg.OldStatus }),
                NewValues = JsonSerializer.Serialize(new { Status = msg.NewStatus })
            };

            await _auditRepository.AddAuditLogAsync(auditLog);
        }
    }
}
