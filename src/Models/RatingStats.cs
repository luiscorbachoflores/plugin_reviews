using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.UserRatings.Models
{
    public class RatingStats
    {
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public Dictionary<Guid, UserRating> UserRatings { get; set; } = new Dictionary<Guid, UserRating>();
    }
}
