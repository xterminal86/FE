using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour 
{
  public SpriteRenderer TileSprite;

  public string InGameDescription;

  public int DefenceModifier = 0;
  public int EvasionModifier = 0;

  // 0 means not traversable
  [Range(0, 3)]
  public int MovementDifficulty = 1;

  [HideInInspector]
  public string PrefabName = string.Empty;

  [HideInInspector]
  public bool FlipFlagX = false;

  [HideInInspector]
  public bool FlipFlagY = false;

  public void FlipX()
  {
    // We cannot put flag inversion in this method because it will also be called
    // during loading, so to prevent flip flags corruption we handle them separately

    TileSprite.flipX = !TileSprite.flipX;
  }

  public void FlipY()
  {
    TileSprite.flipY = !TileSprite.flipY;
  }
}
