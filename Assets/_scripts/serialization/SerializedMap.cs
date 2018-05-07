using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedMap
{
  public int MapSizeX = 0;
  public int MapSizeY = 0;

  public string Path = string.Empty;

  public List<SerializedTile> MapTiles = new List<SerializedTile>();
}
