using System;

namespace Reporting.API.Domain.Entities
{
    /// <summary>
    /// Provides shared audit timestamps for reporting entities.
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// UTC time when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// UTC time when the entity was last updated, when it has changed after creation.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
