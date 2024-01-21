using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [SerializeField]
    TMP_Text itemName, itemCost;

    [SerializeField]
    Image itemImage;

    string itemDesc;

    ShopController shopController;
    ShopDesc shopDesc;

    string itemID;
    string vcType;
    int price;

    public void SetInfo(string itemIDStr, string itemNameStr, Dictionary<string, uint> itemCosts, string itemDescStr, ShopDesc shopDescRef, ShopController shopControllerRef)
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

        itemName.text = itemNameStr;

        foreach (KeyValuePair<string, uint> itemCostPair in itemCosts) //for now only find CN
        {
            if (itemCostPair.Key == "CN")
            {
                vcType = "CN";
                price = (int)itemCostPair.Value;
                itemCost.text = price.ToString();
            }
        }

        itemDesc = itemDescStr;

        shopDesc = shopDescRef;
        shopController = shopControllerRef;

        itemID = itemIDStr;
    }

    public void OnShopItemSelected()
    {
        shopDesc.SetInfo(itemName.text, itemCost.text, itemDesc, itemImage.sprite);
        shopDesc.SetBuyInfo(itemID, vcType, price);

        shopDesc.EnableBuyButton(shopController.CanBuyItem(itemID));

        shopDesc.EnableDesc(true);
    }
}
