using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEditor : MonoBehaviour 
{
  public Camera MainCamera;
  public Text TileIndex;

  public Transform EditorMapGrid;
  public Transform MapHolder;
  public GameObject EditorMapCellPrefab;
  public GameObject Cursor;

  int _mapSize = 16;

  void Start()
  {
    PrefabsManager.Instance.Initialize();

    for (int x = 0; x < _mapSize; x++)
    {
      for (int y = 0; y < _mapSize; y++)
      {
        Instantiate(EditorMapCellPrefab, new Vector3(x, y, 0.0f), Quaternion.identity, EditorMapGrid);  
      }
    }

    _cameraMovement = new Vector3(_mapSize / 2, _mapSize / 2, MainCamera.transform.position.z);

    _previewObject = Instantiate(PrefabsManager.Instance.Prefabs[0]);

    Util.SetGameObjectLayer(_previewObject, LayerMask.NameToLayer("Preview"), true);
  }

  float _cameraMoveSpeed = 10.0f;
  float _cameraZoom = 0.0f;
  Vector3 _cameraMovement = Vector3.zero;
  void ControlCamera()
  {
    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");

    if (Input.GetKey(KeyCode.E))
    {
      _cameraZoom -= Time.smoothDeltaTime * 4;
    }
    else if (Input.GetKey(KeyCode.Q))
    {
      _cameraZoom += Time.smoothDeltaTime * 4;
    }

    _cameraZoom = Mathf.Clamp(_cameraZoom, 5.0f, 15.0f);

    _cameraMovement.x += (Time.smoothDeltaTime * _cameraMoveSpeed) * h;
    _cameraMovement.y += (Time.smoothDeltaTime * _cameraMoveSpeed) * v;

    _cameraMovement.x = Mathf.Clamp(_cameraMovement.x, 0.0f, _mapSize);
    _cameraMovement.y = Mathf.Clamp(_cameraMovement.y, 0.0f, _mapSize);

    MainCamera.orthographicSize = _cameraZoom;

    MainCamera.transform.position = _cameraMovement;
  }

  int _tileIndex = 0;
  void SelectTile()
  {
    int oldIndex = _tileIndex;

    float mouseWheel = Input.GetAxis("Mouse ScrollWheel");

    if (mouseWheel > 0.05f)
    {
      _tileIndex--;
    }
    else if (mouseWheel < -0.05f)
    {
      _tileIndex++;
    }

    _tileIndex = Mathf.Clamp(_tileIndex, 0, PrefabsManager.Instance.Prefabs.Count - 1);

    if (oldIndex != _tileIndex)
    {
      Destroy(_previewObject.gameObject);
      _previewObject = Instantiate(PrefabsManager.Instance.Prefabs[_tileIndex]);
      Util.SetGameObjectLayer(_previewObject, LayerMask.NameToLayer("Preview"), true);
    }
  }

  GameObject _previewObject;

  RaycastHit _hitInfoEditor;
  RaycastHit _hitInfoPlacement;
  void Update()
  {
    ControlCamera();
    SelectTile();

    Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
    int mask = LayerMask.GetMask("EditorMapGrid");

    if (Physics.Raycast(r.origin, r.direction, out _hitInfoEditor, Mathf.Infinity, mask))
    {
      Cursor.transform.position = _hitInfoEditor.collider.transform.position;

      Vector3 cursorPos = Cursor.transform.position;
      cursorPos.z = -2.0f;
      Cursor.transform.position = cursorPos;

      if (_previewObject != null)
      {
        _previewObject.transform.position = Cursor.transform.position;

        Vector3 previewPos = _previewObject.transform.position;
        previewPos.z = -1.0f;
        _previewObject.transform.position = previewPos;

        if (Input.GetKeyDown(KeyCode.F))
        {          
          var to = _previewObject.GetComponent<TileObject>();
          to.FlipX();
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
          var to = _previewObject.GetComponent<TileObject>();
          to.FlipY();
        }

        if (Input.GetMouseButton(0))
        {
          // Objects are Z sorted using Y coordinate

          Vector3 pos = new Vector3(Cursor.transform.position.x, 
                                    Cursor.transform.position.y, 
                                    Cursor.transform.position.y);
        
          if (Physics.Raycast(r.origin, r.direction, out _hitInfoPlacement, Mathf.Infinity, LayerMask.GetMask("Default")))
          { 
            Destroy(_hitInfoPlacement.collider.transform.parent.gameObject);
          }

          var go = Instantiate(_previewObject, pos, Quaternion.identity, MapHolder);
          Util.SetGameObjectLayer(go, LayerMask.NameToLayer("Default"), true);
        }
        else if (Input.GetMouseButton(1))
        {
          if (Physics.Raycast(r.origin, r.direction, out _hitInfoPlacement, Mathf.Infinity, LayerMask.GetMask("Default")))
          {
            Destroy(_hitInfoPlacement.collider.transform.parent.gameObject);
          }
        }
      }
    }

    TileIndex.text = _tileIndex.ToString();
  }
}
