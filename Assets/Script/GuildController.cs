using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuildController : MonoBehaviour
{
    public GameObject panel;
    System.Action playerCallback;

    public Toggle guildsToggle, currGuildToggle;

    public PlayFabUserMgtTMP pfManager;
    public GuildManager guildManager;

    public GameObject loadingGuildListObj, loadingCurrentGuildObj, loadingGuildInfoObj, notinGuildObj;

    public GameObject createGuildPanel;

    public GuildInfoObj guildInfoObj;

    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);

        guildsToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                guildInfoObj.gameObject.SetActive(false);
                guildManager.loadingCurrentGuild = false;
                loadingCurrentGuildObj.SetActive(false);
                guildManager.loadingGuildInfo = false;
                loadingGuildInfoObj.SetActive(false);
                guildManager.ShowGuildList();
                guildsToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
            }
            else
            {
                guildsToggle.targetGraphic.color = new Color(1, 1, 1);
            }
        });
        currGuildToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                guildInfoObj.gameObject.SetActive(true);
                guildManager.loadingGuildList = false;
                loadingGuildListObj.SetActive(false);
                guildManager.ShowCurrentGuild();
                currGuildToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
            }
            else
            {
                currGuildToggle.targetGraphic.color = new Color(1, 1, 1);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (guildManager.loadingGuildList && !loadingGuildListObj.activeSelf)
        {
            loadingGuildListObj.SetActive(true);
        }

        if (!guildManager.loadingGuildList && loadingGuildListObj.activeSelf)
        {
            loadingGuildListObj.SetActive(false);
        }

        if (guildManager.loadingCurrentGuild && !loadingCurrentGuildObj.activeSelf)
        {
            loadingCurrentGuildObj.SetActive(true);
        }

        if (!guildManager.loadingCurrentGuild && loadingCurrentGuildObj.activeSelf)
        {
            loadingCurrentGuildObj.SetActive(false);
        }

        if (guildManager.loadingGuildInfo && !loadingCurrentGuildObj.activeSelf)
        {
            loadingGuildInfoObj.SetActive(true);
        }

        if (!guildManager.loadingGuildInfo && loadingCurrentGuildObj.activeSelf)
        {
            loadingGuildInfoObj.SetActive(false);
        }
    }
    public void OpenPanel(System.Action callBack = null)
    {
        panel.SetActive(true);
        playerCallback = callBack;

        guildInfoObj.gameObject.SetActive(false);
        guildsToggle.isOn = true;
        currGuildToggle.isOn = false;

        guildManager.ShowGuildList();
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        playerCallback();
    }

    public void ShowCreateGuildPanel(bool show)
    {
        createGuildPanel.SetActive(show);
    }
}
