using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveUpText : MonoBehaviour
{
    [SerializeField] float speed, time;
    [SerializeField] TMP_Text movingText;

    RectTransform rectTransform;

    float timer;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        timer = time;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 startPos = transform.position;
        transform.position = new Vector3(startPos.x, startPos.y + speed * Time.deltaTime, startPos.z);

        Color startColor = movingText.color;
        movingText.color = new Color(startColor.r, startColor.g, startColor.b, (timer / time));

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = 0;

            Destroy(gameObject);
        }
    }

    public void SetText(string textDisplay, Color textColor)
    {
        movingText.text = textDisplay;
        movingText.color = textColor;
    }

    public void ResetStretch()
    {
        rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
        rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
    }
}
