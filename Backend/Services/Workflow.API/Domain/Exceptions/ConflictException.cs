namespace Workflow.API.Domain.Exceptions
{
    /// <summary>
    /// Thrown when a resource already exists (e.g., duplicate entry).
    /// Maps to HTTP 409 Conflict.
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message)
            : base(message) { }
    }
}
