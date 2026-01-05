using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrailToTarget : MonoBehaviour
{
    public float heightOffset = 0.1f;

    Transform from;
    Transform to;
    LineRenderer lr;

    public void Init(Transform start, Transform target)
    {
        from = start;
        to = target;
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
    }

    void Update()
    {
        if (!from || !to) return;

        Vector3 a = from.position;
        Vector3 b = to.position;

        a.y += heightOffset;
        b.y += heightOffset;

        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
    }
}
