using Networking.Client;
using Networking.Cliente;
using Networking.Host;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Networking
{
    public class ApplicationController : MonoBehaviour
    {
        [SerializeField] private ClienteSingleton clientPrefab;
        [SerializeField] private HostSingleton hostPrefab;
        [SerializeField] private TextMeshProUGUI loadingText;

        private static ApplicationController instance;

        private ClienteGameManager gameManager;

        private async void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            gameManager = new ClienteGameManager();

            // 1) Auth (dentro de ClienteGameManager)
            bool authOk = await gameManager.InitAsync();

            if (!authOk)
            {
                Debug.LogWarning($"Auth failed: {AuthenticationWrapper.LastException}");
                // Aquí puedes mostrar UI, reintentar, etc. Por ahora NO avanzamos.
                return;
            }

            // 2) Luego bootstrap (ya con sesión)
            await BootstrapAsync();

            // 3) Ir al menú
            gameManager.GoToMenu();
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
            if (loadingText == null) return;

            switch (state)
            {
                case AuthenticationWrapper.AuthState.Authenticating:
                    loadingText.text = "Loading...";
                    break;

                case AuthenticationWrapper.AuthState.Authenticated:
                    loadingText.text = "Connected Successfully";
                    break;

                case AuthenticationWrapper.AuthState.Error:
                    loadingText.text = "Connection Error";
                    break;

                case AuthenticationWrapper.AuthState.Timeout:
                    loadingText.text = "Connection Timeout";
                    break;

                default:
                    loadingText.text = "";
                    break;
            }
        }

        private async Task BootstrapAsync()
        {
            bool isDedicatedServer = IsDedicatedServer();

            if (isDedicatedServer)
            {
                HostSingleton host = Instantiate(hostPrefab);
                await host.InitAsync();
            }
            else
            {
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