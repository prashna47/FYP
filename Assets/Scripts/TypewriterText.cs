using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPTypewriter : MonoBehaviour
{
    public float typingSpeed = 0.05f;

    private TextMeshProUGUI textUI;
    private Coroutine typing;

    void Awake()
    {
        // 🔑 Automatically get the TMP component on this GameObject
        textUI = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        if (typing != null)
            StopCoroutine(typing);

        typing = StartCoroutine(StartTypingNextFrame());
    }

    IEnumerator StartTypingNextFrame()
    {
        // wait one frame so TMP & Canvas fully initialize
        yield return null;

        textUI.ForceMeshUpdate();
        textUI.maxVisibleCharacters = 0;

        int total = textUI.textInfo.characterCount;

        for (int i = 0; i <= total; i++)
        {
            textUI.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }
}
