using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WeaponTester : MonoBehaviour
{
    [Header("Remote Config")]
    [SerializeField] private RemoteConfigLoader configLoader;
    
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown weaponDropdown;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button reloadButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI weaponInfoText;
    
    [Header("Current Weapon")]
    [SerializeField] private Weapon currentWeapon = new Weapon();
    
    private void Start()
    {
        InitializeUI();
        
        if (configLoader == null)
            configLoader = FindObjectOfType<RemoteConfigLoader>();
        
        if (configLoader != null)
        {
            configLoader.onConfigLoaded.AddListener(OnConfigLoaded);
            configLoader.onConfigError.AddListener(OnConfigError);
            configLoader.onWeaponApplied.AddListener(OnWeaponApplied);
        }
    }
    
    private void InitializeUI()
    {
        if (loadButton != null)
            loadButton.onClick.AddListener(() => configLoader?.LoadConfig());
            
        if (applyButton != null)
            applyButton.onClick.AddListener(ApplySelectedWeapon);
            
        if (reloadButton != null)
            reloadButton.onClick.AddListener(() => configLoader?.ReloadConfig());
            
        if (weaponDropdown != null)
        {
            weaponDropdown.options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("Pistol"),
                new TMP_Dropdown.OptionData("Rifle"),
                new TMP_Dropdown.OptionData("Shotgun"),
                new TMP_Dropdown.OptionData("Rocket")
            };
            weaponDropdown.value = 0;
        }
        
        UpdateWeaponInfo();
    }
    
    private void ApplySelectedWeapon()
    {
        if (configLoader == null) return;
        
        string selectedWeapon = weaponDropdown.options[weaponDropdown.value].text.ToLower();
        configLoader.ApplyWeaponConfig(currentWeapon, selectedWeapon);
        UpdateWeaponInfo();
    }
    
    private void UpdateWeaponInfo()
    {
        if (weaponInfoText != null)
        {
            weaponInfoText.text = $"Current Weapon:\n{currentWeapon}";
        }
    }
    
    private void OnConfigLoaded()
    {
        statusText.text = " Config loaded successfully!";
        statusText.color = Color.green;
        
        if (weaponDropdown != null)
        {
            ApplySelectedWeapon();
        }
    }
    
    private void OnConfigError(string error)
    {
        statusText.text = $" Error: {error}";
        statusText.color = Color.red;
    }
    
    private void OnWeaponApplied(Weapon weapon)
    {
        statusText.text = $" Applied: {weapon.id}";
        statusText.color = Color.yellow;
    }
    
    private void OnDestroy()
    {
        if (configLoader != null)
        {
            configLoader.onConfigLoaded.RemoveListener(OnConfigLoaded);
            configLoader.onConfigError.RemoveListener(OnConfigError);
            configLoader.onWeaponApplied.RemoveListener(OnWeaponApplied);
        }
    }
}
