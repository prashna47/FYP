using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSortSprite : MonoBehaviour
{
    public int sortingOffset = 0;
    public float precision = 100f;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // If your character moves up/down on Y:
        sr.sortingOrder = sortingOffset + Mathf.RoundToInt(-transform.position.y * precision);

        // If instead your movement uses Z, comment the line above and use:
        // sr.sortingOrder = sortingOffset + Mathf.RoundToInt(-transform.position.z * precision);
    }
}
