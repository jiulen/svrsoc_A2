using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopDesc : MonoBehaviour
{
    [SerializeField]
    TMP_Text itemName, itemCost, itemDesc;

    [SerializeField]
    Image itemImage;

    [SerializeField]
    GameObject noItemObj, itemObj;

    [SerializeField]
    Button buyButton;

    public string itemID;
    public string vcType;
    public int price;

    public void SetBuyInfo(string buyItemID, string buyVcType, int buyPrice)
    {
        itemID = buyItemID;
        vcType = buyVcType;
        price = buyPrice;
    }

    public void SetInfo(string itemNameStr, string itemCostStr, string itemDescStr, Sprite itemSprite)
    {
        itemName.text = itemNameStr;
        itemCost.text = itemCostStr;
        itemDesc.text = itemDescStr;
        itemImage.sprite = itemSprite;
    }

    public void EnableDesc(bool enable)
    {
        noItemObj.SetActive(!enable);
        itemObj.SetActive(enable);
    }

    public void EnableBuyButton(bool enable)
    {
        buyButton.interactable = enable;
    }
}
