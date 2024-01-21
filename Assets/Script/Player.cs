using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviourPunCallbacks
{
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

    public TMP_Text playerName;

    //strings
    public const string PLAYER_NAME = "PlayerName";

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
        }

        if (photonView.IsMine || isOffline)
        {
            eButton.SetActive(false);
        }
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
            PhotonNetwork.LoadLevel(1);
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

    public void SetDisplayName(string displayName)
    {
        ExitGames.Client.Photon.Hashtable playerProps = new() { { PLAYER_NAME, displayName } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        playerName.text = displayName;
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (!photonView.IsMine && targetPlayer == photonView.Owner)
        {
            if (changedProps.TryGetValue(PLAYER_NAME, out object newPlayerName))
            {
                playerName.text = (string)newPlayerName;
            }
        }
    }

    [PunRPC]
    public void AddCoinsRpc(int coinsToAdd)
    {
        invenManager.AddCoins(coinsToAdd);
    }
}
