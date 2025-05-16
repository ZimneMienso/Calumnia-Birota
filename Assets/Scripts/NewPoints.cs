using UnityEngine;

public class NewPoints : MonoBehaviour
{
    void Start()
    {
        Invoke("DestroySelf", 1);
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
