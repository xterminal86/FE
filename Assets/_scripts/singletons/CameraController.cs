using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoSingleton<CameraController> 
{
  public GameObject CursorPrefab;

  Vector3 _cameraPosition = Vector3.zero;
  Vector3 _cursorPosition = Vector3.zero;

  Camera _camera;
  Transform _cursor;
  AudioSource _cursorSound;

  Dictionary<KeyCode, bool> _keyHoldStatuses = new Dictionary<KeyCode, bool>();

  public override void Initialize()
  {
    _camera = GetComponent<Camera>();
    _cursor = Instantiate(CursorPrefab).GetComponent<Transform>();
    _cursorSound = _cursor.GetComponentInChildren<AudioSource>();

    _keyHoldStatuses[KeyCode.LeftArrow] = false;
    _keyHoldStatuses[KeyCode.RightArrow] = false;
    _keyHoldStatuses[KeyCode.UpArrow] = false;
    _keyHoldStatuses[KeyCode.DownArrow] = false;
    
    _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
  }

  public void SetCameraPosition(int x, int y)
  {
    _cameraPosition.Set(x, y, _camera.transform.position.z);
    _camera.transform.position = _cameraPosition;
  }

  public void SetCursorPosition(int x, int y)
  {
    _cursorPosition.Set(x, y, _camera.transform.position.z + 1);
    _cursor.position = _cursorPosition;

    GUIManager.Instance.TileInfoScript.UpdateTileInfo(x, y);
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

    float camX = _camera.transform.position.x;
    float camY = _camera.transform.position.y;

    float newCamX = _camera.transform.position.x + incX;
    float newCamY = _camera.transform.position.y + incY;

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
          _cameraPosition.x -= Time.deltaTime * _cursorSlideSpeed;
        }
        else if (dx < -GlobalConstants.EdgeScrollX)
        {
          _cameraPosition.x += Time.deltaTime * _cursorSlideSpeed;
        }

        if (dy > GlobalConstants.EdgeScrollY)
        {
          _cameraPosition.y -= Time.deltaTime * _cursorSlideSpeed;
        }
        else if (dy < -GlobalConstants.EdgeScrollY)
        {
          _cameraPosition.y += Time.deltaTime * _cursorSlideSpeed;
        }

        if (incY > 0)
        {
          y = Mathf.Clamp(y, oldY, newY);
          _cameraPosition.y = Mathf.Clamp(_cameraPosition.y, camY, newCamY);
        }
        else
        {
          y = Mathf.Clamp(y, newY, oldY);
          _cameraPosition.y = Mathf.Clamp(_cameraPosition.y, newCamY, camY);
        }

        if (incX > 0)
        {
          x = Mathf.Clamp(x, oldX, newX);
          _cameraPosition.x = Mathf.Clamp(_cameraPosition.x, camX, newCamX);
        }
        else
        {
          x = Mathf.Clamp(x, newX, oldX);
          _cameraPosition.x = Mathf.Clamp(_cameraPosition.x, newCamX, camX);
        }

        _cursorPosition.x = x;
        _cursorPosition.y = y;

        _cursor.position = _cursorPosition;
        _camera.transform.position = _cameraPosition;

        yield return null;
      }

      x = newX;
      y = newY;

      _cursorPosition.x = x;
      _cursorPosition.y = y;

      _cursor.position = _cursorPosition;

      _cursorSound.Play();
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
            _cameraPosition.x -= Time.deltaTime * _cursorSlideSpeed;
          }
          else if (dx < -GlobalConstants.EdgeScrollX)
          {
            _cameraPosition.x += Time.deltaTime * _cursorSlideSpeed;
          }

          if (incX > 0)
          {
            x = Mathf.Clamp(x, oldX, newX);
            _cameraPosition.x = Mathf.Clamp(_cameraPosition.x, camX, newCamX);
          }
          else
          {
            x = Mathf.Clamp(x, newX, oldX);
            _cameraPosition.x = Mathf.Clamp(_cameraPosition.x, newCamX, camX);
          }

          _cursorPosition.x = x;
          _cursor.position = _cursorPosition;
          _camera.transform.position = _cameraPosition;

          yield return null;
        }

        x = newX;
        _cursorPosition.x = x;
        _cursor.position = _cursorPosition;

        _cursorSound.Play();
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
            _cameraPosition.y -= Time.deltaTime * _cursorSlideSpeed;
          }
          else if (dy < -GlobalConstants.EdgeScrollY)
          {
            _cameraPosition.y += Time.deltaTime * _cursorSlideSpeed;
          }

          if (incY > 0)
          {
            y = Mathf.Clamp(y, oldY, newY);
            _cameraPosition.y = Mathf.Clamp(_cameraPosition.y, camY, newCamY);
          }
          else
          {
            y = Mathf.Clamp(y, newY, oldY);
            _cameraPosition.y = Mathf.Clamp(_cameraPosition.y, newCamY, camY);
          }

          _cursorPosition.y = y;
          _cursor.position = _cursorPosition;
          _camera.transform.position = _cameraPosition;

          yield return null;
        }

        y = newY;
        _cursorPosition.y = y;
        _cursor.position = _cursorPosition;

        _cursorSound.Play();
      }
    }

    GUIManager.Instance.TileInfoScript.UpdateTileInfo((int)_cursorPosition.x, (int)_cursorPosition.y);

    _working = false;

    yield return null;
  }

  void HandleKeyDown()
  {    
    if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
      MoveCursor(-1, 0);      
    }
    else if (Input.GetKeyDown(KeyCode.RightArrow))
    {
      MoveCursor(1, 0);      
    }
    else if (Input.GetKeyDown(KeyCode.UpArrow))
    {
      MoveCursor(0, 1);      
    }
    else if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      MoveCursor(0, -1);      
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
    _keyHoldStatuses[KeyCode.LeftArrow]  = Input.GetKey(KeyCode.LeftArrow);
    _keyHoldStatuses[KeyCode.RightArrow] = Input.GetKey(KeyCode.RightArrow);
    _keyHoldStatuses[KeyCode.UpArrow]    = Input.GetKey(KeyCode.UpArrow);
    _keyHoldStatuses[KeyCode.DownArrow]  = Input.GetKey(KeyCode.DownArrow);

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
      //HandleKeyUp();
    }

    HandleKeyRepeat();

    if (!_keyHoldStatuses[KeyCode.UpArrow] && !_keyHoldStatuses[KeyCode.DownArrow]
    && !_keyHoldStatuses[KeyCode.LeftArrow] && !_keyHoldStatuses[KeyCode.RightArrow])
    {
      _repeatTimer = 0.0f;
    }
    
    // TODO: remove on release
    GUIManager.Instance.SetCursorPosition((int)_cursorPosition.x, (int)_cursorPosition.y);
  }

  Vector2Int _checkBoundsResult = Vector2Int.zero;
  Vector2Int CheckBounds(int x, int y)
  {
    int resX = (x < 0 || x > LevelLoader.Instance.MapSize.x - 1) ? 1 : 0;
    int resY = (y < 0 || y > LevelLoader.Instance.MapSize.y - 1) ? 1 : 0;

    _checkBoundsResult.Set(resX, resY);

    return _checkBoundsResult;
  }
}
