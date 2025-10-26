using UnityEngine;

public class DontDestroyOnLoadTag : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
