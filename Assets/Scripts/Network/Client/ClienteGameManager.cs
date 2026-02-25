using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Client
{
    // Clase de "lógica de negocio" (UGS, auth, matchmaking...) sin depender de Unity lifecycle.
    public class ClienteGameManager
    {
        public async Task InitAsync()
        {
            // Implementación mínima simulando trabajo asíncrono (futuro: UGS calls)
            await Task.Yield();
            Debug.Log("[ClienteGameManager] InitAsync completado (mínimo).");
        }
    }
}