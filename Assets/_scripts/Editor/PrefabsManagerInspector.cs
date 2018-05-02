using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(PrefabsManager))]
public class PrefabsManagerInspector : Editor 
{
  string _prefabsList = string.Empty;

  public override void OnInspectorGUI()
  {
    PrefabsManager pm = target as PrefabsManager;

    if (pm == null) return;
        
    string searchPath = "Assets/_prefabs/to-instantiate";

    if (GUILayout.Button("Generate Prefabs List"))
    {
      pm.Prefabs.Clear();
      
      string[] dirs = Directory.GetDirectories(searchPath, "*", SearchOption.AllDirectories);
      if (dirs.Length == 0)
      {
        LoadPrefabs(pm.Prefabs, searchPath);
      }
      else
      {
        for (int i = 0; i < dirs.Length; i++)
        {
          LoadPrefabs(pm.Prefabs, dirs[i]);
        }
      }
    }

    if (pm.Prefabs.Count != 0)
    {
      _prefabsList = string.Empty;

      int counter = 0;
      foreach (var item in pm.Prefabs)
      {
        if (item != null)
        {
          _prefabsList += string.Format("{0}: {1}\n", counter, item.name);
          counter++;
        }
      }

      EditorGUILayout.HelpBox(_prefabsList, MessageType.None);
    }

    if (GUI.changed)
    {
      EditorUtility.SetDirty(pm);
      AssetDatabase.SaveAssets();
    }
  }

  void LoadPrefabs(List<GameObject> listToAdd, string path)
  {
    string[] array = Directory.GetFiles(path, "*.prefab");
    
    for (int j = 0; j < array.Length; j++)
    {
      GameObject o = AssetDatabase.LoadAssetAtPath(array[j], typeof(GameObject)) as GameObject;
      listToAdd.Add(o);
    }
  }
}
