using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowHideToggle : MonoBehaviour
{
    Toggle toggle;
    [SerializeField]
    Image showingObj, hidingObj;

    [SerializeField]
    TMP_InputField tMP_InputField;

    // Start is called before the first frame update
    void Start()
    {
        toggle = GetComponent<Toggle>();
    }

    public void OnShowHideToggled()
    {
        if (toggle.isOn)
        {
            hidingObj.gameObject.SetActive(true);
            showingObj.gameObject.SetActive(false);
            toggle.graphic = hidingObj;

            tMP_InputField.contentType = TMP_InputField.ContentType.Password;
        }
        else
        {
            hidingObj.gameObject.SetActive(false);
            showingObj.gameObject.SetActive(true);
            toggle.graphic = showingObj;

            tMP_InputField.contentType = TMP_InputField.ContentType.Standard;
        }

        tMP_InputField.DeactivateInputField();
        tMP_InputField.ActivateInputField();
    }
}
