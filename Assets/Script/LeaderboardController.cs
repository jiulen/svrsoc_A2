using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardController : MonoBehaviour
{
    public GameObject panel;
    System.Action playerCallback;

    [SerializeField]
    PlayFabUserMgtTMP pfManager;
    [SerializeField] 
    GameObject[] lbItems; //should have 10

    [SerializeField]
    Toggle globalToggle, nearbyToggle, friendsToggle;

    [SerializeField]
    TMP_Text headerStat;

    string leaderboardName = "highscore";

    [SerializeField]
    TMP_Dropdown sortDropdown;

    [SerializeField]
    GameObject leaderboardObj, loadingObj;

    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);

        globalToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                pfManager.OnButtonGetGlobalLeaderboard(leaderboardName);
                globalToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
            }
            else
            {
                globalToggle.targetGraphic.color = new Color(1, 1, 1);
            }
        });
        nearbyToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                pfManager.OnButtonGetNearbyLeaderboard(leaderboardName);
                nearbyToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
            }
            else
            {
                nearbyToggle.targetGraphic.color = new Color(1, 1, 1);
            }
        });
        friendsToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                pfManager.OnButtonGetFriendsLeaderboard(leaderboardName);
                friendsToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
            }
            else
            {
                friendsToggle.targetGraphic.color = new Color(1, 1, 1);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (pfManager.loadingLeaderboard && leaderboardObj.activeSelf)
        {
            leaderboardObj.SetActive(false);
            loadingObj.SetActive(true);
        }

        if (!pfManager.loadingLeaderboard && !leaderboardObj.activeSelf)
        {
            leaderboardObj.SetActive(true);
            loadingObj.SetActive(false);
        }
    }
    public void OpenPanel(System.Action callBack = null)
    {
        foreach (GameObject lbItem in lbItems)
        {
            lbItem.SetActive(false);
        }

        pfManager.OnButtonGetGlobalLeaderboard(leaderboardName);
        globalToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
        nearbyToggle.targetGraphic.color = new Color(1, 1, 1);
        friendsToggle.targetGraphic.color = new Color(1, 1, 1);

        panel.SetActive(true);
        playerCallback = callBack;
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        playerCallback();
    }

    public void LeaderboardSortingChanged()
    {
        switch (sortDropdown.value)
        {
            case 0:
                leaderboardName = "highscore";
                headerStat.text = "SCORE";
                break;
            case 1:
                leaderboardName = "level";
                headerStat.text = "LEVEL";
                break;
        }

        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        if (globalToggle.isOn) pfManager.OnButtonGetGlobalLeaderboard(leaderboardName);
        else if (nearbyToggle.isOn) pfManager.OnButtonGetNearbyLeaderboard(leaderboardName);
        else if (friendsToggle.isOn) pfManager.OnButtonGetFriendsLeaderboard(leaderboardName);
    }
}
