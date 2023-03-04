using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CursorTest : MonoBehaviour 
{
  public Transform Cursor;
  
  public AudioSource CursorSoundStart;
  public AudioSource CursorSoundFinish;

  public TMP_Text InfoText;
  
  Dictionary<KeyCode, bool> _keyStatusByCode = new Dictionary<KeyCode, bool>();
  List<KeyCode> _keysToQuery = new List<KeyCode>()
  {
    KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.RightArrow
  };
  
  void Awake()
  {
    foreach (var item in _keysToQuery) 
    {
      _keyStatusByCode[item] = false;
    }
  }
  
  void CheckKeys()
  {
    foreach (var item in _keysToQuery)
    {      
      _keyStatusByCode[item] = Input.GetKey(item);
    }
  }
  
  string _infoText = string.Empty;
  void DisplayDebugInfo()
  {
    _infoText = string.Empty;
    foreach (var item in _keyStatusByCode)
    {
      _infoText += string.Format("{0} - {1}\n", item.Key, item.Value);
    }
    
    InfoText.text = _infoText;
  }
  
  Vector3 _cursorPosition = Vector3.zero;
  void Update()
  {
    CheckKeys();
        
    DisplayDebugInfo();
  }
}
  