using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    public Health health;
    public Image healthBarImage;



    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsClient) return;
        health.currentHealth.OnValueChanged += HandleHealthChanged;
        HandleHealthChanged(0, health.currentHealth.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsClient) return;
        health.currentHealth.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int oldHealth, int newHealth)
    {
        float maxHealth = health.MaxHealth;
        float fillAmount = newHealth / maxHealth;

        healthBarImage.fillAmount = fillAmount;
    }
}
