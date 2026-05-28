using UnityEngine;

public class BigCursorChangeTrigger : MonoBehaviour
{
    [Header("Custom Cursor")]
    public Texture2D customCursorTexture;
    public Vector2 cursorSize = new Vector2(64, 64);
    public Vector2 hotspot = Vector2.zero;

    private bool activated = false;
    private Vector2 mousePos;

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

        // Force hide every frame so pause menus can't override it
        Cursor.visible = false;

        mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
    }

    void OnGUI()
    {
        if (!activated || customCursorTexture == null) return;

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