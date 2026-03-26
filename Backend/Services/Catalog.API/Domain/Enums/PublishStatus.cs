namespace Catalog.API.Domain.Enums
{
    public enum PublishStatus
    {
        Draft,              // Product record exists but is incomplete 
        InEnrichment,       // Images, attributes, SEO, pricing being added 
        ReadyForReview,     // Mandatory fields complete, waiting for approval 
        Approved,           // Business validation completed successfully 
        Published,          // Product is visible on storefront 
        Rejected,           // Review failed; corrections needed 
        Archived            // Product removed from active use
    }
}
