using UnityEngine;
using System.Collections;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [Header("UI To Hide On Death")]
    public GameObject[] uiToHideOnDeath;

    [System.Serializable]
    public class SpawnZone
    {
        public string label = "Spawn Point 1";
        public Transform spawnPoint;
        [Tooltip("Inclusive objective index that unlocks this spawn point")]
        public int unlockedAtObjective;
        [Tooltip("Objectives >= this index use this spawn point")]
        public int objectiveRangeMin;
        [Tooltip("Objectives <= this index use this spawn point")]
        public int objectiveRangeMax;
    }

    [Header("Spawn Zones")]
    public SpawnZone[] spawnZones;

    [Header("Default Spawn (before any zone unlocks)")]
    public Transform defaultSpawnPoint;

    [Header("References")]
    public RespawnUI respawnUI;
    public SpawnPointUnlockUI unlockUI;

    [Header("Characters")]
    public GameObject maleCharacter;
    public GameObject femaleCharacter;

    [Header("Fade")]
    public ScreenFade fader;
    public float blackScreenHoldTime = 0.5f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnPlayerDied()
    {
        HideGameUI();
        respawnUI.Show();
    }

    // Called by the respawn button
    public void Respawn()
    {
        StartCoroutine(RespawnWithFade());
    }
    void HideGameUI()
    {
        foreach (GameObject ui in uiToHideOnDeath)
            if (ui != null) ui.SetActive(false);
    }

    void ShowGameUI()
    {
        foreach (GameObject ui in uiToHideOnDeath)
            if (ui != null) ui.SetActive(true);
    }

    IEnumerator RespawnWithFade()
    {
        GameState.IsPlayerFrozen = true;

        if (fader != null)
            yield return fader.FadeOut();

        yield return new WaitForSeconds(blackScreenHoldTime);

        Transform target = GetSpawnForCurrentObjective();
        GameObject activeCharacter = GameData.IsMale ? maleCharacter : femaleCharacter;

        CharacterController cc = activeCharacter.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        activeCharacter.transform.position = target.position;

        var cam = FindObjectOfType<camera>();
        if (cam != null) cam.SnapToTarget();

        yield return null;
        yield return new WaitForEndOfFrame();

        if (cc != null) cc.enabled = true;

        var interactor = activeCharacter.GetComponent<PlayerProximityInteractor>();
        if (interactor != null) interactor.ClearAllInteractables();

        var itemHandler = activeCharacter.GetComponent<PlayerItemHandler>();
        if (itemHandler != null) itemHandler.ClearNearbyItem();

        PlayerHealth ph = activeCharacter.GetComponent<PlayerHealth>();
        if (ph != null) ph.HealToFull();

        respawnUI.Hide();
        ShowGameUI();        

        if (fader != null)
            yield return fader.FadeIn();

        GameState.IsPlayerFrozen = false;
        PlayerControlLock.MovementLocked = false;
        InteractionLock.DialoguePlaying = false;
    }
    Transform GetSpawnForCurrentObjective()
    {
        if (QuestManager.Instance == null) return defaultSpawnPoint;

        int idx = QuestManager.Instance.CurrentObjectiveIndex;

        for (int i = 0; i < spawnZones.Length; i++)
        {
            if (idx >= spawnZones[i].objectiveRangeMin &&
                idx <= spawnZones[i].objectiveRangeMax)
            {
                return spawnZones[i].spawnPoint;
            }
        }

        return defaultSpawnPoint;
    }

    public void CheckSpawnUnlock(int completedObjectiveIndex)
    {
        for (int i = 0; i < spawnZones.Length; i++)
        {
            if (completedObjectiveIndex == spawnZones[i].unlockedAtObjective)
            {
                unlockUI.Show(spawnZones[i].label);
                return;
            }
        }
    }
}