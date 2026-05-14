using Identity.API.Domain.Entities;

namespace Identity.API.Domain.Enums
{
    /// <summary>
    /// Defines authorization roles used by the product-management platform.
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// Full system administrator.
        /// </summary>
        Admin = 1, 

        /// <summary>
        /// Catalog owner who manages products and categories.
        /// </summary>
        ProductManager = 2,

        /// <summary>
        /// Content user who enriches product information.
        /// </summary>
        ContentExecutive = 3,

        /// <summary>
        /// Customer account role.
        /// </summary>
        Customer = 4
    }
}
