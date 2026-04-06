using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteYSorter : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Lower Y = closer to camera = render in front
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }
}