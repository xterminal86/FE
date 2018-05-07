using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public delegate void Callback();
public delegate void CallbackO(object sender);
public delegate void CallbackB(bool arg);
public delegate void CallbackC(Collider arg);

public static class GlobalConstants 
{  
  public const float CursorRepeatDelay = 0.05f;
  public const float CursorDelayBeforeRepeat = 0.15f;
}



