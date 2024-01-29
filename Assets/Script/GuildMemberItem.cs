using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.GroupsModels;
using TMPro;

public class GuildMemberItem : MonoBehaviour
{
    public TMP_Text playerName, coins, status;

    PlayFabUserMgtTMP pfManager;

    GuildManager guildManager;

    public string memberPlayfabID;
    public EntityKey memberEntityKey;

    public Button kickButton;

    public float refreshTime, refreshTimer;

    // Start is called before the first frame update
    void Start()
    {
        pfManager = GameObject.Find("PFManager").GetComponent<PlayFabUserMgtTMP>();
        guildManager = GameObject.Find("Guild Panel").GetComponent<GuildManager>();

        kickButton.onClick.AddListener(() =>
        {
            guildManager.OnKickMemberButtonClicked(memberEntityKey);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
