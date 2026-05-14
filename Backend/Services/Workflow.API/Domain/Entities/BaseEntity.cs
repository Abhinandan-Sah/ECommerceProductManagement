namespace Workflow.API.Domain.Entities
{
    /// <summary>
    /// Provides shared identity and audit timestamps for workflow entities.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Unique entity identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// UTC time when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// UTC time when the entity was last updated, when available.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
