using Microsoft.EntityFrameworkCore;
using DevWebSecDemo.Entities;
using DevWebSecDemo.Providers;
using System.Security.Cryptography;

namespace DevWebSecDemo.Business
{
    public class UserService
    {
        /// <summary>
        /// The database context
        /// </summary>
        private readonly UnitOfWork _unitOfWork;

        /// <summary>
        /// Error prefix for the service
        /// </summary>
        private const string ERROR_PREFIX = "EXCEPTION.SERVICE.USER.";

        /// <summary>
        /// The number of iterations for the hash function
        /// </summary>
        private const int ITERATIONS = 10000;

        /// <summary>
        /// The size of the hash in bytes
        /// </summary>
        private const int HASH_SIZE = 32;

        /// <summary>
        /// The administrator service constructor
        /// </summary>
        /// <param name="unitOfWork">The database context instance</param>
        public UserService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Try to get a user by its login/password, and check if he is admin.
        /// </summary>
        /// <param name="username">The user's name</param>
        /// <param name="password">The user's password</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>True if a user exist. False otherwise.</returns>
        public async Task IsCredentialsValidAsync(string username, string password, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException($"{ERROR_PREFIX}AUTHENTICATION_FAILED: Provided username or password is empty.");
            }

            var user = await _unitOfWork.DbContext.Users.Where(a => a.Username == username).FirstOrDefaultAsync(cancellationToken);

            if (user == null || user.Password == null)
            {
                throw new ArgumentException($"{ERROR_PREFIX}AUTHENTICATION_FAILED: No user exist in database with name '{username}'.");
            }

            if (!VerifyPassword(password, user.Password))
            {
                throw new ArgumentException($"{ERROR_PREFIX}AUTHENTICATION_FAILED: Invalid password for user with name '{username}'.");
            }
        }

        /// <summary>
        /// Create a new user with the provided username and password.
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="password">The user's password</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        public async Task CreateUserAsync(string username, string password, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException($"{ERROR_PREFIX}USER_CREATION: Provided username or password is empty.");
            }

            var userInDb = _unitOfWork.DbContext.Users.Where(u => u.Username == username).FirstOrDefault();
            if (userInDb != null)
            {
                throw new ArgumentException($"{ERROR_PREFIX}USER_CREATION: An account with username: {username} already exist.");
            }

            string hashedPassword = HashPassword(password);
            var user = new User
            {
                Username = username,
                Password = hashedPassword
            };

            await _unitOfWork.DbContext.Users.AddAsync(user, cancellationToken);
            var result = _unitOfWork.SaveChanges();

            if (result == -1)
            {
                throw new ArgumentException($"{ERROR_PREFIX}USER_CREATION: An account with username: {username} already exist.");
            }
        }

        /// <summary>
        /// Delete all user from database.
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken.</param>
        public async Task DaleteAllUsersAsync(CancellationToken cancellationToken)
        {
            await _unitOfWork.DbContext.Users.ExecuteDeleteAsync(cancellationToken);
            var result = _unitOfWork.SaveChanges();

            if (result == -1)
            {
                throw new ArgumentException($"{ERROR_PREFIX}USER_DELETION: An unexpected error occurred.");
            }
        }

        /// <summary>
        /// List all user from database.
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken.</param>
        public async Task<List<User>> ListAllUsersAsync(CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.DbContext.Users.ToListAsync(cancellationToken);

            return users;
        }

        /// <summary>
        /// Create a salt with the desired size.
        /// </summary>
        /// <param name="size">Salt size</param>
        /// <returns>The random salt</returns>
        private static byte[] GenerateSalt(int size = 16)
        {
            byte[] salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Génère un hash avec PBKDF2
        /// </summary>
        /// <param name="password">Clear password</param>
        /// <param name="salt">The salt</param>
        /// <param name="iterations">Iteration number</param>
        /// <param name="hashSize">Hash size in bytes</param>
        /// <returns>The hash</returns>
        private static byte[] GenerateHash(string password, byte[] salt, int iterations, int hashSize)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(hashSize);
            }
        }

        /// <summary>
        /// Hashes a password using PBKDF2 with HMAC-SHA256
        /// </summary>
        /// <param name="password">The plain text password to hash</param>
        /// <returns>The salt and hash combined in Base64 format</returns>
        private static string HashPassword(string password)
        {
            byte[] salt = GenerateSalt();

            byte[] hash = GenerateHash(password, salt, ITERATIONS, HASH_SIZE);

            byte[] iterationBytes = BitConverter.GetBytes(ITERATIONS);
            byte[] hashBytes = new byte[salt.Length + iterationBytes.Length + hash.Length];

            Buffer.BlockCopy(salt, 0, hashBytes, 0, salt.Length);
            Buffer.BlockCopy(iterationBytes, 0, hashBytes, salt.Length, iterationBytes.Length);
            Buffer.BlockCopy(hash, 0, hashBytes, salt.Length + iterationBytes.Length, hash.Length);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Compare two byte arrays in constant time to avoid timing attacks
        /// </summary>
        /// <param name="a">First array</param>
        /// <param name="b">Second array</param>
        /// <returns>True if the arrays matches, false otherwise</returns>
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        /// <summary>
        /// Check if a plain text password matches the hashed version
        /// </summary>
        /// <param name="password">The plain text password to verify</param>
        /// <param name="hashedPassword">The hashed password to compare against</param>
        /// <returns>True if the passwords matches, false otherwise</returns>
        private static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                byte[] salt = new byte[16];
                Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);

                int iterations = BitConverter.ToInt32(hashBytes, 16);

                int hashSize = hashBytes.Length - (16 + 4);
                byte[] originalHash = new byte[hashSize];
                Buffer.BlockCopy(hashBytes, 16 + 4, originalHash, 0, hashSize);

                byte[] newHash = GenerateHash(password, salt, iterations, hashSize);

                return SlowEquals(originalHash, newHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
