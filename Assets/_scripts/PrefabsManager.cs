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

  public GameObject FindPrefabByName(string name)
  {
    foreach (var item in Prefabs)
    {
      if (item.name == name)
      {
        return item;
      }
    }

    Debug.LogWarning("Could not find prefab " + name);

    return null;
  }

  public GameObject InstantiatePrefab(string prefabName, Vector3 pos, Quaternion q)
  {
    GameObject prefab = PrefabsManager.Instance.FindPrefabByName(prefabName);
    if (prefab != null)
    {
      return InstantiatePrefab(prefab, pos, q);
    }
    else
    {
      Debug.LogWarning("Could not find prefab " + prefabName);
      return null;
    }
  }

  public GameObject InstantiatePrefab(GameObject prefab, Vector3 pos, Quaternion q)
  {
    if (prefab == null)
    {
      Debug.LogWarning("Trying to instantiate null");
      return null;
    }

    return Instantiate(prefab, pos, q);
  }
}
