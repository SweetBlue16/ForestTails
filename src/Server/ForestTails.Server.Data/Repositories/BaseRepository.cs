using ForestTails.Server.Data.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ForestTails.Server.Data.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        private readonly IDbContextFactory<ForestTailsDbContext> contextFactory;
        protected readonly ILogger logger;

        protected BaseRepository(IDbContextFactory<ForestTailsDbContext> contextFactory, ILogger logger)
        {
            this.contextFactory = contextFactory;
            this.logger = logger;
        }

        protected async Task<TResult> ExecuteSafeAsync<TResult>(Func<ForestTailsDbContext, Task<TResult>> operation, string operationName)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                return await operation(context);
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError("Concurrency conflict during repository operation: {OperationName}", operationName);
                throw;
            }
            catch (TimeoutException)
            {
                logger.LogError("Timeout during repository operation: {OperationName}", operationName);
                throw;
            }
            catch (TaskCanceledException)
            {
                logger.LogError("Task canceled during repository operation: {OperationName}", operationName);
                throw;
            }
            catch (OperationCanceledException)
            {
                logger.LogError("Operation canceled during repository operation: {OperationName}", operationName);
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError("Repository operation failure: {OperationName}", operationName);
                SqlErrorHandler.HandleAndThrow(exception, logger);
                throw;
            }
        }

        protected async Task ExecuteSafeAsync(Func<ForestTailsDbContext, Task> operation, string operationName)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                await operation(context);
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError("Concurrency conflict during repository operation: {OperationName}", operationName);
                throw;
            }
            catch (TimeoutException)
            {
                logger.LogError("Timeout during repository operation: {OperationName}", operationName);
                throw;
            }
            catch (TaskCanceledException)
            {
                logger.LogError("Task canceled during repository operation: {OperationName}", operationName);
                throw;
            }
            catch (OperationCanceledException)
            {
                logger.LogError("Operation canceled during repository operation: {OperationName}", operationName);
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError("Repository operation failure: {OperationName}", operationName);
                SqlErrorHandler.HandleAndThrow(exception, logger);
                throw;
            }
        }
    }
}
