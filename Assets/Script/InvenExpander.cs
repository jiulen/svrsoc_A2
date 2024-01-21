using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvenExpander : MonoBehaviour
{
    [SerializeField] Toggle showHideToggle;
    [SerializeField] Animator animator;

    private void Start()
    {
        showHideToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn) animator.Play("InvenRectMoveDown");
            else animator.Play("InvenRectMoveUp");
        });
    }
}
