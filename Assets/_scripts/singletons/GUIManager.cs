using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GUIManager : MonoSingleton<GUIManager> 
{
  public TMP_Text CursorPositionText;

  public FormTileInfo TileInfoScript;

  public void SetCursorPosition(int x, int y)
  {
    CursorPositionText.text = string.Format("[{0}:{1}]", x, y);
  }
}
