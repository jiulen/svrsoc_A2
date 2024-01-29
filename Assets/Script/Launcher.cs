using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class Launcher : MonoBehaviourPunCallbacks
{
    public GameObject PlayerPrefab;
    public bool isOffline = false;

    public PlayFabUserMgtTMP pfManager;

    Photon.Realtime.Player[] onlinePlayers;

    public Transform friendsParent, guildListContent;

    public GameObject friendNotifPrefab;
    public Transform friendNotifParent;

    public PhotonChatManager chatManager;

    public GuildManager guildManager;

    // Start is called before the first frame update
    void Start()
    {
        if (!isOffline)
        {
            Debug.Log(pfManager.GetPlayerID());
            PhotonNetwork.NickName = pfManager.GetPlayerName();
            PhotonNetwork.AuthValues = new Photon.Realtime.AuthenticationValues();
            PhotonNetwork.AuthValues.UserId = pfManager.GetPlayerID();
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void ChangePlayerName()
    {
        PhotonNetwork.NickName = pfManager.GetPlayerName();
        chatManager.UsernameOnValueChange(pfManager.GetPlayerName());
        chatManager.ChangeChatUsername();
    }

    public override void OnConnectedToMaster()
    {
        if(!isOffline)
        {
            Debug.Log("Connected");
            RoomOptions options = new RoomOptions { PublishUserId = true };

            PhotonNetwork.JoinOrCreateRoom("NewRoom", options, TypedLobby.Default);
        }
    }

    public override void OnJoinedRoom()
    {
        if (!isOffline)
        {
            GameObject playerObj = PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(0, -3.0f, 0), Quaternion.identity);
            playerObj.name = "Player (Me)";
            Player playerScript = playerObj.GetComponent<Player>();

            guildManager.player = playerScript;
            playerScript.guildManager = guildManager;

            playerScript.settingButton.SetActive(false);

            pfManager.player = playerScript;
            pfManager.LoadPlayerInfo();

            FindOnlinePlayers();

            chatManager.UsernameOnValueChange(pfManager.GetPlayerName());
            chatManager.ChatConnect();
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        FindOnlinePlayers();

        CheckFriendNotif(newPlayer);
    }

    public void CheckFriendNotif(Photon.Realtime.Player newPlayer)
    {
        if (pfManager._friends.Any(friend => friend.FriendPlayFabId == newPlayer.UserId))
        {
            Debug.Log($"Player {newPlayer.NickName} has joined");
            GameObject friendNotif = Instantiate(friendNotifPrefab);
            friendNotif.transform.SetParent(friendNotifParent);
            friendNotif.transform.localPosition = Vector3.zero;

            var popupNotif = friendNotif.GetComponent<PopupNotif>();
            popupNotif.SetText1(newPlayer.NickName);
        }
    }

    public void FindOnlinePlayers()
    {
        onlinePlayers = PhotonNetwork.PlayerList;

        foreach (Transform child in friendsParent)
        {
            var info = child.GetComponent<FriendInfoItem>();

            if (info != null)
            {
                bool foundPlayer = false;

                foreach (var friendPlayer in onlinePlayers)
                {
                    if (friendPlayer.UserId == info.friendID)
                    {
                        foundPlayer = true;
                        break;
                    }
                }

                if (foundPlayer)
                {
                    info.status.text = "Online";
                    info.status.color = Color.green;
                }
                else
                {
                    info.status.text = "Offline";
                    info.status.color = Color.gray;
                }
            }
            else
            {
                var req = child.GetComponent<FriendRequestItem>();
                if (req != null)
                {
                    bool foundPlayer = false;
                    foreach (var friendPlayer in onlinePlayers)
                    {
                        if (friendPlayer.UserId == req.friendID)
                        {
                            foundPlayer = true;
                            break;
                        }
                    }

                    if (foundPlayer)
                    {
                        req.status.text = "Online";
                        req.status.color = Color.green;
                    }
                    else
                    {
                        req.status.text = "Offline";
                        req.status.color = Color.gray;
                    }
                }
            }
        }

        foreach (Transform child in guildListContent)
        {
            var info = child.GetComponent<GuildMemberItem>();

            if (info != null)
            {
                bool foundPlayer = false;

                foreach (var friendPlayer in onlinePlayers)
                {
                    if (friendPlayer.UserId == info.memberPlayfabID)
                    {
                        foundPlayer = true;
                        break;
                    }
                }

                if (foundPlayer)
                {
                    info.status.text = "Online";
                    info.status.color = Color.green;
                }
                else
                {
                    info.status.text = "Offline";
                    info.status.color = Color.gray;
                }
            }
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        FindOnlinePlayers();
    }

    public void PhotonLogout()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
