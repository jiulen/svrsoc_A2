using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorSwitcher : MonoBehaviour
{
    [SerializeField] SpriteRenderer playerSprite;

    public void ChangePlayerColor(Color newColor)
    {
        playerSprite.color = newColor;
    }
}
