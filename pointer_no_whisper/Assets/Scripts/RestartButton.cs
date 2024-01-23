using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{
    private void Start()
    {
        // Attach the RestartGame method to the button's click event
        GetComponent<Button>().onClick.AddListener(RestartGame);
        
    }

    private void RestartGame()
    {
        // Restart the game by reloading the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        // changes the colour of the button to red
        // GetComponent<Image>().color = Color.red;
    }
}
