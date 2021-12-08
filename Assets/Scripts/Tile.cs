using UnityEngine;

public class Tile : MonoBehaviour
{
  public int xIndex;
  public int yIndex;

  Board motherBoard;

  [SerializeField] SpriteRenderer spriteRenderer;

  void Start()
  {
    SetAlphaValue(0);
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.T))
      SetAlphaValue(spriteRenderer.color.a == 0 ? 1 : 0);
  }

  public void Init(int x, int y, Board board)
  {
    xIndex = x;
    yIndex = y;
    motherBoard = board;
  }

  public void SetAlphaValue(float newValue)
  {
    if (spriteRenderer.color.r == 1 && spriteRenderer.color.g == 1 && spriteRenderer.color.b == 1 && newValue == 1)
      newValue = 0.5f;
    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newValue);
  }

  void OnMouseDown()
  {
    if(motherBoard != null)
      motherBoard.ClickTile(this);
  }

  void OnMouseEnter()
  {
    if (motherBoard != null)
      motherBoard.DragToTile(this);
  }

  void OnMouseUp()
  {
    if (motherBoard != null)
      motherBoard.ReleaseTile();
  }
}
