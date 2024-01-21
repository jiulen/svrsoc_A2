using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PotionButton : MonoBehaviour
{
    [SerializeField]
    Button button;

    [SerializeField]
    GameObject disableObj;

    [SerializeField]
    TMP_Text itemUses;

    [SerializeField]
    string itemName, itemInstID, itemID;

    [SerializeField]
    InventoryManager invenManager;

    public void EnablePotionButtion(bool enable)
    {
        button.interactable = enable;
        disableObj.SetActive(!enable);
    }

    public void SetValues(string _itemName, string _itemInstID, string _itemID)
    {
        itemName = _itemName;
        itemInstID = _itemInstID;
        itemID = _itemID;
    }

    public void UpdateUses(int remainingUses)
    {
        itemUses.text = remainingUses.ToString();
    }

    public void OnUsed()
    {
        invenManager.ConsumeItem(itemInstID, itemName, itemID);
    }
}
