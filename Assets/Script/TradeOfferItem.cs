using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeOfferItem : MonoBehaviour
{
    [SerializeField]
    public Image itemImage;

    public string invenID;
    public string invenInstID;

    public void SetInfo(string itemIDStr)
    {
        Sprite itemSprite = null;

        switch (itemIDStr)
        {
            case "B1":
                itemSprite = Resources.Load<Sprite>("nightBGIcon");
                break;
            case "B2":
                itemSprite = Resources.Load<Sprite>("colorfulBGIcon");
                break;
            case "P1":
                itemSprite = Resources.Load<Sprite>("bluePotion");
                break;
            case "P2":
                itemSprite = Resources.Load<Sprite>("greenPotion");
                break;
        }
        invenID = itemIDStr;
        itemImage.sprite = itemSprite;
    }

    public void EnableImage(bool enable)
    {
        itemImage.enabled = enable;
    }
}
