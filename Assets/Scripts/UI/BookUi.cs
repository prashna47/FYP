using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BookUI : MonoBehaviour
{
    public static BookUI Instance { get; private set; }
    public static bool IsOpen { get; private set; }

    [Header("Refs")]
    public CanvasGroup root;       // CanvasGroup on BookUI_Root
    public TMP_Text titleText;     // TitleText (TMP)
    public Image pageImage;        // PageImage (UI Image)
    public Button prevBtn, nextBtn, closeBtn;

    [Header("Options")]
    public float fadeSpeed = 12f;  // UI fade in/out speed

    private BookData current;
    private int pageIndex = 0;
    private bool animating = false;

    void Awake()
    {
        Instance = this;

        // Start hidden
        if (root != null)
        {
            root.alpha = 0f;
            root.interactable = false;
            root.blocksRaycasts = false;
        }

        if (pageImage != null)
        {
            pageImage.preserveAspect = true; // avoid stretching
            // Ensure it starts visible when needed
            var c = pageImage.color;
            c.a = 1f;
            pageImage.color = c;
        }

        // Hook buttons
        if (closeBtn) closeBtn.onClick.AddListener(Close);
        if (prevBtn) prevBtn.onClick.AddListener(PrevPage);
        if (nextBtn) nextBtn.onClick.AddListener(NextPage);
    }

    void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape)) Close();
        if (Input.GetKeyDown(KeyCode.LeftArrow)) PrevPage();
        if (Input.GetKeyDown(KeyCode.RightArrow)) NextPage();
    }

    public void Open(BookData data)
    {
        if (IsOpen || data == null) return;

        current = data;
        pageIndex = 0;

        if (titleText) titleText.text = current.title;
        SetPageImage();

        IsOpen = true;

        if (root != null)
        {
            root.blocksRaycasts = true;
            root.interactable = true;
            StartCoroutine(FadeCanvas(1f));
        }

        LockPlayer(true);
        ShowCursor(true);
        UpdateButtons();
    }

    public void Close()
    {
        if (!IsOpen) return;

        StartCoroutine(FadeCanvas(0f, onDone: () =>
        {
            IsOpen = false;
            if (root != null)
            {
                root.blocksRaycasts = false;
                root.interactable = false;
            }
            current = null;
            LockPlayer(false);
            ShowCursor(false);
        }));
    }

    public void NextPage()
    {
        if (!IsOpen || current == null || animating) return;
        if (pageIndex < current.pages.Length - 1)
        {
            pageIndex++;
            PageFlipAnim();
        }
    }

    public void PrevPage()
    {
        if (!IsOpen || current == null || animating) return;
        if (pageIndex > 0)
        {
            pageIndex--;
            PageFlipAnim();
        }
    }

    void SetPageImage()
    {
        if (pageImage == null) return;

        if (current == null || current.pages == null || current.pages.Length == 0)
        {
            pageImage.enabled = false;
            return;
        }

        pageImage.enabled = true;
        pageImage.sprite = current.pages[pageIndex];
        pageImage.preserveAspect = true;
    }

    void UpdateButtons()
    {
        bool hasPrev = pageIndex > 0;
        bool hasNext = current != null && current.pages != null && pageIndex < current.pages.Length - 1;

        if (prevBtn) prevBtn.interactable = hasPrev;
        if (nextBtn) nextBtn.interactable = hasNext;
    }

    IEnumerator FadeCanvas(float target, System.Action onDone = null)
    {
        if (root == null) { onDone?.Invoke(); yield break; }

        while (!Mathf.Approximately(root.alpha, target))
        {
            root.alpha = Mathf.MoveTowards(root.alpha, target, Time.unscaledDeltaTime * fadeSpeed);
            yield return null;
        }
        onDone?.Invoke();
    }

    // Page "flip": quick fade/scale on the image
    void PageFlipAnim()
    {
        if (animating) return;
        StartCoroutine(CoPageFlip());
    }

    IEnumerator CoPageFlip()
    {
        animating = true;

        RectTransform rt = pageImage.rectTransform;
        Vector3 start = Vector3.one;
        Vector3 mid = new Vector3(0.95f, 0.95f, 1f);

        // out (scale down + fade out)
        float t = 0f, dur = 0.08f;
        Color c = pageImage.color;
        float aStart = c.a, aMid = 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = t / dur;
            rt.localScale = Vector3.Lerp(start, mid, k);
            c.a = Mathf.Lerp(aStart, aMid, k);
            pageImage.color = c;
            yield return null;
        }

        // switch sprite
        SetPageImage();
        UpdateButtons();

        // in (scale up + fade in)
        t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = t / dur;
            rt.localScale = Vector3.Lerp(mid, start, k);
            c.a = Mathf.Lerp(aMid, aStart, k);
            pageImage.color = c;
            yield return null;
        }

        animating = false;
    }

    // Replace with your own player controller hooks if desired
    void LockPlayer(bool locked)
    {
        // Example: disable your movement script(s) here if you prefer that over pausing time.
        // FindObjectOfType<YourPlayerController>()?.enabled = !locked;

        Time.timeScale = locked ? 0f : 1f; // Minecraft-like reading pause
    }

    void ShowCursor(bool show)
    {
        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
