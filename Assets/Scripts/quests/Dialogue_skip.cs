using UnityEngine;

public class DialogueSkipUI : MonoBehaviour
{
    public GameObject skipButton;

    void Update()
    {
        if (ObjectiveDialogueUI.Instance == null) return;

        skipButton.SetActive(ObjectiveDialogueUI.Instance.IsDialogueActive());
    }
}