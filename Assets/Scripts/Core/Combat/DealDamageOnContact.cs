using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : NetworkBehaviour
{
    [SerializeField] private int damage = 5;

    private ulong ownerClientId;
    public void SetOwner(ulong ownerClientId)
    {
        this.ownerClientId = ownerClientId;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {

        if (col.attachedRigidbody == null) return;
        NetworkObject hitNetObj = col.attachedRigidbody.GetComponent<NetworkObject>()
                                ?? col.GetComponent<NetworkObject>();

        if (hitNetObj != null && hitNetObj.OwnerClientId == ownerClientId)
            return;

        if (col.attachedRigidbody.TryGetComponent<Health>(out var health) ||
            col.TryGetComponent<Health>(out health))
        {
            health.TakeDamage(damage);
        }
    }
}
