using UnityEngine;

public class PlayerCharacterLoader : MonoBehaviour
{
    public GameObject male;
    public GameObject female;

    void Start()
    {
        male.SetActive(GameData.IsMale);
        female.SetActive(!GameData.IsMale);
    }
}
