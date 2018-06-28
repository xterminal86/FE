using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// A* pathfinder (no diagonal movement)
/// </summary>
public class Pathfinder
{
  Vector2Int _start = new Vector2Int();
  Vector2Int _end = new Vector2Int();

  List<PathNode> _path = new List<PathNode>();

  List<PathNode> _openList = new List<PathNode>();
  List<PathNode> _closedList = new List<PathNode>();

  TileBase[,] _map;

  int _hvCost = 10;
  int _diagonalCost = 20;

  int _mapWidth = 0, _mapHeight = 0;
  public Pathfinder(TileBase[,] map, int width, int height)
  {
    _map = map;

    _mapWidth = width;
    _mapHeight = height;

    _resultReady = false;
    _abortThread = false;

    //PrintMap();
  }

  /// <summary>
  /// Returns traversal cost between two points
  /// </summary>
  /// <param name="point">Point 1</param>
  /// <param name="goal">Point 2</param>
  /// <returns>Cost of the traversal</returns>
  int TraverseCost(Vector2Int point, Vector2Int goal)
  {
    if (point.x == goal.x || point.y == goal.y)
    {    
      return _hvCost;
    }

    return _diagonalCost;
  }

  /// <summary>
  /// Heuristic
  /// </summary>
  int ManhattanDistance(Vector2Int point, Vector2Int end)
  {
    int cost = ( Mathf.Abs(end.y - point.y) + Mathf.Abs(end.x - point.x) ) * _hvCost;

    //Debug.Log(string.Format("Manhattan distance remaining from {0} to {1}: {2}", point.ToString(), end.ToString(), cost));

    return cost;
  }

  /// <summary>
  /// Searches for the element with lowest total cost
  /// </summary>
  int FindCheapestElement(List<PathNode> list)
  {
    int f = int.MaxValue;
    int index = -1;
    int count = 0;

    foreach (var item in list)
    {
      if (item.CostF < f)
      {
        f = item.CostF;
        index = count;
      }

      count++;      
    }

    //Debug.Log("Cheapest element " + list[index].Coordinate + " " + list[index].CostF);

    return index;
  }

  PathNode FindNode(Vector2Int nodeCoordinate, List<PathNode> listToLookIn)
  {
    foreach (var item in listToLookIn)
    {
      if (nodeCoordinate.x == item.Coordinate.x &&
        nodeCoordinate.y == item.Coordinate.y)
      {
        return item;
      }
    }

    return null;
  }

  bool IsNodePresent(Vector2Int nodeCoordinate, List<PathNode> listToLookIn)
  {
    foreach (var item in listToLookIn)
    {
      if (nodeCoordinate.x == item.Coordinate.x &&
          nodeCoordinate.y == item.Coordinate.y)
      {
        return true;
      }
    }

    return false;
  }

  void LookAround4(PathNode node)
  {
    sbyte[,] direction = new sbyte[4, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };

    Vector2Int coordinate = new Vector2Int();
    for (int i = 0; i < 4; i++)
    {
      coordinate.x = node.Coordinate.x + direction[i, 0];
      coordinate.y = node.Coordinate.y + direction[i, 1];

      coordinate.x = Mathf.Clamp(coordinate.x, 0, _mapHeight - 1);
      coordinate.y = Mathf.Clamp(coordinate.y, 0, _mapWidth - 1);

      bool isInClosedList = IsNodePresent(coordinate, _closedList);

      bool objectExists = (_map[coordinate.x, coordinate.y].TileObjectLayer2 != null);
      bool objectWalkable = (objectExists && _map[coordinate.x, coordinate.y].TileObjectLayer2.MovementDifficulty != 0);
      bool groundWalkable = (_map[coordinate.x, coordinate.y].TileObjectLayer1.MovementDifficulty != 0);
      bool condition = ((objectExists && objectWalkable) || (!objectExists && groundWalkable)) && !isInClosedList;
      // Debug.Log("Current cell " + node.Coordinate + " Next cell " + coordinate + " objectLayer " + objectLayer + " groundLayer " + groundLayer);

      if (condition)
      {
        bool isInOpenList = IsNodePresent(coordinate, _openList);

        if (!isInOpenList)
        {
          PathNode newNode = new PathNode(new Vector2Int(coordinate.x, coordinate.y), node);
          newNode.CostG = node.CostG + TraverseCost(node.Coordinate, newNode.Coordinate);
          newNode.CostH = ManhattanDistance(newNode.Coordinate, _end);
          newNode.CostF = newNode.CostG + newNode.CostH;

          _openList.Add(newNode);
        }
      }      
    }    
  }

  bool ExitCondition()
  {
    return (_openList.Count == 0 || IsNodePresent(_end, _closedList));    
  }

  /// <summary>
  /// Method tries to build a path by A* algorithm and returns it as list of nodes
  /// to traverse from start to end
  /// </summary>
  /// <param name="start">Starting point</param>
  /// <param name="end">Destination point</param>
  /// <returns>List of nodes from start to end</returns>
  public List<PathNode> BuildRoad(Vector2Int start, Vector2Int end, bool printPath = false)
  {
    _start = start;
    _end = end;

    _path.Clear();
    _openList.Clear();
    _closedList.Clear();

    // A* starts here

    PathNode node = new PathNode(start);
    node.CostH = ManhattanDistance(start, end);
    node.CostF = node.CostG + node.CostH;

    _openList.Add(node);

    //string debugPrint = string.Empty;

    bool exit = false;
    while (!exit)
    {
      int index = FindCheapestElement(_openList);

      var closedNode = _openList[index];
      _closedList.Add(closedNode);

      //debugPrint += string.Format("{0} ", closedNode.Coordinate);

      _openList.RemoveAt(index);

      LookAround4(closedNode);

      exit = ExitCondition();
    }

    ConstructPath(printPath);

    //Debug.Log("Total closed nodes:\n" + debugPrint);

    return _path;
  }

  bool _threadIsWorking = false;
  public bool IsThreadWorking
  {
    get { return _threadIsWorking; }
  }

  // Async version of above
  public void BuildRoadAsync(Vector2Int start, Vector2Int end)
  {    
    _threadIsWorking = true;

    _start = new Vector2Int(start.x, start.y);
    _end = new Vector2Int(end.x, end.y);

    _path.Clear();
    _openList.Clear();
    _closedList.Clear();

    // A* starts here

    PathNode node = new PathNode(start);
    node.CostH = ManhattanDistance(start, end);
    node.CostF = node.CostG + node.CostH;

    _openList.Add(node);

    _resultReady = false;

    //JobManager.Instance.CreateThreadB(BuildRoadThreadFunction, avoidObstacles);
  }

  volatile bool _abortThread = false;
  void BuildRoadThreadFunction(object arg)
  {
    //bool avoidObstacles = (bool)arg;

    _abortThread = false;

    bool exit = false;
    while (!exit)
    {
      //if (ThreadWatcher.Instance.StopAllThreads || _abortThread)
      if (_abortThread)
      {
        return;
      }

      int index = FindCheapestElement(_openList);

      var closedNode = _openList[index];
      _closedList.Add(closedNode);

      _currentNode = closedNode;

      //Debug.Log(closedNode);

      _openList.RemoveAt(index);

      LookAround4(closedNode);

      exit = ExitCondition();      
    }

    //Debug.Log("building road done!");

    _resultReady = true;

    _threadIsWorking = false;
  }

  public void AbortThread()
  {
    _abortThread = true;
  }

  // Constantly check the result via this property in a coroutine  
  public List<PathNode> GetResult(bool ignoreAsync = false)
  {
    if (_resultReady || ignoreAsync)
    {
      ConstructPath();

      return _path;
    }

    return null;
  }

  PathNode _currentNode;
  public PathNode CurrentNode
  {
    get { return _currentNode; }
  }

  bool _resultReady = false;
  public bool ResultReady
  {
    get { return _resultReady; }
  }

  void ConstructPath(bool printPath = false)
  {
    var node = FindNode(_end, _closedList);

    while (node != null)
    {
      _path.Add(node);
      node = node.ParentNode;
    }

    if (_path.Count != 0)
    {
      _path.Reverse();
      _path.RemoveAt(0);
    }

    if (printPath)
    {
      PrintPath();
    }
  }

  void PrintPath()
  {
    StringBuilder sb = new StringBuilder();

    sb.Append(string.Format("Path from {0} to {1} :", _start.ToString(), _end.ToString()));

    foreach (var item in _path)
    {
      sb.Append(string.Format("[{0};{1} costF: {2}] => ", item.Coordinate.x, item.Coordinate.y, item.CostF));
    }

    sb.Append("Done!");

    Debug.Log(sb.ToString());
  }

  void PrintMap()
  {
    StringBuilder sb = new StringBuilder();

    for (int x = 0; x < _mapHeight; x++)
    {
      for (int y = 0; y < _mapWidth; y++)
      {
        sb.Append(string.Format("({0};{1}) => {2} | ", x, y, _map[x, y]));
      }
    }

    Debug.Log("Map array: " + sb.ToString());
  }

  /// <summary>
  /// Helper class of path node
  /// </summary>
  public class PathNode
  {
    public PathNode(Vector2Int coord)
    {
      Coordinate.x = coord.x;
      Coordinate.y = coord.y;           
    }

    public PathNode(PathNode rhs)
    {
      Coordinate = rhs.Coordinate;
      ParentNode = rhs.ParentNode;
      CostF = rhs.CostF;
      CostG = rhs.CostG;
      CostH = rhs.CostH;
    }

    public PathNode(Vector2Int coord, PathNode parent)
    {
      Coordinate.x = coord.x;
      Coordinate.y = coord.y;
      ParentNode = parent;
    }

    public override string ToString()
    {
      return Coordinate.ToString();
    }

    // Map coordinate of this node
    public Vector2Int Coordinate = new Vector2Int();
    // Reference to parent node
    public PathNode ParentNode = null;

    // Total cost
    public int CostF = 0;
    // Cost of traversal here from the starting point with regard of already traversed path
    public int CostG = 0;
    // Heuristic cost
    public int CostH = 0;
  }
}
