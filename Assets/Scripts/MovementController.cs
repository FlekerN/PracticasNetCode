using UnityEngine;

public class TankMovement2D : MonoBehaviour
{
    [Header("Movimiento del tanque")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 120f;

    [Header("Rotación de la torreta")]
    public Transform turretPivot;
    public float turretRotationSpeed = 120f;

    private Rigidbody2D rb;
    private float moveInput;
    private float rotationInput;
    private float turretInput;
    [SerializeField] private InputReader inputReader;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Entrada de movimiento (W/S o flechas)
        moveInput = Input.GetAxisRaw("Vertical");

        // Entrada de rotación del tanque (A/D o flechas)
        rotationInput = -Input.GetAxisRaw("Horizontal");

        // Entrada de rotación de torreta (Q/E)
        turretInput = 0f;
        if (Input.GetKey(KeyCode.Q)) turretInput = 1f;     // Izquierda
        if (Input.GetKey(KeyCode.E)) turretInput = -1f;    // Derecha

        // Rotar torreta en Update (no física)
        RotateTurret();
    }

    void FixedUpdate()
    {
        // Mover tanque
        rb.MovePosition(rb.position + (Vector2)transform.up * moveInput * moveSpeed * Time.fixedDeltaTime);

        // Rotar tanque
        rb.MoveRotation(rb.rotation + rotationInput * rotationSpeed * Time.fixedDeltaTime);
    }

    void RotateTurret()
    {
        if (turretPivot == null) return;

        turretPivot.Rotate(0f, 0f, turretInput * turretRotationSpeed * Time.deltaTime);
    }
}
