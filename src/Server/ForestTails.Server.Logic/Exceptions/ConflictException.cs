using ForestTails.Shared.Enums;

namespace ForestTails.Server.Logic.Exceptions
{
    public class ConflictException : ForestTailsException
    {
        public ConflictException(string message, MessageCode code = MessageCode.Conflict)
        : base(code, message) {}
    }
}
