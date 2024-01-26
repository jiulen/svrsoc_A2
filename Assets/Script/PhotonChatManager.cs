using Photon.Chat;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using TMPro;

public class PhotonChatManager : MonoBehaviour, IChatClientListener
{
    #region Setup
    ChatClient chatClient;
    bool isConnected;
    [SerializeField] string username;
    public void UsernameOnValueChange(string valueIn)
    {
        username = valueIn;
    }
    public void ChatConnect()
    {
        isConnected = true;
        chatClient = new ChatClient(this);
        //chatClient.ChatRegion = "US";
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion, new AuthenticationValues(username));
        Debug.Log("Connecting");
    }
    public void ChangeChatUsername()
    {
        chatPanel.SetActive(false);
        chatClient.Disconnect();
        chatClient = new ChatClient(this);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion, new AuthenticationValues(username));
    }
    #endregion Setup
    #region General
    [SerializeField] GameObject chatPanel;
    string privateReceiver = "";
    string currentChat;
    [SerializeField] TMP_InputField chatField;
    [SerializeField] TMP_Text chatDisplay;
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        if (isConnected)
        {
            chatClient.Service();
        }
    }
    public void OnTextChange()
    {
        currentChat = chatField.text;

        if (chatField.text.EndsWith("\n"))
        {
            currentChat = currentChat.Remove(currentChat.Length - 1);

            if (currentChat != "")
            {
                SubmitPublicChatOnClick();
                SubmitPrivateChatOnClick();
            }
        }
    }
    #endregion General
    #region PublicChat
    public void SubmitPublicChatOnClick()
    {
        if (privateReceiver == "")
        {
            chatClient.PublishMessage("RegionChannel", currentChat);
            chatField.text = "";
            currentChat = "";
        }
    }
    #endregion PublicChat
    #region PrivateChat
    public void SubmitPrivateChatOnClick()
    {
        if (privateReceiver != "")
        {
            chatClient.SendPrivateMessage(privateReceiver, currentChat);
            chatField.text = "";
            currentChat = "";
        }
    }
    #endregion PrivateChat
    #region Callbacks
    public void DebugReturn(DebugLevel level, string message)
    {
        //throw new System.NotImplementedException();
    }
    public void OnChatStateChange(ChatState state)
    {
        if (state == ChatState.Uninitialized)
        {
            isConnected = false;
            chatPanel.SetActive(false);
        }
    }
    public void OnConnected()
    {
        Debug.Log("ConnectedChat");
        chatClient.Subscribe(new string[] { "RegionChannel" });
    }
    public void OnDisconnected()
    {
        Debug.Log("DisconnectedChat");
        isConnected = false;
        chatPanel.SetActive(false);
        chatClient.Unsubscribe(new string[] { "RegionChannel" });
    }
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        string msgs = "";
        for (int i = 0; i < senders.Length; i++)
        {
            msgs = string.Format("{0}: {1}", senders[i], messages[i]);
            chatDisplay.text += "\n" + msgs;
            Debug.Log(msgs);
        }
    }
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        string msgs = "";
        msgs = string.Format("(Private) {0}: {1}", sender, message);
        chatDisplay.text += "\n " + msgs;
        Debug.Log(msgs);

    }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new System.NotImplementedException();
    }
    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("sus");
        chatPanel.SetActive(true);
    }
    public void OnUnsubscribed(string[] channels)
    {
        throw new System.NotImplementedException();
    }
    public void OnUserSubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }
    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new System.NotImplementedException();
    }
    #endregion Callbacks
}