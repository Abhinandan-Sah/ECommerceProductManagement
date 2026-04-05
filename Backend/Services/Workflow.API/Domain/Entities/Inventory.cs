namespace Workflow.API.Domain.Entities
{
    public class Inventory : BaseEntity
    {
        public Guid ProductId { get; set; }
        public string WarehouseLocation { get; set; } = string.Empty;
        public int AvailableQty { get; set; }
        public int ReservedQty { get; set; }
        public int ReorderLevel { get; set; }
    }
}