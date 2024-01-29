using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GuildMemberItem : MonoBehaviour
{
    public TMP_Text playerName, xp, status;

    PlayFabUserMgtTMP pfManager;

    GuildManager guildManager;

    public string friendID;

    public Button kickButton;

    public float refreshTime, refreshTimer;

    // Start is called before the first frame update
    void Start()
    {
        pfManager = GameObject.Find("PFManager").GetComponent<PlayFabUserMgtTMP>();
        guildManager = GameObject.Find("Guild Panel").GetComponent<GuildManager>();

        kickButton.onClick.AddListener(() =>
        {
            //pfManager.DenyFriendRequest(friendID, true);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
