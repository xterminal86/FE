using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Pathfinding : MonoBehaviour 
{
  public string LevelName = string.Empty;

  void Awake()
  {
    Initializer.Instance.Initialize();

    LevelLoader.Instance.LoadMap(LevelName, () => 
    {
      var ms = LevelLoader.Instance.MapSize;
      CameraController.Instance.SetCameraPosition(ms.x / 2, ms.y / 2);
      CameraController.Instance.SetCursorPosition(ms.x / 2, ms.y / 2);
    });
  }
}
