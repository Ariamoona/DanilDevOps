using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int score = 0;
    public Text scoreText;

    void Awake()
    {
        Instance = this;
    }

    public void AddScore()
    {
        score++;
        scoreText.text = score.ToString();
    }
}
