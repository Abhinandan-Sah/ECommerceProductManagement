using MassTransit;
using Reporting.API.Application.Interfaces.Repositories;
using Shared.Messaging;

namespace Reporting.API.Application.Consumers
{
    public class UserCountChangedConsumer : IConsumer<UserCountChangedEvent>
    {
        private readonly IReportingRepository _repository;

        public UserCountChangedConsumer(IReportingRepository repository)
        {
            _repository = repository;
        }

        public Task Consume(ConsumeContext<UserCountChangedEvent> context)
        {
            return _repository.RefreshDashboardSnapshotAsync(context.Message.Delta);
        }
    }
}
