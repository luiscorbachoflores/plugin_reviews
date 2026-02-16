using System;

namespace Jellyfin.Plugin.UserRatings.Models
{
    public class RatedItemSummary
    {
        public Guid ItemId { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public DateTime LastRated { get; set; }
    }
}
