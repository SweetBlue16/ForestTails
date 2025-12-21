using ForestTails.Server.Logic.Exceptions;
using ForestTails.Shared.Constants;
using ForestTails.Shared.Enums;
using ForestTails.Shared.Models;
using Microsoft.Extensions.Logging;

namespace ForestTails.Server.Logic.Utils
{
    public class ServiceExecutor
    {
        private readonly ILogger<ServiceExecutor> logger;

        public ServiceExecutor(ILogger<ServiceExecutor> logger)
        {
            this.logger = logger;
        }

        public async Task<ServiceResponse<T>> ExecuteSafeAsync<T>(Func<Task<T>> operation, string operationName)
        {
            try
            {
                var result = await operation();
                return ServiceResponse<T>.SuccessResult(result);
            }
            catch (ForestTailsException forestTailsException)
            {
                logger.LogWarning("Business rule error in {Operation}: {Message} (Code: {Code})",
                    operationName, forestTailsException.Message, forestTailsException.Code
                );
                return ServiceResponse<T>.FailureResult(forestTailsException.Code, forestTailsException.Message);
            }
            catch (TimeoutException timeoutException)
            {
                logger.LogError(timeoutException, "Timeout in {Operation}. The database or external service did not respond.", operationName);
                return ServiceResponse<T>.FailureResult(MessageCode.Timeout, "The server took too long to respond. Please try again.");
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Cancellation in {Operation}. Client aborted or token timed out.", operationName);
                return ServiceResponse<T>.FailureResult(MessageCode.Timeout, "The operation was canceled.");
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                logger.LogWarning(unauthorizedAccessException, "Access denied in {Operation}", operationName);
                return ServiceResponse<T>.FailureResult(MessageCode.Unauthorized, "Access denied.");
            }
            catch (ArgumentException argumentException)
            {
                logger.LogWarning(argumentException, "Invalid data in {Operation}", operationName);
                return ServiceResponse<T>.FailureResult(MessageCode.ValidationError, argumentException.Message);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException validationException)
            {
                logger.LogWarning(validationException, "Invalid model in {Operation}", operationName);
                return ServiceResponse<T>.FailureResult(MessageCode.ValidationError, validationException.Message);
            }
            catch (Exception exception)
            {
                var errorCode = ResolveErrorCodeFromMessage(exception.Message);
                if (errorCode != MessageCode.ServerInternalError)
                {
                    logger.LogWarning(exception, "Infrastructure error controlled in {Operation}: {Message}", operationName, exception.Message);
                    var userMessage = exception.Message.Contains(':') ? exception.Message.Split(':')[1].Trim() : exception.Message;
                    return ServiceResponse<T>.FailureResult(errorCode, userMessage);
                }
                logger.LogCritical(exception, "Uncontrolled crash in {Operation}", operationName);
                return ServiceResponse<T>.FailureResult(MessageCode.ServerInternalError, "An unexpected internal error has occurred. The incident has been logged.");
            }
        }

        private static MessageCode ResolveErrorCodeFromMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return MessageCode.ServerInternalError;

            if (message.StartsWith(SqlErrorTags.DataConflict)) return MessageCode.Conflict;
            if (message.StartsWith(SqlErrorTags.DataConstraintViolation)) return MessageCode.ValidationError;
            if (message.StartsWith(SqlErrorTags.ServerBusy)) return MessageCode.Timeout;
            if (message.StartsWith(SqlErrorTags.SecurityAuthFailure)) return MessageCode.Unauthorized;
            if (message.StartsWith(SqlErrorTags.DataStoreUnavailable)) return MessageCode.DatabaseError;

            return MessageCode.ServerInternalError;
        }
    }
}
