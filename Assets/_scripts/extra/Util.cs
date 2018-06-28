using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public static class Util
{
  public static int BlockDistance(Vector2Int point1, Vector2Int point2)
  {
    int cost = ( Mathf.Abs(point1.y - point2.y) + Mathf.Abs(point1.x - point2.x) );

    //Debug.Log(string.Format("Manhattan distance remaining from {0} to {1}: {2}", point.ToString(), end.ToString(), cost));

    return cost;
  }

  public static void SetGameObjectLayer(GameObject go, int layer, bool recursive = false)
  {
    go.layer = layer;
   
    if (recursive)
    {
      foreach (Transform t in go.transform)
      {        
        SetGameObjectLayer(t.gameObject, layer, recursive);
      }
    }
  }

  public static TimeSpan MeasureTime(Callback cb)
  {
    Stopwatch timer = Stopwatch.StartNew();

    if (cb != null)
      cb();

    timer.Stop();

    UnityEngine.Debug.Log("[" + cb.Method.Name + "] => " + timer.Elapsed);

    return timer.Elapsed;
  }
}

