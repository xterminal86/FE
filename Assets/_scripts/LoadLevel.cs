using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

  Dictionary<KeyCode, bool> _keyHoldStatuses = new Dictionary<KeyCode, bool>();
  void Awake()
  {    
    //string path = "D:\\Nick\\Sources\\Unity\\FE.git\\level.bytes";
    string path = "level.bytes";

    LoadMap(path);

    _cameraMovement.Set(_mapSizeX / 2, _mapSizeY / 2, MainCamera.transform.position.z);
    _cursorPosition.Set((int)_mapSizeX / 2, (int)_mapSizeY / 2, -2.0f);

    MainCamera.transform.position = _cameraMovement;
    Cursor.position = _cursorPosition;

    _keyHoldStatuses[KeyCode.LeftArrow] = false;
    _keyHoldStatuses[KeyCode.RightArrow] = false;
    _keyHoldStatuses[KeyCode.UpArrow] = false;
    _keyHoldStatuses[KeyCode.DownArrow] = false;
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

  bool _working = false;
  void MoveCursor(int incX, int incY)
  {
    if (!_working)
    {
      _working = true;
      StartCoroutine(MoveCursorRoutine(incX, incY));
    }

    /*
    _cursorPosition.x += incX;
    _cursorPosition.y += incY;

    CheckCursorBounds();
    */
  }

  float _cursorSlideSpeed = 0.0f;

  IEnumerator MoveCursorRoutine(int incX, int incY)
  {
    _cursorSlideSpeed = _cursorTurboMode ? GlobalConstants.CursorSlideSpeed * 2.0f : GlobalConstants.CursorSlideSpeed;

    float camX = MainCamera.transform.position.x;
    float camY = MainCamera.transform.position.y;

    float newCamX = MainCamera.transform.position.x + incX;
    float newCamY = MainCamera.transform.position.y + incY;

    float x = _cursorPosition.x;
    float y = _cursorPosition.y;

    float oldX = x;
    float oldY = y;

    float newX = _cursorPosition.x + incX;
    float newY = _cursorPosition.y + incY;

    int dx = (int)camX - (int)newX;
    int dy = (int)camY - (int)newY;

    if (incX != 0 && incY != 0)
    {
      
    }
    else
    {
      if (incX != 0)
      {      
        bool condX = (incX > 0) ? (x < newX) : (x > newX);

        int signX = (incX < 0) ? -1 : 1;

        while (condX)
        {
          condX = (incX > 0) ? (x < newX) : (x > newX);

          x += signX * Time.deltaTime * _cursorSlideSpeed;

          if (dx > GlobalConstants.EdgeScrollX)
          {
            _cameraMovement.x -= Time.deltaTime * _cursorSlideSpeed;
          }
          else if (dx < -GlobalConstants.EdgeScrollX)
          {
            _cameraMovement.x += Time.deltaTime * _cursorSlideSpeed;
          }

          if (incX > 0)
          {
            x = Mathf.Clamp(x, oldX, newX);
            _cameraMovement.x = Mathf.Clamp(_cameraMovement.x, camX, newCamX);
          }
          else
          {
            x = Mathf.Clamp(x, newX, oldX);
            _cameraMovement.x = Mathf.Clamp(_cameraMovement.x, newCamX, camX);
          }

          _cursorPosition.x = x;
          Cursor.transform.position = _cursorPosition;
          MainCamera.transform.position = _cameraMovement;

          yield return null;
        }

        x = newX;
        _cursorPosition.x = x;
        Cursor.transform.position = _cursorPosition;

        CursorSound.Play();
      }
  

      if (incY != 0)
      {
        bool condY = (incY > 0) ? (y < newY) : (y > newY);

        int signY = (incY < 0) ? -1 : 1;

        while (condY)
        {
          condY = (incY > 0) ? (y < newY) : (y > newY);

          y += signY * Time.deltaTime * _cursorSlideSpeed;

          if (dy > GlobalConstants.EdgeScrollY)
          {
            _cameraMovement.y -= Time.deltaTime * _cursorSlideSpeed;
          }
          else if (dy < -GlobalConstants.EdgeScrollY)
          {
            _cameraMovement.y += Time.deltaTime * _cursorSlideSpeed;
          }

          if (incY > 0)
          {
            y = Mathf.Clamp(y, oldY, newY);
            _cameraMovement.y = Mathf.Clamp(_cameraMovement.y, camY, newCamY);
          }
          else
          {
            y = Mathf.Clamp(y, newY, oldY);
            _cameraMovement.y = Mathf.Clamp(_cameraMovement.y, newCamY, camY);
          }

          _cursorPosition.y = y;
          Cursor.transform.position = _cursorPosition;
          MainCamera.transform.position = _cameraMovement;

          yield return null;
        }

        y = newY;
        _cursorPosition.y = y;
        Cursor.transform.position = _cursorPosition;

        CursorSound.Play();
      }
    }

    _working = false;

    yield return null;
  }

  void HandleKeyDown()
  {
    if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
      _repeatTimer = 0.0f;
      MoveCursor(-1, 0);
      _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
    }
    else if (Input.GetKeyDown(KeyCode.RightArrow))
    {
      _repeatTimer = 0.0f;
      MoveCursor(1, 0);
      _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
    }
    else if (Input.GetKeyDown(KeyCode.UpArrow))
    {
      _repeatTimer = 0.0f;
      MoveCursor(0, 1);
      _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
    }
    else if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      _repeatTimer = 0.0f;
      MoveCursor(0, -1);
      _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
    }
  }

  float _repeatTimer = 0.0f;
  float _delayValue = 0.0f;
  void HandleKeyRepeat()
  {
    if (_keyHoldStatuses[KeyCode.LeftArrow] && _keyHoldStatuses[KeyCode.UpArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        //_delayValue = GlobalConstants.CursorRepeatDelay;
        MoveCursor(-1, 1);
        //_repeatTimer = 0.0f;
      }
    }
    else if (_keyHoldStatuses[KeyCode.RightArrow] && _keyHoldStatuses[KeyCode.UpArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        //_delayValue = GlobalConstants.CursorRepeatDelay;
        MoveCursor(1, 1);
        //_repeatTimer = 0.0f;
      }
    }
    else if (_keyHoldStatuses[KeyCode.LeftArrow] && _keyHoldStatuses[KeyCode.DownArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        //_delayValue = GlobalConstants.CursorRepeatDelay;
        MoveCursor(-1, -1);
        //_repeatTimer = 0.0f;
      }
    }
    else if (_keyHoldStatuses[KeyCode.RightArrow] && _keyHoldStatuses[KeyCode.DownArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        //_delayValue = GlobalConstants.CursorRepeatDelay;
        MoveCursor(1, -1);
        //_repeatTimer = 0.0f;
      }
    }
    else if (_keyHoldStatuses[KeyCode.LeftArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        //_delayValue = GlobalConstants.CursorRepeatDelay;
        MoveCursor(-1, 0);
        //_repeatTimer = 0.0f;
      }
    }
    else if (_keyHoldStatuses[KeyCode.RightArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        //_delayValue = GlobalConstants.CursorRepeatDelay;
        MoveCursor(1, 0);
        //_repeatTimer = 0.0f;
      }
    }
    else if (_keyHoldStatuses[KeyCode.UpArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        //_delayValue = GlobalConstants.CursorRepeatDelay;
        MoveCursor(0, 1);
        //_repeatTimer = 0.0f;
      }
    }
    else if (_keyHoldStatuses[KeyCode.DownArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        //_delayValue = GlobalConstants.CursorRepeatDelay;
        MoveCursor(0, -1);
        //_repeatTimer = 0.0f;
      }
    }
  }

  Vector3 _cameraScrollPos = Vector3.zero;
  void ControlCamera()
  {    
    int cursorX = (int)_cursorPosition.x;
    int cursorY = (int)_cursorPosition.y;
    int camX = (int)MainCamera.transform.position.x;
    int camY = (int)MainCamera.transform.position.y;

    float scrollSpeed = _cursorTurboMode ? GlobalConstants.CameraEdgeScrollSpeed * 2.0f : GlobalConstants.CameraEdgeScrollSpeed;

    if (camX - cursorX > GlobalConstants.EdgeScrollX && camY - cursorY < -GlobalConstants.EdgeScrollY)
    {
      _cameraScrollPos.Set(camX - 1, camY + 1, MainCamera.transform.position.z);
      Vector3 res = Vector3.MoveTowards(_cameraMovement, _cameraScrollPos, Time.smoothDeltaTime * scrollSpeed);
      _cameraMovement = res;
      MainCamera.transform.position = _cameraMovement;
    }
    else if (camX - cursorX < -GlobalConstants.EdgeScrollX && camY - cursorY < -GlobalConstants.EdgeScrollY)
    {
      _cameraScrollPos.Set(camX + 1, camY + 1, MainCamera.transform.position.z);
      Vector3 res = Vector3.MoveTowards(_cameraMovement, _cameraScrollPos, Time.smoothDeltaTime * scrollSpeed);
      _cameraMovement = res;
      MainCamera.transform.position = _cameraMovement;
    }
    else if (camX - cursorX > GlobalConstants.EdgeScrollX && camY - cursorY > GlobalConstants.EdgeScrollY)
    {
      _cameraScrollPos.Set(camX - 1, camY - 1, MainCamera.transform.position.z);
      Vector3 res = Vector3.MoveTowards(_cameraMovement, _cameraScrollPos, Time.smoothDeltaTime * scrollSpeed);
      _cameraMovement = res;
      MainCamera.transform.position = _cameraMovement;
    }
    else if (camX - cursorX < -GlobalConstants.EdgeScrollX && camY - cursorY > GlobalConstants.EdgeScrollY)
    {
      _cameraScrollPos.Set(camX + 1, camY - 1, MainCamera.transform.position.z);
      Vector3 res = Vector3.MoveTowards(_cameraMovement, _cameraScrollPos, Time.smoothDeltaTime * scrollSpeed);
      _cameraMovement = res;
      MainCamera.transform.position = _cameraMovement;
    }
    else if (camX - cursorX > GlobalConstants.EdgeScrollX)
    {
      _cameraScrollPos.Set(camX - 1, camY, MainCamera.transform.position.z);
      Vector3 res = Vector3.MoveTowards(_cameraMovement, _cameraScrollPos, Time.smoothDeltaTime * scrollSpeed);
      _cameraMovement = res;
      MainCamera.transform.position = _cameraMovement;
    }
    else if (camX - cursorX < -GlobalConstants.EdgeScrollX)
    {
      _cameraScrollPos.Set(camX + 1, camY, MainCamera.transform.position.z);
      Vector3 res = Vector3.MoveTowards(_cameraMovement, _cameraScrollPos, Time.smoothDeltaTime * scrollSpeed);
      _cameraMovement = res;
      MainCamera.transform.position = _cameraMovement;
    }
    else if (camY - cursorY < -GlobalConstants.EdgeScrollY)
    {
      _cameraScrollPos.Set(camX, camY + 1, MainCamera.transform.position.z);
      Vector3 res = Vector3.MoveTowards(_cameraMovement, _cameraScrollPos, Time.smoothDeltaTime * scrollSpeed);
      _cameraMovement = res;
      MainCamera.transform.position = _cameraMovement;
    }
    else if (camY - cursorY > GlobalConstants.EdgeScrollY)
    {
      _cameraScrollPos.Set(camX, camY - 1, MainCamera.transform.position.z);
      Vector3 res = Vector3.MoveTowards(_cameraMovement, _cameraScrollPos, Time.smoothDeltaTime * scrollSpeed);
      _cameraMovement = res;
      MainCamera.transform.position = _cameraMovement;
    }
  }

  bool _cursorTurboMode = false;
  void QueryInput()
  {
    _keyHoldStatuses[KeyCode.LeftArrow] = Input.GetKey(KeyCode.LeftArrow);
    _keyHoldStatuses[KeyCode.RightArrow] = Input.GetKey(KeyCode.RightArrow);
    _keyHoldStatuses[KeyCode.UpArrow] = Input.GetKey(KeyCode.UpArrow);
    _keyHoldStatuses[KeyCode.DownArrow] = Input.GetKey(KeyCode.DownArrow);

    _cursorTurboMode = Input.GetKey(KeyCode.Z);

    if (_cursorTurboMode)
    {
      _delayValue = GlobalConstants.CursorRepeatDelay * 0.1f;
    }
  }

  void Update()
  {   
    QueryInput();

    if (!_cursorTurboMode)
    {
      HandleKeyDown();
    }

    HandleKeyRepeat();
    //ControlCamera();
  }

  void CheckCursorBounds()
  {
    _cursorPosition.x = Mathf.Clamp(_cursorPosition.x, 0, _mapSizeX - 1);
    _cursorPosition.y = Mathf.Clamp(_cursorPosition.y, 0, _mapSizeY - 1);

    if ((int)_cursorPosition.x != (int)Cursor.position.x 
     || (int)_cursorPosition.y != (int)Cursor.position.y)
    {
      CursorSound.Play();
    }

    Cursor.position = _cursorPosition;
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
