using UnityEngine;

public class WorldToUIFollow : MonoBehaviour
{
    public Transform target;      // NPC head / anchor
    public Vector3 offset;        // up offset if needed
    public Camera cam;

    RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (!cam) cam = Camera.main;
    }

    void LateUpdate()
    {
        if (!target || !cam) return;

        Vector3 screenPos = cam.WorldToScreenPoint(target.position + offset);
        rt.position = screenPos;
    }
}
