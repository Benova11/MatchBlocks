using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
  public int MovesLeft { get { return MovesManager.Instance.CurrentMoves; } }




}
