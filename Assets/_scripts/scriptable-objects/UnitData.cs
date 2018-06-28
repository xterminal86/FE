using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "GameData/Unit", order = 1)]
public class UnitData : ScriptableObject 
{  
  public string UnitName = "(not set)";
  public string UnitClass = "(not set)";

  [Range(0, 100)]
  public int Experience = 0;

  [Range(1, 100)]
  public int StartingLevel = 1;

  [Header("Stats")]
  [Range(0, 99)]
  public int Hitpoints = 0;

  [Range(0, 100)]
  public int HpGrowth = 0;

  [Range(0, 99)]
  public int Strength = 0;

  [Range(0, 100)]
  public int StrGrowth = 0;

  [Range(0, 99)]
  public int Skill = 0;

  [Range(0, 100)]
  public int SklGrowth = 0;

  [Range(0, 99)]
  public int Magic = 0;

  [Range(0, 100)]
  public int MagGrowth = 0;

  [Range(0, 99)]
  public int Speed = 0;

  [Range(0, 100)]
  public int SpdGrowth = 0;

  [Range(0, 99)]
  public int Defence = 0;

  [Range(0, 100)]
  public int DefGrowth = 0;

  [Range(0, 99)]
  public int Resistance = 0;

  [Range(0, 100)]
  public int ResGrowth = 0;

  [Range(0, 99)]
  public int Luck = 0;

  [Range(0, 100)]
  public int LckGrowth = 0;

  [Header("Autolevel")]
  public bool AutoLevelThisUnit = false;

  [Range(1, 100)]
  public int AutolevelStop = 1;

  // Runtime

  [HideInInspector]
  public int RuntimeLevel = 1;

  // Current, Maximum and Growth Rate
  [HideInInspector]
  public SerializableStat RuntimeHitpoints = new SerializableStat();

  [HideInInspector]
  public SerializableStat RuntimeStrength = new SerializableStat();

  [HideInInspector]
  public SerializableStat RuntimeSkill = new SerializableStat();

  [HideInInspector]
  public SerializableStat RuntimeMagic = new SerializableStat();

  [HideInInspector]
  public SerializableStat RuntimeSpeed = new SerializableStat();

  [HideInInspector]
  public SerializableStat RuntimeDefence = new SerializableStat();

  [HideInInspector]
  public SerializableStat RuntimeResistance = new SerializableStat();

  [HideInInspector]
  public SerializableStat RuntimeLuck = new SerializableStat();


  bool _isInitialized = false;
  public bool IsInitialized
  {
    get { return _isInitialized; }
  }

  string _details = string.Empty;
  public string Details
  {
    get { return _details; }
  }

  public void Initialize()
  {    
    // TODO: impement proper save / load in the future

    ClearRuntimeData();

    if (AutoLevelThisUnit)
    {
      while (RuntimeLevel < AutolevelStop)
      {
        LevelUp();
      }

      _details = string.Empty;

      _details += string.Format("Autolevelling this unit\n\n{1}\n{2}\n\nto level {0}\n\n", AutolevelStop, UnitName, UnitClass);

      _details += string.Format("HP: {0} ({1}%)\n", RuntimeHitpoints.CurrentValue, HpGrowth);
      _details += string.Format("LV: {0}\n\n", RuntimeLevel);

      _details += string.Format("STR: {0} ({1}%)\n", RuntimeStrength.CurrentValue, StrGrowth);
      _details += string.Format("DEF: {0} ({1}%)\n", RuntimeDefence.CurrentValue, DefGrowth);
      _details += string.Format("MAG: {0} ({1}%)\n", RuntimeMagic.CurrentValue, MagGrowth);
      _details += string.Format("SKL: {0} ({1}%)\n", RuntimeSkill.CurrentValue, SklGrowth);
      _details += string.Format("SPD: {0} ({1}%)\n", RuntimeSpeed.CurrentValue, SpdGrowth);
      _details += string.Format("RES: {0} ({1}%)\n", RuntimeResistance.CurrentValue, ResGrowth);
      _details += string.Format("LCK: {0} ({1}%)\n", RuntimeLuck.CurrentValue, LckGrowth);

      //Debug.Log(_details);
    }

    _isInitialized = true;
  }

  void ClearRuntimeData()
  {
    RuntimeLevel = StartingLevel;

    SetRuntimeData(RuntimeHitpoints, Hitpoints, HpGrowth);
    SetRuntimeData(RuntimeStrength, Strength, StrGrowth);
    SetRuntimeData(RuntimeSkill, Skill, SklGrowth);
    SetRuntimeData(RuntimeMagic, Magic, MagGrowth);
    SetRuntimeData(RuntimeSpeed, Speed, SpdGrowth);
    SetRuntimeData(RuntimeDefence, Defence, DefGrowth);
    SetRuntimeData(RuntimeResistance, Resistance, ResGrowth);
    SetRuntimeData(RuntimeLuck, Luck, LckGrowth);
  }

  void SetRuntimeData(SerializableStat stat, int initialValue, int growthRate)
  {
    stat.CurrentValue = initialValue;
    stat.MaxValue = initialValue;
    stat.GrowthRate = growthRate;
  }

  void LevelUp()
  {
    RollStat(RuntimeHitpoints);
    RollStat(RuntimeStrength);
    RollStat(RuntimeSkill);
    RollStat(RuntimeMagic);
    RollStat(RuntimeSpeed);
    RollStat(RuntimeDefence);
    RollStat(RuntimeResistance);
    RollStat(RuntimeLuck);

    RuntimeLevel++;
  }

  void RollStat(SerializableStat statToRoll)
  {
    int chance = Random.Range(1, 101);

    if (chance <= statToRoll.GrowthRate)
    {
      statToRoll.CurrentValue++;
      statToRoll.MaxValue = statToRoll.CurrentValue;
    }
  }
}

[System.Serializable]
public class SerializableStat
{
  public int CurrentValue = 0;
  public int MaxValue = 0;
  public int GrowthRate = 0;
};
