using UnityEngine;

public class SpawnGameObjectOnDestroy : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;

    private void OnDestroy()
    {
        Instantiate(prefabToSpawn, transform.position, Quaternion.identity); 
    }

}
