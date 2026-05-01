using MassTransit;
using Microsoft.Extensions.Logging;
using Reporting.API.Application.Interfaces.Repositories;
using Reporting.API.Domain.Entities;
using Shared.Messaging;

namespace Reporting.API.Application.Consumers
{
    public class AuditLogCreatedConsumer : IConsumer<AuditLogCreatedEvent>
    {
        private readonly IAuditRepository _auditRepository;
        private readonly ILogger<AuditLogCreatedConsumer> _logger;

        public AuditLogCreatedConsumer(IAuditRepository auditRepository, ILogger<AuditLogCreatedConsumer> logger)
        {
            _auditRepository = auditRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuditLogCreatedEvent> context)
        {
            var msg = context.Message;

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
