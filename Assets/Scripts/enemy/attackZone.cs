using UnityEngine;

public class AttackZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerAttack attack = other.GetComponent<PlayerAttack>();
            if (attack != null)
            {
                attack.canAttack = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerAttack attack = other.GetComponent<PlayerAttack>();
            if (attack != null)
            {
                attack.canAttack = false;
            }
        }
    }
}