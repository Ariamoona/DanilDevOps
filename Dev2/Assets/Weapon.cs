using System;
using UnityEngine;

[Serializable]
public class Weapon
{
    public string id;
    public float damage;
    public float cooldown;
    
    public static Weapon GetDefaultWeapon(string weaponId)
    {
        switch (weaponId.ToLower())
        {
            case "pistol":
                return new Weapon { id = "pistol", damage = 10, cooldown = 0.5f };
            case "rifle":
                return new Weapon { id = "rifle", damage = 25, cooldown = 0.2f };
            case "shotgun":
                return new Weapon { id = "shotgun", damage = 40, cooldown = 1.0f };
            case "rocket":
                return new Weapon { id = "rocket", damage = 100, cooldown = 3.0f };
            default:
                return new Weapon { id = "default", damage = 5, cooldown = 1.0f };
        }
    }
    
    public override string ToString()
    {
        return $"[Weapon: {id}, Damage: {damage}, Cooldown: {cooldown}]";
    }
}

[Serializable]
public class WeaponData
{
    public Weapon[] weapons;
}

[Serializable]
public class RemoteConfig
{
    public Weapon[] weapons;
    public DateTime lastUpdated;
    public string source;
}