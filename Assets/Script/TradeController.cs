using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;

public class TradeController : MonoBehaviour
{
    InventoryManager invenManager;

    List<TradeInvenItem> localTradeInvenItems = new();

    [SerializeField]
    Transform tradeInvenItemContent;

    [SerializeField]
    GameObject tradeInvenItemPrefab;

    [SerializeField]
    GameObject tradeOfferInvenPanel;

    [SerializeField]
    TradeOfferItem[] tradeOfferItems;

    TradeOfferItem selectedSlot;

    bool loadTradeInven = false;

    // Start is called before the first frame update
    void Awake()
    {
        invenManager = GameObject.Find("InvenManager").GetComponent<InventoryManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!invenManager.loadingTradeInventory && loadTradeInven)
        {
            loadTradeInven = false;

            RefreshTradeInven();
        }
    }

    void RefreshTradeInven()
    {
        foreach (TradeInvenItem tradeInvenItem in localTradeInvenItems)
        {
            Destroy(tradeInvenItem.gameObject);
        }

        localTradeInvenItems.Clear();

        foreach (ItemInstance itemInstance in invenManager.invenItems)
        {
            if (itemInstance.ItemId != "BU1") //hide bundle obj
            {
                int? uses = itemInstance.RemainingUses;

                if (uses == null)
                {
                    bool used = false;

                    //Check if item already used for trading
                    foreach (TradeOfferItem item in tradeOfferItems)
                    {
                        if (item.invenID == itemInstance.ItemId)
                        {
                            used = true;
                        }
                    }

                    if (used)
                        continue;
                }
                else
                {
                    int tradeUses = 0;

                    foreach (TradeOfferItem item in tradeOfferItems)
                    {
                        if (item.invenID == itemInstance.ItemId)
                            ++tradeUses;

                    }

                    uses -= tradeUses;

                    if (uses <= 0)
                        continue;
                }

                GameObject newItem = Instantiate(tradeInvenItemPrefab);
                TradeInvenItem newTradeInvenItem = newItem.GetComponent<TradeInvenItem>();

                newItem.transform.SetParent(tradeInvenItemContent);

                newTradeInvenItem.SetInfo(itemInstance.ItemId, uses);
                newTradeInvenItem.invenInstID = itemInstance.ItemInstanceId;
                newTradeInvenItem.tradeController = this;

                localTradeInvenItems.Add(newTradeInvenItem);
            }
        }
    }

    public void OpenTradePanel()
    {
        gameObject.SetActive(true);
        loadTradeInven = true;
        invenManager.GetPlayerTradeInventory();
    }

    public void ResetTradePanel()
    {
        loadTradeInven = false;

        foreach (TradeInvenItem tradeInvenItem in localTradeInvenItems)
        {
            Destroy(tradeInvenItem.gameObject);
        }

        localTradeInvenItems.Clear();

        selectedSlot = null;

        foreach (var item in tradeOfferItems)
        {
            item.invenID = "";
            item.invenInstID = "";
            item.EnableImage(false);
        }
    }

    public void OpenTradeInventory(int slot)
    {
        RefreshTradeInven();

        tradeOfferInvenPanel.SetActive(true);

        selectedSlot = tradeOfferItems[slot];
    }

    public void CloseTradeInventory()
    {
        tradeOfferInvenPanel.SetActive(false);

        selectedSlot = null;
    }

    public void SelectTradeOfferItem(string itemInstID, string itemID, Sprite newSprite)
    {
        if (selectedSlot == null)
            return;

        selectedSlot.itemImage.sprite = newSprite;
        selectedSlot.invenID = itemID;
        selectedSlot.invenInstID = itemInstID;

        tradeOfferInvenPanel.SetActive(false);

        selectedSlot.EnableImage(true);

        selectedSlot = null;
    }
}
