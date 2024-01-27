using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviourPunCallbacks
{
    public string playfabPlayerID;

    private float moveSpeed = 7.0f;
    private bool canMove = true;

    private Rigidbody2D rigidBody2D;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float directionX = 0f;
    private GameObject shopPanel;
    private GameObject guildPanel;
    private GameObject friendsPanel;
    private GameObject leaderPanel;
    private GameObject eButton;

    public bool isOffline = false;

    PlayFabUserMgtTMP pfManager;

    InventoryManager invenManager;

    enum CONTACT_TYPE
    {
        NIL,
        SHOP,
        GUILD,
        FRIEND,
        LEADERBOARD,
        GAME,
    }

    private CONTACT_TYPE contactType;
    private TextMeshProUGUI currentText;

    //player name (head)
    public TMP_Text playerName;
    public GameObject settingButton;

    //player options
    public TMP_Text playerOptionsName;
    public GameObject optionsPanel;
    public Button addFriendButton;
    public TMP_Text addFriendText;
    public Button tradeButton;

    //strings
    public const string PLAYER_NAME = "PlayerName";
    public const string PLAYER_ID = "PlayerID";
    public const string TRADE_PLAYER_ID = "TradePlayerID";
    public const string TRADE_ID = "TradeID";
    public const string TRADE_PHOTON_ID = "TradePhotonID";
    public const string TRADE_RESULT = "TradeResult"; //0 is cancelled, 1 is accepted
    public const string TRADE_CANCELLED = "TradeCancelled"; //only trade owner can send to trade target
    public const string TRADE_RESULT_PHOTON_ID = "TradeResultPhotonID"; //make sure only trade owner receives it

    public TradeController tradeController;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        guildPanel = GameObject.FindGameObjectWithTag("Guild"); 
        shopPanel = GameObject.FindGameObjectWithTag("Shop");
        friendsPanel = GameObject.FindGameObjectWithTag("Friends");
        leaderPanel = GameObject.FindGameObjectWithTag("Leaderboard");
        currentText = GameObject.FindGameObjectWithTag("CurrentText").GetComponent<TextMeshProUGUI>();
        eButton = GameObject.Find("EButton");

        pfManager = GameObject.Find("PFManager").GetComponent<PlayFabUserMgtTMP>();

        invenManager = GameObject.Find("InvenManager").GetComponent<InventoryManager>();

        canMove = true;

        if (photonView.Owner.CustomProperties.TryGetValue(PLAYER_NAME, out object newPlayerName))
        {
            playerName.text = (string)newPlayerName;
            playerOptionsName.text = (string)newPlayerName;
        }
        if (photonView.Owner.CustomProperties.TryGetValue(PLAYER_ID, out object newPlayerID))
        {
            if ((string)newPlayerID != "")
            {
                playfabPlayerID = (string)newPlayerID;
            }
        }

        if (photonView.IsMine || isOffline)
        {
            eButton.SetActive(false);
        }

        //buttons
        addFriendButton.onClick.AddListener(() =>
        {
            pfManager.SendFriendRequest(this, playfabPlayerID);
        });

        tradeButton.onClick.AddListener(() =>
        {
            tradeController.OpenTradePanel();
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine || isOffline)
        {
            if (canMove)
            {
                if (pfManager != null)
                {
                    if (pfManager.openUI) return;
                }

                directionX = Input.GetAxisRaw("Horizontal");
                rigidBody2D.velocity = new Vector2(directionX * moveSpeed, rigidBody2D.velocity.y);

                //If we press E
                if (Input.GetKeyDown(KeyCode.E))
                {
                    switch (contactType)
                    {
                        case CONTACT_TYPE.SHOP:
                            shopPanel.GetComponent<ShopController>().OpenPanel(ClosePanel);
                            break;

                        case CONTACT_TYPE.FRIEND:
                            friendsPanel.GetComponent<FriendsController>().OpenPanel(ClosePanel);
                                  break;

                        case CONTACT_TYPE.LEADERBOARD:
                            leaderPanel.GetComponent<LeaderboardController>().OpenPanel(ClosePanel);
                            break;

                        case CONTACT_TYPE.GUILD:
                            guildPanel.GetComponent<GuildController>().OpenPanel(ClosePanel);
                            break;

                        case CONTACT_TYPE.GAME:
                            GoToGame();
                            break;
                    }

                    if(contactType != CONTACT_TYPE.NIL)
                    {
                        canMove = false;
                        rigidBody2D.velocity = Vector3.zero;
                        animator.enabled = false;
                        eButton.SetActive(false);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    animator.SetTrigger("Is_Dancing");
                }

                //Update the animation
                UpdateAnimationUpdate();
            }
        }
    }

    public void GoToGame()
    {
        if(!isOffline)
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.LoadLevel("Game");
        }
        else
        {
            SceneManager.LoadScene("Game");
        }
    }

    private void UpdateAnimationUpdate()
    {
        if(directionX > 0f)
        {
            animator.SetBool("Is_Running", true);
            spriteRenderer.flipX = false;

        }
        else if(directionX < 0f)
        {
            animator.SetBool("Is_Running", true);
            spriteRenderer.flipX = true;
        }
        else
        {
            animator.SetBool("Is_Running", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (PhotonNetwork.IsMasterClient || isOffline)
        {
            if (collision.gameObject.CompareTag("Coin"))
            {
                RainingCoin rainingCoin = collision.GetComponent<RainingCoin>();

                if (rainingCoin.destroyed)
                    return;

                photonView.RPC("AddCoinsRpc", photonView.Owner, 1);
                rainingCoin.destroyed = true;
                PhotonNetwork.Destroy(rainingCoin.gameObject);
            }
        }
        

        if (photonView.IsMine || isOffline)
        {
            if (collision.gameObject.CompareTag("Guild"))
            {
                contactType = CONTACT_TYPE.GUILD;
                eButton.SetActive(true);
                currentText.text = "Guild";
            }
            else if (collision.gameObject.CompareTag("Shop"))
            {           
                contactType = CONTACT_TYPE.SHOP;
                eButton.SetActive(true);
                currentText.text = "Shop";
            }
            else if (collision.gameObject.CompareTag("Friends"))
            {
                contactType = CONTACT_TYPE.FRIEND;
                eButton.SetActive(true);
                currentText.text = "Friends";
            }
            else if (collision.gameObject.CompareTag("Leaderboard"))
            {
                contactType = CONTACT_TYPE.LEADERBOARD;
                eButton.SetActive(true);
                currentText.text = "Leaderboard";
            }
            else if (collision.gameObject.CompareTag("Game"))
            {
                contactType = CONTACT_TYPE.GAME;
                eButton.SetActive(true);
                currentText.text = "Game";
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (photonView.IsMine || isOffline)
        {
            if (!collision.gameObject.CompareTag("Coin"))
            {
                contactType = CONTACT_TYPE.NIL;
                eButton.SetActive(false);
                currentText.text = "";
            }
        }
    }

    public void ClosePanel()
    {
        if (photonView.IsMine || isOffline)
        {
            canMove = true;
            animator.enabled = true;
            eButton.SetActive(true);
        }
    }

    public void SetDisplayNameID(string displayName, string playerID = "")
    {
        ExitGames.Client.Photon.Hashtable playerProps = new() { { PLAYER_NAME, displayName } };
        if (playerID != "")
        {
            playerProps.Add(PLAYER_ID, playerID);

            playfabPlayerID = playerID;
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        playerName.text = displayName;
        playerOptionsName.text = displayName;
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (targetPlayer == photonView.Owner)
        {
            if (!photonView.IsMine)
            {
                if (changedProps.TryGetValue(PLAYER_NAME, out object newPlayerName))
                {
                    playerName.text = (string)newPlayerName;
                    playerOptionsName.text = (string)newPlayerName;
                }

                if (changedProps.TryGetValue(PLAYER_ID, out object newPlayerID))
                {
                    if ((string)newPlayerID != "")
                    {
                        playfabPlayerID = (string)newPlayerID;
                    }
                }
            }

            if (photonView.IsMine)
            {
                if (changedProps.TryGetValue(TRADE_ID, out object tradeID))
                {
                    tradeController.incomingTradeID = (string)tradeID;

                    if (changedProps.TryGetValue(TRADE_PLAYER_ID, out object tradePlayerID))
                    {
                        tradeController.incomingTradingPlayerID = (string)tradePlayerID;

                        if (changedProps.TryGetValue(TRADE_PHOTON_ID, out object tradePhotonID))
                        {
                            tradeController.incomingTradePhotonID = (string)tradePhotonID;

                            tradeController.ExamineTrade();
                        }
                    }
                }

                if (changedProps.TryGetValue(TRADE_CANCELLED, out object cancelled))
                {
                    bool tradeCancelled = (bool)cancelled;
                    if (tradeCancelled)
                    {
                        tradeController.CloseTradePanel();
                    }
                }
            }

            if (changedProps.TryGetValue(TRADE_RESULT, out object result))
            {
                if (changedProps.TryGetValue(TRADE_RESULT_PHOTON_ID, out object resultPhotonID))
                {
                    string photonID = (string)resultPhotonID;
                    Photon.Realtime.Player tradeOwner = FindPhotonPlayer(photonID);

                    if (tradeOwner != null)
                    {
                        if (tradeOwner.IsLocal)
                        {
                            int tradeResult = (int)result;

                            if (tradeResult == 0)
                            {
                                Debug.Log("target cancelled");
                                tradeController.CloseTradePanel();
                            }
                            else if (tradeResult == 1)
                            {
                                tradeController.currentTradeID = "";
                                tradeController.CloseTradePanel();
                                invenManager.GetPlayerInventory();
                            }
                        }
                    }
                }
            }
        }
    }

    [PunRPC]
    public void AddCoinsRpc(int coinsToAdd)
    {
        invenManager.AddCoins(coinsToAdd);
    }

    public void OpenOptions(bool open)
    {
        optionsPanel.SetActive(open);
        pfManager.openUI = open;

        if (open)
        {
            UpdateOptions();
        }
    }

    public void UpdateOptions()
    {
        PlayFab.ClientModels.FriendInfo thisFriend = pfManager._friends.Find(friend => friend.FriendPlayFabId == playfabPlayerID);
        if (thisFriend != null)
        {
            if (thisFriend.Tags.Contains("confirmed"))
            {
                addFriendText.text = "Already friends!";

                addFriendButton.interactable = false;
            }
            else if (thisFriend.Tags.Contains("requestee"))
            {
                addFriendText.text = "Sent friend request!";

                addFriendButton.interactable = false;
            }
            else if (thisFriend.Tags.Contains("requester"))
            {
                addFriendText.text = "Incoming friend request!";

                addFriendButton.interactable = false;
            }
        }
    }

    public void SendTrade(string tradeID, string tradingPlayerID, string tradePhotonID)
    {
        ExitGames.Client.Photon.Hashtable playerProps = new() { { TRADE_ID, tradeID }, { TRADE_PLAYER_ID, tradingPlayerID} , { TRADE_PHOTON_ID, tradePhotonID} };
        photonView.Owner.SetCustomProperties(playerProps);
    }

    public void OwnerCancelTrade()
    {
        ExitGames.Client.Photon.Hashtable playerProps = new() { { TRADE_CANCELLED, true } };
        photonView.Owner.SetCustomProperties(playerProps);
    }

    public void TargetCancelTrade(string photonID)
    {
        ExitGames.Client.Photon.Hashtable playerProps = new() { { TRADE_RESULT, 0 }, { TRADE_RESULT_PHOTON_ID, photonID } };
        photonView.Owner.SetCustomProperties(playerProps);
    }

    public void TargetAcceptTrade(string photonID)
    {
        ExitGames.Client.Photon.Hashtable playerProps = new() { { TRADE_RESULT, 1 }, { TRADE_RESULT_PHOTON_ID, photonID} };
        photonView.Owner.SetCustomProperties(playerProps);
    }

    public Photon.Realtime.Player FindPhotonPlayer(string photonID)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.UserId == photonID)
                return player;
        }

        return null;
    }
}
