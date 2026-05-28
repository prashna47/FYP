using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    [Header("Objective Reference")]
    public int objectiveIndex;
    public int stepIndex = 0;

    bool triggered = false;

    void OnTriggerEnter(Collider other) => TryTrigger(other);
    void OnTriggerStay(Collider other) => TryTrigger(other);

    void TryTrigger(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        if (QuestManager.Instance == null) return;

        if (QuestManager.Instance.IsCorrectTrigger(objectiveIndex, stepIndex))
        {
            triggered = true;
            QuestManager.Instance.TriggerReached();
        }
    }
}