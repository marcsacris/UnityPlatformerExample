using UnityEngine;

public class SmallBeeController : MonoBehaviour
{
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float respawnTime = 3f;

    private Vector3 initialPosition;
    private SpriteRenderer spriteRenderer;
    private Collider2D collider2D;
    private bool isMovingRight = true;
    private float targetX;
    private bool isDead = false;
    private float respawnTimer;

    private void Start()
    {
        initialPosition = transform.position;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        collider2D = GetComponent<Collider2D>();
        targetX = initialPosition.x + moveDistance;
        spriteRenderer.flipX = false;
    }

    private void Update()
    {
        if (isDead)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                Respawn();
            }
            return;
        }

        // Movimiento patrulla
        if (isMovingRight)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
            spriteRenderer.flipX = false; // Mira a la derecha

            if (transform.position.x >= targetX)
            {
                isMovingRight = false;
                targetX = initialPosition.x - moveDistance;
            }
        }
        else
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
            spriteRenderer.flipX = true; // Mira a la izquierda

            if (transform.position.x <= targetX)
            {
                isMovingRight = true;
                targetX = initialPosition.x + moveDistance;
            }
        }
    }

    public void Die()
    {
        isDead = true;
        respawnTimer = respawnTime;
        gameObject.SetActive(false);
        if (collider2D != null)
            collider2D.enabled = false;
    }

    private void Respawn()
    {
        isDead = false;
        transform.position = initialPosition;
        isMovingRight = true;
        targetX = initialPosition.x + moveDistance;
        
        if (collider2D != null)
            collider2D.enabled = true;
        
        gameObject.SetActive(true);
    }
}