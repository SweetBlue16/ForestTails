using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ForestTails.Shared.Constants;

namespace ForestTails.Server.Data.Helpers
{
    public static class SqlErrorHandler
    {
        public static void HandleAndThrow(Exception exception, ILogger logger)
        {
            if (exception is DbUpdateException dbUpdateException && dbUpdateException.InnerException is SqlException sqlInner)
            {
                HandleSqlException(sqlInner, logger);
            }
            else if (exception is SqlException sqlException)
            {
                HandleSqlException(sqlException, logger);
            }
            logger.LogError(exception, "[CRITICAL] An SQL error occurred, {ErrorMessage}", exception.Message);
            throw exception;
        }

        private static void HandleSqlException(SqlException sqlException, ILogger logger)
        {
            string logMessage = string.Empty;
            bool isCritical = false;
            string exceptionTag = SqlErrorTags.SqlError;

            switch (sqlException.Number)
            {
                case 2:
                case 53:
                    logMessage = $"[SQL-FATAL] SQL service unreachable or network down (Error {sqlException.Number}).";
                    isCritical = true;
                    exceptionTag = SqlErrorTags.DataStoreUnavailable;
                    break;
                case 4060:
                    logMessage = $"[SQL-FATAL] Database does not exist or access denied (Error {sqlException.Number}).";
                    isCritical = true;
                    exceptionTag = SqlErrorTags.DataStoreUnavailable;
                    break;
                case 18456:
                    logMessage = $"[SQL-AUTH] Login failed (incorrect username/password) (Error {sqlException.Number}).";
                    isCritical = true;
                    exceptionTag = SqlErrorTags.SecurityAuthFailure;
                    break;
                case -2:
                    logMessage = $"[SQL-TIMEOUT] Timeout. Server blocked or overloaded (Error {sqlException.Number}).";
                    exceptionTag = SqlErrorTags.ServerBusy;
                    break;
                case 547:
                    logMessage = $"[SQL-CONSTRAINT] Foreign key violation (Error {sqlException.Number}).";
                    exceptionTag = SqlErrorTags.DataConstraintViolation;
                    break;
                case 2601:
                case 2627:
                    logMessage = $"[SQL-DUPLICATE] Duplicate record detected (Error {sqlException.Number}).";
                    exceptionTag = SqlErrorTags.DataConflict;
                    break;
                default:
                    logMessage = $"[SQL-ERROR] General database error: {sqlException.Message} (Code {sqlException.Number}).";
                    break;
            }

            if (isCritical)
            {
                logger.LogCritical(sqlException, logMessage);
            }
            else
            {
                logger.LogError(sqlException, logMessage);
            }
            throw new Exception($"{exceptionTag}: {logMessage}", sqlException);
        }
    }
}
