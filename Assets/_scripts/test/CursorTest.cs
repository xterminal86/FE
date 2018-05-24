using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorTest : MonoBehaviour 
{
  public Transform Cursor;

  Vector3 _cursorPosition = Vector3.zero;
  void Update()
  {
    HandleKeyDown();

    HandleKeyRepeat();

    HandleKeyUp();

    /*
    if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
      _cursorPosition.x--;
    }

    _cursorPosition.x = Mathf.Clamp(_cursorPosition.x, -16.0f, 16.0f);

    Cursor.position = _cursorPosition;
    */
  }

  void HandleKeyDown()
  {
    if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
      MoveCursorLR(-1, 0);
      _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
    }

    if (Input.GetKeyDown(KeyCode.RightArrow))
    {
      MoveCursorLR(1, 0);
      _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
    }

    if (Input.GetKeyDown(KeyCode.UpArrow))
    {
      MoveCursorUD(0, 1);
      _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
    }

    if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      MoveCursorUD(0, -1);
      _delayValue = GlobalConstants.CursorDelayBeforeRepeat;
    }
  }

  float _repeatTimer = 0.0f;
  float _delayValue = 0.0f;
  void HandleKeyRepeat()
  {
    if (Input.GetKey(KeyCode.LeftArrow))
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {        
        MoveCursorLR(-1, 0);
      }
    }

    if (Input.GetKey(KeyCode.RightArrow))
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {        
        MoveCursorLR(1, 0);
      }
    }

    if (Input.GetKey(KeyCode.UpArrow))
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {        
        MoveCursorUD(0, 1);
      }
    }

    if (Input.GetKey(KeyCode.DownArrow))
    {
      _repeatTimer += Time.deltaTime;

      if (_repeatTimer > _delayValue)
      {        
        MoveCursorUD(0, -1);
      }
    }
  }

  void HandleKeyUp()
  {
    if (Input.GetKeyUp(KeyCode.LeftArrow))
    {
      _repeatTimer = 0.0f;
    }

    if (Input.GetKeyUp(KeyCode.RightArrow))
    {
      _repeatTimer = 0.0f;
    }

    if (Input.GetKeyUp(KeyCode.UpArrow))
    {
      _repeatTimer = 0.0f;
    }

    if (Input.GetKeyUp(KeyCode.DownArrow))
    {
      _repeatTimer = 0.0f;
    }
  }

  bool _workingLR = false;
  void MoveCursorLR(int incX, int incY)
  {
    if (!_workingLR)
    {
      _workingLR = true;
      StartCoroutine(MoveCursorRoutine(incX, incY));
    }
  }

  bool _workingUD = false;
  void MoveCursorUD(int incX, int incY)
  {
    if (!_workingUD)
    {
      _workingUD = true;
      StartCoroutine(MoveCursorRoutine(incX, incY));
    }
  }

  float _cursorSlideSpeed = 0.0f;
  IEnumerator MoveCursorRoutine(int incX, int incY)
  {
    _cursorSlideSpeed = GlobalConstants.CursorSlideSpeed;

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

        if (incY > 0)
        {
          y = Mathf.Clamp(y, oldY, newY);
        }
        else
        {
          y = Mathf.Clamp(y, newY, oldY);
        }

        if (incX > 0)
        {
          x = Mathf.Clamp(x, oldX, newX);
        }
        else
        {
          x = Mathf.Clamp(x, newX, oldX);
        }

        _cursorPosition.x = x;
        _cursorPosition.y = y;

        Cursor.position = _cursorPosition;

        yield return null;
      }

      x = newX;
      y = newY;

      _cursorPosition.x = x;
      _cursorPosition.y = y;

      Cursor.position = _cursorPosition;
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

          if (incX > 0)
          {
            x = Mathf.Clamp(x, oldX, newX);
          }
          else
          {
            x = Mathf.Clamp(x, newX, oldX);
          }

          _cursorPosition.x = x;
          Cursor.position = _cursorPosition;

          yield return null;
        }

        x = newX;
        _cursorPosition.x = x;
        Cursor.position = _cursorPosition;

        _workingLR = false;
      }  

      if (incY != 0)
      {
        bool condY = (incY > 0) ? (y < newY) : (y > newY);

        int signY = (incY < 0) ? -1 : 1;

        while (condY)
        {
          condY = (incY > 0) ? (y < newY) : (y > newY);

          y += signY * Time.deltaTime * _cursorSlideSpeed;

          if (incY > 0)
          {
            y = Mathf.Clamp(y, oldY, newY);
          }
          else
          {
            y = Mathf.Clamp(y, newY, oldY);

            _cursorPosition.y = y;
            Cursor.position = _cursorPosition;

            yield return null;
          }

          y = newY;
          _cursorPosition.y = y;
          Cursor.position = _cursorPosition;

          _workingUD = false;
        }
      }
    }

    yield return null;
  }

  Vector2Int _checkBoundsResult = Vector2Int.zero;
  Vector2Int CheckBounds(int x, int y)
  {
    int resX = (x < -16.0f || x > 16.0f) ? 1 : 0;
    int resY = (y < -9.0f || y > 9.0f) ? 1 : 0;

    _checkBoundsResult.Set(resX, resY);

    return _checkBoundsResult;
  }
}
