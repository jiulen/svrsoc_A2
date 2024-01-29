using PlayFab;
using PlayFab.GroupsModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab.Json;
using UnityEngine.UI;

public class GuildManager : MonoBehaviour
{
    public bool loadingGuildList = false;
    public bool loadingCurrentGuild = false;
    public bool loadingGuildInfo = false;

    public TMP_InputField guildName_if, guildDesc_if, guildTag_if;

    public GuildController guildController;

    public Transform guildListContent;
    public GameObject guildListItemPrefab;

    public GuildInfoObj guildInfoObj;
    public GameObject guildMemberItemPrefab;

    public EntityKey ownEntityKey = new() { Id = "", Type = ""};

    public Launcher launcher;

    public Button openCreateGuildButton;

    public PlayFabUserMgtTMP pfManager;

    List<GuildMemberItem> guildMemberItemList = new();

    // A local cache of some bits of PlayFab data
    // This cache pretty much only serves this example , and assumes that entities are uniquely identifiable by EntityId alone, which isn't technically true. Your data cache will have to be better.
    public readonly HashSet<KeyValuePair<string, string>> EntityGroupPairs = new HashSet<KeyValuePair<string, string>>();
    public readonly Dictionary<string, string> GroupNameById = new Dictionary<string, string>();

    public static EntityKey EntityKeyMaker(PlayFab.ClientModels.EntityKey clientEntityKey)
    {
        return new EntityKey { Id = clientEntityKey.Id, Type = clientEntityKey.Type };
    }

    public static EntityKey EntityKeyMaker(string entityID)
    {
        return new EntityKey { Id = entityID };
    }

    private void OnSharedError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void CreateGroupButtonClicked()
    {
        if (ownEntityKey.Id == "")
        {
            GetOwnEntityKey(result =>
            {
                DoCreateGroup(result);
            });
        }
        else
        {
            DoCreateGroup(ownEntityKey);
        }
    }

    void DoCreateGroup(EntityKey entityKey)
    {
        CreateGroup(guildName_if.text, entityKey, guildDesc_if.text, guildTag_if.text);
        guildController.ShowCreateGuildPanel(false);
    }

    public void CreateGroup(string groupName, EntityKey entityKey, string groupDesc, string groupTag)
    {
        Debug.Log("test2 : " + entityKey);
        var data = new Dictionary<string, object>()
        {
            { "Description", groupDesc },
            { "Tag", groupTag }
        };

        var dataList = new List<PlayFab.DataModels.SetObject>()
        {
            new PlayFab.DataModels.SetObject()
            {
                ObjectName = "GroupData",
                DataObject = data
            }
        };

        var req = new PlayFab.ClientModels.ExecuteCloudScriptRequest
        {
            FunctionName = "CreateTitleGroup",
            FunctionParameter = new { groupname = groupName, entitykey = entityKey, datalist = dataList}
        };

        PlayFabClientAPI.ExecuteCloudScript(req
        , result =>
        {
            Debug.Log("Successfully made guild");
            if (result.Logs != null)
            {
                foreach (var log in result.Logs)
                {
                    Debug.Log(log.Message);
                }
            }

            guildController.guildsToggle.isOn = false;
            guildController.currGuildToggle.isOn = true;
        }
        , error =>
        {
            Debug.Log("Failed to make guild");
            OnSharedError(error);
        });
    }

    public void ShowGuildList()
    {
        openCreateGuildButton.interactable = false;
        if (ownEntityKey.Id == "")
        {
            GetOwnEntityKey(ownKey =>
            {
                CheckIfInGroup(inGroup =>
                {
                    openCreateGuildButton.interactable = !inGroup;
                }, ownKey);
            });
        }
        else
        {
            CheckIfInGroup(inGroup =>
            {
                openCreateGuildButton.interactable = !inGroup;
            }, ownEntityKey);
        }

        loadingGuildList = true;
        guildListContent.gameObject.SetActive(false);
        guildInfoObj.gameObject.SetActive(false);

        foreach (Transform child in guildListContent)
        {
            Destroy(child.gameObject);
        }

        var request = new ListMembershipRequest { Entity = new EntityKey { Id = "A8E0B", Type = "title" } };
        PlayFabGroupsAPI.ListMembership(request, 
            result =>
            {
                loadingGuildList = false;
                guildListContent.gameObject.SetActive(true);

                foreach (Transform child in guildListContent)
                {
                    Destroy(child.gameObject);
                }

                Debug.Log("Successfully get guild list");

                foreach (var guild in result.Groups)
                {
                    GameObject newItem = Instantiate(guildListItemPrefab);
                    GuildListItem newGuildListItem = newItem.GetComponent<GuildListItem>();

                    newItem.transform.SetParent(guildListContent);
                    newItem.transform.localPosition = Vector3.zero;

                    newGuildListItem.guildEntityKey = guild.Group;
                    newGuildListItem.guildName.text = guild.GroupName;
                    newGuildListItem.guildMembers.text = "? Members";
                    newGuildListItem.guildWealth.text = "? Total";

                    if (ownEntityKey.Id == "")
                    {
                        GetOwnEntityKey(ownKey =>
                        {
                            CheckIfInGroup(inGroup =>
                            {
                                newGuildListItem.joinButton.interactable = !inGroup;
                            }, ownKey);
                        });
                    }
                    else
                    {
                        CheckIfInGroup(inGroup =>
                        {
                            newGuildListItem.joinButton.interactable = !inGroup;
                        }, ownEntityKey);
                    }

                    //Check guild members
                    var req2 = new ListGroupMembersRequest
                    {
                        Group = guild.Group
                    };

                    PlayFabGroupsAPI.ListGroupMembers(req2,
                        result2 =>
                        {
                            int memberCount = -1; //dont count admin (title)
                            int totalWealth = 0;

                            foreach (var role in result2.Members)
                            {
                                memberCount += role.Members.Count;

                                foreach (var member in role.Members)
                                {
                                    if (member.Key.Type != "title_player_account")
                                        continue;

                                    var playFabId = member.Lineage["master_player_account"].Id;

                                    var req3 = new PlayFab.ClientModels.ExecuteCloudScriptRequest
                                    {
                                        FunctionName = "GetUserVirtualCurrency",
                                        FunctionParameter = new { targetId = playFabId }
                                    };

                                    PlayFabClientAPI.ExecuteCloudScript(req3
                                    , result3 =>
                                    {
                                        Debug.Log("Successfully gotten member coins");

                                        Dictionary<string, object> dict = PlayFabSimpleJson.DeserializeObject<Dictionary<string, object>>(result3.FunctionResult.ToString());
                                        if (dict.TryGetValue("Coins", out object coinsObj))
                                        {
                                            totalWealth += Convert.ToInt32(coinsObj);
                                        }

                                        newGuildListItem.guildWealth.text = totalWealth + " Total";
                                    }
                                    , error3 =>
                                    {
                                        Debug.Log("Failed to make guild");
                                        OnSharedError(error3);
                                    });

                                }
                            }

                            if (memberCount == 1)
                                newGuildListItem.guildMembers.text = memberCount + " Member";
                            else
                                newGuildListItem.guildMembers.text = memberCount + " Members";
                        },
                        error2 =>
                        {

                        });
                }
            },
            error =>
            {
                Debug.Log("Failed to get guild list");
                OnSharedError(error);
            });
    }

    public void ShowCurrentGuild()
    {
        loadingCurrentGuild = true;
        loadingGuildInfo = true;
        guildListContent.gameObject.SetActive(false);
        guildController.notinGuildObj.SetActive(false);
        guildInfoObj.gameObject.SetActive(true);

        foreach (Transform child in guildListContent)
        {
            Destroy(child.gameObject);
        }

        if (ownEntityKey.Id == "")
        {
            GetOwnEntityKey(result =>
            {
                DoListMembership(result);
            });
        }
        else
        {
            DoListMembership(ownEntityKey);
        }
    }

    void DoListMembership(EntityKey entityKey)
    {
        guildInfoObj.playerRole = "";
        guildInfoObj.leaveButton.interactable = false;

        var request = new ListMembershipRequest { Entity = entityKey };
        PlayFabGroupsAPI.ListMembership(request,
            result =>
            {
                loadingGuildInfo = false;

                Debug.Log("Successfully get current guild info");

                if (result.Groups.Count <= 0)
                {
                    loadingCurrentGuild = false;
                    guildController.notinGuildObj.SetActive(true);
                }
                else
                {
                    var group = result.Groups[0]; //get first group player is in
                    guildInfoObj.guildEntityKey = group.Group;
                    guildInfoObj.guildNameStr = group.GroupName;
                    guildInfoObj.guildName.text = group.GroupName;

                    //get group tag and desc

                    GetGroupData(dataResult =>
                    {
                        var data = dataResult["GroupData"];
                        Dictionary<string, object> dict = PlayFabSimpleJson.DeserializeObject<Dictionary<string, object>>(data.DataObject.ToString());

                        if (dict.TryGetValue("Description", out object descObj))
                        {
                            guildInfoObj.guildDesc.text = (string)descObj;
                        }

                        if (dict.TryGetValue("Tag", out object tagObj))
                        {
                            guildInfoObj.guildTagStr = (string)tagObj;
                            guildInfoObj.guildName.text = guildInfoObj.guildNameStr + "(" + guildInfoObj.guildTagStr + ")";
                        }
                    }, group.Group);

                    //get members
                    var req2 = new ListGroupMembersRequest
                    {
                        Group = group.Group
                    };

                    PlayFabGroupsAPI.ListGroupMembers(req2,
                        result2 =>
                        {
                            int memberCount = -1; //dont count admin (title)
                            int totalWealth = 0;

                            loadingCurrentGuild = false;
                            guildListContent.gameObject.SetActive(true);

                            foreach (Transform child in guildListContent)
                            {
                                Destroy(child.gameObject);
                            }

                            guildMemberItemList.Clear();

                            foreach (var role in result2.Members)
                            {
                                memberCount += role.Members.Count;

                                foreach (var member in role.Members)
                                {
                                    if (member.Key.Type != "title_player_account")
                                        continue;

                                    var playFabId = member.Lineage["master_player_account"].Id;

                                    if (playFabId == pfManager.GetPlayerID())
                                    {
                                        guildInfoObj.playerRole = role.RoleId;
                                        if (guildInfoObj.playerRole == "Owner")
                                        {
                                            guildInfoObj.leaveButtonText.text = "Delete Guild";

                                            foreach (var item in guildMemberItemList)
                                            {
                                                if (item.memberPlayfabID != playFabId)
                                                {
                                                    item.kickButton.gameObject.SetActive(true);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            guildInfoObj.leaveButtonText.text = "Leave Guild";
                                        }
                                        guildInfoObj.leaveButton.interactable = true;
                                    }

                                    //get members info
                                    GameObject newItem = Instantiate(guildMemberItemPrefab);
                                    GuildMemberItem newGuildMemberItem = newItem.GetComponent<GuildMemberItem>();
                                    guildMemberItemList.Add(newGuildMemberItem);

                                    newItem.transform.SetParent(guildListContent);
                                    newItem.transform.localPosition = Vector3.zero;

                                    newGuildMemberItem.playerName.text = "?";
                                    newGuildMemberItem.coins.text = "?";
                                    newGuildMemberItem.status.text = "Unknown";
                                    newGuildMemberItem.status.color = Color.gray;
                                    newGuildMemberItem.memberPlayfabID = playFabId;

                                    if (guildInfoObj.playerRole == "Owner" && playFabId != pfManager.GetPlayerID()) //cannot kick myself
                                    {
                                        newGuildMemberItem.kickButton.gameObject.SetActive(true);
                                    }
                                    else
                                    {
                                        newGuildMemberItem.kickButton.gameObject.SetActive(false);
                                    }

                                    GetDisplayName(dispNameResult =>
                                    {
                                        newGuildMemberItem.playerName.text = dispNameResult;
                                    }, playFabId);

                                    //get coins to show total wealth
                                    var req3 = new PlayFab.ClientModels.ExecuteCloudScriptRequest
                                    {
                                        FunctionName = "GetUserVirtualCurrency",
                                        FunctionParameter = new { targetId = playFabId }
                                    };

                                    PlayFabClientAPI.ExecuteCloudScript(req3
                                    , result3 =>
                                    {
                                        Debug.Log("Successfully gotten member coins");

                                        Dictionary<string, object> dict = PlayFabSimpleJson.DeserializeObject<Dictionary<string, object>>(result3.FunctionResult.ToString());
                                        if (dict.TryGetValue("Coins", out object coinsObj))
                                        {
                                            int thisPlayerCoins = Convert.ToInt32(coinsObj);

                                            totalWealth += thisPlayerCoins;
                                            newGuildMemberItem.coins.text = thisPlayerCoins.ToString();
                                        }

                                        guildInfoObj.guildWealth.text = totalWealth.ToString();
                                    }
                                    , error3 =>
                                    {
                                        Debug.Log("Failed to make guild");
                                        OnSharedError(error3);
                                    });

                                }
                            }

                            guildInfoObj.guildMembers.text = memberCount.ToString();
                        },
                        error2 =>
                        {

                        });
                }
            },
            error =>
            {
                OnSharedError(error);
            });
    }

    public void DeleteGroup(string groupId)
    {
        // A title, or player-controlled entity with authority to do so, decides to destroy an existing group
        var request = new DeleteGroupRequest { Group = EntityKeyMaker(groupId) };
        PlayFabGroupsAPI.DeleteGroup(request, OnDeleteGroup, OnSharedError);
    }
    private void OnDeleteGroup(EmptyResponse response)
    {
        var prevRequest = (DeleteGroupRequest)response.Request;
        Debug.Log("Group Deleted: " + prevRequest.Group.Id);

        var temp = new HashSet<KeyValuePair<string, string>>();
        foreach (var each in EntityGroupPairs)
            if (each.Value != prevRequest.Group.Id)
                temp.Add(each);
        EntityGroupPairs.IntersectWith(temp);
        GroupNameById.Remove(prevRequest.Group.Id);
    }
    public void KickMember(string groupId, EntityKey entityKey)
    {
        var request = new RemoveMembersRequest { Group = EntityKeyMaker(groupId), Members = new List<EntityKey> { entityKey } };
        PlayFabGroupsAPI.RemoveMembers(request, OnKickMembers, OnSharedError);
    }
    private void OnKickMembers(EmptyResponse response)
    {
        var prevRequest = (RemoveMembersRequest)response.Request;

        Debug.Log("Entity kicked from Group: " + prevRequest.Members[0].Id + " to " + prevRequest.Group.Id);
        EntityGroupPairs.Remove(new KeyValuePair<string, string>(prevRequest.Members[0].Id, prevRequest.Group.Id));
    }

    public void GetPlayerEntityID(string targetPlayfabID)
    {
        var entityTokenReq = new PlayFab.AuthenticationModels.GetEntityTokenRequest
        {

        };

        PlayFabAuthenticationAPI.GetEntityToken(entityTokenReq,
                                          result => {

                                          },
                                          error => {

                                          });
    }

    public void GetPlayerEntityKey(Action<EntityKey> onEntityKeyReceived, string targetID)
    {
        var req = new PlayFab.ClientModels.GetAccountInfoRequest
        {
            PlayFabId = targetID
        };

        PlayFabClientAPI.GetAccountInfo(req, 
            (result) =>
            {
                PlayFab.ClientModels.UserAccountInfo userAccInfo = result.AccountInfo;
                var playerEntity = userAccInfo.TitleInfo.TitlePlayerAccount;
                onEntityKeyReceived(EntityKeyMaker(playerEntity));
            },
            (error) =>
            {
                OnSharedError(error);
            });
    }

    public void GetOwnEntityKey(Action<EntityKey> onEntityKeyReceived)
    {
        var req = new PlayFab.ClientModels.GetAccountInfoRequest
        {
        };

        PlayFabClientAPI.GetAccountInfo(req,
            (result) =>
            {
                PlayFab.ClientModels.UserAccountInfo userAccInfo = result.AccountInfo;
                var playerEntity = userAccInfo.TitleInfo.TitlePlayerAccount;
                ownEntityKey = EntityKeyMaker(playerEntity);
                onEntityKeyReceived(EntityKeyMaker(playerEntity));
            },
            (error) =>
            {
                OnSharedError(error);
            });
    }

    public void GetGroupData(Action<Dictionary<string, PlayFab.DataModels.ObjectResult>> response, EntityKey groupEntityKey)
    {
        var getRequest = new PlayFab.DataModels.GetObjectsRequest { Entity = new PlayFab.DataModels.EntityKey { Id = groupEntityKey.Id, Type = groupEntityKey.Type } };
        PlayFabDataAPI.GetObjects(getRequest,
            result => {
                var objs = result.Objects;
                response(objs);
            },
            error =>
            {
                OnSharedError(error);
            }
        );
    }

    public void GetDisplayName(Action<string> response, string playfabId)
    {
        var ProfileRequestParams = new PlayFab.ClientModels.GetPlayerProfileRequest
        {
            PlayFabId = playfabId
        };

        PlayFabClientAPI.GetPlayerProfile(ProfileRequestParams,
                                          result => {
                                              response(result.PlayerProfile.DisplayName);
                                          },
                                          error => { });
    }

    public void CheckIfInGroup(Action<bool> inGroup, EntityKey entityKey)
    {
        var request = new ListMembershipRequest { Entity = entityKey };
        PlayFabGroupsAPI.ListMembership(request,
            result =>
            {
                if (result.Groups.Count <= 0)
                    inGroup(false);
                else
                    inGroup(true);
            },
            error =>
            {
                OnSharedError(error);
            });
    }

    public void OnJoinButtonClicked(EntityKey groupEntityKey)
    {
        if (ownEntityKey.Id == "")
        {
            GetOwnEntityKey(ownKey =>
            {
                JoinGuild(groupEntityKey, ownKey);
            });
        }
        else
        {
            JoinGuild(groupEntityKey, ownEntityKey);
        }
    }

    void JoinGuild(EntityKey groupEntityKey, EntityKey playerEntityKey)
    {
        var req = new PlayFab.ClientModels.ExecuteCloudScriptRequest
        {
            FunctionName = "JoinTitleGroup",
            FunctionParameter = new { groupkey = groupEntityKey, entitykey = playerEntityKey }
        };

        PlayFabClientAPI.ExecuteCloudScript(req
        , result =>
        {
            Debug.Log("Successfully joined guild");

            guildController.guildsToggle.isOn = false;
            guildController.currGuildToggle.isOn = true;
        }
        , error =>
        {
            Debug.Log("Failed to join guild");
            OnSharedError(error);
        });
    }

    public void OnLeaveButtonClicked()
    {
        if (guildInfoObj.playerRole == "Owner")
        {
            //delete group instead
            var req = new PlayFab.ClientModels.ExecuteCloudScriptRequest
            {
                FunctionName = "DeleteTitleGroup",
                FunctionParameter = new { groupkey = guildInfoObj.guildEntityKey }
            };

            PlayFabClientAPI.ExecuteCloudScript(req
            , result =>
            {
                Debug.Log("Successfully deleted group");

                if (result.Error != null)
                {
                    Debug.Log(result.Error.StackTrace);
                }

                guildController.guildsToggle.isOn = true;
                guildController.currGuildToggle.isOn = false;
            }
            , error =>
            {
                Debug.Log("Failed to delete group");
                OnSharedError(error);
            });
        }
        else
        {
            //leave group like normal
            if (ownEntityKey.Id == "")
            {
                GetOwnEntityKey(ownKey =>
                {
                    LeaveGuild(ownKey);
                });
            }
            else
            {
                LeaveGuild(ownEntityKey);
            }
            
        }
    }

    void LeaveGuild(EntityKey playerEntityKey)
    {
        var leaveReq = new RemoveMembersRequest
        {
            Group = guildInfoObj.guildEntityKey,
            Members = new List<EntityKey> { playerEntityKey }
        };

        PlayFabGroupsAPI.RemoveMembers(leaveReq,
            result =>
            {
                guildController.guildsToggle.isOn = true;
                guildController.currGuildToggle.isOn = false;
            },
            error =>
            {
                OnSharedError(error);
            });
    }
}
