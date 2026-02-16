using System;
using System.Linq;
using System.Net.Mime;
using Jellyfin.Plugin.UserRatings.Data;
using Jellyfin.Plugin.UserRatings.Models;
using MediaBrowser.Common.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
// using MediaBrowser.Controller.Activity;
// using MediaBrowser.Model.Activity;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Notifications; 
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Users;

namespace Jellyfin.Plugin.UserRatings.Api
{
    [ApiController]
    [Route("api/UserRatings")]
    public class RatingsController : ControllerBase
    {
        private readonly RatingRepository _repository;
        // private readonly IActivityManager _activityManager;
        private readonly IUserManager _userManager;
        private readonly ILogger<RatingsController> _logger;

        public RatingsController(
            IApplicationPaths appPaths,
            // IActivityManager activityManager,
            IUserManager userManager,
            ILogger<RatingsController> logger)
        {
            _repository = new RatingRepository(appPaths);
            // _activityManager = activityManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("Rate")]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult RateItem([FromQuery] Guid itemId, [FromQuery] Guid userId, [FromQuery] int rating, [FromQuery] string? note, [FromQuery] string? userName)
        {
            try
            {
                if (rating < 1 || rating > 5)
                {
                    return BadRequest(new { success = false, message = "Rating must be between 1 and 5" });
                }

                var userRating = new UserRating
                {
                    ItemId = itemId,
                    UserId = userId,
                    Rating = rating,
                    Note = note,
                    Timestamp = DateTime.UtcNow,
                    UserName = userName ?? "Unknown"
                };

                _repository.SaveRating(userRating);

                // Send notification (fire and forget)
                // _ = CreateActivityLogAsync(userId, userRating);

                // Log to history file
                try 
                {
                    _repository.AppendToHistory(userRating);
                }
                catch { /* ignore */ }

                return Ok(new { success = true, message = "Rating saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /*
        private async Task CreateActivityLogAsync(Guid userId, UserRating rating)
        {
            try
            {
                var user = _userManager.GetUserById(userId);
                var reviewerName = user?.Username ?? rating.UserName ?? "Unknown User";
                
                var overview = $"{reviewerName} rated an item with {rating.Rating} stars.";
                if (!string.IsNullOrWhiteSpace(rating.Note))
                {
                    overview += $" Note: {rating.Note}";
                }

                await _activityManager.CreateAsync(new ActivityLogEntry
                {
                    Name = "New User Rating",
                    Overview = overview,
                    Date = DateTime.UtcNow,
                    UserId = userId,
                    Type = "UserRating",
                    Severity = LogSeverity.Info
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity log for new rating");
            }
        }
        */

        [HttpGet("ExportHistory")]
        [Authorize(Policy = "RequiresElevation")]
        [Produces("text/csv")]
        public ActionResult ExportHistory()
        {
            try
            {
                var filePath = _repository.GetHistoryFilePath();
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { success = false, message = "No history log found." });
                }

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "text/csv", "ratings_history.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("Export")]
        [Authorize(Policy = "RequiresElevation")]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult ExportRatings()
        {
            try
            {
                var ratings = _repository.GetAllRatings();
                var exportDtos = ratings.Select(r => new RatingExportDto
                {
                    ItemId = r.ItemId,
                    UserId = r.UserId,
                    Rating = r.Rating,
                    Note = r.Note,
                    Timestamp = r.Timestamp,
                    UserName = r.UserName
                }).ToList();

                return Ok(exportDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("ExportCsv")]
        [Authorize(Policy = "RequiresElevation")]
        [Produces("text/csv")]
        public ActionResult ExportRatingsCsv()
        {
            try
            {
                var ratings = _repository.GetAllRatings();
                var csv = new System.Text.StringBuilder();

                // Header
                csv.AppendLine("Timestamp,UserId,UserName,ItemId,Rating,Note");

                foreach (var r in ratings)
                {
                    // Basic CSV escaping
                    var note = r.Note?.Replace("\"", "\"\"") ?? "";
                    if (note.Contains(",") || note.Contains("\n") || note.Contains("\r"))
                    {
                        note = $"\"{note}\"";
                    }

                    var userName = r.UserName?.Replace("\"", "\"\"") ?? "Unknown";
                    if (userName.Contains(","))
                    {
                        userName = $"\"{userName}\"";
                    }

                    csv.AppendLine($"{r.Timestamp:O},{r.UserId},{userName},{r.ItemId},{r.Rating},{note}");
                }

                return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "userratings-export.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Import")]
        [Authorize(Policy = "RequiresElevation")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult ImportRatings([FromBody] List<RatingExportDto> importedRatings)
        {
            try
            {
                int added = 0;
                int updated = 0;

                foreach (var dto in importedRatings)
                {
                    var existing = _repository.GetRating(dto.ItemId, dto.UserId);
                    if (existing == null) added++;
                    else updated++;

                    var rating = new UserRating
                    {
                        ItemId = dto.ItemId,
                        UserId = dto.UserId,
                        Rating = dto.Rating,
                        Note = dto.Note,
                        Timestamp = dto.Timestamp,
                        UserName = dto.UserName
                    };
                    _repository.SaveRating(rating);
                }

                return Ok(new { success = true, message = $"Imported {importedRatings.Count} ratings (Added: {added}, Updated: {updated})" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("Item/{itemId}")]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult GetItemRatings(Guid itemId)
        {
            try
            {
                var ratings = _repository.GetRatingsForItem(itemId);
                var stats = _repository.GetStatsForItem(itemId);

                return Ok(new
                {
                    success = true,
                    ratings = ratings.Select(r => new
                    {
                        userId = r.UserId,
                        userName = r.UserName,
                        rating = r.Rating,
                        note = r.Note,
                        timestamp = r.Timestamp
                    }),
                    averageRating = stats.AverageRating,
                    totalRatings = stats.TotalRatings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("User/{userId}")]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult GetUserRatings(Guid userId)
        {
            try
            {
                var ratings = _repository.GetRatingsForUser(userId);

                return Ok(new
                {
                    success = true,
                    ratings = ratings.Select(r => new
                    {
                        itemId = r.ItemId,
                        rating = r.Rating,
                        note = r.Note,
                        timestamp = r.Timestamp
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("Rating")]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult DeleteRating([FromQuery] Guid itemId, [FromQuery] Guid userId)
        {
            try
            {
                _repository.DeleteRating(itemId, userId);

                return Ok(new { success = true, message = "Rating deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("MyRating/{itemId}")]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult GetMyRating(Guid itemId, [FromQuery] Guid userId)
        {
            try
            {
                var rating = _repository.GetRating(itemId, userId);

                if (rating == null)
                {
                    return Ok(new { success = true, rating = (int?)null });
                }

                return Ok(new
                {
                    success = true,
                    rating = rating.Rating,
                    note = rating.Note,
                    timestamp = rating.Timestamp
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("DeleteAll")]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult DeleteAllRatings()
        {
            try
            {
                _repository.DeleteAllRatings();

                return Ok(new { success = true, message = "All ratings have been deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("AllRatedItems")]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult GetAllRatedItems()
        {
            try
            {
                var ratedItems = _repository.GetAllRatedItems();

                return Ok(new
                {
                    success = true,
                    items = ratedItems.Select(item => new
                    {
                        itemId = item.ItemId,
                        averageRating = item.AverageRating,
                        totalRatings = item.TotalRatings,
                        lastRated = item.LastRated
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
