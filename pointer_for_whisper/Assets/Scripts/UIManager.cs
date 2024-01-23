using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Text questionText;
    public Button[] answerButtons;
    public Text prizeMoneyText;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    /*
    private void Awake()
    {
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
    }*/

    public void DisplayQuestion(Question question)
    {
        questionText.text = question.question;
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.content[i];
            answerButtons[i].onClick.RemoveAllListeners();
            int index = i; // Local copy for the closure
            answerButtons[i].onClick.AddListener(() => FindObjectOfType<GameManager>().AnswerSelected(index));
        }
    }

    public void UpdatePrizeMoney(int prizeMoney)
    {
        prizeMoneyText.text = $"Prize Money: ${prizeMoney}";
    }

    public void ShowGameOver(int prizeMoney)
    {
        gameOverPanel.SetActive(true);
        // add prize money to game over panel
        gameOverPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"You won ${prizeMoney}!";
    }

    public void ShowWinMessage(int prizeMoney)
    {
        winPanel.SetActive(true);
        // add prize money to win panel
        winPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"You won ${prizeMoney}!";
    }
    
    public void HideWinPanel()
    {
    winPanel.SetActive(false);
    }

    public void HideLosePanel()
    {
    gameOverPanel.SetActive(false);
    }
}