using UnityEngine;

public class FaceCameraSimple : MonoBehaviour
{
    void LateUpdate()
    {
        if (!Camera.main) return;
        transform.rotation = Quaternion.LookRotation(
            transform.position - Camera.main.transform.position
        );
    }
}
