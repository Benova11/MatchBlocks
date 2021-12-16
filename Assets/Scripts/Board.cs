using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Board : MonoBehaviour
{
  public int width;
  public int height;

  public float borderSize;

  public GameObject tilePrefab;
  public GameObject[] gamePiecePrefabs;

  public float swapTime = 0.5f;

  Tile[,] allTileList;
  GamePiece[,] allGamePiecesList;

  Tile clickedTile;
  Tile targetTile;

  bool playerInputEnaled = true;

  void Start()
  {
    SetupTiles();
    SetupCamera();

    int[,] array2D = new int[,] {
      { -1, -1, -1, -1, -1, -1, -1 },
      { -1, -1, -1, -1, -1, -1, -1 },
      { -1, -1, -1, -1, -1, -1, -1 },
      { -1, -1, -1, -1, -1, -1, -1 },
      { -1, -1, 2, 1, 2, -1, -1 },
      { 3, -1, 1, 2, 1, -1, 2 },
      { 3, -1, 2, 1, 2, -1, 1 }};

    FillBoard(AdjustMatrixForUserDisplay(array2D));
  }

  int[,] AdjustMatrixForUserDisplay(int[,] matrix)
  {
    int[,] array2D = new int[width, height];
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        //Debug.Log(matrix[j, i]);
        array2D[i, height - j - 1] = matrix[j, i];
      }
    }
    return array2D;
  }

  void SetupTiles()
  {
    allTileList = new Tile[width, height];

    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        GameObject tile = Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity) as GameObject;
        tile.name = "Tile (" + i + "," + j + ")";
        allTileList[i, j] = tile.GetComponent<Tile>();
        tile.transform.parent = transform;
        allTileList[i, j].Init(i, j, this);
      }
    }
  }

  void SetupCamera()
  {
    Camera.main.transform.position = new Vector3((float)(width - 1) / 2f, (float)(height - 1) / 2f, -10f);

    float aspectRatio = (float)Screen.width / (float)Screen.height;

    float verticalSize = (float)height / 2f + (float)borderSize;

    float horizontalSize = ((float)width / 2f + (float)borderSize) / aspectRatio;

    Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;

  }

  GameObject GetGamePiece(int pieceType)
  {
    if (gamePiecePrefabs[pieceType] == null)
      Debug.LogWarning("BOARD:  " + pieceType + "does not contain a valid GamePiece prefab!");
    return gamePiecePrefabs[pieceType];
  }

  public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
  {
    if (gamePiece == null)
    {
      Debug.LogWarning("BOARD:  Invalid GamePiece!");
      return;
    }

    gamePiece.transform.position = new Vector3(x, y, 0);
    gamePiece.transform.rotation = Quaternion.identity;

    if (IsWithinBounds(x, y))
    {
      allGamePiecesList[x, y] = gamePiece;
    }

    gamePiece.SetCoord(x, y);
  }

  GamePiece FillPieceAt(int x, int y, int pieceType)
  {
    GameObject gamePiece = null;
    if (pieceType != -1)
    {
      gamePiece = Instantiate(GetGamePiece(pieceType), Vector3.zero, Quaternion.identity) as GameObject;
      gamePiece.GetComponent<GamePiece>().Init(this);
      PlaceGamePiece(gamePiece.GetComponent<GamePiece>(), x, y);
      gamePiece.transform.parent = transform;
    }
    return gamePiece.GetComponent<GamePiece>();
  }

  void FillBoard(int[,] matrix = null)
  {
    allGamePiecesList = new GamePiece[width, height];

    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        GamePiece gamePiece;
        if (matrix[i, j] != -1)
          gamePiece = FillPieceAt(i, j, matrix[i, j]);
      }
    }
  }

  public void ClickTile(Tile tile)
  {
    if (clickedTile == null)
      clickedTile = tile;
  }

  public void DragToTile(Tile tile)
  {
    if (clickedTile != null && IsNextTo(tile, clickedTile))
      targetTile = tile;
  }

  public void ReleaseTile()
  {
    if (clickedTile != null && targetTile != null)
      SwitchTiles(clickedTile, targetTile);
    clickedTile = null;
    targetTile = null;
  }

  void SwitchTiles(Tile clickedTile, Tile targetTile)
  {
    StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
  }

  IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
  {
    if (playerInputEnaled)
    {
      GamePiece clickedPiece = allGamePiecesList[clickedTile.xIndex, clickedTile.yIndex];
      GamePiece targetPiece = allGamePiecesList[targetTile.xIndex, targetTile.yIndex];

      if (clickedPiece != null && IsMoveSafe(clickedTile, targetTile))
      {
        clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
        if (targetPiece != null)
          targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
        else
          allGamePiecesList[clickedTile.xIndex, clickedTile.yIndex] = null;

        yield return new WaitForSeconds(swapTime);

        List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
        List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
        yield return new WaitForSeconds(swapTime / 2);

        ClearAndCollapse(clickedPieceMatches.Union(targetPieceMatches).ToList());
        CollapseColumn(clickedTile.xIndex);
        CollapseColumn(targetTile.xIndex);
      }
    }
  }

  bool IsMoveSafe(Tile clickedTile, Tile targetTile)
  {
    if (targetTile.yIndex > clickedTile.yIndex && allGamePiecesList[targetTile.xIndex, targetTile.yIndex] == null)
      return false;
    return true;
  }

  bool IsNextTo(Tile start, Tile end)
  {
    if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex) return true;
    if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex) return true;
    return false;
  }

  bool HasMatchOnFill(int x, int y, int minLength = 3)
  {
    List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
    List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

    if (leftMatches == null)
      leftMatches = new List<GamePiece>();

    if (downwardMatches == null)
      downwardMatches = new List<GamePiece>();

    return leftMatches.Count > 0 || downwardMatches.Count > 0;
  }

  List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
  {
    List<GamePiece> matches = new List<GamePiece>();
    GamePiece startPiece = null;

    if (IsWithinBounds(startX, startY))
      startPiece = allGamePiecesList[startX, startY];

    if (startPiece != null)
      matches.Add(startPiece);
    else
      return null;

    int nextX, nextY;
    int maxValue = (width > height) ? width : height;

    for (int i = 1; i < maxValue - 1; i++)
    {
      nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
      nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

      if (!IsWithinBounds(nextX, nextY))
        break;

      GamePiece nextPiece = allGamePiecesList[nextX, nextY];

      if (nextPiece == null)
        break;
      else
      {
        if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
          matches.Add(nextPiece);
        else
          break;
      }
    }

    if (matches.Count >= minLength)
      return matches;

    return null;
  }

  List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
  {
    List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
    List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

    if (upwardMatches == null) upwardMatches = new List<GamePiece>();
    if (downwardMatches == null) downwardMatches = new List<GamePiece>();

    List<GamePiece> combinedMatches = upwardMatches.Union(downwardMatches).ToList();
    return (combinedMatches.Count >= minLength) ? combinedMatches : null;
  }

  List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
  {
    List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
    List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

    if (rightMatches == null)
      rightMatches = new List<GamePiece>();

    if (leftMatches == null)
      leftMatches = new List<GamePiece>();

    var combinedMatches = rightMatches.Union(leftMatches).ToList();

    return (combinedMatches.Count >= minLength) ? combinedMatches : null;

  }

  List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
  {
    List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
    List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLength);

    if (horizMatches == null)
      horizMatches = new List<GamePiece>();

    if (vertMatches == null)
      vertMatches = new List<GamePiece>();

    var combinedMatches = horizMatches.Union(vertMatches).ToList();
    return combinedMatches;
  }

  List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
  {
    List<GamePiece> matches = new List<GamePiece>();
    foreach (GamePiece piece in gamePieces)
    {
      matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
    }
    return matches;
  }

  List<GamePiece> FindAllMatches()
  {
    List<GamePiece> combinedMatches = new List<GamePiece>();

    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        var matches = FindMatchesAt(i, j);
        combinedMatches = combinedMatches.Union(matches).ToList();
      }
    }
    return combinedMatches;
  }

  List<GamePiece> CollapseColumn(int column, float collaspseTime = 0.1f)
  {
    List<GamePiece> movingPieces = new List<GamePiece>();
    for (int i = 0; i < height - 1; i++)
    {
      if (allGamePiecesList[column, i] == null)
      {
        for (int j = i + 1; j < height; j++)
        {
          if (allGamePiecesList[column, j] != null)
          {
            allGamePiecesList[column, j].Move(column, i, collaspseTime * (j / 1.1f));//collaspseTime *(j-i)
            allGamePiecesList[column, i] = allGamePiecesList[column, j];
            allGamePiecesList[column, i].SetCoord(column, i);
            if (!movingPieces.Contains(allGamePiecesList[column, i]))
              movingPieces.Add(allGamePiecesList[column, i]);
            allGamePiecesList[column, j] = null;
            break;
          }
        }
      }
    }
    return movingPieces;
  }

  List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
  {
    List<GamePiece> movingPieces = new List<GamePiece>();
    List<int> columnsToCollapse = GetColumns(gamePieces);

    foreach (int column in columnsToCollapse)
    {
      movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
    }
    return movingPieces;
  }

  void ClearAndCollapse(List<GamePiece> gamePieces)
  {
    StartCoroutine(ClearAndCollapseRoutine(gamePieces));
  }

  IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
  {
    playerInputEnaled = false;

    List<GamePiece> movingPieces = new List<GamePiece>();
    List<GamePiece> matches = new List<GamePiece>();

    HighlightPieces(gamePieces);
    yield return new WaitForSeconds(0.25f);

    bool isFinished = false;
    while (!isFinished)
    {
      ClearPieceAt(gamePieces);
      //yield return new WaitForSeconds(0.1f);

      movingPieces = CollapseColumn(gamePieces);

      while (!IsCollapsed(movingPieces))
        yield return null;
      yield return new WaitForSeconds(0.1f);

      matches = FindAllMatches();//FindMatchesAt(movingPieces);

      if (matches.Count == 0)
      { isFinished = true; break; }
      else
        yield return StartCoroutine(ClearAndCollapseRoutine(matches));
    }
    yield return null;

    playerInputEnaled = true;
  }

  bool IsCollapsed(List<GamePiece> gamePieces)
  {
    foreach (GamePiece gamePiece in gamePieces)
    {
      if (gamePiece != null)
        if (gamePiece.transform.position.y - gamePiece.yIndex > 0.001f)
          return false;
    }
    return true;
  }

  void HighlightTileOff(int x, int y)
  {
    SpriteRenderer spriteRenderer = allTileList[x, y].GetComponent<SpriteRenderer>();
    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
  }

  void HighlightTileOn(int x, int y, Color col)
  {
    SpriteRenderer spriteRenderer = allTileList[x, y].GetComponent<SpriteRenderer>();
    spriteRenderer.color = col;
  }

  void HighlightMatchesAt(int x, int y)
  {
    HighlightTileOff(x, y);
    var combinedMatches = FindMatchesAt(x, y);
    if (combinedMatches.Count > 0)
      foreach (GamePiece piece in combinedMatches)
        HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
  }

  void HighlightPieces(List<GamePiece> gamePieces)
  {
    foreach (GamePiece gamePiece in gamePieces)
    {
      if (gamePiece != null)
        HighlightTileOn(gamePiece.xIndex, gamePiece.yIndex, gamePiece.GetComponent<SpriteRenderer>().color);
    }
  }

  void HighlightMatches()
  {
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        HighlightMatchesAt(i, j);
      }
    }
  }

  void ClearPieceAt(int x, int y)
  {
    GamePiece pieceToClear = allGamePiecesList[x, y];

    if (pieceToClear != null)
    {
      allGamePiecesList[x, y] = null;
      Destroy(pieceToClear.gameObject);
    }

    HighlightTileOff(x, y);
  }

  void ClearPieceAt(List<GamePiece> gamePieces)
  {
    foreach (GamePiece piece in gamePieces)
    {
      if (piece != null)
        ClearPieceAt(piece.xIndex, piece.yIndex);
    }
  }

  void ClearBoard()
  {
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        ClearPieceAt(i, j);
      }
    }
  }

  List<int> GetColumns(List<GamePiece> gamePieces)
  {
    List<int> columns = new List<int>();
    foreach (GamePiece gamePiece in gamePieces)
    {
      if (!columns.Contains(gamePiece.xIndex))
        columns.Add(gamePiece.xIndex);
    }
    return columns;
  }

  bool IsWithinBounds(int x, int y)
  {
    return (x >= 0 && x < width && y >= 0 && y < height);
  }

  #region Unused

  //GameObject GetRandomGamePiece()
  //{
  //  int randomIdx = Random.Range(0, gamePiecePrefabs.Length);

  //  if (gamePiecePrefabs[randomIdx] == null)
  //  {
  //    Debug.LogWarning("BOARD:  " + randomIdx + "does not contain a valid GamePiece prefab!");
  //  }

  //  return gamePiecePrefabs[randomIdx];
  //}

  //GamePiece FillRandomAt(int x, int y)
  //{
  //  GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;
  //  if (randomPiece != null)
  //  {
  //    randomPiece.GetComponent<GamePiece>().Init(this);
  //    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), x, y);
  //    randomPiece.transform.parent = transform;
  //  }
  //  return randomPiece.GetComponent<GamePiece>();
  //}

  //void ReplaceWithRandom(List<GamePiece> gamePieces)
  //{
  //  foreach (GamePiece piece in gamePieces)
  //  {
  //    ClearPieceAt(piece.xIndex, piece.yIndex);
  //    FillRandomAt(piece.xIndex, piece.yIndex);
  //  }
  //}
  #endregion

}
