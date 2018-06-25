using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectsTest : MonoBehaviour 
{
  public ScriptableObject Test;

  void Start()
  {
    Debug.Log(Test);
    Debug.Log((Test is UnitData));
  }    
}
