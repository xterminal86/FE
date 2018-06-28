using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "GameData/Weapon", order = 1)]
public class WeaponData : ScriptableObject 
{
  public string Name = "(not set)";

  [Range(0, 100)]
  public int Might = 0;

  [Range(0, 100)]
  public int Hit = 0;

  [Range(0, 100)]
  public int Crit = 0;

  [Range(0, 100)]
  public int Weight = 0;

  // -1 means weapon is indestructible
  [Range(-1, 100)]
  public int Durability = 0;

  [HideInInspector]
  public SerializableWeaponData RuntimeWeaponData = new SerializableWeaponData();

  public void Initialize()
  {
    RuntimeWeaponData.Might = Might;
    RuntimeWeaponData.Hit = Hit;
    RuntimeWeaponData.Crit = Crit;
    RuntimeWeaponData.Weight = Weight;
    RuntimeWeaponData.Durability = Durability;
  }
}

[System.Serializable]
public class SerializableWeaponData
{  
  public int Might = 0;
  public int Hit = 0;
  public int Crit = 0;
  public int Weight = 0;
  public int Durability = 0;
};
