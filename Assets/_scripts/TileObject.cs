using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour 
{
  public List<SpriteRenderer> Sprites;

  public string InGameDescription;

  public int DefenceModifier = 0;
  public int EvasionModifier = 0;

  [HideInInspector]
  public int MovementDifficulty = 0;

  public void FlipX()
  {
    foreach (var item in Sprites)
    {
      item.flipX = !item.flipX;
    }
  }

  public void FlipY()
  {
    foreach (var item in Sprites)
    {
      item.flipY = !item.flipY;
    }
  }
}
