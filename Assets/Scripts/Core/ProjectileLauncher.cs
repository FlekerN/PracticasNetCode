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
        if (!IsOwner) return;
        if (!shouldFire) return;
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
        projectileInstance.transform.up = direction;
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
