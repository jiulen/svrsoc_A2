using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab.GroupsModels;

public class GuildInfoObj : MonoBehaviour
{
    public TMP_Text guildName, guildMembers, guildWealth, guildDesc;

    public GuildManager guildManager;

    public EntityKey guildEntityKey;

    public string guildNameStr, guildTagStr;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
