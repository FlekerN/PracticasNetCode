using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Host
{
    // Lógica específica del Host (iniciar servidor y cliente a la vez, etc.)
    public class HostGameManager
    {
        private const int MAX_CONNECTIONS = 20;
        private const string GAME_SCENE_NAME = "Game";

        private Allocation allocation;
        private string joinCode;

        public async Task InitAsync()
        {
            // Implementación mínima (futuro: start server + start client)
            await Task.Yield();
            Debug.Log("[HostGameManager] InitAsync completado (mínimo).");
        }
        public async Task StartHostAsync()
        {
            Debug.Log("[HostGameManager] Starting Host with Relay...");

            // 1️⃣ Crear allocation en Relay
            allocation = await RelayService.Instance.CreateAllocationAsync(MAX_CONNECTIONS);

            // 2️⃣ Obtener join code
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"[HostGameManager] JOIN CODE: {joinCode}");

            // 3️⃣ Configurar Unity Transport
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var relayServerData = new RelayServerData(allocation, "udp"); 

            transport.SetRelayServerData(relayServerData);

            // 4️⃣ Iniciar Host
            NetworkManager.Singleton.StartHost();

            // 5️⃣ Cambiar escena
            NetworkManager.Singleton.SceneManager.LoadScene(
                GAME_SCENE_NAME,
                LoadSceneMode.Single
            );
        }
    }
}