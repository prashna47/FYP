using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider slider;

    public void UpdateHealth(int current, int max)
    {
        slider.value = (float)current / max;
    }
}