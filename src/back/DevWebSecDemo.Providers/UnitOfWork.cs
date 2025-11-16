using Microsoft.EntityFrameworkCore;

namespace DevWebSecDemo.Providers
{
    /// <summary>
    /// Encapsulates Entity Framework database updates by managing transactions and change tracking.
    /// </summary>
    public class UnitOfWork
    {
        /// <summary>
        /// Database context used for managing entity changes.
        /// </summary>
        private readonly DatabaseContext _dbContext;

        /// <summary>
        /// Gets the current database context.
        /// </summary>
        public DatabaseContext DbContext { get { return _dbContext; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class with a specified database context.
        /// Ensures that the database is created.
        /// </summary>
        /// <param name="context">The database context to be used.</param>
        public UnitOfWork(DatabaseContext context)
        {
            _dbContext = context;
            _dbContext.Database.EnsureCreated();
        }

        /// <summary>
        /// Saves changes made to the database context.
        /// If an error occurs, changes are rejected to maintain data integrity.
        /// </summary>
        /// <returns>The number of affected rows, or -1 if an error occurs.</returns>
        public int SaveChanges()
        {
            int result = 0;
            try
            {
                result = _dbContext.SaveChanges();
            }
            catch
            {
                RejectChanges();
                return -1;
            }
            foreach (var entity in _dbContext.ChangeTracker.Entries()
                         .Where(e => e.State != EntityState.Detached))
            {
                entity.State = EntityState.Detached;
            }
            return result;
        }

        /// <summary>
        /// Rejects all pending changes in the database context,
        /// restoring entities to their original state.
        /// </summary>
        public void RejectChanges()
        {
            foreach (var entry in _dbContext.ChangeTracker.Entries()
                         .Where(e => e.State != EntityState.Unchanged))
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
        }
    }
}
