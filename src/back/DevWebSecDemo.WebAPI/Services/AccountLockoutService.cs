using System.Collections.Concurrent;

namespace DevWebSecDemo.WebAPI.Services
{
    /// <summary>
    /// Service for account lockout management
    /// Locks accounts after consecutive failed login attempts
    /// </summary>
    public class AccountLockoutService
    {
        private readonly ConcurrentDictionary<string, AccountLockoutInfo> _lockoutInfo = new();
        private readonly int _maxFailedAttempts = 5; // Lock after 5 failed attempts
        private readonly TimeSpan _lockoutDuration = TimeSpan.FromMinutes(15); // 15 minute lockout

        /// <summary>
        /// Check if an account is currently locked out
        /// </summary>
        public bool IsLockedOut(string username)
        {
            if (_lockoutInfo.TryGetValue(username.ToLower(), out var info))
            {
                if (info.LockoutEnd > DateTime.UtcNow)
                {
                    return true;
                }
                else
                {
                    // Lockout expired, clean up
                    _lockoutInfo.TryRemove(username.ToLower(), out _);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Record a failed login attempt
        /// </summary>
        public void RecordFailedAttempt(string username)
        {
            var key = username.ToLower();
            var now = DateTime.UtcNow;

            _lockoutInfo.AddOrUpdate(
                key,
                new AccountLockoutInfo { FailedAttempts = 1, LastAttempt = now },
                (k, existing) =>
                {
                    // Reset if last attempt was more than lockout duration ago
                    if (now - existing.LastAttempt > _lockoutDuration)
                    {
                        existing.FailedAttempts = 1;
                    }
                    else
                    {
                        existing.FailedAttempts++;
                    }

                    existing.LastAttempt = now;

                    // Lock account if max attempts reached
                    if (existing.FailedAttempts >= _maxFailedAttempts)
                    {
                        existing.LockoutEnd = now.Add(_lockoutDuration);
                    }

                    return existing;
                });
        }

        /// <summary>
        /// Reset failed attempts counter (on successful login)
        /// </summary>
        public void ResetFailedAttempts(string username)
        {
            _lockoutInfo.TryRemove(username.ToLower(), out _);
        }

        /// <summary>
        /// Get remaining attempts before lockout
        /// </summary>
        public int GetRemainingAttempts(string username)
        {
            if (_lockoutInfo.TryGetValue(username.ToLower(), out var info))
            {
                if (info.LockoutEnd > DateTime.UtcNow)
                {
                    return 0; // Already locked
                }

                return Math.Max(0, _maxFailedAttempts - info.FailedAttempts);
            }

            return _maxFailedAttempts;
        }

        /// <summary>
        /// Get time remaining on lockout
        /// </summary>
        public TimeSpan GetLockoutTimeRemaining(string username)
        {
            if (_lockoutInfo.TryGetValue(username.ToLower(), out var info))
            {
                var remaining = info.LockoutEnd - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// Reset all account lockouts
        /// </summary>
        public void ResetAll()
        {
            _lockoutInfo.Clear();
        }
    }

    public class AccountLockoutInfo
    {
        public int FailedAttempts { get; set; }
        public DateTime LastAttempt { get; set; }
        public DateTime LockoutEnd { get; set; }
    }
}
