namespace Workflow.API.Domain.Exceptions
{
    /// <summary>
    /// Thrown when the request data is invalid.
    /// Maps to HTTP 400 Bad Request.
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string message)
            : base(message) { }
    }
}
