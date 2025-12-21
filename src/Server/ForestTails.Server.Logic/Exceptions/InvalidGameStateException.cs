using ForestTails.Shared.Enums;

namespace ForestTails.Server.Logic.Exceptions
{
    public class InvalidGameStateException : ForestTailsException
    {
        public InvalidGameStateException(string message) : base(MessageCode.Conflict, message) {}

        public InvalidGameStateException(string message, MessageCode specificCode)
        : base(specificCode, message) {}
    }
}
