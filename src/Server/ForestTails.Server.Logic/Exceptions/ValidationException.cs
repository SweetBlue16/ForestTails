using ForestTails.Shared.Enums;

namespace ForestTails.Server.Logic.Exceptions
{
    public class ValidationException : ForestTailsException
    {
        public ValidationException(string message, MessageCode code = MessageCode.ValidationError)
        : base(code, message) {}
    }
}
