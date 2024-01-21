using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementBox : MonoBehaviour
{
    [SerializeField] TMP_Text achievementName, achievementProgress, achievementDesc;

    [SerializeField] Image[] stars = new Image[3];

    [SerializeField] Slider progressSlider;

    public void SetUI(Achievement achievement)
    {
        achievementName.text = achievement.name + " " + new string('I', achievement.level);
        achievementProgress.text = achievement.value + " / " + achievement.goal;
        achievementDesc.text = achievement.desc;

        for (int i = 0; i < stars.Length; ++i)
        {
            if (achievement.level > i)
            {
                stars[i].color = Color.white;
            }
            else
            {
                stars[i].color = Color.gray;
            }
        }

        progressSlider.maxValue = achievement.goal;
        progressSlider.value = achievement.value;
    }
}
