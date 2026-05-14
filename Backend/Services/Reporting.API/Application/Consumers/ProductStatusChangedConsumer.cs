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
    /// Consumes workflow status changes and records them in reporting audit history.
    /// </summary>
    public class ProductStatusChangedConsumer : IConsumer<ProductStatusChangedEvent>
    {
        private readonly IAuditRepository _auditRepository;
        private readonly IReportingRepository _reportingRepository;
        private readonly ILogger<ProductStatusChangedConsumer> _logger;

        /// <summary>
        /// Creates the product status consumer with audit/reporting repositories and logging.
        /// </summary>
        public ProductStatusChangedConsumer(
            IAuditRepository auditRepository,
            IReportingRepository reportingRepository,
            ILogger<ProductStatusChangedConsumer> logger)
        {
            _auditRepository = auditRepository;
            _reportingRepository = reportingRepository;
            _logger = logger;
        }

        /// <summary>
        /// Records an audit entry for a product status transition received from Workflow.
        /// </summary>
        /// <param name="context">MassTransit context containing the status changed event.</param>
        /// <returns>A task that completes when the audit log has been saved.</returns>
        public async Task Consume(ConsumeContext<ProductStatusChangedEvent> context)
        {
            var msg = context.Message;

            _logger.LogInformation("Received ProductStatusChanged event for product {ProductId}: {OldStatus} → {NewStatus}",
                msg.ProductId, msg.OldStatus, msg.NewStatus);

            // Keep status decisions visible in audit history even though the change happened in Workflow.
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
