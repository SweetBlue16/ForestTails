namespace ForestTails.Shared.Enums
{
    public enum MessageCode
    {
        None = 0,
        Success = 1,
        ConnectionStable = 2,

        ServerInternalError = 100,
        DatabaseError = 101,
        Timeout = 102,

        ValidationError = 200,
        ResourceNotFound = 201,
        Unauthorized = 202,
        ConflictError = 203,
        InvalidCredentials = 204,
        UserNotFound = 205,
        UserAlreadyExists = 206,

        InventoryFull = 300,
        ItemNotOwned = 301,
        InvalidMovement = 302,
        MapInstanceNotFound = 303
    }
}
