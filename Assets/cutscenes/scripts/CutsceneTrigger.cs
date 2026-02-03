using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    public CutsceneScript cutscene;
    public string playerTag = "Player";

    bool hasPlayed = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasPlayed)
            return;

        if (other.CompareTag(playerTag))
        {
            hasPlayed = true;

            cutscene.Play();

            // Optional: disable trigger forever
            gameObject.SetActive(false);
        }
    }
}
