using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI msg;

    public bool loadingStoreItems = false;
    public List<StoreItem> storeItems = new();

    public bool loadingCatalogItems = false;
    public Dictionary<string, CatalogItem> catalogMap = new();

    public bool loadingCoins = false;
    public int coins;

    public bool loadingInventory = false;
    public List<ItemInstance> invenItems = new();

    public bool buyingItem = false;

    public bool consumingItem = false;
    public string consumedItemID = "";

    [SerializeField] GameObject moveUpTextPrefab;
    [SerializeField] Transform popupTextHolder;

    [SerializeField] PlayFabUserMgtTMP pfManager;

    //Trading
    public bool loadingTradeInventory = false;


    void UpdateMsg()
    {

    }

    void OnError(PlayFabError e)
    {
        Debug.Log(e.GenerateErrorReport());
        Debug.Log(e.Error);
    }

    public void GetVirtualCurrencies()
    {
        loadingCoins = true;

        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        r =>
        {
            coins = r.VirtualCurrency["CN"];
            loadingCoins = false;
        }, OnError);
    }

    public void AddCoins(int coinsToAdd)
    {
        PlayFabClientAPI.AddUserVirtualCurrency(new AddUserVirtualCurrencyRequest
        {
            Amount = coinsToAdd,
            VirtualCurrency = "CN"
        },
        r =>
        {
            pfManager.gettingCoins = true;
            GetVirtualCurrencies();
        }, OnError);
    }

    public void GetCatalog()
    {
        loadingCatalogItems = true;

        var req = new GetCatalogItemsRequest { CatalogVersion = "main" };
        PlayFabClientAPI.GetCatalogItems(req, result =>
        {
            catalogMap.Clear();
            foreach (CatalogItem catalogItem in result.Catalog)
            {
                catalogMap.Add(catalogItem.ItemId, catalogItem);
            }

            loadingCatalogItems = false;

        }, OnError);
    }

    public void GetStore(string storeID)
    {
        loadingStoreItems = true;
        storeItems.Clear();


        var req = new GetStoreItemsRequest { CatalogVersion = "main", StoreId = storeID };
        PlayFabClientAPI.GetStoreItems(req, result =>
        {
            storeItems = result.Store;
            loadingStoreItems = false;
        }, OnError);
    }

    public void GetPlayerInventory()
    {
        loadingInventory = true;
        invenItems.Clear();

        var UserInv = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(UserInv,
            result =>
            {
                invenItems = result.Inventory;
                loadingInventory = false;
            }, OnError);
    }

    public void BuyItem(string storeID, string itemID, string vc, int price)
    {
        buyingItem = true;

        var buyReq = new PurchaseItemRequest
        {
            CatalogVersion = "main",
            StoreId = storeID,
            ItemId = itemID,
            VirtualCurrency = vc,
            Price = price
        };
        PlayFabClientAPI.PurchaseItem(buyReq,
            result => {
                buyingItem = false;
                string itemName = result.Items[0].DisplayName;

                pfManager.MakeScrollNotif(itemName + " bought", Color.green);
            },
            error =>
            {
                pfManager.MakeScrollNotif(error.Error.ToString(), Color.red);
            });
    }

    public void ConsumeItem(string itemInstId, string itemName, string itemID)
    {
        consumingItem = true;

        var consumeReq = new ConsumeItemRequest
        {
            ConsumeCount = 1,
            ItemInstanceId = itemInstId,
        };
        PlayFabClientAPI.ConsumeItem(consumeReq,
            result =>
            {
                consumingItem = false;
                consumedItemID = itemID;

                pfManager.MakeScrollNotif("Consumed " + itemName, Color.green);

                GetPlayerInventory();
            },
            error =>
            {
                pfManager.MakeScrollNotif("Failed to consume " + itemName, Color.red);
            });
    }

    //Trading

    public void GetPlayerTradeInventory()
    {
        loadingTradeInventory = true;
        invenItems.Clear();

        var UserInv = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(UserInv,
            result =>
            {
                invenItems = result.Inventory;
                loadingTradeInventory = false;
            }, OnError);
    }

    public void OpenTradeRequest()
    {

    }
}
