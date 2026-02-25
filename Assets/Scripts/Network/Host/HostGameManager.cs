using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Host
{
    // Lógica específica del Host (iniciar servidor y cliente a la vez, etc.)
    public class HostGameManager
    {
        public async Task InitAsync()
        {
            // Implementación mínima (futuro: start server + start client)
            await Task.Yield();
            Debug.Log("[HostGameManager] InitAsync completado (mínimo).");
        }
    }
}