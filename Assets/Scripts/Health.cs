using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    int health = 100;

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Die();
    }

    void Die() => Destroy(gameObject);
}
