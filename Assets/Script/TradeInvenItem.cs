using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeInvenItem : MonoBehaviour
{
    [SerializeField]
    Image itemImage;

    [SerializeField]
    TMP_Text itemCountText;

    public string invenInstID, itemID;

    public TradeController tradeController;

    public void SetInfo(string itemIDStr, int? itemCount)
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

        if (itemCount == null)
        {
            itemCountText.text = "";
        }
        else
        {
            itemCountText.text = itemCount.ToString();
        }

        itemID = itemIDStr;
    }

    public void OnItemSelected()
    {
        tradeController.SelectTradeOfferItem(invenInstID, itemID, itemImage.sprite);
    }
}
