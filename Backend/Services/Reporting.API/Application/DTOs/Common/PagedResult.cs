using System.Collections.Generic;

namespace Reporting.API.Application.DTOs.Common
{
    /// <summary>
    /// Represents one page of query results and the paging metadata needed by clients.
    /// </summary>
    /// <typeparam name="T">Type of item included in the page.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Items included in the current page.
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Total number of items that match the query before paging.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// One-based page number returned by the query.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Maximum number of items requested for the page.
        /// </summary>
        public int PageSize { get; set; }
    }
}
