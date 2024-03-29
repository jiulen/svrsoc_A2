using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class GameController : MonoBehaviour
{
    public static GameController instance = null;
    public GameObject gameOverObject;
    public TextMeshProUGUI scoreText;

    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalHighScoreText;
    public TextMeshProUGUI finalXPText;
    public TextMeshProUGUI finalCoinsText;
    public int difficultyMax = 5;

    [HideInInspector]
    public bool isGameOver = false;
    public float scrollSpeed = -5f;

    private int score = 0;
    private int highestScore = 0;
    private int coins;

    public Spawner spawner;

    [SerializeField]
    PlayFabUserMgtTMP pfManager;
    [SerializeField]
    InventoryManager invenManager;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        LoadHighScore();
        scoreText.text = "0";
    }

    // Start is called before the first frame update
    void Start()
    {
        gameOverObject.SetActive(false);
        InvokeRepeating("AddScore", 0.25f, 0.25f);
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    void AddScore()
    {
        score += 10;
        scoreText.text = score.ToString();

        //Check and update the level
        CheckLevel();

        if (score >= highestScore)
        {
            SaveHighScore(score);
        }
    }

    public void AddScore(int value)
    {
        score += value;
        scoreText.text = score.ToString();

        //Check and update the level
        CheckLevel();

        if (score >= highestScore)
        {
            SaveHighScore(score);
        }
    }

    void CheckLevel()
    {
        //500 to 1000
        if (score > 500 && score < 1000)
        {
            spawner.SetLevel(1);
            scrollSpeed = -6;
        }
        else if (score > 1000 && score < 2500)
        {
            spawner.SetLevel(2);
            scrollSpeed = -7.5f;
        }
        else if (score > 2500)
        {
            spawner.SetLevel(3);
            scrollSpeed = -10.0f;
        }
    }

    public void ResetGame()
    {
        //Reset the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToLanding()
    {
        SceneManager.LoadScene("Landing");
    }

    public void GameOver()
    {
        gameOverObject.SetActive(true);
        isGameOver = true;
        CancelInvoke();

        finalScoreText.text = score.ToString();
        finalHighScoreText.text = highestScore.ToString();

        pfManager.SendLeaderboard("highscore", score);

        //get xp
        int xpIncrease = score / 10;
        int newXP = pfManager.curXP + xpIncrease;
        //check lvl increase
        int xpForLvl = 50 + pfManager.curLevel * 50;
        int lvlIncrease = 0;
        while (newXP >= xpForLvl)
        {
            newXP -= xpForLvl;
            ++lvlIncrease;
            xpForLvl += 50;
        }
        int newLevel = pfManager.curLevel + lvlIncrease;

        finalXPText.text = "+" + xpIncrease;

        if (lvlIncrease > 0)
            pfManager.SendLeaderboard("level", newLevel);

        //get coins
        coins = score / 100;
        invenManager.AddCoins(coins);

        finalCoinsText.text = "+" + coins;

        pfManager.SetUserData(newXP, newLevel);

        AchievementManager.Instance.UpdateAchievement("High Scorer", AchievementManager.Instance.GetAchievement("High Scorer") + score);
        AchievementManager.Instance.UpdateAchievement("Noob", AchievementManager.Instance.GetAchievement("Noob") + 1);
        AchievementManager.Instance.SendJSON();
    }

    private void SaveHighScore(int score)
    {
        highestScore = score;
        PlayerPrefs.SetInt("highestScore", highestScore);
        //highScoreText.text = highestScore.ToString();
    }

    private void LoadHighScore()
    {
        if(PlayerPrefs.HasKey("highestScore"))
        {
            highestScore = PlayerPrefs.GetInt("highestScore");
            //highScoreText.text = highestScore.ToString();
        }
    }
}
