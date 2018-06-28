using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Pathfinding : MonoBehaviour 
{
  public TMP_Text Map;

  public int MapX = 10;
  public int MapY = 10;

  int[,] _map;

  void Awake()
  {
    _map = new int[MapX, MapY];

    RandomizeMap();
    DisplayMap();
  }

  void RandomizeMap()
  {
    for (int x = 1; x < MapX - 1; x++)
    {
      for (int y = 1; y < MapY - 1; y++)
      {
        int value = Random.Range(0, 101);
        if (value >= 90)
        {
          _map[x, y] = 1;
        }
        else
        {
          _map[x, y] = 0;
        }
      }
    }
  }

  void DisplayMap()
  {
    string mapString = string.Empty;

    for (int x = 0; x < MapX; x++)
    {
      for (int y = 0; y < MapY; y++)
      {
        mapString += (_map[x, y] == 0) ? '.' : '#';
      }

      mapString += '\n';
    }

    Map.text = mapString;
  }
}
