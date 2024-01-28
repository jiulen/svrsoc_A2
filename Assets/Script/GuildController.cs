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

    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);

        guildsToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {

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
                ShowCurrentGuild();
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

    }
    public void OpenPanel(System.Action callBack = null)
    {
        panel.SetActive(true);
        playerCallback = callBack;
        
        guildsToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
        currGuildToggle.targetGraphic.color = new Color(1, 1, 1);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        playerCallback();
    }

    void ShowCurrentGuild()
    {

    }
}
