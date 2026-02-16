using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Jellyfin.Plugin.UserRatings.Models;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Plugin.UserRatings.Data
{
    public class RatingRepository
    {
        private readonly string _dataPath;
        private Dictionary<string, UserRating> _ratings = new();
        private readonly object _lock = new object();

        public RatingRepository(IApplicationPaths appPaths)
        {
            _dataPath = Path.Combine(appPaths.PluginConfigurationsPath, "UserRatings", "ratings.json");
            Directory.CreateDirectory(Path.GetDirectoryName(_dataPath)!);
            LoadRatings();
        }

        private void LoadRatings()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_dataPath))
                    {
                        var json = File.ReadAllText(_dataPath);
                        _ratings = JsonSerializer.Deserialize<Dictionary<string, UserRating>>(json) ?? new();
                    }
                }
                catch (Exception)
                {
                    _ratings = new Dictionary<string, UserRating>();
                }
            }
        }

        private void SaveRatings()
        {
            lock (_lock)
            {
                try
                {
                    var json = JsonSerializer.Serialize(_ratings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_dataPath, json);
                }
                catch (Exception)
                {
                    // Log error
                }
            }
        }

        private static string GetKey(Guid itemId, Guid userId) => $"{itemId}_{userId}";

        public void SaveRating(UserRating rating)
        {
            lock (_lock)
            {
                var key = GetKey(rating.ItemId, rating.UserId);
                _ratings[key] = rating;
                SaveRatings();
            }
        }

        public UserRating? GetRating(Guid itemId, Guid userId)
        {
            lock (_lock)
            {
                var key = GetKey(itemId, userId);
                return _ratings.TryGetValue(key, out var rating) ? rating : null;
            }
        }

        public List<UserRating> GetRatingsForItem(Guid itemId)
        {
            lock (_lock)
            {
                return _ratings.Values
                    .Where(r => r.ItemId == itemId)
                    .OrderByDescending(r => r.Timestamp)
                    .ToList();
            }
        }

        public List<UserRating> GetRatingsForUser(Guid userId)
        {
            lock (_lock)
            {
                return _ratings.Values
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.Timestamp)
                    .ToList();
            }
        }

        public void DeleteRating(Guid itemId, Guid userId)
        {
            lock (_lock)
            {
                var key = GetKey(itemId, userId);
                _ratings.Remove(key);
                SaveRatings();
            }
        }

        public RatingStats GetStatsForItem(Guid itemId)
        {
            lock (_lock)
            {
                var ratings = _ratings.Values.Where(r => r.ItemId == itemId).ToList();
                
                return new RatingStats
                {
                    AverageRating = ratings.Any() ? ratings.Average(r => r.Rating) : 0,
                    TotalRatings = ratings.Count,
                    UserRatings = ratings.ToDictionary(r => r.UserId, r => r)
                };
            }
        }

        public void DeleteAllRatings()
        {
            lock (_lock)
            {
                _ratings.Clear();
                SaveRatings();
            }
        }

        public List<RatedItemSummary> GetAllRatedItems()
        {
            lock (_lock)
            {
                return _ratings.Values
                    .GroupBy(r => r.ItemId)
                    .Select(g => new RatedItemSummary
                    {
                        ItemId = g.Key,
                        AverageRating = g.Average(r => r.Rating),
                        TotalRatings = g.Count(),
                        LastRated = g.Max(r => r.Timestamp)
                    })
                    .OrderByDescending(s => s.LastRated)
                    .ToList();
            }
        }

        public List<UserRating> GetAllRatings()
        {
            lock (_lock)
            {
                return _ratings.Values.ToList();
            }
        }

        public string GetHistoryFilePath()
        {
            return Path.Combine(Path.GetDirectoryName(_dataPath)!, "ratings_history.csv");
        }

        public void AppendToHistory(UserRating rating)
        {
            try
            {
                var filePath = GetHistoryFilePath();
                bool fileExists = File.Exists(filePath);

                lock (_lock)
                {
                    using (var sw = new StreamWriter(filePath, true))
                    {
                        if (!fileExists)
                        {
                            sw.WriteLine("Timestamp,UserId,UserName,ItemId,Rating,Note");
                        }

                        // Basic CSV escaping
                        var note = rating.Note?.Replace("\"", "\"\"") ?? "";
                        if (note.Contains(",") || note.Contains("\n") || note.Contains("\r"))
                        {
                            note = $"\"{note}\"";
                        }

                        var userName = rating.UserName?.Replace("\"", "\"\"") ?? "Unknown";
                        if (userName.Contains(","))
                        {
                            userName = $"\"{userName}\"";
                        }

                        sw.WriteLine($"{rating.Timestamp:O},{rating.UserId},{userName},{rating.ItemId},{rating.Rating},{note}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error safely? Or just ignore preventing crash?
                // For now we swallow to ensure main flow continues, but ideally should log via injected logger if available.
                // Since this class doesn't have logger, we proceed.
                Console.WriteLine($"Error writing history: {ex.Message}");
            }
        }
    }
}
