using MassTransit;
using Microsoft.Extensions.Logging;
using Reporting.API.Application.Interfaces.Repositories;
using Reporting.API.Domain.Entities;
using Shared.Messaging;

namespace Reporting.API.Application.Consumers
{
    /// <summary>
    /// Consumes audit events from other services and persists them in Reporting.
    /// </summary>
    public class AuditLogCreatedConsumer : IConsumer<AuditLogCreatedEvent>
    {
        private readonly IAuditRepository _auditRepository;
        private readonly ILogger<AuditLogCreatedConsumer> _logger;

        /// <summary>
        /// Creates the audit log consumer with audit persistence and logging.
        /// </summary>
        public AuditLogCreatedConsumer(IAuditRepository auditRepository, ILogger<AuditLogCreatedConsumer> logger)
        {
            _auditRepository = auditRepository;
            _logger = logger;
        }

        /// <summary>
        /// Persists an audit event received from the message bus.
        /// </summary>
        /// <param name="context">MassTransit context containing the audit event.</param>
        /// <returns>A task that completes when the audit log has been saved.</returns>
        public async Task Consume(ConsumeContext<AuditLogCreatedEvent> context)
        {
            var msg = context.Message;

            // Other services publish the business context; Reporting persists it as the audit system of record.
            var auditLog = new AuditLog
            {
                EntityName = msg.EntityName,
                EntityId = msg.EntityId,
                Action = msg.Action,
                ByUserId = msg.ByUserId,
                OldValues = msg.OldValues,
                NewValues = msg.NewValues
            };

            await _auditRepository.AddAuditLogAsync(auditLog);

            _logger.LogInformation("Audit log created for {EntityName} {EntityId} action {Action}",
                msg.EntityName, msg.EntityId, msg.Action);
        }
    }
}
