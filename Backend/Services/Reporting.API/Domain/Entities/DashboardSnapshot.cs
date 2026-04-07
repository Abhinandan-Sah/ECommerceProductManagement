using System;

namespace Reporting.API.Domain.Entities
{
    public class DashboardSnapshot : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime SnapshotDate { get; set; }
        public int TotalProducts { get; set; }
        public int PublishedProducts { get; set; }
        public int PendingApprovals { get; set; }
        public int RejectedProducts { get; set; }
        public int TotalUsers { get; set; }
    }
}
