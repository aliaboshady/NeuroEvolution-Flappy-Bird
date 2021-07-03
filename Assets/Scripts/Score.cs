using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public GameObject highScoreObj, currentScoreObj, generationObj, aliveObj;
    public static Text highScoreText, currentScoreText, generationText, aliveText;
    public static int highScore = 0;
    public static int currentScore = 0;
    public static int generation = 1;
    public static int alive = GameSpawner.birdCountStatic;

    void Start()
    {
        highScoreText = highScoreObj.GetComponent<Text>();
        currentScoreText = currentScoreObj.GetComponent<Text>();
        generationText = generationObj.GetComponent<Text>();
        aliveText = aliveObj.GetComponent<Text>();
    }

    private void Update()
    {
        DecreaseAlive();
    }

    public static void IncreaseHighScore()
    {
        highScore++;
        highScoreText.text = "High Score: " + highScore.ToString();
    }

    public static int CountBirds()
    {
        Bird[] foundBirds = FindObjectsOfType<Bird>();
        return foundBirds.Length;
    }

    public static void DecreaseAlive()
    {
        aliveText.text = "Alive: " + CountBirds().ToString() + " / " + GameSpawner.birdCountStatic;
    }

    public static void IncreaseCurrentScore()
    {
        currentScore++;
        currentScoreText.text = "Score: " + currentScore.ToString();

        if (currentScore > highScore)
        {
            IncreaseHighScore();
        }
    }

    public static void IncreaseGeneration()
    {
        generation++;
        generationText.text = "Generation: " + generation.ToString();
    }

}
