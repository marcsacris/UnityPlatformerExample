using UnityEngine;

public class BoarHead : MonoBehaviour
{
    private Rigidbody2D rb;
    public float jumpForce = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            // Hacer daño al boss
            BoarBoss boss = GetComponentInParent<BoarBoss>();
            if (boss != null)
            {
                boss.TakeHit();
            }

            // Rebote DEL JUGADOR
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, jumpForce);
            }
        }
    }
}
