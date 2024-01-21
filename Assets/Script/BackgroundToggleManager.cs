using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundToggleManager : MonoBehaviour
{
    [SerializeField]
    Toggle dayToggle, nightToggle, colorToggle;

    [SerializeField]
    GameObject nightDisable, colorDisable;

    [SerializeField]
    BackgroundManager bgManager;

    [SerializeField]
    InventoryManager invenManager;

    [SerializeField]
    PlayFabUserMgtTMP pfManager;

    bool loadingInven = false;

    // Start is called before the first frame update
    void Start()
    {
        dayToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                bgManager.UpdateBG("DAY");
                pfManager.SetUserBG("DAY");
            }
        });

        nightToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                bgManager.UpdateBG("NIGHT");
                pfManager.SetUserBG("NIGHT");
            }
        });

        colorToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                bgManager.UpdateBG("COLORFUL");
                pfManager.SetUserBG("COLORFUL");
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (invenManager.loadingInventory && !loadingInven)
        {
            loadingInven = true;
        }
        if (!invenManager.loadingInventory && loadingInven)
        {
            loadingInven = false;

            //check for night and colorful bg available
            bool nightAvail = invenManager.invenItems.Exists(item => item.ItemId == "B1");
            nightToggle.interactable = nightAvail;
            nightDisable.SetActive(!nightAvail);

            bool colorAvail = invenManager.invenItems.Exists(item => item.ItemId == "B2");
            colorToggle.interactable = colorAvail;
            colorDisable.SetActive(!colorAvail);
        }
    }
}
