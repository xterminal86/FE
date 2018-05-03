using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameEditor : MonoBehaviour 
{
  public Image TileInfoSprite;
  public TMP_Text TileDetails;
  public TMP_Text TileName;

  public Camera MainCamera;

  public Transform MapHolder;
  public GameObject Cursor;

  int _mapSize = 16;

  TileObject[,] _map;

  void Start()
  {
    PrefabsManager.Instance.Initialize();

    _map = new TileObject[_mapSize, _mapSize];

    for (int x = 0; x < _mapSize; x++)
    {
      for (int y = 0; y < _mapSize; y++)
      {
        var go = Instantiate(PrefabsManager.Instance.Prefabs[0], new Vector3(x, y, 0.0f), Quaternion.identity, MapHolder);
        TileObject to = go.GetComponent<TileObject>();
        _map[x, y] = to;
        _map[x, y].IndexInPrefabsManager = 0;
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

  int _selectedTileIndex = 0;
  void SelectTile()
  {
    int oldIndex = _selectedTileIndex;

    float mouseWheel = Input.GetAxis("Mouse ScrollWheel");

    if (mouseWheel > 0.05f)
    {
      _selectedTileIndex--;
    }
    else if (mouseWheel < -0.05f)
    {
      _selectedTileIndex++;
    }

    _selectedTileIndex = Mathf.Clamp(_selectedTileIndex, 0, PrefabsManager.Instance.Prefabs.Count - 1);

    if (oldIndex != _selectedTileIndex)
    {
      Destroy(_previewObject.gameObject);
      _previewObject = Instantiate(PrefabsManager.Instance.Prefabs[_selectedTileIndex]);
      Util.SetGameObjectLayer(_previewObject, LayerMask.NameToLayer("Preview"), true);
    }
  }

  Queue<Vector2Int> _fillQueue = new Queue<Vector2Int>();
  void FillMap(int replacement)
  {
    int cx = (int)Cursor.transform.position.x;
    int cy = (int)Cursor.transform.position.y;

    int target = _map[cx, cy].IndexInPrefabsManager;

    if (target == replacement)
    {
      return;
    }

    _fillQueue.Clear();

    PlaceSelectedTile(new Vector3(cx, cy, cy));

    _fillQueue.Enqueue(new Vector2Int(cx, cy));

    int safeguard = 0;

    while (_fillQueue.Count != 0)
    {
      if (safeguard > 1000)
      {
        Debug.LogWarning("Terminated by safeguard!");
        break;
      }

      Vector2Int node = _fillQueue.Dequeue();

      int lx = node.x - 1;
      int hx = node.x + 1;
      int ly = node.y - 1;
      int hy = node.y + 1;

      if (lx >= 0 && _map[lx, node.y].IndexInPrefabsManager == target)
      {
        PlaceSelectedTile(new Vector3(lx, node.y, node.y));
        _fillQueue.Enqueue(new Vector2Int(lx, node.y));
      }

      if (ly >= 0 && _map[node.x, ly].IndexInPrefabsManager == target)
      {
        PlaceSelectedTile(new Vector3(node.x, ly, ly));
        _fillQueue.Enqueue(new Vector2Int(node.x, ly));
      }

      if (hx < _mapSize && _map[hx, node.y].IndexInPrefabsManager == target)
      {
        PlaceSelectedTile(new Vector3(hx, node.y, node.y));
        _fillQueue.Enqueue(new Vector2Int(hx, node.y));
      }

      if (hy < _mapSize && _map[node.x, hy].IndexInPrefabsManager == target)
      {
        PlaceSelectedTile(new Vector3(node.x, hy, hy));
        _fillQueue.Enqueue(new Vector2Int(node.x, hy));
      }

      safeguard++;
    }
  }

  void UpdateTileInfo()
  {
    int mx = (int)Cursor.transform.position.x;
    int my = (int)Cursor.transform.position.y;

    if (_map[mx, my] != null)
    {
      TileInfoSprite.sprite = _map[mx, my].Sprites[_map[mx, my].Sprites.Count - 1].sprite;
      TileDetails.text = string.Format("D:{0} E:{1}", _map[mx, my].DefenceModifier, _map[mx, my].EvasionModifier);
      TileName.text = _map[mx, my].InGameDescription;
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
    int mask = LayerMask.GetMask("Default");

    if (Physics.Raycast(r.origin, r.direction, out _hitInfoEditor, Mathf.Infinity, mask))
    {      
      Cursor.transform.position = _hitInfoEditor.collider.transform.position;

      UpdateTileInfo();

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
          PlaceSelectedTile(Cursor.transform.position);
        }
        else if (Input.GetMouseButton(1))
        {
          PlaceSelectedTile(Cursor.transform.position, true);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
          FillMap(_selectedTileIndex);
        }
      }

      // Tile picker

      if (Input.GetMouseButtonDown(2))
      {
        PickTileFromMap();
      }
    }
  }

  void PickTileFromMap()
  {
    int mx = (int)Cursor.transform.position.x;
    int my = (int)Cursor.transform.position.y;

    if (_map[mx, my] != null)
    {
      int tileIndex = _map[mx, my].IndexInPrefabsManager;

      if (_selectedTileIndex != tileIndex)
      {
        if (_previewObject != null)
        {
          Destroy(_previewObject.gameObject);
        }

        _previewObject = Instantiate(PrefabsManager.Instance.Prefabs[tileIndex]);
        Util.SetGameObjectLayer(_previewObject, LayerMask.NameToLayer("Preview"), true);

        _selectedTileIndex = tileIndex;
      }
    }
  }

  void PlaceSelectedTile(Vector3 placementPos, bool erase = false)
  {    
    // Objects are Z sorted using Y coordinate

    Vector3 pos = new Vector3(placementPos.x, 
                              placementPos.y, 
                              placementPos.y);
    
    int posX = (int)placementPos.x;
    int posY = (int)placementPos.y;

    if (_map[posX, posY] != null)
    {
      Destroy(_map[posX, posY].gameObject);
      _map[posX, posY] = null;
    }

    var objectToPlace = erase ? PrefabsManager.Instance.Prefabs[0] : _previewObject;

    var go = Instantiate(objectToPlace, pos, Quaternion.identity, MapHolder);         
    Util.SetGameObjectLayer(go, LayerMask.NameToLayer("Default"), true);
    _map[posX, posY] = go.GetComponent<TileObject>();
    _map[posX, posY].IndexInPrefabsManager = _selectedTileIndex;
  }
}
