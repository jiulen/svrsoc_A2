using PlayFab;
using PlayFab.GroupsModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab.Json;

public class GuildManager : MonoBehaviour
{
    public bool loadingGuildList = false;
    public bool loadingCurrentGuild = false;

    public TMP_InputField guildName_if, guildDesc_if, guildTag_if;

    public GuildController guildController;

    public Transform guildListContent;
    public GameObject guildListItemPrefab;

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

    public void ListGroups(EntityKey entityKey)
    {
        var request = new ListMembershipRequest { Entity = entityKey };
        PlayFabGroupsAPI.ListMembership(request, OnListGroups, OnSharedError);
    }
    private void OnListGroups(ListMembershipResponse response)
    {
        var prevRequest = (ListMembershipRequest)response.Request;
        foreach (var pair in response.Groups)
        {
            GroupNameById[pair.Group.Id] = pair.GroupName;
            EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, pair.Group.Id));
        }
    }

    public void CreateGroupButtonClicked()
    {
        GetOwnEntityKey(result => 
        {
            CreateGroup(guildName_if.text, result, guildDesc_if.text, guildTag_if.text);
            guildController.ShowCreateGuildPanel(false);
        });
    }

    public void CreateGroup(string groupName, EntityKey entityKey, string groupDesc, string groupTag)
    {
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
        }
        , error =>
        {
            Debug.Log("Failed to make guild");
            OnSharedError(error);
        });
    }

    public void ShowGuildList()
    {
        loadingGuildList = true;
        guildListContent.gameObject.SetActive(false);

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
                                        if (result3.Logs != null)
                                        {
                                            foreach (var log in result3.Logs)
                                            {
                                                Debug.Log(log.Message);
                                            }
                                        }
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
        guildListContent.gameObject.SetActive(false);

        foreach (Transform child in guildListContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnCreateGroup(CreateGroupResponse response)
    {
        Debug.Log("Group Created: " + response.GroupName + " - " + response.Group.Id);

        var prevRequest = (CreateGroupRequest)response.Request;
        EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, response.Group.Id));
        GroupNameById[response.Group.Id] = response.GroupName;
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

    public void InviteToGroup(string groupId, EntityKey entityKey)
    {
        // A player-controlled entity invites another player-controlled entity to an existing group
        var request = new InviteToGroupRequest { Group = EntityKeyMaker(groupId), Entity = entityKey };
        PlayFabGroupsAPI.InviteToGroup(request, OnInvite, OnSharedError);
    }
    public void OnInvite(InviteToGroupResponse response)
    {
        var prevRequest = (InviteToGroupRequest)response.Request;

        // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
        var request = new AcceptGroupInvitationRequest { Group = EntityKeyMaker(prevRequest.Group.Id), Entity = prevRequest.Entity };
        PlayFabGroupsAPI.AcceptGroupInvitation(request, OnAcceptInvite, OnSharedError);
    }
    public void OnAcceptInvite(EmptyResponse response)
    {
        var prevRequest = (AcceptGroupInvitationRequest)response.Request;
        Debug.Log("Entity Added to Group: " + prevRequest.Entity.Id + " to " + prevRequest.Group.Id);
        EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, prevRequest.Group.Id));
    }

    public void ApplyToGroup(string groupId, EntityKey entityKey)
    {
        // A player-controlled entity applies to join an existing group (of which they are not already a member)
        var request = new ApplyToGroupRequest { Group = EntityKeyMaker(groupId), Entity = entityKey };
        PlayFabGroupsAPI.ApplyToGroup(request, OnApply, OnSharedError);
    }
    public void OnApply(ApplyToGroupResponse response)
    {
        var prevRequest = (ApplyToGroupRequest)response.Request;

        // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
        var request = new AcceptGroupApplicationRequest { Group = prevRequest.Group, Entity = prevRequest.Entity };
        PlayFabGroupsAPI.AcceptGroupApplication(request, OnAcceptApplication, OnSharedError);
    }
    public void OnAcceptApplication(EmptyResponse response)
    {
        var prevRequest = (AcceptGroupApplicationRequest)response.Request;
        Debug.Log("Entity Added to Group: " + prevRequest.Entity.Id + " to " + prevRequest.Group.Id);
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
}
