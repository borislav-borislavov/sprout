using System;

namespace Sprout.Core.Models
{
    /// <summary>
    /// Command parameter used when a <see cref="Views.Controls.SproutList"/> launches a page from its
    /// page-launch menu. It identifies the list that triggered the action and the page to open so the
    /// page view model can pass the list's selected item as the page parameter.
    /// </summary>
    public class ListPageLaunchInfo
    {
        public string ListName { get; set; }

        public Guid PageId { get; set; }
    }
}
