using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections.Generic;
using TMPro; //for text mesh pro UI elements
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayFabUserMgtTMP : MonoBehaviour
{
    [Header("Login Screen")]
    [SerializeField] GameObject loginPanel;
    [SerializeField] TMP_InputField loginNameEmail;
    [SerializeField] TMP_InputField loginPassword;
    [SerializeField] Toggle loginRmb;
    [SerializeField] TMP_Text loginTxt;

    [Header("Register Screen")]
    [SerializeField] GameObject registerPanel;
    [SerializeField] TMP_InputField regisUsername;
    [SerializeField] TMP_InputField regisEmail;
    [SerializeField] TMP_InputField regisPassword;
    [SerializeField] TMP_InputField regisPasswordConfirm;
    [SerializeField] TMP_Text regisTxt;

    [Header("Reset Password Screen")]
    [SerializeField] GameObject resetPanel;
    [SerializeField] TMP_InputField resetEmail;
    [SerializeField] TMP_Text resetTxt;

    [Header ("Leaderboard")]
    [SerializeField] TMP_Text lbTxt;
    [SerializeField] GameObject[] lbItems; //should have 10
    const int lbMax = 10;

    [Header("Player Info")]
    public TMP_InputField dispname_field;
    [SerializeField] TMP_Text XP_field;
    [SerializeField] TMP_Text level_field;
    [SerializeField] TMP_Text coins_field, shop_coins_field;
    [SerializeField] TMP_Text dispname_error;
    [SerializeField] Slider XP_slider;
    [SerializeField] Button editDispnameButton;
    public int curXP;
    public int curLevel;
    string oldDispname;

    [SerializeField]
    InventoryManager invenManager;
    public bool gettingCoins = false;

    public bool loggedIn = true;

    public bool openUI = false; //for landing page

    bool boughtItem = false;

    [SerializeField]
    BackgroundManager bgManager;

    [SerializeField]
    PlayerColorSwitcher playerColorSwitcher;

    [SerializeField]
    MoveRightText motdText;

    [SerializeField]
    GameObject landingLoadingObj, gameLoadingObj;

    List<FriendInfo> _friends = null;

    //bools
    public bool loadingLeaderboard = false;
    public bool loadingPlayerData = false;
    public bool loadingDispName = false;
    public bool loadingMOTD = false;

    //photon stuff
    public Player player;

    private void Awake()
    {
        if (loginNameEmail != null && loginPassword != null)
        {
            loginNameEmail.text = PlayerPrefs.GetString("loginUser");
            loginPassword.text = PlayerPrefs.GetString("loginPassword");
        }

        if (loginRmb != null)
        {
            loginRmb.isOn = PlayerPrefs.GetInt("loginRmb") == 1;
        }

        if (loggedIn)
        {
            GetUserData();
            ClientGetTitleData();
            if (invenManager != null)
            {
                gettingCoins = true;
                invenManager.GetVirtualCurrencies();
            }
        }

        if (dispname_field != null)
        {
            loadingDispName = true;

            dispname_field.text = "";

            var ProfileRequestParams = new GetPlayerProfileRequest();

            PlayFabClientAPI.GetPlayerProfile(ProfileRequestParams, 
                                              result => {
                                                  loadingDispName = false;

                                                  dispname_field.text = result.PlayerProfile.DisplayName;
                                              },
                                              error => { });
        }
    }

    public void LoadPlayerHeadName()
    {
        var ProfileRequestParams = new GetPlayerProfileRequest();

        PlayFabClientAPI.GetPlayerProfile(ProfileRequestParams,
                                          result => {
                                              if (player != null) player.SetDisplayName(result.PlayerProfile.DisplayName);
                                          },
                                          error => { });
    }

    private void Update()
    {
        if (loggedIn)
        {
            if (invenManager != null)
            {
                if (invenManager.buyingItem)
                {
                    boughtItem = true;
                }

                if (boughtItem && !invenManager.buyingItem)
                {
                    boughtItem = false;
                    gettingCoins = true;
                    invenManager.GetVirtualCurrencies();
                }

                if (!invenManager.loadingCoins && gettingCoins)
                {
                    coins_field.text = invenManager.coins.ToString();
                    shop_coins_field.text = invenManager.coins.ToString();

                    gettingCoins = false;
                }
            }
        }

        if (landingLoadingObj != null)
        {
            if (landingLoadingObj.activeSelf)
            {
                if (!loadingLeaderboard && !loadingPlayerData && !loadingDispName && !loadingMOTD && !AchievementManager.Instance.loadingAchievements)
                {
                    landingLoadingObj.SetActive(false);
                }
            }
        }

        if (gameLoadingObj != null)
        {
            if (gameLoadingObj.activeSelf)
            {
                if (!loadingPlayerData && !AchievementManager.Instance.loadingAchievements)
                {
                    gameLoadingObj.SetActive(false);
                }
            }
        }
    }

    //Screen switching
    public void GoToLogin()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        resetPanel.SetActive(false);
    }
    public void GoToRegister()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        resetPanel.SetActive(false);

        regisUsername.text = "";
        regisEmail.text = "";
        regisPassword.text = "";
        regisPasswordConfirm.text = "";
        regisTxt.text = "";
    }
    public void GoToReset()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        resetPanel.SetActive(true);
    }

    void UpdateMsg(string msg, TMP_Text textBox) //to display in console and messagebox (if available)
    {
        Debug.Log(msg);
        if (textBox != null) textBox.text=msg+'\n';
    }
    void OnLoginScreenError(PlayFabError e)
    {
        UpdateMsg(e.ErrorMessage, loginTxt);
    }
    void OnRegisScreenError(PlayFabError e)
    {
        UpdateMsg(e.ErrorMessage, regisTxt);
    }
    void OnResetScreenError(PlayFabError e)
    {
        UpdateMsg(e.ErrorMessage, resetTxt);
    }
    void OnLeaderboardError(PlayFabError e)
    {
        UpdateMsg(e.ErrorMessage, lbTxt);
    }
    void OnErrorDefault(PlayFabError e) //dont need show anything on screen
    {
        UpdateMsg(e.ErrorMessage, null);
    }

    void SaveLoginInfo()
    {
        PlayerPrefs.SetString("loginUser", loginNameEmail.text);
        PlayerPrefs.SetString("loginPassword", loginPassword.text);
    }
    void ForgetLoginInfo()
    {
        PlayerPrefs.DeleteKey("loginUser");
        PlayerPrefs.DeleteKey("loginPassword");
    }
    public void OnRmbToggled()
    {
        PlayerPrefs.SetInt("loginRmb", loginRmb.isOn ? 1 : 0);
    }

    public void OnButtonRegUser()
    { //for button click
        if (regisPassword.text != regisPasswordConfirm.text)
        {
            UpdateMsg("Passwords do not match", regisTxt);
            return;
        }

        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = regisEmail.text,
            Password = regisPassword.text,
            Username = regisUsername.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegSuccess, OnRegisScreenError);
    }
    void OnRegSuccess(RegisterPlayFabUserResult r)
    {
        UpdateMsg("Registration success!", regisTxt);

        //To create a player display name 
        var req = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = regisUsername.text,
        };
        // update to profile
        PlayFabClientAPI.UpdateUserTitleDisplayName(req, OnDisplayNameUpdate, OnRegisScreenError);
    }
    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult r)
    {
        //UpdateMsg("display name updated!" + r.DisplayName, regisTxt);
    }

    void OnLoginSuccess(LoginResult r)
    {
        if (r.NewlyCreated)
        {
            int randGuestID;
            string GuestID;

            randGuestID = Random.Range(0, 10000);
            GuestID = "Guest " + randGuestID.ToString("D4");

            //To create a player display name (For guest login first time)
            var req = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = GuestID,
            };
            // update to profile
            PlayFabClientAPI.UpdateUserTitleDisplayName(req, OnDisplayNameUpdate, OnLoginScreenError);
        }

        UpdateMsg("Login success!", loginTxt);
        //GetUserData(); //Player Data
        if (loginRmb.isOn)
            SaveLoginInfo();
        else
            ForgetLoginInfo();

        SceneManager.LoadScene("Landing");
    }
    public void OnButtonLogin()
    {
        if (loginNameEmail.text.Contains('@'))
            OnLoginEmail();
        else
            OnLoginUserName();
    }

    void OnLoginEmail() //login using email + password
    {
        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = loginNameEmail.text,
            Password = loginPassword.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnLoginScreenError);
    }
    void OnLoginUserName() //login using username + password
    {
        var loginRequest = new LoginWithPlayFabRequest
        {
            Username = loginNameEmail.text,
            Password = loginPassword.text
        };
        PlayFabClientAPI.LoginWithPlayFab(loginRequest, OnLoginSuccess, OnLoginScreenError);
    }
    public void OnButtonDeviceLogin()
    {
        var req = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(req, OnLoginSuccess, OnLoginScreenError);
    }
    public void OnButtonLogout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        Debug.Log("logged out");

        SceneManager.LoadScene("Login");
    }
    public void PasswordResetRequest()
    {
        var req = new SendAccountRecoveryEmailRequest
        {
            Email = resetEmail.text,
            TitleId = PlayFabSettings.TitleId
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(req, OnPasswordReset, OnResetScreenError);
    }
    void OnPasswordReset(SendAccountRecoveryEmailResult r)
    {
        UpdateMsg("Recovery email sent", resetTxt);
    }
    public void ClientGetTitleData()
    { //MOTD
        loadingMOTD = true;

        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            result =>
            {
                loadingMOTD = false;

                if (result.Data == null || !result.Data.ContainsKey("MOTD"))
                {
                    Debug.Log("No MOTD");
                }
                else
                {
                    Debug.Log("MOTD: " + result.Data["MOTD"]);
                    if (motdText != null) motdText.SetText(result.Data["MOTD"]);
                }
            },
            error =>
            {
                //UpdateMsg("Got error getting titleData:");
                //UpdateMsg(error.GenerateErrorReport());
            }
        );
    }
    public void OnActivateDisplayNameInput()
    {
        dispname_field.enabled = true;
        dispname_field.Select();

        oldDispname = dispname_field.text;

        editDispnameButton.interactable = false;

        openUI = true;
    }
    public void OnEditDisplayName()
    {
        dispname_field.enabled = false;

        openUI = false;

        string newDispName = dispname_field.text;

        if (newDispName == oldDispname)
        {
            editDispnameButton.interactable = true;
        }
        else
        {
            var req = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = newDispName,
            };
            // update to profile
            PlayFabClientAPI.UpdateUserTitleDisplayName(req, 
                                                        result => {
                                                            dispname_error.color = Color.black;
                                                            dispname_error.text = "Display name changed!";
                                                            StartCoroutine(ErrorTimer(1, dispname_error));

                                                            editDispnameButton.interactable = true;

                                                            if (player != null) player.SetDisplayName(newDispName);
                                                        }, 
                                                        error => {
                                                            dispname_field.text = oldDispname;

                                                            if (player != null) player.SetDisplayName(oldDispname);

                                                            dispname_error.color = Color.red;
                                                            dispname_error.text = "Failed to change display name";
                                                            StartCoroutine(ErrorTimer(1, dispname_error));

                                                            editDispnameButton.interactable = true;
                                                        });
        }
    }

    public void OnButtonGetGlobalLeaderboard(string leaderboardName)
    {
        loadingLeaderboard = true;

        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = leaderboardName, //playfab leaderboard statistic name
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, OnLeaderboardGet, OnErrorDefault);
    }
    public void OnButtonGetNearbyLeaderboard(string leaderboardName)
    {
        loadingLeaderboard = true;

        var lbreq = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = leaderboardName, //playfab leaderboard statistic name
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(lbreq, OnAroundLeaderboardGet, OnErrorDefault);
    }
    public void OnButtonGetFriendsLeaderboard(string leaderboardName)
    {
        loadingLeaderboard = true;

        var lbreq = new GetFriendLeaderboardRequest
        {
            StatisticName = leaderboardName, //playfab leaderboard statistic name
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetFriendLeaderboard(lbreq, OnLeaderboardGet, OnLeaderboardError);
    }
    void OnLeaderboardGet(GetLeaderboardResult r)
    {
        int lbItemNum = 0;

        foreach (var item in r.Leaderboard)
        {
            var lbItemScript = lbItems[lbItemNum].GetComponent<LeaderboardItem>();
            lbItemScript.SetInfo((item.Position + 1).ToString(), item.DisplayName, item.StatValue.ToString());
            lbItemScript.gameObject.SetActive(true);

            ++lbItemNum;
        }

        for (int i = lbItemNum; i < lbMax; ++i)
        {
            var lbItemScript = lbItems[i].GetComponent<LeaderboardItem>();
            lbItemScript.SetInfo("", "", "");
            lbItemScript.gameObject.SetActive(false);
        }

        loadingLeaderboard = false;
    }
    void OnAroundLeaderboardGet(GetLeaderboardAroundPlayerResult r)
    {
        int lbItemNum = 0;

        foreach (var item in r.Leaderboard)
        {
            var lbItemScript = lbItems[lbItemNum].GetComponent<LeaderboardItem>();
            lbItemScript.SetInfo((item.Position + 1).ToString(), item.DisplayName, item.StatValue.ToString());
            lbItemScript.gameObject.SetActive(true);

            ++lbItemNum;
        }

        for (int i = lbItemNum; i < lbMax; ++i)
        {
            var lbItemScript = lbItems[i].GetComponent<LeaderboardItem>();
            lbItemScript.SetInfo("", "", "");
            lbItemScript.gameObject.SetActive(false);
        }

        loadingLeaderboard = false;
    }
    public void SendLeaderboard(string leaderboardName, int newValue)
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>{ //playfab leaderboard statistic name
                new StatisticUpdate{
                    StatisticName=leaderboardName,
                    Value=newValue
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(req, OnLeaderboardUpdate, OnErrorDefault);
    }
    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult r)
    {
        Debug.Log("Successful leaderboard sent:" + r.ToString());
    }

    public void SetUserData(int XP, int level)
    { //Player Data
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {
                {"XP", XP.ToString()},
                {"Level", level.ToString()}
            }
        },
        result => Debug.Log("Successfully updated user data"),
        error =>
        {
            Debug.Log("Error setting user data");
            Debug.Log(error.GenerateErrorReport());
        });
    }
    public void SetUserBG(string bgName)
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {
                {"BG", bgName}
            }
        },
        result => Debug.Log("Successfully updated user data"),
        error =>
        {
            Debug.Log("Error setting user background");
            Debug.Log(error.GenerateErrorReport());
        });
    }
    public void SetUserColor(string skinColor)
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {
                {"SkinColor", skinColor}
            }
        },
        result =>
        {
            switch (skinColor)
            {
                case "BLUE":
                    playerColorSwitcher.ChangePlayerColor(Color.blue);
                    break;
                case "GREEN":
                    playerColorSwitcher.ChangePlayerColor(Color.green);
                    break;
            }

            Debug.Log("Successfully updated user data");
        },
        error =>
        {
            Debug.Log("Error setting user skin");
            Debug.Log(error.GenerateErrorReport());
        });
    }
    public void GetUserData()
    { //Player Data
        loadingPlayerData = true;

        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        , result =>
        {
            loadingPlayerData = false;

            Debug.Log("Got user data:");

            if (result.Data == null || !result.Data.ContainsKey("Level"))
            {
                curLevel = 0;
                if (level_field != null) level_field.text = curLevel.ToString();
            }
            else
            {
                if (int.TryParse(result.Data["Level"].Value, out curLevel))
                {
                    if (level_field != null) level_field.text = curLevel.ToString();
                }
                else
                {
                    Debug.Log("Level isnt int");
                }
            }
            int xpForLvl = 50 + curLevel * 50;
            if (XP_slider != null) XP_slider.maxValue = xpForLvl;
            if (result.Data == null || !result.Data.ContainsKey("XP"))
            {
                curXP = 0;
                if (XP_field != null) XP_field.text = curXP + " / " + xpForLvl + " XP";
                if (XP_slider != null) XP_slider.value = curXP;
            }
            else
            {
                if (int.TryParse(result.Data["XP"].Value, out curXP))
                {
                    if (XP_field != null) XP_field.text = curXP + " / " + xpForLvl + " XP";
                    if (XP_slider != null) XP_slider.value = curXP;
                }
                else
                {
                    Debug.Log("XP isnt int");
                }
            }

            if (result.Data == null || !result.Data.ContainsKey("BG"))
            {
                if (bgManager != null)
                {
                    bgManager.UpdateBG("DAY");
                }
            }
            else
            {
                if (bgManager != null)
                {
                    bgManager.UpdateBG(result.Data["BG"].Value);
                }
            }

            if (result.Data == null || !result.Data.ContainsKey("SkinColor"))
            {
                if (playerColorSwitcher != null)
                {
                    playerColorSwitcher.ChangePlayerColor(Color.white);
                }
            }
            else
            {
                if (playerColorSwitcher != null)
                {
                    switch (result.Data["SkinColor"].Value)
                    {
                        case "BLUE":
                            playerColorSwitcher.ChangePlayerColor(Color.blue);
                            break;
                        case "GREEN":
                            playerColorSwitcher.ChangePlayerColor(Color.green);
                            break;
                    }
                }
            }

        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
    }

    IEnumerator ErrorTimer(float stayTime, TMP_Text tMP_Text)
    {
        yield return new WaitForSeconds(stayTime);

        tMP_Text.text = "";
    }

    //friends

    void DisplayFriends(List<FriendInfo> friendsCache)
    {
        /*txtFrdList.text = "";
        friendsCache.ForEach(f => {
            Debug.Log(f.FriendPlayFabId + "," + f.TitleDisplayName);
            txtFrdList.text += f.TitleDisplayName + "[" + f.FriendPlayFabId + "]\n";
            if (f.Profile != null) Debug.Log(f.FriendPlayFabId + "/" + f.Profile.DisplayName);
        });*/
    }

    public void GetFriends()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            // ExternalPlatformFriends = false,
            // XboxToken = null
        }, result => {
            _friends = result.Friends;
            DisplayFriends(_friends); // triggers your UI
        }, OnErrorDefault);
    }

    enum FriendIdType { PlayFabId, Username, Email, DisplayName };
    void AddFriend(FriendIdType idType, string friendId)
    {
        var request = new AddFriendRequest();
        switch (idType)
        {
            case FriendIdType.PlayFabId:
                request.FriendPlayFabId = friendId;
                break;
            case FriendIdType.Username:
                request.FriendUsername = friendId;
                break;
            case FriendIdType.Email:
                request.FriendEmail = friendId;
                break;
            case FriendIdType.DisplayName:
                request.FriendTitleDisplayName = friendId;
                break;
        }
        // Execute request and update friends when we are done
        PlayFabClientAPI.AddFriend(request, result => {
            Debug.Log("Friend added successfully!");
        }, OnErrorDefault);
    }
    public void OnAddFriend()
    { 
        //to add friend based on display name
        //AddFriend(FriendIdType.DisplayName, tgtFriend.text);
    }
    // unlike AddFriend, RemoveFriend only takes a PlayFab ID
    // you can get this from the FriendInfo object under FriendPlayFabId
    void RemoveFriend(FriendInfo friendInfo)
    { //to investigat
        PlayFabClientAPI.RemoveFriend(new RemoveFriendRequest
        {
            FriendPlayFabId = friendInfo.FriendPlayFabId
        }, result => {
            _friends.Remove(friendInfo);
        }, OnErrorDefault);
    }
    public void OnUnFriend()
    {
        //RemoveFriend(tgtunfrnd.text);
    }
    void RemoveFriend(string pfid)
    {
        var req = new RemoveFriendRequest
        {
            FriendPlayFabId = pfid
        };
        PlayFabClientAPI.RemoveFriend(req
        , result => {
            Debug.Log("unfriend!");
        }, OnErrorDefault);
    }

}

