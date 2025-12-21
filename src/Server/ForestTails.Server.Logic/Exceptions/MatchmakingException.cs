using ForestTails.Shared.Enums;

namespace ForestTails.Server.Logic.Exceptions
{
    public class MatchmakingException : ForestTailsException
    {
        public MatchmakingException(string message) : base(MessageCode.MapInstanceFull, message) {}

        public MatchmakingException(string message, MessageCode code) : base(code, message) {}
    }
}
