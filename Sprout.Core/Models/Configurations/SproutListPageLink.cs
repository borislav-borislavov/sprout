using System;

namespace Sprout.Core.Models.Configurations
{
    /// <summary>
    /// Describes a single page that a <see cref="SproutListConfig"/> can open from its page-launch menu.
    /// When chosen the configured page is opened and the list's currently selected item is passed as the
    /// page parameter (mirroring the data grid's row double-click behaviour).
    /// </summary>
    public class SproutListPageLink
    {
        /// <summary>
        /// The optional text shown for this entry in the list's page-launch menu. When empty the page's
        /// own title is used instead.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The identifier of the page that is opened when this entry is selected.
        /// </summary>
        public Guid PageId { get; set; }
    }
}
