using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RemoteConfigDebugUI : MonoBehaviour
{
    [SerializeField] private RemoteConfigLoader configLoader;
    [SerializeField] private GameObject weaponItemPrefab;
    [SerializeField] private Transform weaponListContainer;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button clearCacheButton;
    [SerializeField] private TextMeshProUGUI statusText;
    
    private void Start()
    {
        refreshButton?.onClick.AddListener(OnRefreshClicked);
        clearCacheButton?.onClick.AddListener(OnClearCacheClicked);
        
        if (configLoader != null)
        {
            configLoader.onConfigLoaded.AddListener(RefreshWeaponList);
            configLoader.onConfigError.AddListener((error) => statusText.text = $"Error: {error}");
        }
    }
    
    private void OnRefreshClicked()
    {
        configLoader?.ReloadConfig();
        statusText.text = "Loading...";
    }
    
    private void OnClearCacheClicked()
    {
        configLoader?.ReloadConfig();
        statusText.text = "Cache cleared";
    }
    
    private void RefreshWeaponList()
    {
        foreach (Transform child in weaponListContainer)
            Destroy(child.gameObject);
        
        foreach (var weapon in configLoader.GetAllWeapons())
        {
            var item = Instantiate(weaponItemPrefab, weaponListContainer);
            var text = item.GetComponentInChildren<TextMeshProUGUI>();
            text.text = $"{weapon.Value.id}: DMG {weapon.Value.damage}, CD {weapon.Value.cooldown}s";
            
            var button = item.GetComponent<Button>();
            button.onClick.AddListener(() => {
                configLoader.ApplyWeaponById(weapon.Key);
                statusText.text = $"Applied: {weapon.Value.id}";
            });
        }
        
        statusText.text = $"Loaded {configLoader.GetAllWeapons().Count} weapons";
    }
}