using CoreWCF;
using ForestTails.Shared.Contracts;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ForestTails.Server.Logic.Utils
{
    public class SessionManager
    {
        private readonly ConcurrentDictionary<string, ISessionCallback> activeSessions = new();
        private readonly ILogger<SessionManager> logger;

        public SessionManager(ILogger<SessionManager> logger)
        {
            this.logger = logger;
        }

        public void AddSession(string username, ISessionCallback sessionCallback)
        {
            if (string.IsNullOrWhiteSpace(username) || sessionCallback == null) return;

            if (activeSessions.TryRemove(username, out var oldCallback))
            {
                logger.LogWarning("Duplicate login detected for {Username}. Closing old session.", username);
                CloseChannelSafe(sessionCallback);
            }

            if (activeSessions.TryAdd(username, sessionCallback))
            {
                logger.LogInformation("User {Username} connected. Total sessions: {Count}", username, activeSessions.Count);
                if (sessionCallback is ICommunicationObject channel)
                {
                    var capturedUser = username;
                    channel.Closed += (s, e) => RemoveSession(capturedUser);
                    channel.Faulted += (s, e) => RemoveSession(capturedUser);
                }
            }
        }

        public void RemoveSession(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return;

            if (activeSessions.TryRemove(username, out _))
            {
                logger.LogInformation("User {Username} disconnected.", username);
            }
        }

        public TCallback? GetCallback<TCallback>(string username) where TCallback : class, ISessionCallback
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            if (activeSessions.TryGetValue(username, out var callback))
            {
                if (callback is ICommunicationObject channel &&
                (channel.State == CommunicationState.Opened || channel.State == CommunicationState.Opening))
                {
                    return callback as TCallback;
                }
                else
                {
                    RemoveSession(username);
                }
            }
            return null;
        }

        public bool IsOnline(string username)
        {
            return GetCallback<ISessionCallback>(username) != null;
        }

        public List<string> GetOnlineUsers()
        {
            return activeSessions.Keys.ToList();
        }

        private void CloseChannelSafe(ISessionCallback sessionCallback)
        {
            if (sessionCallback is ICommunicationObject channel)
            {
                try
                {
                    var state = channel.State;
                    if (state == CommunicationState.Opened || state == CommunicationState.Opening)
                    {
                        logger.LogDebug("Attempting to close channel. Current state: {State}", state);
                        _ = channel.CloseAsync();
                        logger.LogInformation("Close initiated for channel.");
                    }
                    else
                    {
                        logger.LogDebug("Channel not open (state: {State}). Aborting...", state);
                        channel.Abort();
                        logger.LogInformation("Channel aborted.");
                    }
                }
                catch (CommunicationException communicationException)
                {
                    logger.LogWarning(communicationException,
                        "CommunicationException during close. Aborting channel."
                    );
                    HandleAbortDuringException(channel, nameof(CommunicationException));
                }
                catch (TimeoutException timeoutException)
                {
                    logger.LogWarning(timeoutException,
                        "TimeoutException during channel close. Aborting channel."
                    );
                    HandleAbortDuringException(channel, nameof(TimeoutException));
                }
                catch (ObjectDisposedException objDisposedException)
                {
                    logger.LogDebug(objDisposedException,
                        "Channel already disposed while closing. No further action required."
                    );
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    logger.LogWarning(invalidOperationException, 
                        "InvalidOperationException during channel close. Aborting channel."
                    );
                    HandleAbortDuringException(channel, nameof(InvalidOperationException));
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, 
                        "Unexpected exception during channel close. Aborting channel."
                    );
                    HandleAbortDuringException(channel, exception.GetType().Name);
                }
            }
        }

        private void HandleAbortDuringException(ICommunicationObject channel, string exceptionName)
        {
            try
            {
                channel.Abort();
            }
            catch (Exception abortException)
            {
                logger.LogError(abortException,
                    "Failed to abort channel after {ExceptionName}.",
                    exceptionName
                );
            }
        }
    }
}
