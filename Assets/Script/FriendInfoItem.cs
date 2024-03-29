using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendInfoItem : MonoBehaviour
{
    public TMP_Text playerName, xp, status;

    PlayFabUserMgtTMP pfManager;

    public string friendID;

    public Button unfriendButton;

    public float refreshTime, refreshTimer;

    // Start is called before the first frame update
    void Start()
    {
        pfManager = GameObject.Find("PFManager").GetComponent<PlayFabUserMgtTMP>();

        unfriendButton.onClick.AddListener(() =>
        {
            pfManager.DenyFriendRequest(friendID, true);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
