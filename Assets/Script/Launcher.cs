using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public GameObject PlayerPrefab;
    public bool isOffline = false;

    public PlayFabUserMgtTMP pfManager;

    Dictionary<int, Photon.Realtime.Player> onlinePlayers = new();

    public Transform friendsParent;

    // Start is called before the first frame update
    void Start()
    {
        if (!isOffline)
        {
            Debug.Log(pfManager.GetPlayerID());
            PhotonNetwork.AuthValues = new Photon.Realtime.AuthenticationValues();
            PhotonNetwork.AuthValues.UserId = pfManager.GetPlayerID();
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        if(!isOffline)
        {
            Debug.Log("Connected");
            RoomOptions options = new RoomOptions { PlayerTtl = 100000, PublishUserId = true };

            PhotonNetwork.JoinRandomOrCreateRoom(null, 0, MatchmakingMode.FillRoom, null, null, "NewRoom", options);
        }
    }

    public override void OnJoinedRoom()
    {
        if (!isOffline)
        {
            Debug.Log("Joined");
            GameObject playerObj = PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(0, -3.0f, 0), Quaternion.identity);
            playerObj.name = "Player (Me)";
            Player playerScript = playerObj.GetComponent<Player>();
            playerScript.settingButton.SetActive(false);

            pfManager.player = playerScript;
            pfManager.LoadPlayerInfo();

            FindOnlinePlayers();
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        FindOnlinePlayers();
    }

    public void FindOnlinePlayers()
    {
        onlinePlayers = PhotonNetwork.CurrentRoom.Players;

        foreach (Transform child in friendsParent)
        {
            var info = child.GetComponent<FriendInfoItem>();
            if (info != null)
            {
                bool foundPlayer = false;
                foreach (var friendPlayer in onlinePlayers)
                {
                    if (friendPlayer.Value.UserId == info.friendID)
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
                        if (friendPlayer.Value.UserId == req.friendID)
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
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        FindOnlinePlayers();
    }

    public void PhotonLogout()
    {
        PhotonNetwork.LeaveRoom(false);
        PhotonNetwork.Disconnect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
