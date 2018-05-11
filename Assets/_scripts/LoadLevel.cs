using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadLevel : MonoBehaviour 
{
  public AudioSource CursorSound;
  public Camera MainCamera;
  public Transform MapHolder;
  public Transform Cursor;
  public Image TileInfoSprite;
  public TMP_Text TileName;
  public TMP_Text TileDetails;

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

    UpdateTileInfo();
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

    var boundsCheckRes = CheckBounds((int)newX, (int)newY);

    if (boundsCheckRes.x != 0)
    {
      incX = 0;
    }

    if (boundsCheckRes.y != 0)
    {
      incY = 0;
    }

    int dx = (int)camX - (int)newX;
    int dy = (int)camY - (int)newY;

    if (incX != 0 && incY != 0)
    {
      // Diagonal cursor movement

      bool condX = (incX > 0) ? (x < newX) : (x > newX);
      bool condY = (incY > 0) ? (y < newY) : (y > newY);

      int signX = (incX < 0) ? -1 : 1;
      int signY = (incY < 0) ? -1 : 1;

      while (condX && condY)
      {
        condX = (incX > 0) ? (x < newX) : (x > newX);
        condY = (incY > 0) ? (y < newY) : (y > newY);

        x += signX * Time.deltaTime * _cursorSlideSpeed;
        y += signY * Time.deltaTime * _cursorSlideSpeed;

        if (dx > GlobalConstants.EdgeScrollX)
        {
          _cameraMovement.x -= Time.deltaTime * _cursorSlideSpeed;
        }
        else if (dx < -GlobalConstants.EdgeScrollX)
        {
          _cameraMovement.x += Time.deltaTime * _cursorSlideSpeed;
        }

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
        _cursorPosition.y = y;

        Cursor.transform.position = _cursorPosition;
        MainCamera.transform.position = _cameraMovement;

        yield return null;
      }

      x = newX;
      y = newY;

      _cursorPosition.x = x;
      _cursorPosition.y = y;

      Cursor.transform.position = _cursorPosition;

      CursorSound.Play();
    }
    else
    {
      // Horizontal and vertical cursor movement

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

    UpdateTileInfo();

    _working = false;

    yield return null;
  }

  void HandleKeyDown()
  {    
    if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
      MoveCursor(-1, 0);
      _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
    }
    else if (Input.GetKeyDown(KeyCode.RightArrow))
    {
      MoveCursor(1, 0);
      _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
    }
    else if (Input.GetKeyDown(KeyCode.UpArrow))
    {
      MoveCursor(0, 1);
      _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
    }
    else if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      MoveCursor(0, -1);
      _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
    }
  }

  void HandleKeyUp()
  {
    if (Input.GetKeyUp(KeyCode.LeftArrow))
    {
      _repeatTimer = 0.0f;
    }
    else if (Input.GetKeyUp(KeyCode.RightArrow))
    {
      _repeatTimer = 0.0f;
    }
    else if (Input.GetKeyUp(KeyCode.UpArrow))
    {
      _repeatTimer = 0.0f;
    }
    else if (Input.GetKeyUp(KeyCode.DownArrow))
    {
      _repeatTimer = 0.0f;
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
        MoveCursor(-1, 1);
      }
    }
    else if (_keyHoldStatuses[KeyCode.RightArrow] && _keyHoldStatuses[KeyCode.UpArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        MoveCursor(1, 1);
      }
    }
    else if (_keyHoldStatuses[KeyCode.LeftArrow] && _keyHoldStatuses[KeyCode.DownArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        MoveCursor(-1, -1);
      }
    }
    else if (_keyHoldStatuses[KeyCode.RightArrow] && _keyHoldStatuses[KeyCode.DownArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        MoveCursor(1, -1);
      }
    }
    else if (_keyHoldStatuses[KeyCode.LeftArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        MoveCursor(-1, 0);
      }
    }
    else if (_keyHoldStatuses[KeyCode.RightArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        MoveCursor(1, 0);
      }
    }
    else if (_keyHoldStatuses[KeyCode.UpArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        MoveCursor(0, 1);
      }
    }
    else if (_keyHoldStatuses[KeyCode.DownArrow])
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {
        MoveCursor(0, -1);
      }
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
      _delayValue = 0.0f;
    }
  }

  void Update()
  {   
    QueryInput();

    if (!_cursorTurboMode)
    {
      HandleKeyDown();
      HandleKeyUp();
    }

    HandleKeyRepeat();
  }

  Vector2Int _checkBoundsResult = Vector2Int.zero;
  Vector2Int CheckBounds(int x, int y)
  {
    int resX = (x < 0 || x > _mapSizeX - 1) ? 1 : 0;
    int resY = (y < 0 || y > _mapSizeY - 1) ? 1 : 0;

    _checkBoundsResult.Set(resX, resY);

    return _checkBoundsResult;
  }

  Vector3 _tileInfoScale = Vector3.zero;
  void UpdateTileInfo()
  {
    int mx = (int)Cursor.transform.position.x;
    int my = (int)Cursor.transform.position.y;

    bool flipX = _map[mx, my].Sprites[_map[mx, my].Sprites.Count - 1].flipX;
    bool flipY = _map[mx, my].Sprites[_map[mx, my].Sprites.Count - 1].flipY;

    _tileInfoScale.Set(flipX ? -1.0f : 1.0f, flipY ? -1.0f : 1.0f, 1.0f);

    TileInfoSprite.sprite = _map[mx, my].Sprites[_map[mx, my].Sprites.Count - 1].sprite;
    TileInfoSprite.rectTransform.localScale = _tileInfoScale;
    TileDetails.text = string.Format("D:{0} E:{1}", _map[mx, my].DefenceModifier, _map[mx, my].EvasionModifier);
    TileName.text = _map[mx, my].InGameDescription;
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
