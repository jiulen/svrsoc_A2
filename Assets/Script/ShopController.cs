using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;

public class ShopController : MonoBehaviour
{
    public GameObject panel;
    System.Action playerCallback;

    [SerializeField]
    InventoryManager invenManager;

    [SerializeField]
    Toggle backgroundToggle, potionToggle;

    [SerializeField]
    Transform shopItemContent, shopInvenItemContent;

    [SerializeField]
    GameObject loadingObj, invenLoadingObj;

    List<ShopItem> localShopItems = new();
    List<ShopInvenItem> localShopInvenItems = new();

    [SerializeField]
    GameObject shopItemPrefab, shopInvenItemPrefab;

    [SerializeField]
    ShopDesc shopDesc;

    string storeID = "BACKGROUND";

    bool boughtItem = false;
    bool resetingInven = false;

    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);

        backgroundToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                storeID = "BACKGROUND";
                invenManager.GetCatalog();
                invenManager.GetStore(storeID);

                backgroundToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
            }
            else
            {
                backgroundToggle.targetGraphic.color = new Color(1, 1, 1);
            }
        });
        potionToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                storeID = "POTION";
                invenManager.GetCatalog();
                invenManager.GetStore(storeID);

                potionToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
            }
            else
            {
                potionToggle.targetGraphic.color = new Color(1, 1, 1);
            }
        });

        invenManager.GetPlayerInventory();
    }

    // Update is called once per frame
    void Update()
    {
        if (boughtItem && !invenManager.buyingItem)
        {
            boughtItem = false;
            resetingInven = true;
            invenManager.GetPlayerInventory();
        }

        if ((invenManager.loadingStoreItems || invenManager.loadingCatalogItems) && shopItemContent.gameObject.activeSelf)
        {
            foreach (ShopItem shopItem in localShopItems)
            {
                Destroy(shopItem.gameObject);
            }

            localShopItems.Clear();

            shopItemContent.gameObject.SetActive(false);
            loadingObj.SetActive(true);
        }

        if (!invenManager.loadingStoreItems && !invenManager.loadingCatalogItems && !shopItemContent.gameObject.activeSelf)
        {
            foreach (StoreItem storeItem in invenManager.storeItems)
            {
                GameObject newItem = Instantiate(shopItemPrefab);
                ShopItem newShopItem = newItem.GetComponent<ShopItem>();

                newItem.transform.SetParent(shopItemContent);
                newItem.transform.localPosition = Vector3.zero;

                string storeItemID = storeItem.ItemId;
                newShopItem.SetInfo(storeItemID, invenManager.catalogMap[storeItemID].DisplayName, 
                    storeItem.VirtualCurrencyPrices, invenManager.catalogMap[storeItemID].Description, shopDesc, this);

                localShopItems.Add(newShopItem);
            }

            shopItemContent.gameObject.SetActive(true);
            loadingObj.SetActive(false);
        }

        if (invenManager.loadingInventory && shopInvenItemContent.gameObject.activeSelf && localShopInvenItems.Count == 0) //only show loading screen when empty
        {
            shopInvenItemContent.gameObject.SetActive(false);
            invenLoadingObj.SetActive(true);
        }

        if (!invenManager.loadingInventory && (!shopInvenItemContent.gameObject.activeSelf || resetingInven))
        {
            resetingInven = false;

            foreach (ShopInvenItem shopInvenItem in localShopInvenItems)
            {
                Destroy(shopInvenItem.gameObject);
            }

            localShopInvenItems.Clear();

            foreach (ItemInstance itemInstance in invenManager.invenItems)
            {
                if (itemInstance.ItemId != "BU1") //hide bundle obj
                {
                    GameObject newItem = Instantiate(shopInvenItemPrefab);
                    ShopInvenItem newShopInvenItem = newItem.GetComponent<ShopInvenItem>();

                    newItem.transform.SetParent(shopInvenItemContent);

                    newShopInvenItem.SetInfo(itemInstance.ItemId, itemInstance.RemainingUses);

                    localShopInvenItems.Add(newShopInvenItem);
                }
            }

            shopInvenItemContent.gameObject.SetActive(true);
            invenLoadingObj.SetActive(false);

            //buy button check
            if (shopDesc.itemID != "")
            {
                shopDesc.EnableBuyButton(CanBuyItem(shopDesc.itemID));
            }
        }
    }

    public void OpenPanel(System.Action callBack = null)
    {
        shopDesc.itemID = "";

        invenManager.GetCatalog();
        invenManager.GetStore(storeID);
        backgroundToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
        potionToggle.targetGraphic.color = new Color(1, 1, 1);

        shopDesc.EnableDesc(false);

        panel.SetActive(true);
        playerCallback = callBack;
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        playerCallback();
    }

    public void OnBuyButtonClicked()
    {
        if (invenManager.catalogMap[shopDesc.itemID].Tags.Contains("unique"))
        {
            shopDesc.EnableBuyButton(false);
        }

        invenManager.BuyItem(storeID, shopDesc.itemID, shopDesc.vcType, shopDesc.price);
        boughtItem = true;
    }

    public bool CanBuyItem(string buyingItemID)
    {
        if (!invenManager.catalogMap[buyingItemID].Tags.Contains("unique"))
        {
            return true;
        }
        else
        {
            if (invenManager.invenItems.Exists(item => item.ItemId == buyingItemID))
            {
                return false;
            }

            return true;
        }
    }
}
