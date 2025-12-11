using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;

    private Vector3 currentMovement;
    private bool isFiring;

    [Header("Movimiento")]
    public float speed = 5f;

    private void Start()
    {
        inputReader.MoveEvent += HandleMove;
        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    private void OnDestroy()
    {
        inputReader.MoveEvent -= HandleMove;
        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    private void Update()
    {
        // Aplicar movimiento
        transform.position += currentMovement * speed * Time.deltaTime;

        // Simular disparo
        if (isFiring)
        {
            Debug.Log("Disparando...");

        }
    }

    private void HandleMove(Vector2 movement)
    {
        currentMovement = new Vector3(movement.x, movement.y,0 ); // movimiento plano 3D
        Debug.Log("Movimiento: " + movement);
    }

    private void HandlePrimaryFire(bool firing)
    {
        isFiring = firing;
        Debug.Log("Disparando: " + firing);
    }
}
