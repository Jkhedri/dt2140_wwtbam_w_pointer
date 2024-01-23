using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private List<Question> questions;
    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private int prizeMoney = 0;
    public UIManager uiManager;
    public TextAsset jsonAsset;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        uiManager.gameOverPanel.SetActive(false);
        uiManager.winPanel.SetActive(false);
        
        questions = QuestionLoader.LoadQuestions(jsonAsset);
        currentQuestionIndex = UnityEngine.Random.Range(0, 75);     // Generate a random question number between 0 and 74 
        Debug.Log("Current Question Index: " + currentQuestionIndex);
        Debug.Log("Question count: " + questions.Count);
        DisplayNextQuestion();
        
    }

    /*
    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
        questions = QuestionLoader.LoadQuestions();
        //currentQuestionIndex = Random.Range(0, 36);     // Generate a random question
        DisplayNextQuestion();
        uiManager.gameOverPanel.SetActive(false);
        uiManager.winPanel.SetActive(false);
    }
    */

    public void AnswerSelected(int index)
    {      
        if (questions[currentQuestionIndex-1].IsCorrectAnswer(index))
        {
            correctAnswers++;
            prizeMoney = CalculatePrizeMoney(correctAnswers);
            uiManager.UpdatePrizeMoney(prizeMoney);

            if (correctAnswers >= 11)
            {
                uiManager.ShowWinMessage(prizeMoney);
            }
            else
            {
                DisplayNextQuestion();
            }
        }
        else
        {
            uiManager.ShowGameOver(prizeMoney);
        }
    }

    private void DisplayNextQuestion()
    {
        currentQuestionIndex = currentQuestionIndex % questions.Count;
        if (currentQuestionIndex < questions.Count)
        {
            uiManager.DisplayQuestion(questions[currentQuestionIndex]);
            currentQuestionIndex++;
        }
    }

    private int CalculatePrizeMoney(int correctAnswers)
    {
        return (int)Mathf.Pow(2, correctAnswers - 1) * 1000;
    }
}