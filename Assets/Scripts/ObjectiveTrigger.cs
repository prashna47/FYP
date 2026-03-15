using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    [Header("Objective Reference")]
    public int objectiveIndex; // The index of the objective in QuestManager
    public int stepIndex = 0;  // Step index for multi-step objectives (0 for single-step)

    bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (!other.CompareTag("Player")) return;

        if (QuestManager.Instance == null) return;

        // Only trigger if this matches current objective and step
        if (QuestManager.Instance.IsCorrectTrigger(objectiveIndex, stepIndex))
        {
            triggered = true;

            // Notify QuestManager
            QuestManager.Instance.TriggerReached();
        }
    }
}