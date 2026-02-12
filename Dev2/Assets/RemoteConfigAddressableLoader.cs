using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class RemoteConfigAddressableLoader : MonoBehaviour
{
    [SerializeField] private string addressableKey = "remote_config";
    [SerializeField] private RemoteConfigLoader configLoader;
    
    public void LoadConfigFromAddressable()
    {
        Addressables.LoadAssetAsync<TextAsset>(addressableKey).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var textAsset = handle.Result;
                Debug.Log($" Loaded from Addressables: {textAsset.text.Length} chars");
                
                ProcessConfig(textAsset.text);
                
                Addressables.Release(handle);
            }
            else
            {
                Debug.LogError(" Failed to load config from Addressables");
            }
        };
    }
    
    private void ProcessConfig(string configData)
    {
    }
}
