using ForestTails.Server.Data.Helpers;
using Microsoft.Extensions.Logging;

namespace ForestTails.Server.Data.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        protected readonly ForestTailsDbContext context;
        protected readonly ILogger logger;

        protected BaseRepository(ForestTailsDbContext context, ILogger logger)
        {
            this.context = context;
            this.logger = logger;
        }

        protected async Task<T> ExecuteSafeAsync<T>(Func<Task<T>> operation, string operationName)
        {
            try
            {
                return await operation();
            }
            catch (Exception exception)
            {
                logger.LogError($"Repository operation failure: {operationName}");
                SqlErrorHandler.HandleAndThrow(exception, logger);
                throw;
            }
        }

        protected async Task ExecuteSafeAsync(Func<Task> operation, string operationName)
        {
            try
            {
                await operation();
            }
            catch (Exception exception)
            {
                logger.LogError($"Repository operation failure: {operationName}");
                SqlErrorHandler.HandleAndThrow(exception, logger);
                throw;
            }
        }
    }
}
