using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeReqShopItem : MonoBehaviour
{
    public Image itemImage;

    public string itemID;

    public TradeController tradeController;

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
            case "BU1":
                itemSprite = Resources.Load<Sprite>("potionsBundle");
                break;
        }

        itemImage.sprite = itemSprite;

        itemID = itemIDStr;
    }

    public void OnItemSelected()
    {
        tradeController.SelectTradeReqItem(itemID, itemImage.sprite);
    }
}
