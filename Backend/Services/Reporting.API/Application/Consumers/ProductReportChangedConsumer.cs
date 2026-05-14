using MassTransit;
using Reporting.API.Application.Interfaces.Repositories;
using Reporting.API.Domain.Entities;
using Shared.Messaging;

namespace Reporting.API.Application.Consumers
{
    /// <summary>
    /// Consumes catalog product projection events and updates reporting read models.
    /// </summary>
    public class ProductReportChangedConsumer : IConsumer<ProductReportChangedEvent>
    {
        private readonly IReportingRepository _repository;

        /// <summary>
        /// Creates the product report consumer with reporting persistence.
        /// </summary>
        public ProductReportChangedConsumer(IReportingRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Upserts or removes a product report row and refreshes dashboard metrics.
        /// </summary>
        /// <param name="context">MassTransit context containing the product report event.</param>
        /// <returns>A task that completes when the reporting projection has been updated.</returns>
        public async Task Consume(ConsumeContext<ProductReportChangedEvent> context)
        {
            var msg = context.Message;

            if (msg.IsDeleted)
            {
                // Catalog owns the product; Reporting removes only its read-model copy.
                await _repository.DeleteProductReportAsync(msg.ProductId);
            }
            else
            {
                // Upsert keeps this consumer idempotent when RabbitMQ redelivers a product event.
                await _repository.UpsertProductReportAsync(new ProductReport
                {
                    ProductId = msg.ProductId,
                    ProductName = msg.ProductName,
                    SKU = msg.SKU,
                    Status = msg.Status,
                    CategoryName = msg.CategoryName,
                    CreatedByUserId = msg.CreatedByUserId,
                    PublishedAt = msg.Status == "Published" ? DateTime.UtcNow : null
                });
            }

            // Recalculate after every product change so the dashboard follows the reporting read model.
            await _repository.RefreshDashboardSnapshotAsync();
        }
    }
}
