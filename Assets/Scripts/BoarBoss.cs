using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BoarBoss : MonoBehaviour
{
    public enum BossState
    {
        Patrol,
        ChargeWindup,
        Charging,
        Stunned,
        Dead
    }

    [Header("Vida")]
    public int maxHealth = 3;
    public int health = 3;

    [Header("Movimiento básico")]
    public float patrolSpeed = 1.5f;
    public Transform leftLimit;
    public Transform rightLimit;
    private bool movingRight = true;

    [Header("Carga")]
    public Transform player;
    public float chargeRange = 6f;        // Distancia a la que “ve” al jugador
    public float chargeWindupTime = 1f;   // Tiempo quieto antes de cargar
    public float chargeSpeed = 4f;
    public float stunTime = 2f;           // Tiempo aturdido si falla
    public LayerMask wallLayer;           // Para detectar choque con pared

    [Header("Fases / Invocación")]
    public GameObject minionPrefab;
    public Transform[] summonPoints;      // Puntos donde aparecen minions
    public float summonCooldown = 5f;     // Cada cuánto puede invocar en fase final
    private float summonTimer;

    [Header("UI")]
    public Text winText;

    private BossState state = BossState.Patrol;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool canAct = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Buscar SpriteRenderer en este objeto o en cualquiera de sus hijos
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        if (winText != null)
            winText.gameObject.SetActive(false);

        health = maxHealth;
    }

    void Update()
    {
        state = BossState.Patrol;
        canAct = true;

        if (!canAct || state == BossState.Dead) return;

        switch (state)
        {
            case BossState.Patrol:
                Patrol();
                CheckStartCharge();
                break;

            case BossState.ChargeWindup:
                // No mover; la corrutina se encarga
                break;

            case BossState.Charging:
                Charging();
                break;

            case BossState.Stunned:
                // Se queda quieto; la corrutina lo saca del estado
                // En fase final puede invocar minions
                HandleSummon();
                break;
        }
    }

    // ================== PATRULLA ==================
    void Patrol()
    {
        float speed = patrolSpeed * PhaseSpeedMultiplier();
        Vector2 dir = movingRight ? Vector2.right : Vector2.left;
        transform.Translate(dir * speed * Time.deltaTime);

        if (transform.position.x >= rightLimit.position.x)
            movingRight = false;
        else if (transform.position.x <= leftLimit.position.x)
            movingRight = true;
    }

    // ================== CARGA ==================
    void CheckStartCharge()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= chargeRange)
        {
            StartCoroutine(ChargeRoutine());
        }
    }

    IEnumerator ChargeRoutine()
    {
        state = BossState.ChargeWindup;
        canAct = false;

        // Telegrafiar: pararse y cambiar color
        rb.linearVelocity = Vector2.zero;
        Color original = sr.color;
        sr.color = Color.yellow;

        yield return new WaitForSeconds(chargeWindupTime);

        // Preparar dirección de carga (solo en X)
        float dirX = player.position.x > transform.position.x ? 1f : -1f;
        movingRight = dirX > 0;

        sr.color = original;
        state = BossState.Charging;
        canAct = true;
    }

    void Charging()
    {
        float speed = chargeSpeed * PhaseSpeedMultiplier();
        float dirX = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dirX * speed, rb.linearVelocity.y);

        // Comprobar choque con pared usando un raycast
        RaycastHit2D hit = Physics2D.Raycast(transform.position,
                                              Vector2.right * dirX,
                                              0.5f,
                                              wallLayer);
        if (hit.collider != null)
        {
            // Falló la carga -> aturdido
            rb.linearVelocity = Vector2.zero;
            StartCoroutine(StunRoutine());
        }
    }

    IEnumerator StunRoutine()
    {
        state = BossState.Stunned;
        canAct = false;

        Color original = sr.color;
        sr.color = Color.cyan;

        yield return new WaitForSeconds(stunTime);

        sr.color = original;
        canAct = true;
        state = BossState.Patrol;
    }

    // ================== FASES / INVOCACIÓN ==================
    float PhaseSpeedMultiplier()
    {
        // Fase 1 (vida completa)
        if (health == maxHealth) return 1f;
        // Fase 2 (vida media): un poco más rápido
        if (health == maxHealth - 1) return 1.3f;
        // Fase 3 (vida baja): bastante más rápido
        return 1.6f;
    }

    void HandleSummon()
    {
        // Solo en fase final (1 de vida) y si hay prefab
        if (health > 1 || minionPrefab == null || summonPoints == null || summonPoints.Length == 0)
            return;

        summonTimer -= Time.deltaTime;
        if (summonTimer <= 0f)
        {
            summonTimer = summonCooldown;
            foreach (Transform t in summonPoints)
            {
                Instantiate(minionPrefab, t.position, Quaternion.identity);
            }
        }
    }

    // ================== DAÑO Y MUERTE ==================
    public void TakeHit()
    {
        if (state == BossState.Dead) return;

        health--;
        StartCoroutine(HitFlash());

        if (health <= 0)
        {
            Die();
        }
    }

    IEnumerator HitFlash()
    {
        Color original = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = original;
    }

    void Die()
    {
        state = BossState.Dead;
        canAct = false;
        rb.linearVelocity = Vector2.zero;

        gameObject.SetActive(false);

        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            StartCoroutine(WinDelay());
        }
    }

    IEnumerator WinDelay()
    {
        yield return new WaitForSeconds(10f);
        SceneManager.LoadScene("Credits");
    }
}
