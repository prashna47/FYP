using UnityEngine;

public class GenderedCutscenePlayer : MonoBehaviour
{
    [Header("Cutscenes")]
    public CutsceneScript maleCutscene;
    public CutsceneScript femaleCutscene;

    [Header("AUTO")]
    public bool playOnSceneLoad = false;
    public GenderedCutscenePlayer nextGenderedCutscene;

    public System.Action onCutsceneFinished;

    void Start()
    {
        if (playOnSceneLoad)
            Play();
    }

    public void Play()
    {
        CutsceneScript chosen = GameData.IsMale ? maleCutscene : femaleCutscene;

        if (chosen == null)
        {
            Debug.LogWarning($"GenderedCutscenePlayer: No cutscene assigned for {(GameData.IsMale ? "male" : "female")}!");
            onCutsceneFinished?.Invoke();
            onCutsceneFinished = null;
            return;
        }

        // Clear any leftover listeners on the chosen cutscene
        chosen.onCutsceneFinished = null;

        chosen.onCutsceneFinished += () =>
        {
            onCutsceneFinished?.Invoke();
            onCutsceneFinished = null;

            if (nextGenderedCutscene != null)
                nextGenderedCutscene.Play();
        };

        chosen.Play();
    }
}