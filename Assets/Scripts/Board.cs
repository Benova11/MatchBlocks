using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
  [SerializeField] int width;
  [SerializeField] int height;
  [SerializeField] int borderSize;

  [SerializeField] GameObject tilePrefab;
  public GameObject[] gamePiecePrefabs;

  [SerializeField] float swapTime = 0.25f;

  Tile[,] allTiles;
  GamePiece[,] allGamePieces;

  Tile clickedTile;
  Tile targetTile;

  void Start()
  {
    SetupTiles();
    //allGamePieces = new GamePiece[width, height];
    SetCameraSize();
    FillBoardRandomly();
    HighlightMatches();
  }

  void SetupTiles()
  {
    allTiles = new Tile[width, height];
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        GameObject tile = Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity, transform);
        tile.name = "Tile (" + i + "," + j + ")";
        allTiles[i, j] = tile.GetComponent<Tile>();
        allTiles[i, j].Init(i, j, this);
      }
    }
  }

  void SetCameraSize()
  {
    Camera.main.transform.position = new Vector3((float)(width - 1) / 2, (float)(height - 1) / 2, -10);

    float aspectRatio = (float)Screen.width / (float)Screen.height;

    float verticalSize = (float)height / 2 + (float)borderSize;

    float horizontalSize = ((float)width / 2 + (float)borderSize) / aspectRatio;

    Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
  }

  GameObject GetRandomGamePiece()
  {
    int randomIdx = Random.Range(0, gamePiecePrefabs.Length);
    if (gamePiecePrefabs[randomIdx] == null)
      Debug.LogWarning("BOARD:  " + randomIdx + "does not contaion a valid GamePiece prefab!");
    return gamePiecePrefabs[randomIdx];
  }

  public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
  {
    if (gamePiece == null)
    {
      Debug.LogWarning("BOARD: Ivalid GamePiece!");
      return;
    }

    gamePiece.transform.position = new Vector3(x, y, 0);
    gamePiece.transform.rotation = Quaternion.identity;
    if (IsWithinBoardBounds(x, y))
      allGamePieces[x, y] = gamePiece;
    gamePiece.SetCoord(x, y);
  }

  void FillBoardRandomly()
  {
    allGamePieces = new GamePiece[width, height];
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);
        if (randomPiece != null)
        {
          randomPiece.GetComponent<GamePiece>().Init(this);
          randomPiece.transform.SetParent(transform);
          PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), i, j);
        }
      }
    }
  }

  public void ClickTile(Tile tile)
  {
    if (clickedTile == null)
    {
      clickedTile = tile;
      //Debug.Log($"clicked tile: {tile.name}");
    }
  }

  public void DragToTile(Tile tile)
  {
    if (clickedTile != null)
      targetTile = tile;
  }

  public void ReleaseTile()
  {
    if (clickedTile != null && targetTile != null)
    {
      SwitchTilesPieces(clickedTile, targetTile);
    }
    clickedTile = null;
    targetTile = null;

  }

  void SwitchTilesPieces(Tile clickedTile, Tile targetTile)
  {
    if (IsPiecesAdjecent(clickedTile, targetTile))
    {
      GamePiece clickedPiece = allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
      GamePiece targetPiece = allGamePieces[targetTile.xIndex, targetTile.yIndex];

      clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
      targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
    }
  }

  List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
  {
    List<GamePiece> matches = new List<GamePiece>();
    GamePiece startPiece = null;

    if (IsWithinBoardBounds(startX, startY))
      startPiece = allGamePieces[startX, startY];

    if (startPiece != null)
      matches.Add(startPiece);
    else return null;

    int nextX;
    int nextY;

    int maxValue = (width > height) ? width : height;

    for (int i = 1; i < maxValue - 1; i++)
    {
      nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
      nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

      if (!IsWithinBoardBounds(nextX, nextY))
        break;

      GamePiece nextGamePiece = allGamePieces[nextX, nextY];

      if (nextGamePiece.matchValue == startPiece.matchValue && !matches.Contains(nextGamePiece))
        matches.Add(nextGamePiece);
      else break;
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

    if (rightMatches == null) rightMatches = new List<GamePiece>();
    if (leftMatches == null) leftMatches = new List<GamePiece>();

    List<GamePiece> combinedMatches = rightMatches.Union(leftMatches).ToList();

    return (combinedMatches.Count >= minLength) ? combinedMatches : null;
  }

  void HighlightMatches()
  {
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        SpriteRenderer spriteRenderer = allTiles[i, j].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);

        List<GamePiece> combinedMatches = FindMatchsFrom(i, j);
        if (combinedMatches.Count > 0)
        {
          foreach (GamePiece piece in combinedMatches)
          {
            spriteRenderer = allTiles[piece.xIndex, piece.yIndex].GetComponent<SpriteRenderer>();
            spriteRenderer.color = piece.GetComponent<SpriteRenderer>().color;
          }
        }
      }
    }
  }

  private List<GamePiece> FindMatchsFrom(int i, int j, int minLength = 3)
  {
    List<GamePiece> horizontalMatches = FindHorizontalMatches(i, j, minLength);
    List<GamePiece> verticalMatches = FindVerticalMatches(i, j, minLength);

    if (horizontalMatches == null) horizontalMatches = new List<GamePiece>();
    if (verticalMatches == null) verticalMatches = new List<GamePiece>();

    List<GamePiece> combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
    return combinedMatches;
  }

  bool IsPiecesAdjecent(Tile tileA, Tile tileB)
  {
    return (Mathf.Abs(tileA.xIndex - tileB.xIndex) <= 1 && tileA.yIndex == tileB.yIndex)
        || (Mathf.Abs(tileA.yIndex - tileB.yIndex) <= 1 && tileA.xIndex == tileB.xIndex);
  }

  bool IsWithinBoardBounds(int x, int y)
  {
    return (x >= 0 && x < width && y >= 0 && y < height);
  }
}
