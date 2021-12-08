using System.Collections;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
  public enum InterpType { Linear, EaseOut, EaseIn, SmoothStep, SmootherStep }
  public enum MatchValue { Yellow,Blue,Magenta,Indigo,Green,Teal,Red,Cyan,Wild}

  public InterpType interpulation = InterpType.SmootherStep;
  public MatchValue matchValue = MatchValue.Yellow;

  public int xIndex;
  public int yIndex;

  Board motherBoard;

  bool isMoving = false;

  public void Init(Board board)
  {
    motherBoard = board;
  }

  public void SetCoord(int x, int y)
  {
    xIndex = x;
    yIndex = y;
  }

  public void Move(int xDest, int yDest, float timeToMove)
  {
    if (!isMoving)
      StartCoroutine(MoveRutine(new Vector3(xDest, yDest, 0), timeToMove));
  }

  IEnumerator MoveRutine(Vector3 destination, float timeToMove)
  {
    isMoving = true;
    Vector3 startPosition = transform.position;
    bool reachedDestination = false;
    float elapsedTime = 0;

    while (!reachedDestination)
    {
      if (Vector3.Distance(transform.position, destination) < 0.01f)
      {
        reachedDestination = true;
        if (motherBoard != null)
          motherBoard.PlaceGamePiece(this, (int)destination.x, (int)destination.y); 
        isMoving = false;
       }

      elapsedTime += Time.deltaTime;
      float t = Mathf.Clamp(elapsedTime / timeToMove, 0, 1);
      AdjustInterpulationByType(t);

      transform.position = Vector3.Lerp(startPosition, destination, t);
      yield return null;
    }
  }

  float AdjustInterpulationByType(float t)
  {
    switch (interpulation)
    {
      case InterpType.Linear:
        break;
      case InterpType.EaseOut:
        t = Mathf.Sin(t * Mathf.PI * 0.5f);
        break;
      case InterpType.EaseIn:
        t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
        break;
      case InterpType.SmoothStep:
        t = t * t * (3 - 2 * t);
        break;
      case InterpType.SmootherStep:
        t = t * t * t * (t * (t * 6 - 15) + 10);
        break;
    }
    return t;
  }
}
