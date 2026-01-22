using UnityEngine;
using Unity.Netcode;

public class TankNetController2D : NetworkBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform turretPivot;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;         
    [SerializeField] private float turnSpeed = 180f;        

    [Header("Turret")]
    [SerializeField] private float turretTurnSpeed = 220f;  

    [Header("Smoothing (clients)")]
    [SerializeField] private float posLerp = 18f;
    [SerializeField] private float rotLerp = 18f;
    [SerializeField] private float turretLerp = 22f;

    // ===== Network state (server -> everyone) =====
    private readonly NetworkVariable<Vector2> netPos =
        new(writePerm: NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<float> netTankRot =
        new(writePerm: NetworkVariableWritePermission.Server); 

    private readonly NetworkVariable<float> netTurretRot =
        new(writePerm: NetworkVariableWritePermission.Server); 


    private float srvMove;     
    private float srvTurn;     
    private float srvTurret;  

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Inicializar estado en server
            netPos.Value = transform.position;
            netTankRot.Value = transform.eulerAngles.z;

            if (turretPivot != null)
                netTurretRot.Value = turretPivot.localEulerAngles.z;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            // Input local (teclado)
            float move = (Input.GetKey(KeyCode.W) ? 1f : 0f) + (Input.GetKey(KeyCode.S) ? -1f : 0f);
            float turn = (Input.GetKey(KeyCode.D) ? 1f : 0f) + (Input.GetKey(KeyCode.A) ? -1f : 0f);
            float turret = (Input.GetKey(KeyCode.E) ? 1f : 0f) + (Input.GetKey(KeyCode.Q) ? -1f : 0f);

            // Enviar al server (dueño -> server)
            SubmitInputServerRpc(move, turn, turret);
        }


        if (!IsServer)
        {
            // Posición
            Vector3 targetPos = new Vector3(netPos.Value.x, netPos.Value.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * posLerp);

            // Rotación del tanque (Z)
            float targetTankZ = netTankRot.Value;
            float currentZ = transform.eulerAngles.z;
            float newZ = Mathf.LerpAngle(currentZ, targetTankZ, Time.deltaTime * rotLerp);
            transform.rotation = Quaternion.Euler(0f, 0f, newZ);

            // Rotación de la torreta (Z local)
            if (turretPivot != null)
            {
                float targetTurretZ = netTurretRot.Value;
                float currentTurretZ = turretPivot.localEulerAngles.z;
                float newTurretZ = Mathf.LerpAngle(currentTurretZ, targetTurretZ, Time.deltaTime * turretLerp);
                turretPivot.localRotation = Quaternion.Euler(0f, 0f, newTurretZ);
            }
        }
        else
        {
            ApplyServerStateInstant();
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        float dt = Time.fixedDeltaTime;

        // 1) Girar tanque sobre sí mismo
        float tankZ = transform.eulerAngles.z + srvTurn * turnSpeed * dt;
        transform.rotation = Quaternion.Euler(0f, 0f, tankZ);

        // 2) Avanzar/retroceder según donde mira (transform.up en 2D)
        Vector2 forward = transform.up; // dirección "hacia arriba" del sprite
        Vector2 newPos = (Vector2)transform.position + forward * (srvMove * moveSpeed * dt);
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);

        // 3) Rotar torreta (local Z)
        if (turretPivot != null)
        {
            float turretZ = turretPivot.localEulerAngles.z + srvTurret * turretTurnSpeed * dt;
            turretPivot.localRotation = Quaternion.Euler(0f, 0f, turretZ);
        }

        // 4) Publicar estado a la red
        netPos.Value = transform.position;
        netTankRot.Value = transform.eulerAngles.z;
        if (turretPivot != null) netTurretRot.Value = turretPivot.localEulerAngles.z;
    }

    [ServerRpc]
    private void SubmitInputServerRpc(float move, float turn, float turret)
    {
        srvMove = Mathf.Clamp(move, -1f, 1f);
        srvTurn = Mathf.Clamp(turn, -1f, 1f);
        srvTurret = Mathf.Clamp(turret, -1f, 1f);
    }

    private void ApplyServerStateInstant()
    {
        transform.position = new Vector3(netPos.Value.x, netPos.Value.y, transform.position.z);
        transform.rotation = Quaternion.Euler(0f, 0f, netTankRot.Value);

        if (turretPivot != null)
            turretPivot.localRotation = Quaternion.Euler(0f, 0f, netTurretRot.Value);
    }
}
