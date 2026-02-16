using System;

namespace Jellyfin.Plugin.UserRatings.Models
{
    public class RatingExportDto
    {
        public Guid ItemId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string? Note { get; set; }
        public DateTime Timestamp { get; set; }
        public string? UserName { get; set; }
    }
}
