using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevel : MonoBehaviour 
{
  public AudioSource CursorSound;
  public Camera MainCamera;
  public Transform MapHolder;
  public Transform Cursor;

  TileObject[,] _map;

  SerializedMap _level;

  int _mapSizeX = 0;
  int _mapSizeY = 0;

  Vector3 _cameraMovement = Vector3.zero;
  Vector3 _cursorPosition = Vector3.zero;

  void Awake()
  {
    string path = "D:\\Nick\\Sources\\Unity\\FE.git\\level.bytes";

    LoadMap(path);

    _cameraMovement.Set(_mapSizeX / 2, _mapSizeY / 2, MainCamera.transform.position.z);
    _cursorPosition.Set((int)_mapSizeX / 2, (int)_mapSizeY / 2, -2.0f);

    MainCamera.transform.position = _cameraMovement;
    Cursor.position = _cursorPosition;

    _arrowKeysStatus[KeyCode.LeftArrow] = false;
    _arrowKeysStatus[KeyCode.RightArrow] = false;
    _arrowKeysStatus[KeyCode.UpArrow] = false;
    _arrowKeysStatus[KeyCode.DownArrow] = false;

    _delayBeforeRepeat[KeyCode.LeftArrow] = 0.0f;
    _delayBeforeRepeat[KeyCode.RightArrow] = 0.0f;
    _delayBeforeRepeat[KeyCode.UpArrow] = 0.0f;
    _delayBeforeRepeat[KeyCode.DownArrow] = 0.0f;

    _repeatTimers[KeyCode.LeftArrow] = 0.0f;
    _repeatTimers[KeyCode.RightArrow] = 0.0f;
    _repeatTimers[KeyCode.UpArrow] = 0.0f;
    _repeatTimers[KeyCode.DownArrow] = 0.0f;
  }

  void LoadMap(string path)
  {
    var formatter = new BinaryFormatter();  
    Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);  
    _level = (SerializedMap)formatter.Deserialize(stream);  
    stream.Close();

    _mapSizeX = _level.MapSizeX;
    _mapSizeY = _level.MapSizeY;

    _map = new TileObject[_mapSizeX, _mapSizeY];

    foreach (var tile in _level.MapTiles)
    {
      var res = PrefabsManager.Instance.FindPrefabByName(tile.PrefabName);
      if (res.Value != null)
      {
        var go = Instantiate(res.Value, new Vector3(tile.CoordX, tile.CoordY, tile.CoordY), Quaternion.identity, MapHolder);

        TileObject to = go.GetComponent<TileObject>();
        to.PrefabName = tile.PrefabName;
        to.IndexInPrefabsManager = res.Key;
        to.InGameDescription = tile.InGameDescription;
        to.DefenceModifier = tile.DefenceModifier;
        to.EvasionModifier = tile.EvasionModifier;
        to.MovementDifficulty = tile.MovementDifficulty;
        to.FlipFlagX = tile.FlipFlagX;
        to.FlipFlagY = tile.FlipFlagY;

        if (tile.FlipFlagX)
        {          
          to.FlipX();
        }

        if (tile.FlipFlagY)
        {
          to.FlipY();
        }

        int x = tile.CoordX;
        int y = tile.CoordY;

        _map[x, y] = to;
      }
    }
  }

  Dictionary<KeyCode, float> _delayBeforeRepeat = new Dictionary<KeyCode, float>();
  void CheckKeyDown(KeyCode keyCode, Callback cb = null)
  {
    if (Input.GetKeyDown(keyCode))
    {      
      _arrowKeysStatus[keyCode] = true;

      if (cb != null)
      {
        cb();
      }
    }
    else if (Input.GetKeyUp(keyCode))
    {
      _delayBeforeRepeat[keyCode] = 0.0f;
      _repeatTimers[keyCode] = 0.0f;
      _arrowKeysStatus[keyCode] = false;
    }
  }

  Dictionary<KeyCode, float> _repeatTimers = new Dictionary<KeyCode, float>();
  void ProcessRepeat(KeyCode keyCode, Callback cb = null)
  {
    if (_arrowKeysStatus[keyCode])
    {
      _delayBeforeRepeat[keyCode] += Time.smoothDeltaTime;
    }

    if (_delayBeforeRepeat[keyCode] > GlobalConstants.CursorDelayBeforeRepeat)
    {
      _repeatTimers[keyCode] += Time.smoothDeltaTime;

      if (_repeatTimers[keyCode] > GlobalConstants.CursorRepeatDelay)
      {
        _repeatTimers[keyCode] = 0.0f;

        if (cb != null)
        {
          cb();
        }
      }
    }
  }

  Dictionary<KeyCode, bool> _arrowKeysStatus = new Dictionary<KeyCode, bool>();
  void ControlCursor()
  {
    CheckKeyDown(KeyCode.LeftArrow, () => 
    {
      _cursorPosition.x--;
    });

    CheckKeyDown(KeyCode.RightArrow, () =>
    {
      _cursorPosition.x++;
    });
    CheckKeyDown(KeyCode.UpArrow, () =>
    {
      _cursorPosition.y++;
    });

    CheckKeyDown(KeyCode.DownArrow, () =>
    {
      _cursorPosition.y--;
    });



    ProcessRepeat(KeyCode.LeftArrow, () =>
    {
      _cursorPosition.x--;
    });

    ProcessRepeat(KeyCode.RightArrow, () =>
    {
      _cursorPosition.x++;
    });

    ProcessRepeat(KeyCode.UpArrow, () =>
    {
      _cursorPosition.y++;
    });

    ProcessRepeat(KeyCode.DownArrow, () =>
    {
      _cursorPosition.y--;
    });

    _cursorPosition.x = Mathf.Clamp(_cursorPosition.x, 0, _mapSizeX - 1);
    _cursorPosition.y = Mathf.Clamp(_cursorPosition.y, 0, _mapSizeY - 1);

    if ((int)_cursorPosition.x != (int)Cursor.position.x 
     || (int)_cursorPosition.y != (int)Cursor.position.y)
    {
      CursorSound.Play();
    }

    Cursor.position = _cursorPosition;
  }

  void Update()
  {    
    ControlCursor();
  }

  void DestroyChildren(Transform t)
  {
    int childCount = t.childCount;
    for (int i = 0; i < childCount; i++)
    {
      Destroy(t.GetChild(i).gameObject);
    }
  }
}
