using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;
using TMPro;
using Photon.Pun;

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
    TMP_Text sendBtnText, cancelBtnText;

    public GameObject blocker;

    public string incomingTradeID, incomingTradingPlayerID, incomingTradePhotonID;

    List<string> incomingTradeSendItemIDs = new();

    public TMP_Text infoText;

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
                if (!invenManager.catalogMap[itemInstance.ItemId].IsTradable)
                    continue;

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
                if (!catItem.Value.IsTradable)
                    continue;

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

        blocker.SetActive(false);

        pfManager.openUI = true;

        cancelBtnText.text = "Cancel";

        infoText.text = "";
        infoText.color = Color.black;
    }

    public void CloseTradePanel() //also cancels trade if possible
    {
        if (currentTradeID != "")
        {
            CancelTradeRequest();

            cancelButton.interactable = false;
            cancelBtnText.text = "Cancelling";
        }
        else
        {
            gameObject.SetActive(false);
            if (!thisPlayer.optionsPanel.activeSelf)
            {
                pfManager.openUI = false;
            }

            //notify other client trade is cancelled
        }

        incomingTradeID = "";
        incomingTradingPlayerID = "";
        incomingTradePhotonID = "";
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
        if (incomingTradeID == "")
        {
            List<string> offerItems = new();
            foreach (var item in tradeOfferItems)
            {
                if (item.invenInstID == "")
                    continue;

                offerItems.Add(item.invenInstID);
            }

            List<string> requestItems = new();
            foreach (var item in tradeReqItems)
            {
                if (item.invenID == "")
                    continue;

                requestItems.Add(item.invenID);
            }

            OpenTradeRequest(thisPlayer.playfabPlayerID, offerItems, requestItems);
        }
        else
        {
            var acceptTradeReq = new AcceptTradeRequest
            {
                OfferingPlayerId = incomingTradingPlayerID,
                TradeId = incomingTradeID,
                AcceptedInventoryInstanceIds = incomingTradeSendItemIDs
            };

            sendButton.interactable = false;
            cancelButton.interactable = false;
            sendBtnText.text = "Accepting";

            PlayFabClientAPI.AcceptTrade(acceptTradeReq,
                result =>
                {
                    CloseTradePanel();
                    invenManager.GetPlayerInventory();

                    //notify other client trade is accepted
                },
                error =>
                {
                    OnError(error);

                    infoText.text = "Failed to accept trade";
                    infoText.color = Color.red;

                    sendButton.interactable = true;
                    cancelButton.interactable = true;
                    sendBtnText.text = "Accept";
                });
        }
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

                thisPlayer.SendTrade(currentTradeID, pfManager.GetPlayerID(), PhotonNetwork.LocalPlayer.UserId);

                blocker.SetActive(true);

                infoText.text = "Waiting for player to accept trade";
                infoText.color = Color.black;
            }, 
            error =>
            {
                Debug.Log("Trade failed to open");
                sendBtnText.text = "Send";
                sendButton.interactable = true;
                cancelButton.interactable = true;

                OnError(error);

                infoText.text = "Failed to send trade request";
                infoText.color = Color.red;
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

                thisPlayer.OwnerCancelTrade();

                gameObject.SetActive(false);
                if (!thisPlayer.optionsPanel.activeSelf)
                {
                    pfManager.openUI = false;
                }
            }, 
            error =>
            {
                cancelBtnText.text = "Cancel";
                cancelButton.interactable = true;

                OnError(error);

                infoText.text = "Failed to cancel trade request";
                infoText.color = Color.red;
            });
    }

    void OnError(PlayFabError e)
    {
        Debug.Log(e.GenerateErrorReport());
        Debug.Log(e.Error);
    }

    public void ExamineTrade()
    {
        var checkTradeReq = new GetTradeStatusRequest
        {
            OfferingPlayerId = incomingTradingPlayerID,
            TradeId = incomingTradeID
        };

        PlayFabClientAPI.GetTradeStatus(checkTradeReq,
            result =>
            {
                OpenTradePanel();

                //ui stuff

                sendButton.interactable = false;
                sendBtnText.text = "Accept";
                blocker.SetActive(true); //dont let user interact with incoming trade

                List<string> offers = result.Trade.OfferedCatalogItemIds;
                List<string> requests = result.Trade.RequestedCatalogItemIds;

                //show trade

                for (int i = 0; i < offers.Count; ++i)
                {
                    tradeReqItems[i].SetInfo(offers[i]);
                    tradeReqItems[i].EnableImage(true);
                }

                for (int i = 0; i < requests.Count; ++i)
                {
                    tradeOfferItems[i].SetInfo(requests[i]);
                    tradeOfferItems[i].EnableImage(true);
                }

                infoText.text = "Finding items to trade";
                infoText.color = Color.black;

                //try to get items to trade
                var UserInv = new GetUserInventoryRequest();
                PlayFabClientAPI.GetUserInventory(UserInv,
                    result2 =>
                    {
                        List<ItemInstance> inventoryItems = result2.Inventory;
                        incomingTradeSendItemIDs.Clear();

                        foreach (ItemInstance itemInst in inventoryItems)
                        {
                            for (int i = 0; i < requests.Count; ++i)
                            {
                                if (itemInst.ItemId == requests[i])
                                {
                                    requests.RemoveAt(i);
                                    incomingTradeSendItemIDs.Add(itemInst.ItemInstanceId);
                                    continue;
                                }
                            }
                        }

                        if (requests.Count == 0)
                        {
                            sendButton.interactable = true;

                            infoText.text = "";
                            infoText.color = Color.black;
                        }
                        else
                        {
                            infoText.text = "Not enough items to trade";
                            infoText.color = Color.red;
                        }

                    }, 
                    error2 =>
                    {
                        infoText.text = "Failed to get inventory";
                        infoText.color = Color.red;

                        OnError(error2);
                    });

            },
            error =>
            {
                OnError(error);

                infoText.text = "Failed to get trade";
                infoText.color = Color.red;
            });
    }

    Photon.Realtime.Player FindPhotonPlayer()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.UserId == incomingTradePhotonID)
                return player;
        }

        return null;
    }
}
