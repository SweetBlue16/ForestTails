using ForestTails.Shared.Dtos;
using ForestTails.Shared.Models;
using System.ServiceModel;

namespace ForestTails.Shared.Contracts
{
    [ServiceContract(CallbackContract = typeof(IShopCallback), SessionMode = SessionMode.Required)]
    public interface IShopService
    {
        [OperationContract]
        Task RequestCosmeticCatalogAsync();

        [OperationContract]
        Task RequestItemCatalogAsync();

        [OperationContract]
        Task BuyCosmeticAsync(int cosmeticId);

        [OperationContract]
        Task BuyItemAsync(int itemId, int quantity);

        [OperationContract]
        Task SellItemAsync(int itemId, int quantity);
    }

    [ServiceContract]
    public interface IShopCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnCosmeticCatalogReceived(ServiceResponse<List<CosmeticDTO>> response);

        [OperationContract(IsOneWay = true)]
        void OnItemCatalogReceived(ServiceResponse<List<GameItemDTO>> response);

        [OperationContract(IsOneWay = true)]
        void OnPurchaseResult(ServiceResponse<ShopTransactionResultDTO> response);

        [OperationContract(IsOneWay = true)]
        void OnSellResult(ServiceResponse<ShopTransactionResultDTO> response);
    }
}
