using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;

public class PotionsManager : MonoBehaviour
{
    [SerializeField]
    PotionButton bluePotionButton, greenPotionButton;

    [SerializeField]
    InventoryManager invenManager;

    [SerializeField]
    PlayFabUserMgtTMP pfManager;

    bool loadingInven;

    bool consumingPotion;

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

            //check for potions
            UpdatePotion("P1");
            UpdatePotion("P2");
        }

        if (invenManager.consumingItem && !consumingPotion)
        {
            consumingPotion = true;
        }
        if (!invenManager.consumingItem && consumingPotion)
        {
            consumingPotion = false;

            //use potion
            switch (invenManager.consumedItemID)
            {
                case "P1":
                    pfManager.SetUserColor("BLUE");
                    break;
                case "P2":
                    pfManager.SetUserColor("GREEN");
                    break;
            }
        }
    }

    void UpdatePotion(string updatedID = "")
    {
        if (updatedID == "P1")
        {
            ItemInstance bluePotionII = invenManager.invenItems.Find(item => item.ItemId == "P1");
            bluePotionButton.EnablePotionButtion(bluePotionII != null);
            if (bluePotionII != null)
            {
                bluePotionButton.SetValues(bluePotionII.DisplayName, bluePotionII.ItemInstanceId, bluePotionII.ItemId);

                int remainingUses = (int)bluePotionII.RemainingUses;
                bluePotionButton.UpdateUses(remainingUses);
            }
            else
            {
                bluePotionButton.UpdateUses(0);
            }
        }
        else if (updatedID == "P2")
        {
            ItemInstance greenPotionII = invenManager.invenItems.Find(item => item.ItemId == "P2");
            greenPotionButton.EnablePotionButtion(greenPotionII != null);
            if (greenPotionII != null)
            {
                greenPotionButton.SetValues(greenPotionII.DisplayName, greenPotionII.ItemInstanceId, greenPotionII.ItemId);

                int remainingUses = (int)greenPotionII.RemainingUses;
                greenPotionButton.UpdateUses(remainingUses);
            }
            else
            {
                greenPotionButton.UpdateUses(0);
            }
        }
    }
}
