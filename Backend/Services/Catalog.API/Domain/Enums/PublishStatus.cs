namespace Catalog.API.Domain.Enums
{
    /// <summary>
    /// Describes where a product sits in the catalog publishing workflow.
    /// </summary>
    public enum PublishStatus
    {
        /// <summary>
        /// Product record exists but is incomplete.
        /// </summary>
        Draft,              // Product record exists but is incomplete 

        /// <summary>
        /// Images, attributes, SEO, or pricing are still being added.
        /// </summary>
        InEnrichment,       // Images, attributes, SEO, pricing being added 

        /// <summary>
        /// Mandatory fields are complete and the product is waiting for approval.
        /// </summary>
        ReadyForReview,     // Mandatory fields complete, waiting for approval 

        /// <summary>
        /// Business validation completed successfully.
        /// </summary>
        Approved,           // Business validation completed successfully 

        /// <summary>
        /// Product is visible on the storefront.
        /// </summary>
        Published,          // Product is visible on storefront 

        /// <summary>
        /// Review failed and corrections are needed.
        /// </summary>
        Rejected,           // Review failed; corrections needed 

        /// <summary>
        /// Product has been removed from active catalog use.
        /// </summary>
        Archived            // Product removed from active use
    }
}
