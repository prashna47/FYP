using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        ScreenFade fader = FindObjectOfType<ScreenFade>();

        if (fader != null)
        {
            fader.FadeToScene("CharacterSelection");
        }
        else
        {
            // Fallback in case no fader exists
            SceneManager.LoadScene("CharacterSelection");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game closed!");
    }
}