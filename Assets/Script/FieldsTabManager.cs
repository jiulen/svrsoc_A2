using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class FieldsTabManager : MonoBehaviour
{
    EventSystem system;

    // Start is called before the first frame update
    void Start()
    {
        system = EventSystem.current;
    }

    // Update is called once per frame
    void Update()
    {
        if (system.currentSelectedGameObject != null)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Selectable next;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
                }
                else
                {
                    next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
                }

                if (next != null)
                {
                    TMP_InputField inputField = next.GetComponent<TMP_InputField>();
                    if (inputField != null)
                    {
                        inputField.OnPointerClick(new PointerEventData(system)); //set text caret if input field
                    }

                    system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
                }
            }
        }
    }
}
