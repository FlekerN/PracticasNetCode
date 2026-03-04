using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Networking.Cliente;

namespace Networking.Client
{
    public class ClienteGameManager
    {
        public const string MenuSceneName = "Menu";

        public async Task<bool> InitAsync()
        {
            // 1) Inicializa UGS
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            // 2) Autenticación
            var authResult = await AuthenticationWrapper.DoAuth(maxTries: 5, retryDelayMs: 1000, timeoutMs: 20000);

            bool ok = authResult == AuthenticationWrapper.AuthState.Authenticated;
            Debug.Log($"[ClienteGameManager] Auth ok? {ok} | Result: {authResult}");

            return ok;
        }

        public void GoToMenu()
        {
            SceneManager.LoadScene(MenuSceneName);
        }
    }
}