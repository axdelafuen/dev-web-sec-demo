using System.Collections.Concurrent;

namespace DevWebSecDemo.WebAPI.Services
{
    /// <summary>
    /// Service for rate limiting authentication attempts
    /// Prevents brute force attacks by limiting requests from same IP
    /// </summary>
    public class RateLimitingService
    {
        private readonly ConcurrentDictionary<string, List<DateTime>> _attemptLog = new();
        private readonly int _maxAttemptsPerWindow = 5; // Max attempts
        private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(5); // Time window

        /// <summary>
        /// Check if IP address is currently rate limited
        /// </summary>
        public bool IsRateLimited(string identifier)
        {
            CleanOldAttempts(identifier);

            if (_attemptLog.TryGetValue(identifier, out var attempts))
            {
                return attempts.Count >= _maxAttemptsPerWindow;
            }

            return false;
        }

        /// <summary>
        /// Record an authentication attempt
        /// </summary>
        public void RecordAttempt(string identifier)
        {
            var now = DateTime.UtcNow;
            _attemptLog.AddOrUpdate(
                identifier,
                new List<DateTime> { now },
                (key, existing) =>
                {
                    existing.Add(now);
                    return existing;
                });
        }

        /// <summary>
        /// Remove attempts older than the time window
        /// </summary>
        private void CleanOldAttempts(string identifier)
        {
            if (_attemptLog.TryGetValue(identifier, out var attempts))
            {
                var cutoff = DateTime.UtcNow.Subtract(_timeWindow);
                attempts.RemoveAll(time => time < cutoff);

                if (attempts.Count == 0)
                {
                    _attemptLog.TryRemove(identifier, out _);
                }
            }
        }

        /// <summary>
        /// Get remaining attempts before rate limit
        /// </summary>
        public int GetRemainingAttempts(string identifier)
        {
            CleanOldAttempts(identifier);

            if (_attemptLog.TryGetValue(identifier, out var attempts))
            {
                return Math.Max(0, _maxAttemptsPerWindow - attempts.Count);
            }

            return _maxAttemptsPerWindow;
        }
    }
}
