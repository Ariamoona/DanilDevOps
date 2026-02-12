using UnityEngine;

public class Weap : MonoBehaviour
{
    public string weaponId = "pistol";
    public float damage = 10f;
    public float cooldown = 0.5f;

    void Start()
    {
        Debug.Log($"Weapon initialized: {weaponId}, DMG: {damage}, CD: {cooldown}");
    }
}