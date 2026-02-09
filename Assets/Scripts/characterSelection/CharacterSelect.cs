using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CharacterSelect : MonoBehaviour
{
    public void SelectMale()
    {
        GameData.IsMale = true;
        SceneManager.LoadScene("GameScene");
    }

    public void SelectFemale()
    {
        GameData.IsMale = false;
        SceneManager.LoadScene("GameScene");
    }
}
