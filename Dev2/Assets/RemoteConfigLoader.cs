using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using UnityEngine.Events;

public class RemoteConfigLoader : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string configURL = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQxxxxxxxx/pub?gid=0&single=true&output=csv";
    [SerializeField] private ConfigFormat configFormat = ConfigFormat.CSV;
    [SerializeField] private float timeoutSeconds = 10f;
    
    [Header("Debug")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private bool forceReloadOnStart = false;
    
    [Header("Events")]
    public UnityEvent onConfigLoaded;
    public UnityEvent<string> onConfigError;
    public UnityEvent<Weapon> onWeaponApplied;
    
    private Dictionary<string, Weapon> weaponCache = new Dictionary<string, Weapon>();
    private string localFilePath;
    private bool isLoading = false;
    
    public enum ConfigFormat
    {
        CSV,
        JSON
    }
    
    void Awake()
    {
        localFilePath = Path.Combine(Application.persistentDataPath, "remote_config.cache");
        Log($"📁 Local cache path: {localFilePath}");
        
        LoadLocalCache();
        
        if (forceReloadOnStart)
            StartCoroutine(FetchRemoteConfig());
    }
    
    void Start()
    {
        if (weaponCache.Count == 0 && !isLoading)
        {
            StartCoroutine(FetchRemoteConfig());
        }
    }
    
    #region Public Methods
    
    public void LoadConfig()
    {
        if (!isLoading)
            StartCoroutine(FetchRemoteConfig());
    }
    
    public Weapon GetWeapon(string weaponId)
    {
        weaponId = weaponId.ToLower();
        
        if (weaponCache.ContainsKey(weaponId))
        {
            Log($"✅ Found weapon: {weaponId}");
            return weaponCache[weaponId];
        }
        
        LogWarning($"⚠️ Weapon not found: {weaponId}, using defaults");
        return Weapon.GetDefaultWeapon(weaponId);
    }
    
    public bool ApplyWeaponConfig(Weapon targetWeapon, string weaponId)
    {
        if (targetWeapon == null)
        {
            LogError("❌ Target weapon is null!");
            return false;
        }
        
        var config = GetWeapon(weaponId);
        targetWeapon.id = config.id;
        targetWeapon.damage = config.damage;
        targetWeapon.cooldown = config.cooldown;
        
        Log($"🔧 Applied config to weapon: {targetWeapon}");
        onWeaponApplied?.Invoke(targetWeapon);
        
        return true;
    }
    
    public void ApplyWeaponById(string weaponId)
    {
        var weapon = GetWeapon(weaponId);
        Debug.Log($"🎯 Remote config applied: {weapon}");
    }
    
    public void ReloadConfig()
    {
        if (File.Exists(localFilePath))
        {
            File.Delete(localFilePath);
            Log("🗑️ Local cache cleared");
        }
        
        weaponCache.Clear();
        StartCoroutine(FetchRemoteConfig());
    }
    
    public Dictionary<string, Weapon> GetAllWeapons()
    {
        return new Dictionary<string, Weapon>(weaponCache);
    }
    
    #endregion
    
    #region Remote Loading
    
    IEnumerator FetchRemoteConfig()
    {
        isLoading = true;
        Log($"🌐 Fetching remote config from: {configURL}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(configURL))
        {
            request.timeout = (int)timeoutSeconds;
            request.downloadHandler = new DownloadHandlerBuffer();
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string rawData = request.downloadHandler.text;
                Log($"📦 Received data: {rawData.Length} bytes");
                
                bool parsed = false;
                
                switch (configFormat)
                {
                    case ConfigFormat.CSV:
                        parsed = ParseCSV(rawData);
                        break;
                    case ConfigFormat.JSON:
                        parsed = ParseJSON(rawData);
                        break;
                }
                
                if (parsed)
                {
                    onConfigLoaded?.Invoke();
                    SaveLocalCache(rawData);
                }
                else
                {
                    HandleError("Failed to parse config data");
                }
            }
            else
            {
                HandleError($"Network error: {request.error}");
            }
        }
        
        isLoading = false;
    }
    
    #endregion
    
    #region Parsing Methods
    
    private bool ParseCSV(string csvData)
    {
        try
        {
            var weapons = new Dictionary<string, Weapon>();
            string[] lines = csvData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length < 2)
            {
                LogError("❌ CSV has no data rows");
                return false;
            }
            
            string[] headers = lines[0].Split(',');
            int idIndex = -1, damageIndex = -1, cooldownIndex = -1;
            
            for (int i = 0; i < headers.Length; i++)
            {
                string header = headers[i].Trim().ToLower();
                if (header.Contains("id")) idIndex = i;
                else if (header.Contains("damage")) damageIndex = i;
                else if (header.Contains("cooldown")) cooldownIndex = i;
            }
            
            if (idIndex == -1 || damageIndex == -1 || cooldownIndex == -1)
            {
                LogError("❌ CSV missing required columns");
                return false;
            }
            
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                
                string[] values = lines[i].Split(',');
                if (values.Length <= Math.Max(idIndex, Math.Max(damageIndex, cooldownIndex)))
                    continue;
                
                try
                {
                    string id = values[idIndex].Trim().ToLower();
                    float damage = float.Parse(values[damageIndex].Trim());
                    float cooldown = float.Parse(values[cooldownIndex].Trim());
                    
                    if (ValidateWeaponData(id, damage, cooldown))
                    {
                        weapons[id] = new Weapon
                        {
                            id = id,
                            damage = damage,
                            cooldown = cooldown
                        };
                        Log($"✅ Parsed: {id}, Dmg: {damage}, Cd: {cooldown}");
                    }
                }
                catch (Exception e)
                {
                    LogWarning($"⚠️ Failed to parse row {i}: {e.Message}");
                }
            }
            
            if (weapons.Count > 0)
            {
                weaponCache = weapons;
                Log($"🎯 Loaded {weapons.Count} weapons from CSV");
                return true;
            }
            
            return false;
        }
        catch (Exception e)
        {
            LogError($"❌ CSV parsing error: {e.Message}");
            return false;
        }
    }
    
    private bool ParseJSON(string jsonData)
    {
        try
        {
            var weaponData = JsonUtility.FromJson<WeaponData>(jsonData);
            
            if (weaponData?.weapons == null || weaponData.weapons.Length == 0)
            {
                LogError("❌ JSON has no weapons data");
                return false;
            }
            
            var weapons = new Dictionary<string, Weapon>();
            
            foreach (var weapon in weaponData.weapons)
            {
                if (ValidateWeaponData(weapon.id, weapon.damage, weapon.cooldown))
                {
                    weapons[weapon.id.ToLower()] = weapon;
                    Log($"✅ Parsed: {weapon.id}, Dmg: {weapon.damage}, Cd: {weapon.cooldown}");
                }
            }
            
            if (weapons.Count > 0)
            {
                weaponCache = weapons;
                Log($"🎯 Loaded {weapons.Count} weapons from JSON");
                return true;
            }
            
            return false;
        }
        catch (Exception e)
        {
            LogError($"❌ JSON parsing error: {e.Message}");
            return false;
        }
    }
    
    private bool ValidateWeaponData(string id, float damage, float cooldown)
    {
        if (string.IsNullOrEmpty(id))
        {
            LogWarning("⚠️ Weapon ID is empty");
            return false;
        }
        
        if (damage < 0)
        {
            LogWarning($"⚠️ Weapon {id} has negative damage: {damage}");
            return false;
        }
        
        if (cooldown <= 0)
        {
            LogWarning($"⚠️ Weapon {id} has invalid cooldown: {cooldown}");
            return false;
        }
        
        return true;
    }
    
    #endregion
    
    #region Local Cache
    
    private void SaveLocalCache(string rawData)
    {
        try
        {
            var config = new RemoteConfig
            {
                weapons = new Weapon[weaponCache.Count],
                lastUpdated = DateTime.Now,
                source = configURL
            };
            
            int i = 0;
            foreach (var weapon in weaponCache.Values)
            {
                config.weapons[i++] = weapon;
            }
            
            string json = JsonUtility.ToJson(config, true);
            File.WriteAllText(localFilePath, json);
            
            Log($"💾 Saved {weaponCache.Count} weapons to cache");
        }
        catch (Exception e)
        {
            LogError($"❌ Failed to save cache: {e.Message}");
        }
    }
    
    private void LoadLocalCache()
    {
        try
        {
            if (File.Exists(localFilePath))
            {
                string json = File.ReadAllText(localFilePath);
                var config = JsonUtility.FromJson<RemoteConfig>(json);
                
                if (config?.weapons != null)
                {
                    weaponCache.Clear();
                    foreach (var weapon in config.weapons)
                    {
                        if (ValidateWeaponData(weapon.id, weapon.damage, weapon.cooldown))
                        {
                            weaponCache[weapon.id.ToLower()] = weapon;
                        }
                    }
                    
                    Log($"📦 Loaded {weaponCache.Count} weapons from cache");
                    Log($"📅 Cache date: {config.lastUpdated}");
                }
            }
            else
            {
                Log("📭 No local cache found");
                LoadDefaultConfig();
            }
        }
        catch (Exception e)
        {
            LogError($"❌ Failed to load cache: {e.Message}");
            LoadDefaultConfig();
        }
    }
    
    private void LoadDefaultConfig()
    {
        Log("📋 Loading default configuration");
        
        weaponCache.Clear();
        
        weaponCache["pistol"] = new Weapon { id = "pistol", damage = 10, cooldown = 0.5f };
        weaponCache["rifle"] = new Weapon { id = "rifle", damage = 25, cooldown = 0.2f };
        weaponCache["shotgun"] = new Weapon { id = "shotgun", damage = 40, cooldown = 1.0f };
        weaponCache["rocket"] = new Weapon { id = "rocket", damage = 100, cooldown = 3.0f };
        
        Log($"✅ Loaded {weaponCache.Count} default weapons");
    }
    
    #endregion
    
    #region Error Handling
    
    private void HandleError(string error)
    {
        LogError($"❌ {error}");
        onConfigError?.Invoke(error);
        
        if (weaponCache.Count == 0)
        {
            Log("⚠️ No cache available, loading defaults");
            LoadDefaultConfig();
        }
    }
    
    #endregion
    
    #region Logging
    
    private void Log(string message)
    {
        if (enableLogging)
            Debug.Log($"[RemoteConfig] {message}");
    }
    
    private void LogWarning(string message)
    {
        if (enableLogging)
            Debug.LogWarning($"[RemoteConfig] {message}");
    }
    
    private void LogError(string message)
    {
        if (enableLogging)
            Debug.LogError($"[RemoteConfig] {message}");
    }
    
    #endregion
    
    #region Editor Debug Methods
    
    [ContextMenu("Force Reload Config")]
    private void DebugReloadConfig()
    {
        ReloadConfig();
    }
    
    [ContextMenu("Clear Cache")]
    private void DebugClearCache()
    {
        if (File.Exists(localFilePath))
        {
            File.Delete(localFilePath);
            Log("🗑️ Cache deleted");
        }
    }
    
    [ContextMenu("Print Cache")]
    private void DebugPrintCache()
    {
        Log($"📊 Current cache: {weaponCache.Count} weapons");
        foreach (var weapon in weaponCache.Values)
        {
            Log($"   • {weapon}");
        }
    }
    
    #endregion
}