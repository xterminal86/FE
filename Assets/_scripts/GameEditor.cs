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
  public TMP_Text CurrentLayer;
  
  public Camera MainCamera;

  public Transform MapHolder;
  public GameObject Cursor;

  public int MapSizeX = 16;
  public int MapSizeY = 16;

  public RectTransform HelpWindow;
  
  public TMP_Text FillRoutineProgressText;
  
  TileBase[,] _map;

  SerializedMap _levelToSave;

  void Start()
  {
    PrefabsManager.Instance.Initialize();

    _map = new TileBase[MapSizeX, MapSizeY];

    for (int x = 0; x < MapSizeX; x++)
    {
      for (int y = 0; y < MapSizeY; y++)
      {
        var go = Instantiate(PrefabsManager.Instance.TileBasePrefab, new Vector3(x, y, 0.0f), Quaternion.identity, MapHolder);
        TileBase to = go.GetComponent<TileBase>();
        _map[x, y] = to;
        var l1 = Instantiate(PrefabsManager.Instance.PrefabsLayer1[0], new Vector3(x, y, y), Quaternion.identity, go.transform);
        to.TileObjectLayer1 = l1.GetComponent<TileObject>();
        to.TileObjectLayer1.PrefabName = PrefabsManager.Instance.PrefabsLayer1[0].name;
      }
    }

    _cameraMovement = new Vector3(MapSizeX / 2, MapSizeY / 2, MainCamera.transform.position.z);

    _previewObject = Instantiate(PrefabsManager.Instance.PrefabsLayer1[0]);
    _previewObject.name = _previewObject.name.Replace("(Clone)", "") + "-preview";
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

  int _objectsLayer = 0;
  int _selectedTileIndex = 0;
  void SelectTile()
  {
    int oldIndex = _selectedTileIndex;
    int oldLayer = _objectsLayer;

    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
      _objectsLayer = 0;
    }
    else if (Input.GetKeyDown(KeyCode.Alpha2))
    {
      _objectsLayer = 1;
    }

    var prefabsList = (_objectsLayer == 0) ? PrefabsManager.Instance.PrefabsLayer1 : PrefabsManager.Instance.PrefabsLayer2;

    float mouseWheel = Input.GetAxis("Mouse ScrollWheel");

    if (mouseWheel > 0.05f)
    {
      _selectedTileIndex--;
    }
    else if (mouseWheel < -0.05f)
    {
      _selectedTileIndex++;
    }

    _selectedTileIndex = Mathf.Clamp(_selectedTileIndex, 0, prefabsList.Count - 1);

    if (oldIndex != _selectedTileIndex || oldLayer != _objectsLayer)
    {
      Destroy(_previewObject.gameObject);
      _previewObject = Instantiate(prefabsList[_selectedTileIndex]);
      _previewObject.name = _previewObject.name.Replace("(Clone)", "") + "-preview";
      _previewObject.GetComponent<TileObject>().PrefabName = prefabsList[_selectedTileIndex].name;
      Util.SetGameObjectLayer(_previewObject, LayerMask.NameToLayer("Preview"), true);
    }
  }

  List<string> _progressText = new List<string>()
  {
    "-", "\\", "|", "/"
  };
  
  bool _blockInteraction = false;
  
  string _targetPrefabName = string.Empty;
  Queue<Vector2Int> _fillQueue = new Queue<Vector2Int>();
  IEnumerator FillMapRoutine()
  {
    _blockInteraction = true;
    
    FillRoutineProgressText.gameObject.SetActive(true);
    
    int progressIndex = 0;
        
    while (_fillQueue.Count != 0)
    {
      FillRoutineProgressText.text = _progressText[progressIndex];
      
      Vector2Int node = _fillQueue.Dequeue();

      int lx = node.x - 1;
      int hx = node.x + 1;
      int ly = node.y - 1;
      int hy = node.y + 1;

      if (lx >= 0 && _map[lx, node.y].TileObjectLayer1.PrefabName == _targetPrefabName)
      {
        PlaceSelectedObject(new Vector3(lx, node.y, node.y));
        _fillQueue.Enqueue(new Vector2Int(lx, node.y));
      }

      if (ly >= 0 && _map[node.x, ly].TileObjectLayer1.PrefabName == _targetPrefabName)
      {
        PlaceSelectedObject(new Vector3(node.x, ly, ly));
        _fillQueue.Enqueue(new Vector2Int(node.x, ly));
      }

      if (hx < MapSizeX && _map[hx, node.y].TileObjectLayer1.PrefabName == _targetPrefabName)
      {
        PlaceSelectedObject(new Vector3(hx, node.y, node.y));
        _fillQueue.Enqueue(new Vector2Int(hx, node.y));
      }

      if (hy < MapSizeX && _map[node.x, hy].TileObjectLayer1.PrefabName == _targetPrefabName)
      {
        PlaceSelectedObject(new Vector3(node.x, hy, hy));
        _fillQueue.Enqueue(new Vector2Int(node.x, hy));
      }

      progressIndex++;
      
      progressIndex %= _progressText.Count;
      
      yield return null;
    }
    
    FillRoutineProgressText.gameObject.SetActive(false);
    
    _blockInteraction = false;
    
    yield return null;
  }
  
  void FillMap()
  {
    int cx = (int)Cursor.transform.position.x;
    int cy = (int)Cursor.transform.position.y;

    _targetPrefabName = _map[cx, cy].TileObjectLayer1.PrefabName;

    if (_targetPrefabName == _previewObject.GetComponent<TileObject>().PrefabName)
    {
      return;
    }

    _fillQueue.Clear();

    PlaceSelectedObject(new Vector3(cx, cy, cy));

    _fillQueue.Enqueue(new Vector2Int(cx, cy));

    StartCoroutine(FillMapRoutine());    
  }

  void UpdateTileInfo()
  {
    int mx = (int)Cursor.transform.position.x;
    int my = (int)Cursor.transform.position.y;

    var tileObject = (_map[mx, my].TileObjectLayer2 != null) ? _map[mx, my].TileObjectLayer2 : _map[mx, my].TileObjectLayer1;

    TileInfoSprite.sprite = tileObject.TileSprite.sprite;
    TileDetails.text = string.Format("D:{0} E:{1}", tileObject.DefenceModifier, tileObject.EvasionModifier);
    TileName.text = tileObject.InGameDescription;
  }

  bool _helpShown = false;
  
  GameObject _previewObject;

  RaycastHit _hitInfoEditor;
  RaycastHit _hitInfoPlacement;
  void Update()
  {
    ControlCamera();
    
    if (_blockInteraction)    
    {
      return;
    }
    
    SelectTile();
    
    CurrentLayer.text = (_objectsLayer == 0) ? "Ground" : "Objects";

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
          PlaceSelectedObject(Cursor.transform.position);
        }
        else if (Input.GetMouseButton(1) && !isOverGui)
        {
          RemoveSelectedObject(Cursor.transform.position);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
          FillMap();
        }
      }

      // Tile picker

      if (Input.GetMouseButtonDown(2) && !isOverGui)
      {
        PickTileFromMap();
      }
    }
    
    if (Input.GetKeyDown(KeyCode.H))
    {
      _helpShown = !_helpShown;
      HelpWindow.gameObject.SetActive(_helpShown);
    }    
  }

  void PickTileFromMap()
  {
    int mx = (int)Cursor.transform.position.x;
    int my = (int)Cursor.transform.position.y;

    var objectToPick = (_objectsLayer == 0) ? _map[mx, my].TileObjectLayer1 : _map[mx, my].TileObjectLayer2;

    if (objectToPick != null)
    {
      Destroy(_previewObject.gameObject);
      _previewObject = Instantiate(objectToPick.gameObject, Cursor.transform.position, Quaternion.identity);
      _previewObject.name = objectToPick.name.Replace("(Clone)", "");
      Util.SetGameObjectLayer(_previewObject, LayerMask.NameToLayer("Preview"), true);
      //var res = PrefabsManager.Instance.FindPrefabByName(_previewObject.GetComponent<TileObject>().PrefabName);
      Debug.Log(objectToPick.PrefabName);
    }
  }

  void PlaceSelectedObject(Vector3 placementPos)
  { 
    float zDepth = (_objectsLayer == 0) ? placementPos.y : placementPos.y - 1;

    // Objects are Z sorted using Y coordinate

    Vector3 pos = new Vector3(placementPos.x, 
                              placementPos.y, 
                              zDepth);

    int posX = (int)placementPos.x;
    int posY = (int)placementPos.y;

    var go = Instantiate(_previewObject, pos, Quaternion.identity, _map[posX, posY].transform);         
    Util.SetGameObjectLayer(go, LayerMask.NameToLayer("Default"), true);

    if (_objectsLayer == 0)
    {
      Destroy(_map[posX, posY].TileObjectLayer1.gameObject);
      _map[posX, posY].TileObjectLayer1 = go.GetComponent<TileObject>();
      _map[posX, posY].TileObjectLayer1.PrefabName = go.GetComponent<TileObject>().PrefabName;
    }
    else if (_objectsLayer == 1)
    {
      if (_map[posX, posY].TileObjectLayer2 != null)
      {
        Destroy(_map[posX, posY].TileObjectLayer2.gameObject);
      }

      // PrefabName is assigned during SelectTile() method in _previewObject

      _map[posX, posY].TileObjectLayer2 = go.GetComponent<TileObject>();
    }
  }

  void RemoveSelectedObject(Vector3 placementPos)
  {
    float zDepth = (_objectsLayer == 0) ? placementPos.y : placementPos.y - 1;

    // Objects are Z sorted using Y coordinate

    Vector3 pos = new Vector3(placementPos.x, 
                              placementPos.y, 
                              zDepth);

    int posX = (int)placementPos.x;
    int posY = (int)placementPos.y;

    // Replace ground tile with emptiness for layer1 and just destroy for layer2

    if (_objectsLayer == 0)
    {
      Destroy(_map[posX, posY].TileObjectLayer1.gameObject);
      var go = Instantiate(PrefabsManager.Instance.PrefabsLayer1[0], pos, Quaternion.identity, _map[posX, posY].transform);         
      Util.SetGameObjectLayer(go, LayerMask.NameToLayer("Default"), true);
      _map[posX, posY].TileObjectLayer1 = go.GetComponent<TileObject>();
      _map[posX, posY].TileObjectLayer1.PrefabName = PrefabsManager.Instance.PrefabsLayer1[0].name;
    }
    else if (_objectsLayer == 1 && _map[posX, posY].TileObjectLayer2 != null)
    {
      Destroy(_map[posX, posY].TileObjectLayer2.gameObject);
      _map[posX, posY].TileObjectLayer2 = null;
    }
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
      TileBase to = t.GetComponent<TileBase>();

      SerializedTile st = new SerializedTile();

      var layer1 = FillSerializedTileData(to.TileObjectLayer1);
      st.TileLayer1 = layer1;

      if (to.TileObjectLayer2 != null)
      {
        var layer2 = FillSerializedTileData(to.TileObjectLayer2);
        st.TileLayer2 = layer2;
      }

      _levelToSave.MapTiles.Add(st);
    }
  }

  SerializedTileObject FillSerializedTileData(TileObject tileObject)
  {
    SerializedTileObject serialiedTileObject = new SerializedTileObject();

    serialiedTileObject.CoordX = (int)tileObject.transform.position.x;
    serialiedTileObject.CoordY = (int)tileObject.transform.position.y;
    serialiedTileObject.DefenceModifier = tileObject.DefenceModifier;
    serialiedTileObject.EvasionModifier = tileObject.EvasionModifier;
    serialiedTileObject.FlipFlagX = tileObject.FlipFlagX;
    serialiedTileObject.FlipFlagY = tileObject.FlipFlagY;
    serialiedTileObject.InGameDescription = tileObject.InGameDescription;
    serialiedTileObject.MovementDifficulty = tileObject.MovementDifficulty;
    serialiedTileObject.PrefabName = tileObject.PrefabName;

    return serialiedTileObject;
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

    _map = new TileBase[MapSizeX, MapSizeY];

    foreach (var tile in _levelToSave.MapTiles)
    {
      var layer1 = tile.TileLayer1;

      int x = layer1.CoordX;
      int y = layer1.CoordY;

      var go = Instantiate(PrefabsManager.Instance.TileBasePrefab, new Vector3(x, y, 0.0f), Quaternion.identity, MapHolder);

      TileBase tb = go.GetComponent<TileBase>();

      _map[x, y] = tb;

      InstantiateTileObject(tb, tile.TileLayer1, 0);

      if (tile.TileLayer2 != null)
      {
        InstantiateTileObject(tb, tile.TileLayer2, 1);
      }
    }
  }

  void InstantiateTileObject(TileBase tb, SerializedTileObject sto, int layer)
  {
    string prefabName = sto.PrefabName;

    var res = PrefabsManager.Instance.FindPrefabByName(prefabName, layer);
    if (res.Value != null)
    {
      float zDepth = (layer == 0) ? sto.CoordY : sto.CoordY - 1;
      Vector3 pos = new Vector3(sto.CoordX, sto.CoordY, zDepth);
      var go = Instantiate(res.Value, pos, Quaternion.identity, tb.transform);
      TileObject to = go.GetComponent<TileObject>();
      to.PrefabName = prefabName;
      to.DefenceModifier = sto.DefenceModifier;
      to.EvasionModifier = sto.EvasionModifier;
      to.FlipFlagX = sto.FlipFlagX;
      to.FlipFlagY = sto.FlipFlagY;
      to.InGameDescription = sto.InGameDescription;
      to.MovementDifficulty = sto.MovementDifficulty;

      if (sto.FlipFlagX)
      {
        to.FlipX();
      }

      if (sto.FlipFlagY)
      {
        to.FlipY();
      }

      if (layer == 0)
      {
        tb.TileObjectLayer1 = to;
      }
      else if (layer == 1)
      {
        tb.TileObjectLayer2 = to;
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
