using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendRequestItem : MonoBehaviour
{
    public TMP_Text playerName, xp, status;

    PlayFabUserMgtTMP pfManager;

    public string friendID;

    public Button acceptButton, denyButton;

    public float refreshTime, refreshTimer;

    // Start is called before the first frame update
    void Start()
    {
        pfManager = GameObject.Find("PFManager").GetComponent<PlayFabUserMgtTMP>();

        //buttons
        acceptButton.onClick.AddListener(() =>
        {
            pfManager.AcceptFriendRequest(friendID);
        });

        denyButton.onClick.AddListener(() =>
        {
            pfManager.DenyFriendRequest(friendID);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
