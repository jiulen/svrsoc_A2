using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;
using TMPro;

public class TradeController : MonoBehaviour
{
    [SerializeField] Player thisPlayer;

    InventoryManager invenManager;

    List<TradeInvenItem> localTradeInvenItems = new();
    List<TradeReqShopItem> localTradeReqItems = new();

    [SerializeField]
    Transform tradeInvenItemContent, tradeReqItemContent;

    [SerializeField]
    GameObject tradeInvenItemPrefab, tradeReqItemPrefab;

    [SerializeField]
    GameObject tradeOfferInvenPanel, tradeReqItemsPanel;

    [SerializeField]
    TradeOfferItem[] tradeOfferItems;
    [SerializeField]
    TradeReqItem[] tradeReqItems;

    TradeOfferItem selectedSlot;
    TradeReqItem selectedReqSlot;

    bool loadTradeInven = false;

    PlayFabUserMgtTMP pfManager;

    string currentTradeID;

    [SerializeField]
    Button sendButton, cancelButton;

    [SerializeField]
    TMP_Text sendBtnText;

    // Start is called before the first frame update
    void Awake()
    {
        invenManager = GameObject.Find("InvenManager").GetComponent<InventoryManager>();
        pfManager = GameObject.Find("PFManager").GetComponent<PlayFabUserMgtTMP>();
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

    void RefreshTradeReqItems()
    {
        foreach (var item in localTradeReqItems)
        {
            Destroy(item.gameObject);
        }

        localTradeReqItems.Clear();

        foreach (var catItem in invenManager.catalogMap)
        {
            if (catItem.Key != "BU1") //hide bundle obj
            {
                GameObject newItem = Instantiate(tradeReqItemPrefab);
                TradeReqShopItem newTradeReqShopItem = newItem.GetComponent<TradeReqShopItem>();

                newItem.transform.SetParent(tradeReqItemContent);

                newTradeReqShopItem.SetInfo(catItem.Key);
                newTradeReqShopItem.tradeController = this;

                localTradeReqItems.Add(newTradeReqShopItem);
            }
        }
    }

    public void OpenTradePanel()
    {
        gameObject.SetActive(true);
        loadTradeInven = true;
        invenManager.GetPlayerTradeInventory();

        invenManager.GetCatalog();

        currentTradeID = "";

        sendButton.interactable = true;
        cancelButton.interactable = true;
        sendBtnText.text = "Send";

        ResetTradePanel();
        ResetReqPanel();
    }

    public void CloseTradePanel() //also cancels trade if possible
    {
        gameObject.SetActive(false);

        if (currentTradeID != "")
        {
            CancelTradeRequest();
        }
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

    public void ResetReqPanel()
    {
        foreach (var item in localTradeReqItems)
        {
            Destroy(item.gameObject);
        }

        localTradeReqItems.Clear();

        selectedReqSlot = null;

        foreach (var item in tradeReqItems)
        {
            item.invenID = "";
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

    public void OpenTradeReqCatalog(int slot)
    {
        RefreshTradeReqItems();

        tradeReqItemsPanel.SetActive(true);

        selectedReqSlot = tradeReqItems[slot];
    }

    public void CloseTradeReqCatalog()
    {
        tradeReqItemsPanel.SetActive(false);

        selectedReqSlot = null;
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

    public void SelectTradeReqItem(string reqItemID, Sprite newSprite)
    {
        if (selectedReqSlot == null)
            return;

        selectedReqSlot.itemImage.sprite = newSprite;
        selectedReqSlot.invenID = reqItemID;

        tradeReqItemsPanel.SetActive(false);

        selectedReqSlot.EnableImage(true);

        selectedReqSlot = null;
    }

    public void SendTradeReq()
    {
        List<string> offerItems = new();
        foreach (var item in tradeOfferItems)
        {
            offerItems.Add(item.invenInstID);
        }

        List<string> requestItems = new();

        OpenTradeRequest(thisPlayer.playfabPlayerID, offerItems, requestItems);
    }

    public void OpenTradeRequest(string targetID, List<string> offerItems, List<string> requestItems)
    {
        sendButton.interactable = false;
        cancelButton.interactable = false;
        sendBtnText.text = "Sending";

        var tradeReq = new OpenTradeRequest
        {
            AllowedPlayerIds = new List<string> { targetID },
            OfferedInventoryInstanceIds = offerItems,
            RequestedCatalogItemIds = requestItems
        };
        PlayFabClientAPI.OpenTrade(tradeReq,
            result =>
            {
                Debug.Log("Trade opened");
                sendBtnText.text = "Sent";
                currentTradeID = result.Trade.TradeId;

                cancelButton.interactable = true;
            }, 
            error =>
            {
                Debug.Log("Trade failed to open");
                sendBtnText.text = "Send";
                sendButton.interactable = true;
                cancelButton.interactable = true;

                Debug.Log(error.GenerateErrorReport());
                Debug.Log(error.Error);
            });
    }

    public void CancelTradeRequest()
    {
        var cancelReq = new CancelTradeRequest
        {
            TradeId = currentTradeID
        };
        PlayFabClientAPI.CancelTrade(cancelReq,
            result =>
            {
                Debug.Log("Trade cancelled");
            }, OnError);
    }

    void OnError(PlayFabError e)
    {
        Debug.Log(e.GenerateErrorReport());
        Debug.Log(e.Error);
    }
}
