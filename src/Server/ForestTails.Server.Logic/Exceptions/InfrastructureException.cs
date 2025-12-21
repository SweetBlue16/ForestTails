using ForestTails.Shared.Enums;

namespace ForestTails.Server.Logic.Exceptions
{
    public class InfrastructureException : ForestTailsException
    {
        public InfrastructureException(string message, MessageCode code = MessageCode.ServerInternalError)
        : base(code, message) { }
    }
}
