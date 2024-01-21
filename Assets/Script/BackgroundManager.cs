using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] List<Renderer> bgRenderers = new();

    [SerializeField] List<Material> dayMaterials = new();
    [SerializeField] List<Material> nightMaterials = new();
    [SerializeField] List<Material> colorfulMaterials = new();

    public void Start()
    {
    }

    public void UpdateBG(string bgName)
    {
        switch (bgName)
        {
            case "DAY":
                for (int i = 0; i < 4; ++i)
                {
                    bgRenderers[i].material = dayMaterials[i];
                }
                break;

            case "NIGHT":
                for (int i = 0; i < 4; ++i)
                {
                    bgRenderers[i].material = nightMaterials[i];
                }
                break;

            case "COLORFUL":
                for (int i = 0; i < 4; ++i)
                {
                    bgRenderers[i].material = colorfulMaterials[i];
                }
                break;
        }
    }
}
