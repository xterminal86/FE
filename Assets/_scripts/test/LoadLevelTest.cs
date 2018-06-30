using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevelTest : MonoBehaviour 
{
  public string LevelName = string.Empty;

  void Awake()
  {
    PrefabsManager.Instance.Initialize();
    GUIManager.Instance.Initialize();
    CameraController.Instance.Initialize();
    LevelLoader.Instance.Initialize();

    LevelLoader.Instance.LoadMap(LevelName, () => 
    {
      var ms = LevelLoader.Instance.MapSize;
      CameraController.Instance.SetCameraPosition(ms.x / 2, ms.y / 2);
      CameraController.Instance.SetCursorPosition(ms.x / 2, ms.y / 2);
    });

    /*
    Util.MeasureTime(() =>
    {
      var map = LevelLoader.Instance.Map;
      var ms = LevelLoader.Instance.MapSize;

      Pathfinder pf = new Pathfinder(map, ms.x, ms.y);
      pf.BuildRoad(new Vector2Int(0, 0), new Vector2Int(ms.x - 1, ms.y - 1), true);
    });
    */
  }
}
