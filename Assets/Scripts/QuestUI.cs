using UnityEngine;
using TMPro;
using System.Collections;

public class QuestUI : MonoBehaviour
{
    public static QuestUI Instance;

    [Header("UI")]
    public TMP_Text objectiveText;
    public CanvasGroup group;
    public GameObject tickIcon;

    public bool IsAnimating { get; private set; }


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (group)
            group.alpha = 0f;

        if (tickIcon)
            tickIcon.SetActive(false);
    }

    public void ShowObjective(string text)
    {
        objectiveText.text = text;
        StartCoroutine(FadeGroup(1f));

        if (tickIcon)
            tickIcon.SetActive(false);
    }

    IEnumerator FadeGroup(float targetAlpha)
    {
        while (!Mathf.Approximately(group.alpha, targetAlpha))
        {
            group.alpha = Mathf.MoveTowards(
                group.alpha,
                targetAlpha,
                Time.deltaTime * 4f
            );
            yield return null;
        }
    }



    public void PlayObjectiveComplete()
    {
        if (!IsAnimating)
            StartCoroutine(ObjectiveCompleteRoutine());
    }


    IEnumerator ObjectiveCompleteRoutine()
    {

        IsAnimating = true;

        // 1️⃣ Change text
        objectiveText.text = "Objective Complete";

        // 2️⃣ Show tick
        if (tickIcon)
        {
            tickIcon.SetActive(true);
            tickIcon.transform.localScale = Vector3.zero;
        }

        // 3️⃣ Tick pop animation
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 6f;
            if (tickIcon)
                tickIcon.transform.localScale = Vector3.Lerp(
                    Vector3.zero,
                    Vector3.one,
                    t
                );
            yield return null;
        }

        yield return new WaitForSeconds(0.8f);

        yield return new WaitForSeconds(0.8f);

        // NEW fade-out logic
        yield return StartCoroutine(FadeGroup(0f));

        if (tickIcon)
            tickIcon.SetActive(false);


        IsAnimating = false;
    }
    

}
