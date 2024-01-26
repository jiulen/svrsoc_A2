using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopupNotif : MonoBehaviour
{
    public TMP_Text text1;

    public void SetText1(string newText)
    {
        text1.text = newText;
    }
}
