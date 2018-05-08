using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SFB;

public class GameEditor : MonoBehaviour 
{
  public Image TileInfoSprite;
  public TMP_Text TileDetails;
  public TMP_Text TileName;

  public Camera MainCamera;

  public Transform MapHolder;
  public GameObject Cursor;

  public int MapSizeX = 16;
  public int MapSizeY = 16;

  TileObject[,] _map;

  SerializedMap _levelToSave;

  void Start()
  {
    PrefabsManager.Instance.Initialize();

    _map = new TileObject[MapSizeX, MapSizeY];

    for (int x = 0; x < MapSizeX; x++)
    {
      for (int y = 0; y < MapSizeY; y++)
      {
        var go = Instantiate(PrefabsManager.Instance.Prefabs[0], new Vector3(x, y, 0.0f), Quaternion.identity, MapHolder);
        TileObject to = go.GetComponent<TileObject>();
        _map[x, y] = to;
        _map[x, y].PrefabName = PrefabsManager.Instance.Prefabs[0].name;
        _map[x, y].IndexInPrefabsManager = 0;
      }
    }

    _cameraMovement = new Vector3(MapSizeX / 2, MapSizeY / 2, MainCamera.transform.position.z);

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

    _cameraMovement.x = Mathf.Clamp(_cameraMovement.x, 0.0f, MapSizeX);
    _cameraMovement.y = Mathf.Clamp(_cameraMovement.y, 0.0f, MapSizeY);

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
      if (safeguard > 1000000)
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

      if (hx < MapSizeX && _map[hx, node.y].IndexInPrefabsManager == target)
      {
        PlaceSelectedTile(new Vector3(hx, node.y, node.y));
        _fillQueue.Enqueue(new Vector2Int(hx, node.y));
      }

      if (hy < MapSizeX && _map[node.x, hy].IndexInPrefabsManager == target)
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

      bool isOverGui = EventSystem.current.IsPointerOverGameObject();

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
          to.FlipFlagX = !to.FlipFlagX;
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
          var to = _previewObject.GetComponent<TileObject>();
          to.FlipY();
          to.FlipFlagY = !to.FlipFlagY;
        }

        if (Input.GetMouseButton(0) && !isOverGui)
        {
          PlaceSelectedTile(Cursor.transform.position);
        }
        else if (Input.GetMouseButton(1) && !isOverGui)
        {
          PlaceSelectedTile(Cursor.transform.position, true);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
          FillMap(_selectedTileIndex);
        }
      }

      // Tile picker

      if (Input.GetMouseButtonDown(2) && !isOverGui)
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
      //int tileIndex = _map[mx, my].IndexInPrefabsManager;
      var res = PrefabsManager.Instance.FindPrefabByName(_map[mx, my].PrefabName);

      if (_selectedTileIndex != res.Key)
      {
        if (_previewObject != null)
        {
          Destroy(_previewObject.gameObject);
        }

        _previewObject = Instantiate(PrefabsManager.Instance.Prefabs[res.Key], Cursor.transform.position, Quaternion.identity);
        Util.SetGameObjectLayer(_previewObject, LayerMask.NameToLayer("Preview"), true);

        _selectedTileIndex = res.Key;
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
    _map[posX, posY].PrefabName = PrefabsManager.Instance.Prefabs[_selectedTileIndex].name;
    _map[posX, posY].IndexInPrefabsManager = _selectedTileIndex;
  }

  void PrepareDataToSave(string path)
  {
    _levelToSave = new SerializedMap();

    _levelToSave.Path = path;
    _levelToSave.MapSizeX = MapSizeX;
    _levelToSave.MapSizeY = MapSizeY;

    _levelToSave.MapTiles.Clear();

    foreach (Transform t in MapHolder)
    {
      TileObject to = t.GetComponent<TileObject>();

      SerializedTile st = new SerializedTile();
      st.CoordX = (int)to.transform.position.x;
      st.CoordY = (int)to.transform.position.y;
      st.DefenceModifier = to.DefenceModifier;
      st.EvasionModifier = to.EvasionModifier;
      st.FlipFlagX = to.FlipFlagX;
      st.FlipFlagY = to.FlipFlagY;
      st.IndexInPrefabsManager = to.IndexInPrefabsManager;
      st.InGameDescription = to.InGameDescription;
      st.MovementDifficulty = to.MovementDifficulty;
      st.PrefabName = to.PrefabName;

      _levelToSave.MapTiles.Add(st);
    }
  }

  public void SaveMapHandler()
  {
    string path = StandaloneFileBrowser.SaveFilePanel("Save Map", "", "level", "bytes");
    if (!string.IsNullOrEmpty(path))
    {
      PrepareDataToSave(path);

      var formatter = new BinaryFormatter();
      Stream s = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
      formatter.Serialize(s, _levelToSave);
      s.Close();
    }
  }

  void LoadLevel(string path)
  {
    var formatter = new BinaryFormatter();  
    Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);  
    _levelToSave = (SerializedMap)formatter.Deserialize(stream);  
    stream.Close();

    DestroyChildren(MapHolder);

    MapSizeX = _levelToSave.MapSizeX;
    MapSizeY = _levelToSave.MapSizeY;

    _map = new TileObject[MapSizeX, MapSizeY];

    foreach (var tile in _levelToSave.MapTiles)
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

  public void LoadMapHandler()
  {
    string[] paths = StandaloneFileBrowser.OpenFilePanel("Save Map", "", "bytes", false);
    if (paths.Length != 0 && !string.IsNullOrEmpty(paths[0]))
    {
      LoadLevel(paths[0]);
    }
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
