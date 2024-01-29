using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections.Generic;
using TMPro; //for text mesh pro UI elements
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using Photon.Pun;

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

    public List<FriendInfo> _friends = null;
    public List<FriendInfo> confirmedFriends = null;
    List<string> confirmedIDs = null;

    public GameObject moveUpTextPrefab;
    public Transform popupTextHolder;

    public static string mePlayerID;

    public bool pendingColorSwitch = false;
    public Color pendingColor;

    //friends
    public Transform friendsParent;
    public GameObject friendInfoPrefab, friendReqPrefab;

    //bools
    public bool isOffline = false;
    public bool loadingLeaderboard = false;
    public bool loadingPlayerData = false;
    public bool loadingDispName = false;
    public bool loadingMOTD = false;
    public bool loadingFriendList = false;
    public bool loadingFriendRequests = false;

    //photon stuff
    public Player player;

    public Launcher launcher;

    public static string dispName;

    [SerializeField] float updateFriendsTimer = 0, updateFriendsTime = 10;

    public string GetPlayerName()
    {
        return dispName;
    }

    public string GetPlayerID()
    {
        return mePlayerID;
    }

    public void MakeScrollNotif(string textDisplayed, Color textColor)
    {
        GameObject newTextPopup = Instantiate(moveUpTextPrefab);
        newTextPopup.transform.SetParent(popupTextHolder, false);
        newTextPopup.transform.localPosition = Vector3.zero;

        MoveUpText moveUpText = newTextPopup.GetComponent<MoveUpText>();

        moveUpText.SetText(textDisplayed, textColor);
        moveUpText.ResetStretch();
    }

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
            GetFriends();
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

    public void LoadPlayerInfo()
    {
        var ProfileRequestParams = new GetPlayerProfileRequest();

        PlayFabClientAPI.GetPlayerProfile(ProfileRequestParams,
                                          result => {
                                              if (player != null)
                                              {
                                                  player.SetDisplayNameID(result.PlayerProfile.DisplayName, result.PlayerProfile.PlayerId);
                                              }
                                          },
                                          error => { });
    }

    private void Update()
    {
        if (pendingColorSwitch)
        {
            if (playerColorSwitcher == null)
            {
                if (player != null)
                {
                    playerColorSwitcher = player.GetComponent<PlayerColorSwitcher>();

                    playerColorSwitcher.ChangePlayerColor(pendingColor);
                    pendingColorSwitch = false;
                }
            }
        }

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

            updateFriendsTimer += Time.deltaTime;
            if (updateFriendsTimer >= updateFriendsTime)
            {
                updateFriendsTimer = 0;
                GetShowFriends();
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

        if (regisUsername.text.Contains(" "))
        {
            UpdateMsg("Username cannot contain spaces", regisTxt);
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
            GuestID = "Guest" + randGuestID.ToString("D4");

            //To create a player display name (For guest login first time)
            var req = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = GuestID,
            };
            // update to profile
            PlayFabClientAPI.UpdateUserTitleDisplayName(req, OnDisplayNameUpdate, OnLoginScreenError);
        }

        mePlayerID = r.PlayFabId;
        Debug.Log("My Playfab ID : " + mePlayerID);

        UpdateMsg("Login success!", loginTxt);
        //GetUserData(); //Player Data
        if (loginRmb.isOn)
            SaveLoginInfo();
        else
            ForgetLoginInfo();

        dispName = r.InfoResultPayload.PlayerProfile.DisplayName;

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
            Password = loginPassword.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnLoginScreenError);
    }
    void OnLoginUserName() //login using username + password
    {
        var loginRequest = new LoginWithPlayFabRequest
        {
            Username = loginNameEmail.text,
            Password = loginPassword.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithPlayFab(loginRequest, OnLoginSuccess, OnLoginScreenError);
    }
    public void OnButtonDeviceLogin()
    {
        var req = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
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

        if (newDispName.Contains(" "))
        {
            dispname_field.text = oldDispname;

            dispname_error.color = Color.red;
            dispname_error.text = "Display name cannot contain spaces";
            StartCoroutine(ErrorTimer(1, dispname_error));
            return;
        }

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

                                                            if (player != null) player.SetDisplayNameID(newDispName);

                                                            dispName = newDispName;
                                                            launcher.ChangePlayerName();
                                                        }, 
                                                        error => {
                                                            dispname_field.text = oldDispname;

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

        //get friend list to filter leaderboard first
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            // ExternalPlatformFriends = false,
            // XboxToken = null
        }, result => {
            _friends = result.Friends;

            //filter friends by "confirmed" tag
            confirmedFriends = _friends.FindAll(friend => friend.Tags.Contains("confirmed"));
            //take confirmed friend ids
            confirmedIDs = confirmedFriends.Select(friend => friend.FriendPlayFabId).ToList();

            //send get leaderboard req
            var lbreq = new GetFriendLeaderboardRequest
            {
                StatisticName = leaderboardName, //playfab leaderboard statistic name
                StartPosition = 0,
                MaxResultsCount = 10 + _friends.Count - confirmedFriends.Count //to guarantee 10 results if possible
            };
            PlayFabClientAPI.GetFriendLeaderboard(lbreq, OnFriendLeaderboardGet, OnLeaderboardError);


        }, error => UpdateMsg("Failed to get friend list", lbTxt));
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
    void OnFriendLeaderboardGet(GetLeaderboardResult r)
    {
        var confirmedLeaderboard = r.Leaderboard.FindAll(entry => confirmedIDs.Contains(entry.PlayFabId) || entry.PlayFabId == mePlayerID);
        //take top 10
        var finalLeaderboard = confirmedLeaderboard.Take(10).ToList();

        //make leaderboard
        int lbItemNum = 0;

        foreach (var item in finalLeaderboard)
        {
            var lbItemScript = lbItems[lbItemNum].GetComponent<LeaderboardItem>();
            lbItemScript.SetInfo((lbItemNum + 1).ToString(), item.DisplayName, item.StatValue.ToString());
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
            },
            Permission = UserDataPermission.Public
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
            if (playerColorSwitcher == null)
            {
                if (player != null)
                {
                    playerColorSwitcher = player.GetComponent<PlayerColorSwitcher>();
                }
            }

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


            if (playerColorSwitcher == null)
            {
                if (player != null)
                {
                    playerColorSwitcher = player.GetComponent<PlayerColorSwitcher>();
                }
            }

            if (result.Data == null || !result.Data.ContainsKey("SkinColor"))
            {
                if (playerColorSwitcher != null)
                {
                    playerColorSwitcher.ChangePlayerColor(Color.white);
                }
                else
                {
                    pendingColorSwitch = true;
                    pendingColor = Color.white;
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
                else
                {
                    pendingColorSwitch = true;
                    switch (result.Data["SkinColor"].Value)
                    {
                        case "BLUE":
                            pendingColor = Color.blue;
                            break;
                        case "GREEN":
                            pendingColor = Color.green;
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

    //friend
    public void GetFriends(Player friendPlayer = null)
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            // ExternalPlatformFriends = false,
            // XboxToken = null
        }, result => {
            _friends = result.Friends;
            if (friendPlayer != null)
            {
                friendPlayer.UpdateOptions();
            }
        }, OnErrorDefault);
    }


    public void GetShowFriends()
    {
        loadingFriendList = true;
        foreach (Transform child in friendsParent)
        {
            Destroy(child.gameObject);
        }

        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            // ExternalPlatformFriends = false,
            // XboxToken = null
        }, result => {
            loadingFriendList = false;
            //clear
            foreach (Transform child in friendsParent)
            {
                Destroy(child.gameObject);
            }

            _friends = result.Friends;
            //filter friends by "confirmed" tag
            confirmedFriends = _friends.FindAll(friend => friend.Tags.Contains("confirmed"));
            foreach (FriendInfo friend in confirmedFriends)
            {
                GameObject newItem = Instantiate(friendInfoPrefab);
                FriendInfoItem newFriendInfoItem = newItem.GetComponent<FriendInfoItem>();

                newItem.transform.SetParent(friendsParent, false);
                newItem.transform.localPosition = Vector3.zero;

                newFriendInfoItem.playerName.text = friend.TitleDisplayName;
                newFriendInfoItem.xp.text = "LVL ?";
                newFriendInfoItem.status.text = "Unknown";
                newFriendInfoItem.status.color = Color.gray;
                newFriendInfoItem.friendID = friend.FriendPlayFabId;

                var getDataReq = new GetUserDataRequest()
                {
                    PlayFabId = friend.FriendPlayFabId
                };
                PlayFabClientAPI.GetUserData(getDataReq
                , result =>
                {
                    if (newFriendInfoItem != null)
                    {
                        if (result.Data == null || !result.Data.ContainsKey("Level"))
                        {
                            newFriendInfoItem.xp.text = "LVL " + 0;
                        }
                        else
                        {
                            if (int.TryParse(result.Data["Level"].Value, out curLevel))
                            {
                                newFriendInfoItem.xp.text = "LVL " + curLevel;
                            }
                            else
                            {
                                newFriendInfoItem.xp.text = "LVL ?";
                            }
                        }
                    }
                }, error => { });
            }

            launcher.FindOnlinePlayers();

        }, OnErrorDefault);
    }

    public void GetFriendRequests()
    {
        loadingFriendRequests = true;

        foreach (Transform child in friendsParent)
        {
            Destroy(child.gameObject);
        }

        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            // ExternalPlatformFriends = false,
            // XboxToken = null
        }, result => {
            loadingFriendRequests = false;
            //clear
            foreach (Transform child in friendsParent)
            {
                Destroy(child.gameObject);
            }

            _friends = result.Friends;
            foreach (FriendInfo friend in _friends)
            {
                if (friend.Tags.Contains("requester"))
                {
                    GameObject newItem = Instantiate(friendReqPrefab);
                    FriendRequestItem newFriendRequestItem = newItem.GetComponent<FriendRequestItem>();

                    newItem.transform.SetParent(friendsParent, false);
                    newItem.transform.localPosition = Vector3.zero;

                    newFriendRequestItem.playerName.text = friend.TitleDisplayName;
                    newFriendRequestItem.xp.text = "LVL ?";
                    newFriendRequestItem.status.text = "Unknown";
                    newFriendRequestItem.status.color = Color.gray;
                    newFriendRequestItem.friendID = friend.FriendPlayFabId;

                    var getDataReq = new GetUserDataRequest()
                    {
                        PlayFabId = friend.FriendPlayFabId
                    };
                    PlayFabClientAPI.GetUserData(getDataReq
                    , result =>
                    {
                        if (newFriendRequestItem != null)
                        {
                            if (result.Data == null || !result.Data.ContainsKey("Level"))
                            {
                                newFriendRequestItem.xp.text = "LVL " + 0;
                            }
                            else
                            {
                                if (int.TryParse(result.Data["Level"].Value, out curLevel))
                                {
                                    newFriendRequestItem.xp.text = "LVL " + curLevel;
                                }
                                else
                                {
                                    newFriendRequestItem.xp.text = "LVL ?";
                                }
                            }
                        }
                    }, error => { });
                }
            }

            launcher.FindOnlinePlayers();

        }, OnErrorDefault);
    }

    public void SendFriendRequest(Player friendPlayer, string friendID)
    {
        var sendReq = new ExecuteCloudScriptRequest
        {
            FunctionName = "SendFriendRequest",
            FunctionParameter = new { FriendPlayFabId = friendID }
        };
        PlayFabClientAPI.ExecuteCloudScript(sendReq
        , result =>
        {
            MakeScrollNotif("Friend request sent successfully!", Color.green);
            GetFriends(friendPlayer);
        }
        , error =>
        {
            MakeScrollNotif("Failed to send friend request!", Color.red);
        });
    }

    public void AcceptFriendRequest(string friendID)
    {
        var acceptReq = new ExecuteCloudScriptRequest
        {
            FunctionName = "AcceptFriendRequest",
            FunctionParameter = new { FriendPlayFabId = friendID }
        };
        PlayFabClientAPI.ExecuteCloudScript(acceptReq
        , result =>
        {
            //MakeScrollNotif("Friend request accepted successfully!", Color.green);
            GetFriendRequests();
        }
        , error =>
        {
            MakeScrollNotif("Failed to accept friend request!", Color.red);
        });
    }

    public void DenyFriendRequest(string friendID, bool removeFriend = false)
    {
        var denyReq = new ExecuteCloudScriptRequest
        {
            FunctionName = "DenyFriendRequest",
            FunctionParameter = new { FriendPlayFabId = friendID }
        };
        PlayFabClientAPI.ExecuteCloudScript(denyReq
        , result =>
        {
            //MakeScrollNotif("Friend request denied successfully!", Color.green);
            if (!removeFriend) GetFriendRequests();
            else GetShowFriends();
        }
        , error =>
        {
            MakeScrollNotif("Failed to deny friend request!", Color.red);
        });
    }
}

