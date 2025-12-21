using ForestTails.Shared.Enums;

namespace ForestTails.Server.Logic.Exceptions
{
    public class InventoryException : ForestTailsException
    {
        public InventoryException(string message) : base(MessageCode.InventoryFull, message) {}

        public InventoryException(string message, MessageCode specificCode)
        : base(specificCode, message) {}
    }
}
