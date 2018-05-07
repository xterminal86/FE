using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class for storing and finding all to be instantiated prefabs 
/// </summary>
public class PrefabsManager : MonoSingleton<PrefabsManager>
{
  public List<GameObject> Prefabs = new List<GameObject>();

  protected override void Init()
  {
    base.Init();
  }


  /// <summary>
  /// Finds prefab by name and returns pair with index of this prefab and prefab itself.
  /// </summary>
  public KeyValuePair<int, GameObject> FindPrefabByName(string name)
  {
    int index = 0;
    foreach (var item in Prefabs)
    {
      if (item.name == name)
      {
        return new KeyValuePair<int, GameObject>(index, item);
      }

      index++;
    }

    Debug.LogWarning("Could not find prefab " + name);

    return new KeyValuePair<int, GameObject>(-1, null);
  }
}
