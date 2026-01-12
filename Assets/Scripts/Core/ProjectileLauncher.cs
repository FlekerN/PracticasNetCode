using UnityEngine;
using Unity.Netcode;

public class ProjectileLauncher : NetworkBehaviour
{
    [SerializeField]
    private GameObject serverProjectilePrefab;

    [SerializeField]
    private GameObject clientProjectilePrefab;

    [SerializeField]
    private Transform projectileSpawnPoint;

    [SerializeField]
    private InputReader inputReader;

    [SerializeField]
    private GameObject muzzleFlash;

    [SerializeField]
    private Collider2D playerCollider;

    [SerializeField]
    private float fireRate = 1f; 
    private float previousFireTime = 0f;

    [SerializeField]
    private float muzzleFlashDuration = 0.75f;
    private float muzzleFlashTimer = 0f;

    [Header("Settings")]
    [SerializeField]
    private float projectileSpeed;
    private bool shouldFire = false;
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            inputReader.PrimaryFireEvent -= HandlePrimaryFire;
        }
    }

    private void HandlePrimaryFire(bool shouldFire)
    {
        this.shouldFire = shouldFire;
    }

    private void Update()
    {

        if (muzzleFlash != null && muzzleFlash.activeSelf)
        {
            muzzleFlashTimer -= Time.deltaTime;
            
            if (muzzleFlashTimer <= 0f)
            {
                muzzleFlash.SetActive(false);
            }
        }
        
        if (!IsOwner) return;
        if (!shouldFire) return;
        if (Time.time < previousFireTime + (1f / fireRate)) return;
        
        previousFireTime = Time.time;

        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
   
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        GameObject projectileInstance = Instantiate(
        clientProjectilePrefab,
        spawnPos,
        Quaternion.identity
        );

        var projectileCollider =
        projectileInstance.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playerCollider, projectileCollider);

        if (projectileInstance.TryGetComponent<Rigidbody2D>(out var rb))
        {
            // Importante en 2D usar "transform.up" en lugar de "transform.forward"
            rb.linearVelocity = rb.transform.up * projectileSpeed;
        }
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
            muzzleFlashTimer = muzzleFlashDuration;
        }
        projectileInstance.transform.up = direction;
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        // Instanciar el proyectil real
        GameObject projectileInstance = Instantiate(
        serverProjectilePrefab,
        spawnPos,
        Quaternion.identity
        );

        var projectileCollider =
        projectileInstance.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playerCollider, projectileCollider);
        projectileInstance.transform.up = direction;

        if (projectileInstance.TryGetComponent<Rigidbody2D>(out var rb))
        {
            // Importante en 2D usar "transform.up" en lugar de "transform.forward"
            rb.linearVelocity = rb.transform.up * projectileSpeed;
        }
        // Notificar a todos los clientes
        SpawnDummyProjectileClientRpc(spawnPos, direction);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (IsOwner) return; // Evita crear doble proyectil en quien disparo
        SpawnDummyProjectile(spawnPos, direction);
    }

}
