using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FormTileInfo : MonoBehaviour 
{
  public Image TileInfoImageLayer1;
  public Image TileInfoImageLayer2;
  public TMP_Text TileName;
  public TMP_Text TileDetails;

  Vector3 _tileInfoScale = Vector3.zero;
  public void UpdateTileInfo(int x, int y)
  {    
    var map = LevelLoader.Instance.Map;

    DisplayTileInfo(map[x, y].TileObjectLayer1, 0);

    if (map[x, y].TileObjectLayer2 != null)
    {
      TileInfoImageLayer2.gameObject.SetActive(true);
      DisplayTileInfo(map[x, y].TileObjectLayer2, 1);
    }
    else
    {
      TileInfoImageLayer2.gameObject.SetActive(false);
    }
  }

  void DisplayTileInfo(TileObject tileObject, int layer)
  {
    bool flipX = tileObject.FlipFlagX;
    bool flipY = tileObject.FlipFlagY;

    _tileInfoScale.Set(flipX ? -1.0f : 1.0f, flipY ? -1.0f : 1.0f, 1.0f);

    Image tileInfoImage = (layer == 0) ? TileInfoImageLayer1 : TileInfoImageLayer2;

    tileInfoImage.sprite = tileObject.TileSprite.sprite;
    tileInfoImage.rectTransform.localScale = _tileInfoScale;

    TileDetails.text = string.Format("D:{0} E:{1}", tileObject.DefenceModifier, tileObject.EvasionModifier);
    TileName.text = tileObject.InGameDescription;
  }
}
