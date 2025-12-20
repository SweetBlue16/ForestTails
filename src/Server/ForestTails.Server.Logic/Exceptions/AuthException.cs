using ForestTails.Shared.Enums;

namespace ForestTails.Server.Logic.Exceptions
{
    public class AuthException : ForestTailsException
    {
        public AuthException(string message, MessageCode code = MessageCode.Unauthorized)
        : base(code, message) {}
    }
}
