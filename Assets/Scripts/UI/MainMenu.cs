using Networking.Host;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public async void OnClickHostButton()
    {
        await HostSingleton.Instance.gameManager.StartHostAsync();
    }
}
