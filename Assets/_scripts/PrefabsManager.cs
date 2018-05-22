using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class for storing and finding all to be instantiated prefabs 
/// </summary>
public class PrefabsManager : MonoSingleton<PrefabsManager>
{
  public GameObject TileBasePrefab;

  public List<GameObject> PrefabsLayer1 = new List<GameObject>();
  public List<GameObject> PrefabsLayer2 = new List<GameObject>();

  protected override void Init()
  {
    base.Init();
  }


  /// <summary>
  /// Finds prefab by name and returns pair with index of this prefab and prefab itself.
  /// </summary>
  public KeyValuePair<KeyValuePair<int, int>, GameObject> FindPrefabByName(string name, int layer)
  {
    var listToSearch = (layer == 0) ? PrefabsLayer1 : PrefabsLayer2;

    int index = 0;
    foreach (var item in listToSearch)
    {
      if (item.name == name)
      {
        return new KeyValuePair<KeyValuePair<int, int>, GameObject>(new KeyValuePair<int, int>(layer, index), item);
      }

      index++;
    }

    Debug.LogWarning("Could not find prefab " + name);

    return new KeyValuePair<KeyValuePair<int, int>, GameObject>(new KeyValuePair<int, int>(layer, -1), null);
  }
}
