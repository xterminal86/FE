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
        
    string searchPathLayer1 = "Assets/_prefabs/ground-tiles";
    string searchPathLayer2 = "Assets/_prefabs/objects";

    pm.TileBasePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Base Prefab", pm.TileBasePrefab, typeof(GameObject), false);

    if (GUILayout.Button("Generate Prefabs List"))
    {
      LoadPrefabs(pm.PrefabsLayer1, searchPathLayer1);
      LoadPrefabs(pm.PrefabsLayer2, searchPathLayer2);
    }

    EditorGUILayout.HelpBox(searchPathLayer1, MessageType.Info);
    PrintPrefabsList(pm.PrefabsLayer1);
    EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
    EditorGUILayout.HelpBox(searchPathLayer2, MessageType.Info);
    PrintPrefabsList(pm.PrefabsLayer2);

    if (GUI.changed)
    {
      EditorUtility.SetDirty(pm);
      AssetDatabase.SaveAssets();
    }
  }

  void PrintPrefabsList(List<GameObject> listToPrint)
  {
    if (listToPrint.Count != 0)
    {
      _prefabsList = string.Empty;

      int counter = 0;
      foreach (var item in listToPrint)
      {
        if (item != null)
        {
          _prefabsList += string.Format("{0}: {1}\n", counter, item.name);
          counter++;
        }
      }

      EditorGUILayout.HelpBox(_prefabsList, MessageType.None);
    }
  }

  void LoadPrefabs(List<GameObject> listToAdd, string path)
  {
    listToAdd.Clear();

    string[] array = Directory.GetFiles(path, "*.prefab");
    
    for (int j = 0; j < array.Length; j++)
    {
      GameObject o = AssetDatabase.LoadAssetAtPath(array[j], typeof(GameObject)) as GameObject;
      listToAdd.Add(o);
    }
  }
}
