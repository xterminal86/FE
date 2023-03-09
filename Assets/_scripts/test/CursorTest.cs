using System;
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
    KeyCode.UpArrow,
    KeyCode.LeftArrow,
    KeyCode.DownArrow,
    KeyCode.RightArrow
  };

  Dictionary<KeyCode, int> _keyMaskByCode = new Dictionary<KeyCode, int>()
  {
    { KeyCode.LeftArrow,  0x01 },
    { KeyCode.RightArrow, 0x02 },
    { KeyCode.UpArrow,    0x04 },
    { KeyCode.DownArrow,  0x08 }
  };

  event Action<int> _cursorEvent;
  void HandleCursorEvent(int mask)
  {
    CursorSoundStart.Play();
  }

  void Awake()
  {
    foreach (var item in _keysToQuery)
    {
      _keyStatusByCode[item] = false;
    }

    _cursorEvent += HandleCursorEvent;
  }

  bool _isRepeating = false;

  int _prevKeysMask = -1;
  int _keysMask = 0;

  float _timer = 0.0f;
  float _checkValue = 0.0f;

  void CheckKeys()
  {
    foreach (var item in _keysToQuery)
    {
      _keyStatusByCode[item] = Input.GetKey(item);
    }

    _keysMask = 0;

    foreach (var kvp in _keyStatusByCode)
    {
      if (kvp.Value == true)
      {
        _keysMask |= _keyMaskByCode[kvp.Key];
      }
    }

    if (_keysMask != 0)
    {
      if (_keysMask != _prevKeysMask)
      {
        _timer = 0.0f;
        _isRepeating = false;
        _prevKeysMask = _keysMask;
        _cursorEvent?.Invoke(_keysMask);
      }

      _checkValue = _isRepeating ? GlobalConstants.CursorRepeatDelay : GlobalConstants.CursorDelayBeforeRepeat;

      if (_timer > _checkValue)
      {
        if (!_isRepeating)
        {
          _isRepeating = true;
        }

        _timer = 0.0f;
        _cursorEvent?.Invoke(_keysMask);
      }

      _timer += Time.unscaledDeltaTime;
    }
    else
    {
      _isRepeating = false;
      _timer = 0.0f;
      _prevKeysMask = -1;
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

    _infoText += string.Format("Mask = {0:X2}", _keysMask);

    InfoText.text = _infoText;
  }

  Vector3 _cursorPosition = Vector3.zero;
  void Update()
  {
    CheckKeys();

    DisplayDebugInfo();
  }
}
