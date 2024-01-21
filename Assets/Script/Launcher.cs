using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Launcher : MonoBehaviourPunCallbacks
{
    public GameObject PlayerPrefab;
    public bool isOffline = false;

    public PlayFabUserMgtTMP pfManager;

    // Start is called before the first frame update
    void Start()
    {
        if (!isOffline)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        if(!isOffline)
        {
            Debug.Log("Connected");
            PhotonNetwork.JoinRandomOrCreateRoom();
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
        }
    }    

    // Update is called once per frame
    void Update()
    {
        
    }
}
