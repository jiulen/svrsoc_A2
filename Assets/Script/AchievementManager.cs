using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

[System.Serializable]
public class Achievement
{
    public string name;
    public int level;
    public int value;
    public int goal;
    public string desc;

    public JSListWrapper<int> goals;
    public JSListWrapper<string> descs;

    public Achievement(string _name, int _value, List<int> _goals, List<string> _descs)
    {
        name = _name;
        value = _value;

        goals = new JSListWrapper<int>(_goals);
        descs = new JSListWrapper<string>(_descs);

        List<int> goalsList = goals.list;
        List<string> descList = descs.list;

        for (int i = 0; i < goalsList.Count; ++i)
        {
            if (value < goalsList[i])
            {
                level = i;
                goal = goalsList[i];
                desc = descList[i];
                break;
            }

            if (i == goalsList.Count - 1) //completed all levels
            {
                level = goalsList.Count;
                goal = goalsList[i];
                desc = "Completed!";
            }
        }
    }

    public void UpdateValue(int newValue)
    {
        value = newValue;

        List<int> goalsList = goals.list;
        List<string> descList = descs.list;

        for (int i = 0; i < goalsList.Count; ++i)
        {
            if (value < goalsList[i])
            {
                level = i;
                goal = goalsList[i];
                desc = descList[i];
                break;
            }

            if (i == goalsList.Count - 1) //completed all levels
            {
                level = goalsList.Count;
                goal = goalsList[i];
                desc = "Completed!";
            }
        }
    }
}

[System.Serializable]
public class JSListWrapper<T>
{
    public List<T> list;
    public JSListWrapper(List<T> list) => this.list = list;
}

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance = null;

    [SerializeField] GameObject panel;
    [SerializeField] PlayFabUserMgtTMP pfManager;

    [SerializeField] GameObject loadingObj;

    [SerializeField] GameObject achievementBoxPrefab;
    [SerializeField] Transform achievementParent;
    List<GameObject> achievementBoxes = new();

    List<Achievement> achievementsList = new();

    public bool loadingAchievements;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);

        LoadJSON();
    }

    private void Update()
    {
        if (panel != null)
        {
            if (loadingAchievements && achievementParent.gameObject.activeSelf)
            {
                achievementParent.gameObject.SetActive(false);
                loadingObj.SetActive(true);
            }
            else if (!loadingAchievements && !achievementParent.gameObject.activeSelf)
            {
                achievementParent.gameObject.SetActive(true);
                loadingObj.SetActive(false);
            }
        }
    }

    public void OpenPanel()
    {
        foreach (GameObject achievementBox in achievementBoxes)
        {
            Destroy(achievementBox);
        }
        achievementBoxes.Clear();

        LoadJSON();

        pfManager.openUI = true;
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        pfManager.openUI = false;
        panel.SetActive(false);
    }

    public int GetAchievement(string achievementName)
    {
        Achievement findingAchievement = achievementsList.Find(achievement => achievement.name == achievementName);
        return findingAchievement.value;
    }

    public void UpdateAchievement(string achievementName, int newValue)
    {
        Achievement updatedAchievement = achievementsList.Find(achievement => achievement.name == achievementName);
        updatedAchievement.UpdateValue(newValue);
    }

    public void SendJSON()
    {
        string strListAsJSON = JsonUtility.ToJson(new JSListWrapper<Achievement>(achievementsList));
        Debug.Log("JSON data prepared: " + strListAsJSON);

        var updateReq = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"Achievements", strListAsJSON}
            }
        };
        PlayFabClientAPI.UpdateUserData(updateReq, result => Debug.Log("Achievements updated successfully"), OnError);
    }

    public void InitAchievements()
    {
        List<int> scoreGoals = new();
        scoreGoals.Add(1000);
        scoreGoals.Add(2500);
        scoreGoals.Add(5000);
        List<string> scoreDescs = new();
        scoreDescs.Add("Get 1000 score");
        scoreDescs.Add("Get 2500 score");
        scoreDescs.Add("Get 5000 score");
        Achievement scoreAchievement = new Achievement("High Scorer", 0, scoreGoals, scoreDescs);
        achievementsList.Add(scoreAchievement);

        List<int> dieGoals = new();
        dieGoals.Add(1);
        dieGoals.Add(5);
        dieGoals.Add(10);
        List<string> dieDescs = new();
        dieDescs.Add("Die 1 time");
        dieDescs.Add("Die 5 times");
        dieDescs.Add("Die 10 times");
        Achievement dieAchievement = new Achievement("Noob", 0, dieGoals, dieDescs);
        achievementsList.Add(dieAchievement);

        SendJSON();
    }

    public void LoadJSON()
    {
        loadingAchievements = true;

        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnJSONDataReceived, OnError);
    }

    void OnJSONDataReceived(GetUserDataResult r)
    {
        loadingAchievements = false;

        Debug.Log("Received JSON data");
        if (r.Data != null)
        {
            if (r.Data.ContainsKey("Achievements"))
            {
                Debug.Log(r.Data["Achievements"].Value);
                JSListWrapper<Achievement> jlw = JsonUtility.FromJson<JSListWrapper<Achievement>>(r.Data["Achievements"].Value);

                if (achievementBoxPrefab != null)
                {
                    foreach (Achievement achievement in jlw.list)
                    {
                        //instantiate box
                        GameObject achievementBox = Instantiate(achievementBoxPrefab, achievementParent);
                        achievementBox.GetComponent<AchievementBox>().SetUI(achievement);

                        achievementBoxes.Add(achievementBox);
                    }
                }

                achievementsList = jlw.list;
            }
            else
            {
                InitAchievements();
            }
        }
    }

    void OnError(PlayFabError e)
    {
        Debug.Log("Error: " + e.GenerateErrorReport());
    }
}
