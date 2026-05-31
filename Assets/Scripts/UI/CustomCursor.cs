using UnityEngine;
public class BigCursorChangeTrigger : MonoBehaviour
{
    [Header("Custom Cursor")]
    public Texture2D customCursorTexture;
    public Vector2 cursorSize = new Vector2(64, 64);
    public Vector2 hotspot = Vector2.zero;
    private bool activated = false;
    private Vector2 mousePos;

    // ✅ Set this true whenever UI needs the cursor
    public static bool UIOverride = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Activate();
    }
    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!activated) Activate();
    }
    void Activate()
    {
        activated = true;
    }
    void Update()
    {
        if (!activated) return;

        mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

        if (UIOverride) return;

        Cursor.visible = false;
    }
    void OnGUI()
    {
        // ✅ Also hide custom cursor drawn texture during UI
        if (!activated || customCursorTexture == null || UIOverride) return;
        GUI.DrawTexture(
            new Rect(
                mousePos.x - hotspot.x,
                mousePos.y - hotspot.y,
                cursorSize.x,
                cursorSize.y
            ),
            customCursorTexture
        );
    }
}