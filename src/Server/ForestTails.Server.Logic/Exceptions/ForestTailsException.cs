using ForestTails.Shared.Enums;

namespace ForestTails.Server.Logic.Exceptions
{
    public abstract class ForestTailsException : Exception
    {
        public MessageCode Code { get; }

        protected ForestTailsException(MessageCode code, string message) : base(message)
        { 
            Code = code;
        }
    }
}
