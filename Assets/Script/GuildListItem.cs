using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab.GroupsModels;

public class GuildListItem : MonoBehaviour
{
    public TMP_Text guildName, guildMembers, guildWealth;

    GuildManager guildManager;

    public EntityKey guildEntityKey;

    public Button joinButton;

    // Start is called before the first frame update
    void Start()
    {
        guildManager = GameObject.Find("Guild Panel").GetComponent<GuildManager>();

        joinButton.onClick.AddListener(() =>
        {
            //guildManager.
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
