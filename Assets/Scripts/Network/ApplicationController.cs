using Networking.Client;
using Networking.Cliente;
using Networking.Host;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using static Networking.Cliente.AuthenticationWrapper;

namespace Networking
{
    public class ApplicationController : MonoBehaviour
    {
        [SerializeField] private ClienteSingleton clientPrefab;
        [SerializeField] private HostSingleton hostPrefab;
        [SerializeField] private TextMeshProUGUI loadingText;

        private static ApplicationController instance;

        private async void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // 1) Auth primero (inicializa UnityServices dentro)
            var authResult = await AuthenticationWrapper.DoAuth(maxTries: 5, retryDelayMs: 1000, timeoutMs: 20000);
            Debug.Log($"Auth result: {authResult} | Active: {AuthenticationWrapper.IsSessionActive()}");

            if (authResult != AuthenticationWrapper.AuthState.Authenticated)
            {
                Debug.LogWarning($"Auth failed: {AuthenticationWrapper.LastException}");
                // Aquí decides: ¿sigues offline? ¿paras? ¿muestras UI?
                // return; // si quieres cortar el bootstrap si no hay auth
            }

            // 2) Luego bootstrap (ya con servicios inicializados)
            await BootstrapAsync();
        }

        private void Update()
        {
            if (Time.frameCount % 30 == 0) // cada ~0.5s
                Debug.Log($"[AUTH] Current: {AuthenticationWrapper.State}");
        }

        private void OnEnable()
        {
            AuthenticationWrapper.OnStateChanged += UpdateLoadingText;
        }

        private void OnDisable()
        {
            AuthenticationWrapper.OnStateChanged -= UpdateLoadingText;
        }

        private void UpdateLoadingText(AuthenticationWrapper.AuthState state)
        {
            switch (state)
            {
                case AuthState.Authenticating:
                    loadingText.text = "Loading...";
                    break;

                case AuthState.Authenticated:
                    loadingText.text = "Conected Succesfully";
                    break;

                case AuthState.Error:
                    loadingText.text = "Connection Error";
                    break;
            }
        }

        private async Task BootstrapAsync()
        {
            bool isDedicatedServer = IsDedicatedServer();

            if (isDedicatedServer)
            {
                // Solo servidor
                HostSingleton host = Instantiate(hostPrefab);
                await host.InitAsync();
            }
            else
            {
                // Host + Cliente
                HostSingleton host = Instantiate(hostPrefab);
                ClienteSingleton client = Instantiate(clientPrefab);

                await host.InitAsync();
                await client.InitAsync();
            }

            Debug.Log("[ApplicationController] Inicialización completa.");
        }

        private bool IsDedicatedServer()
        {
            return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
        }
  
    }
}