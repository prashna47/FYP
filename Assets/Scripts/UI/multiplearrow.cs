using UnityEngine;

public class MultiEnemyArrow : MonoBehaviour
{
    public RectTransform arrow;
    public float screenPadding = 60f;
    public float onScreenOffset = 80f;

    private Transform target;

    // ✅ Now works for ANY enemy type
    public void SetTarget(Transform t)
    {
        target = t;
    }

    void Update()
    {
        // Destroy arrow when target is gone
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Camera cam = Camera.main;
        Vector3 screenPos = cam.WorldToScreenPoint(target.position);
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 dir = ((Vector2)screenPos - screenCenter).normalized;

        bool onScreen =
            screenPos.z > 0 &&
            screenPos.x > 0 &&
            screenPos.x < Screen.width &&
            screenPos.y > 0 &&
            screenPos.y < Screen.height;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrow.rotation = Quaternion.Euler(0, 0, angle - 90f);

        if (onScreen)
            arrow.position = (Vector2)screenPos - dir * onScreenOffset;
        else
        {
            float radius = Mathf.Min(screenCenter.x, screenCenter.y) - screenPadding;
            arrow.position = screenCenter + dir * radius;
        }
    }
}