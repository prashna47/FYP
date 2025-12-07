using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // name of scenee
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game closed!");
    }
}
