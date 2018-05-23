using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedTile
{
  public SerializedTileObject TileLayer1;
  public SerializedTileObject TileLayer2;
}

[Serializable]
public class SerializedTileObject
{
  public int CoordX = 0;
  public int CoordY = 0;

  public string PrefabName = string.Empty;
  public string InGameDescription = string.Empty;

  public int DefenceModifier = 0;
  public int EvasionModifier = 0;

  public bool FlipFlagX = false;
  public bool FlipFlagY = false;

  public int MovementDifficulty = -1;
}