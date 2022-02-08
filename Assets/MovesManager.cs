using TMPro;

public class MovesManager : Singleton<MovesManager>
{
  int currentMoves = 0;

  public TextMeshProUGUI numofMovesText;

  public int CurrentMoves { get { return currentMoves;  } }

  public void SetMoves(int numOfMoves)
  {
    currentMoves = numOfMoves;
    UpdateMovesText(currentMoves);
  }

  public void UpdateMovesText(int movesValue)
  {
    if (numofMovesText != null)
    {
      numofMovesText.text = movesValue.ToString();
    }
  }

  public void AddMoves(int value)
  {
    currentMoves += value;
    UpdateMovesText(currentMoves);
  }

  public void OnTurnPlayed()
  {
    currentMoves--;
    UpdateMovesText(currentMoves);
  }
}
