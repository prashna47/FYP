using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StoryProgress : MonoBehaviour
{
    public static StoryProgress Instance;

    [Header("UI")]
    public Image fillImage; // <-- assign the Fill Image here

    [Header("Points")]
    public int totalPoints = 100;
    private int currentPoints = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (fillImage != null)
            fillImage.fillAmount = 0f; // start empty
    }

    public void AddPoints(int points)
    {
        currentPoints += points;
        currentPoints = Mathf.Clamp(currentPoints, 0, totalPoints);

        if (fillImage != null)
            fillImage.fillAmount = (float)currentPoints / totalPoints;
    }

    public void AddPointsSmooth(int points)
    {
        StartCoroutine(SmoothAdd(points));
    }

    private IEnumerator SmoothAdd(int points)
    {
        int target = currentPoints + points;
        target = Mathf.Clamp(target, 0, totalPoints);

        float t = 0f;
        int startValue = currentPoints;
        float duration = 0.5f;

        while (t < duration)
        {
            t += Time.deltaTime;
            int val = Mathf.RoundToInt(Mathf.Lerp(startValue, target, t / duration));
            fillImage.fillAmount = (float)val / totalPoints;
            yield return null;
        }

        currentPoints = target;
        fillImage.fillAmount = (float)currentPoints / totalPoints;
    }
}