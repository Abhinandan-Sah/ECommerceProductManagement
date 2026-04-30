namespace Catalog.API.Domain.Exceptions
{
    /// <summary>
    /// Thrown when the caller lacks permission to perform the requested action.
    /// Maps to HTTP 403 Forbidden.
    /// </summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message)
            : base(message) { }
    }
}
