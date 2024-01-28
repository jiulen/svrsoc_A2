using PlayFab;
using PlayFab.GroupsModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GuildManager : MonoBehaviour
{
    public bool loadingGuildList = false;
    public bool loadingCurrentGuild = false;

    public TMP_InputField guildName_if, guildDesc_if, guildTag_if;

    public GuildController guildController;

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

    public void Test()
    {
        GetPlayerEntityKey(TestA, "0");
    }

    public void TestA(EntityKey key)
    {

    }
}
