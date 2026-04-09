using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messaging;
using Reporting.API.Application.Interfaces.Repositories;
using Reporting.API.Domain.Entities;
using System.Text.Json;

namespace Reporting.API.Application.Consumers
{
    /// <summary>
    /// Listens for ProductStatusChangedEvent from Workflow.API or Catalog.API
    /// and creates an audit log entry.
    /// </summary>
    public class ProductStatusChangedConsumer : IConsumer<ProductStatusChangedEvent>
    {
        private readonly IAuditRepository _auditRepository;
        private readonly ILogger<ProductStatusChangedConsumer> _logger;
        
        public ProductStatusChangedConsumer(IAuditRepository auditRepository, ILogger<ProductStatusChangedConsumer> logger)
        {
            _auditRepository = auditRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductStatusChangedEvent> context)
        {
            var msg = context.Message;

            _logger.LogInformation("Received ProductStatusChanged event for product {ProductId}: {OldStatus} → {NewStatus}",
                msg.ProductId, msg.OldStatus, msg.NewStatus);

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

            _logger.LogInformation("Audit log created for product {ProductId} status change", msg.ProductId);
        }
    }
}
