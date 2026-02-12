using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private RemoteConfigLoader configLoader;
    [SerializeField] private Weapon playerWeapon;
    
    void Start()
    {
        configLoader.onConfigLoaded.AddListener(OnRemoteConfigLoaded);
        
        configLoader.LoadConfig();
    }
    
    void OnRemoteConfigLoaded()
    {
        configLoader.ApplyWeaponConfig(playerWeapon, "rifle");
        
        var shotgun = configLoader.GetWeapon("shotgun");
        Debug.Log($"Shotgun damage: {shotgun.damage}");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            configLoader.ReloadConfig();
        }
    }
}