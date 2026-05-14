namespace Identity.API.Application.DTOs.User
{
    /// <summary>
    /// Captures the requested active state for a user account.
    /// </summary>
    public class SetUserStatusRequestDto
    {
        /// <summary>
        /// True to activate the account; false to deactivate it.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
