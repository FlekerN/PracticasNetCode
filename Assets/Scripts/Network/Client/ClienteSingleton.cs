using Networking.Cliente;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Client
{
    public class ClienteSingleton : MonoBehaviour
    {
        private static ClienteSingleton instance;

        // Manager de lógica (sin MonoBehaviour)
        private ClienteGameManager gameManager;

        public static ClienteSingleton Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                instance = FindAnyObjectByType<ClienteSingleton>();

                if (instance == null)
                {
                    Debug.LogError("[ClienteSingleton] No se encontró ninguna instancia en la escena. " +
                                   "Asegúrate de tener un GameObject con ClienteSingleton.");
                }

                return instance;
            }
        }

        private void Awake()
        {

            // Enlaza la instancia si aún no existe
            if (instance == null)
            {
                instance = this;
            }

            // Evita duplicados
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private async void Start()
        {
            await InitAsync();
            // Persistir entre escenas
            DontDestroyOnLoad(gameObject);
        }
        

        /// <summary>
        /// Crea e inicializa el ClienteGameManager de forma asíncrona.
        /// </summary>
        public async Task InitAsync()
        {
            if (gameManager != null)
                return;

            gameManager = new ClienteGameManager();
            await gameManager.InitAsync();

            Debug.Log("[ClienteSingleton] Cliente inicializado correctamente.");
        }
    }
}