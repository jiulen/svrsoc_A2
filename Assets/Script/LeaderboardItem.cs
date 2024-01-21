using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardItem : MonoBehaviour
{
    [SerializeField]
    TMP_Text rank, username, score;

    public void SetInfo(string _rank, string _name, string _score)
    {
        rank.text = _rank;
        username.text = _name;
        score.text = _score;
    }
}
