using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendsController : MonoBehaviour
{
    public GameObject panel;
    System.Action playerCallback;

    public Toggle friendsToggle, requestsToggle;

    public PlayFabUserMgtTMP pfManager;
    
    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);

        friendsToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                pfManager.GetShowFriends();
                friendsToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
            }
            else
            {
                friendsToggle.targetGraphic.color = new Color(1, 1, 1);
            }
        });
        requestsToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                pfManager.GetFriendRequests();
                requestsToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
            }
            else
            {
                requestsToggle.targetGraphic.color = new Color(1, 1, 1);
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

        friendsToggle.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
        requestsToggle.targetGraphic.color = new Color(1, 1, 1);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        playerCallback();
    }
}
