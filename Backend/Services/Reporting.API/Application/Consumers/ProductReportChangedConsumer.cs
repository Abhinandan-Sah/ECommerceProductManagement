using MassTransit;
using Reporting.API.Application.Interfaces.Repositories;
using Reporting.API.Domain.Entities;
using Shared.Messaging;

namespace Reporting.API.Application.Consumers
{
    public class ProductReportChangedConsumer : IConsumer<ProductReportChangedEvent>
    {
        private readonly IReportingRepository _repository;

        public ProductReportChangedConsumer(IReportingRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<ProductReportChangedEvent> context)
        {
            var msg = context.Message;

            if (msg.IsDeleted)
            {
                await _repository.DeleteProductReportAsync(msg.ProductId);
            }
            else
            {
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

            await _repository.RefreshDashboardSnapshotAsync();
        }
    }
}
