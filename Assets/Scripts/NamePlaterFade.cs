using UnityEngine;

public class NameFadeSimple : MonoBehaviour
{
    public Transform player;
    public float showDistance = 5f;
    public float fadeSpeed = 5f;

    TextMesh tm;

    void Awake()
    {
        tm = GetComponent<TextMesh>();
        Color c = tm.color;
        c.a = 0f;
        tm.color = c;
    }

    void Update()
    {
        if (!player) return;

        float d = Vector3.Distance(player.position, transform.parent.position);
        float target = d <= showDistance ? 1f : 0f;

        Color c = tm.color;
        c.a = Mathf.MoveTowards(c.a, target, fadeSpeed * Time.deltaTime);
        tm.color = c;
    }
}
