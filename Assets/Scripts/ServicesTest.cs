using Unity.Services.Core;
using UnityEngine;

public class UGSInit : MonoBehaviour
{
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("✅ Unity Gaming Services initialized!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ UGS Init failed: " + e);
        }
    }
}