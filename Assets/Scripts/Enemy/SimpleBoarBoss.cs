using UnityEngine;
using Platformer2DSystem.Example;

public class SimpleBoarBoss : MonoBehaviour
{
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;
    [SerializeField] private float baseSpeed = 2f;
    private float speed;
    private int direction = 1;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.flipX = true;
        speed = baseSpeed;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (transform.position.x < minX)
        {
            direction = 1;
            spriteRenderer.flipX = true;
        }
        else if (transform.position.x > maxX)
        {
            direction = -1;
            spriteRenderer.flipX = false;
        }
    }

    private void FixedUpdate()
    {
        Vector2 move = Vector2.right * direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb == null) return;

            Vector2 playerVelocity = playerRb.linearVelocity;
            bool playerFalling = playerVelocity.y < 0f;

            if (playerFalling && collision.bounds.min.y > this.GetComponent<Collider2D>().bounds.max.y - 0.1f)
            {
                Die();
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 10f);
            }
            else
            {
                PlayerController player = collision.GetComponent<PlayerController>();
                if (player != null && !player.IsHit())
                {
                    player.TakeDamage();
                    float pushDirection = transform.position.x > collision.transform.position.x ? -1f : 1f;
                    playerRb.linearVelocity = new Vector2(50f * pushDirection, playerRb.linearVelocity.y);
                }
            }
        }
    }

    private void Die()
    {
        GameObject finishText = GameObject.FindWithTag("Finish");
        if (finishText != null)
        {
            finishText.SetActive(true);
            Debug.Log("Victory!");
        }

        Destroy(gameObject);
    }
}
