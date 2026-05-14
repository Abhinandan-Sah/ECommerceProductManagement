using System;

namespace Reporting.API.Domain.Entities
{
    /// <summary>
    /// Represents a point-in-time rollup of dashboard KPI values.
    /// </summary>
    public class DashboardSnapshot : BaseEntity
    {
        /// <summary>
        /// Unique dashboard snapshot identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// UTC time the dashboard values were calculated.
        /// </summary>
        public DateTime SnapshotDate { get; set; }

        /// <summary>
        /// Total number of products tracked by the reporting read model.
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Number of products currently published.
        /// </summary>
        public int PublishedProducts { get; set; }

        /// <summary>
        /// Number of products waiting for approval.
        /// </summary>
        public int PendingApprovals { get; set; }

        /// <summary>
        /// Number of products currently rejected.
        /// </summary>
        public int RejectedProducts { get; set; }

        /// <summary>
        /// Total number of users tracked from identity events.
        /// </summary>
        public int TotalUsers { get; set; }
    }
}
