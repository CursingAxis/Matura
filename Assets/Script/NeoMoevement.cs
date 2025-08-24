using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class NeoMovement : MonoBehaviour
{
    [Header("Bewegung")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    private float currentSpeed;
    private Vector2 moveInput;

    [Header("Springen")]
    public float jumpForce = 10f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Klettern")]
    public LayerMask climbableLayer;
    public float climbSpeed = 3f;
    private bool isClimbing;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool canDash = true;
    private bool isDashing;

    [Header("Leben / Schaden")]
    public int maxHealth = 100;
    public float deathYLevel = -10f;
    public LayerMask hazardLayer;
    private int currentHealth;
    private bool isDead = false;

    [Header("Checkpoint / Respawn")]
    public Transform respawnPoint; // aktueller Respawnpunkt

    // Komponenten
    private Rigidbody2D rb;
    private PlayerInputActions input;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        input = new PlayerInputActions();

        // Eingaben verbinden
        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Jump.performed += ctx => TryJump();
        input.Player.Run.performed += ctx => currentSpeed = runSpeed;
        input.Player.Run.canceled += ctx => currentSpeed = walkSpeed;

        input.Player.Dash.performed += ctx => TryDash();
    }

    private void OnEnable() => input.Player.Enable();
    private void OnDisable() => input.Player.Disable();

    private void Start()
    {
        currentSpeed = walkSpeed;
        currentHealth = maxHealth;

        // Standard-Spawn = Startposition
        if (respawnPoint == null)
            respawnPoint = transform;
    }

    private void Update()
    {
        if (isDead) return;

        // --- Boden prüfen ---
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        else
            isGrounded = false;

        // --- Klettern prüfen ---
        bool touchingClimbable = Physics2D.Raycast(transform.position, Vector2.up, 1f, climbableLayer).collider != null;
        isClimbing = touchingClimbable && Mathf.Abs(moveInput.y) > 0.1f;

        // --- Animator (Run/Idle) ---
        if (animator != null)
            animator.SetBool("isRunning", Mathf.Abs(moveInput.x) > 0.1f && isGrounded && !isClimbing);

        // --- Flip links/rechts ---
        if (moveInput.x > 0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (moveInput.x < -0.01f)
            transform.localScale = new Vector3(1, 1, 1);

        // --- Tod durch Fall ---
        if (transform.position.y <= deathYLevel)
        {
            Debug.Log("Pitfall → Die()");
            Die();
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isDashing) return;

        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, moveInput.y * climbSpeed);
        }
        else
        {
            rb.gravityScale = 3f;
            rb.linearVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
        }
    }

    // --- Springen ---
    private void TryJump()
    {
        if (isDead) return;
        if (isGrounded && !isClimbing)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    // --- Dash ---
    private void TryDash()
    {
        if (isDead) return;
        if (canDash && !isDashing)
            StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        Vector2 dashDir = (Mathf.Abs(moveInput.x) > 0.01f ? new Vector2(moveInput.x, 0f) : Vector2.right).normalized;
        rb.linearVelocity = dashDir * dashForce;

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // --- Schaden nehmen ---
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= Mathf.Abs(damage);
        Debug.Log($"Neo nimmt {damage} Schaden. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    // --- Sterben + Respawn starten ---
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        input.Player.Disable();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        Debug.Log("Neo ist gestorben.");

        StartCoroutine(RespawnRoutine());
    }

    // --- Respawn ---
    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(2f);

        currentHealth = maxHealth;

        transform.position = respawnPoint.position;

        rb.bodyType = RigidbodyType2D.Dynamic;
        input.Player.Enable();
        isDead = false;

        Debug.Log("Neo ist am Checkpoint respawned!");
    }

    // --- Hazard Trigger ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & hazardLayer) != 0)
        {
            Debug.Log($"Hazard getroffen: {other.name} → Die()");
            Die();
        }
    }

    // --- Gizmos ---
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
