using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour 
{
  public UnitData UnitData_;
  public WeaponData WeaponData_;

  void Awake()
  {
    UnitData_.Initialize();
    WeaponData_.Initialize();
  }
}
