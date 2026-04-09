namespace Identity.API.Domain.Exceptions
{
    /// <summary>
    /// Thrown when a requested resource does not exist.
    /// Maps to HTTP 404 Not Found.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string entityName, object key)
            : base($"{entityName} with identifier '{key}' was not found.") { }
    }
}
