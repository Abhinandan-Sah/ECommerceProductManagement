using MassTransit;
using Reporting.API.Application.Interfaces.Repositories;
using Shared.Messaging;

namespace Reporting.API.Application.Consumers
{
    /// <summary>
    /// Consumes identity user-count deltas and folds them into dashboard snapshots.
    /// </summary>
    public class UserCountChangedConsumer : IConsumer<UserCountChangedEvent>
    {
        private readonly IReportingRepository _repository;

        /// <summary>
        /// Creates the user count consumer with reporting persistence.
        /// </summary>
        public UserCountChangedConsumer(IReportingRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Refreshes dashboard metrics using the received user-count delta.
        /// </summary>
        /// <param name="context">MassTransit context containing the user-count event.</param>
        /// <returns>A task that completes when a new dashboard snapshot has been saved.</returns>
        public Task Consume(ConsumeContext<UserCountChangedEvent> context)
        {
            // Identity sends only the delta, and Reporting folds it into the next dashboard snapshot.
            return _repository.RefreshDashboardSnapshotAsync(context.Message.Delta);
        }
    }
}
