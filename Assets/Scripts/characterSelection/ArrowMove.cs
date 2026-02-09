using UnityEngine;

public class ArrowBounce : MonoBehaviour
{
    public float bounceHeight = 10f;
    public float bounceSpeed = 2f;

    private Vector3 startPos;

    void OnEnable()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        transform.localPosition = startPos + new Vector3(0, offset, 0);
    }

    public void ResetBounce()
    {
        startPos = transform.localPosition;
    }
}
