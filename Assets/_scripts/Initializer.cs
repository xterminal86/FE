using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initializer : MonoSingleton<Initializer> 
{
  void Awake()
  {
    PrefabsManager.Instance.Initialize();
    GUIManager.Instance.Initialize();
    CameraController.Instance.Initialize();
    LevelLoader.Instance.Initialize();
  }
}
