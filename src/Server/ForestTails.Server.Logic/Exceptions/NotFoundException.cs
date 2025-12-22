using ForestTails.Shared.Enums;

namespace ForestTails.Server.Logic.Exceptions
{
    public class NotFoundException : ForestTailsException
    {
        public NotFoundException(string message, MessageCode code = MessageCode.NotFound)
        : base(code, message) {}
    }
}
