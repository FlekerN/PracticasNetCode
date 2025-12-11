using UnityEngine;
using Unity.Netcode;
public class JoinServer : MonoBehaviour
{
    public void Join()
    {
        NetworkManager.Singleton.StartClient(); 
    }
}

