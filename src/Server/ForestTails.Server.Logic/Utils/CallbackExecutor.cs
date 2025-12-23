using Microsoft.Extensions.Logging;
using System.ServiceModel;

namespace ForestTails.Server.Logic.Utils
{
    public class CallbackExecutor
    {
        private readonly ILogger<CallbackExecutor> logger;

        public CallbackExecutor(ILogger<CallbackExecutor> logger)
        {
            this.logger = logger;
        }

        public virtual void Execute<TCallback>(Action<TCallback> callbackAction) where TCallback : class
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<TCallback>();
                if (((ICommunicationObject)callback).State == CommunicationState.Opened)
                {
                    callbackAction(callback);
                }
                else
                {
                    logger.LogWarning("Callback channel is not opened.");
                }
            }
            catch (CommunicationObjectAbortedException commObjectAbortedEx)
            {
                logger.LogWarning(commObjectAbortedEx, "Callback channel was aborted.");
            }
            catch (CommunicationObjectFaultedException commObjectFaultedEx)
            {
                logger.LogWarning(commObjectFaultedEx, "Callback channel is faulted.");
            }
            catch (CommunicationException commException)
            {
                logger.LogWarning(commException, "Communication error occurred while executing the callback.");
            }
            catch (TimeoutException timeoutException)
            {
                logger.LogWarning(timeoutException, "Callback channel operation timed out.");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred while executing the callback.");
            }
        }
    }
}
