using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoSingleton<LevelLoader>
{  
  SerializedMap _level;

  int _mapSizeX = 0;
  int _mapSizeY = 0;

  public Vector2Int MapSize
  {
    get { return new Vector2Int(_mapSizeX, _mapSizeY); }
  }

  TileBase[,] _map;
  public TileBase[,] Map
  {
    get { return _map; }
  }

  public void LoadMap(string levelName, Callback onLoadingFinished = null)
  {    
    LoadMap(levelName);    

    if (onLoadingFinished != null)
    {
      onLoadingFinished();
    }
  }

  void LoadMap(string path)
  {
    var formatter = new BinaryFormatter();  
    Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);  
    _level = (SerializedMap)formatter.Deserialize(stream);  
    stream.Close();

    _mapSizeX = _level.MapSizeX;
    _mapSizeY = _level.MapSizeY;

    _map = new TileBase[_mapSizeX, _mapSizeY];

    GameObject mapHolder = new GameObject("map-holder");

    foreach (var tile in _level.MapTiles)
    {
      var layer1 = tile.TileLayer1;

      int x = layer1.CoordX;
      int y = layer1.CoordY;

      var go = Instantiate(PrefabsManager.Instance.TileBasePrefab, new Vector3(x, y, 0.0f), Quaternion.identity, mapHolder.transform);

      TileBase tb = go.GetComponent<TileBase>();

      _map[x, y] = tb;

      InstantiateTileObject(tb, tile.TileLayer1, 0);

      if (tile.TileLayer2 != null)
      {
        InstantiateTileObject(tb, tile.TileLayer2, 1);
      }
    }
  }

  void InstantiateTileObject(TileBase tb, SerializedTileObject sto, int layer)
  {
    string prefabName = sto.PrefabName;

    var res = PrefabsManager.Instance.FindPrefabByName(prefabName, layer);
    if (res.Value != null)
    {
      float zDepth = (layer == 0) ? sto.CoordY : sto.CoordY - 1;
      Vector3 pos = new Vector3(sto.CoordX, sto.CoordY, zDepth);
      var go = Instantiate(res.Value, pos, Quaternion.identity, tb.transform);
      TileObject to = go.GetComponent<TileObject>();
      to.PrefabName = prefabName;
      to.DefenceModifier = sto.DefenceModifier;
      to.EvasionModifier = sto.EvasionModifier;
      to.FlipFlagX = sto.FlipFlagX;
      to.FlipFlagY = sto.FlipFlagY;
      to.InGameDescription = sto.InGameDescription;
      to.MovementDifficulty = sto.MovementDifficulty;

      if (sto.FlipFlagX)
      {
        to.FlipX();
      }

      if (sto.FlipFlagY)
      {
        to.FlipY();
      }

      if (layer == 0)
      {
        tb.TileObjectLayer1 = to;
      }
      else if (layer == 1)
      {
        tb.TileObjectLayer2 = to;
      }
    }
  }
}
