using UnityEngine;

public class CameraObjectiveTrigger : MonoBehaviour
{
    [Header("Objective Reference")]
    public int objectiveIndex;
    public int stepIndex = 0;

    [Header("Camera Pan")]
    public Transform cameraPanTarget;   // Drag a Transform here for the pan destination
    public float holdTime = 2f;         // How long the camera stays at the target

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        if (QuestManager.Instance == null) return;

        if (QuestManager.Instance.IsCorrectTrigger(objectiveIndex, stepIndex))
        {
            triggered = true;

            // Pan the camera
            if (cameraPanTarget != null)
            {
                camera cam = Camera.main?.GetComponent<camera>();
                if (cam != null)
                    cam.PanToPosition(cameraPanTarget.position, holdTime);
            }

            // Notify QuestManager
            QuestManager.Instance.TriggerReached();
        }
    }
}