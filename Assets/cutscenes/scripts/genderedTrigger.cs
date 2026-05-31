using UnityEngine;

public class GenderedCutsceneTrigger : MonoBehaviour
{
    public GenderedCutscenePlayer genderedCutscene;
    public string playerTag = "Player";
    bool hasPlayed = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasPlayed) return;

        if (other.CompareTag(playerTag))
        {
            hasPlayed = true;
            genderedCutscene.Play();
            gameObject.SetActive(false);
        }
    }
}