using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab.GroupsModels;
using UnityEngine.UI;

public class GuildInfoObj : MonoBehaviour
{
    public TMP_Text guildName, guildMembers, guildWealth, guildDesc;

    public GuildManager guildManager;

    public EntityKey guildEntityKey;

    public string guildNameStr, guildTagStr;

    public string playerRole; //role id of local player

    public Button leaveButton;

    public TMP_Text leaveButtonText;
}
