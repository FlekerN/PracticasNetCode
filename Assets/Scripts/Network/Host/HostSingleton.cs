using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Host
{
    public class HostSingleton : MonoBehaviour
    {
        private static HostSingleton instance;

        public HostGameManager gameManager { get; private set; }

        public static HostSingleton Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                instance = FindAnyObjectByType<HostSingleton>();

                if (instance == null)
                {
                    Debug.LogError("[HostSingleton] No se encontró ninguna instancia en la escena. " +
                                   "Asegúrate de tener un GameObject con HostSingleton.");
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public async Task InitAsync()
        {
            if (gameManager != null)
                return;

            gameManager = new HostGameManager();
            await gameManager.InitAsync();

            Debug.Log("[HostSingleton] Host inicializado correctamente.");
        }
    }
}