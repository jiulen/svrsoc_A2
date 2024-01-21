using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveRightText : MonoBehaviour
{
    [SerializeField] TMP_Text movingText;

    [SerializeField] float speed, minX, maxX;

    // Update is called once per frame
    void Update()
    {
        if (movingText.text != "")
        {
            movingText.transform.position += new Vector3(speed * Time.deltaTime, 0, 0);

            if (movingText.transform.localPosition.x > maxX)
            {
                movingText.transform.localPosition = new Vector3(minX, movingText.transform.localPosition.y, movingText.transform.localPosition.z);
            }
        }
    }

    public void SetText(string newText)
    {
        movingText.text = newText;
    }
}
