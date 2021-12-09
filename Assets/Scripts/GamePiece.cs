using UnityEngine;
using System.Collections;

public class GamePiece : MonoBehaviour
{

	public int xIndex;
	public int yIndex;

	Board motherBoard;

	bool isMoving = false;

	public InterpType interpolation = InterpType.SmootherStep;

	public enum InterpType
	{
		Linear,
		EaseOut,
		EaseIn,
		SmoothStep,
		SmootherStep
	};

	public MatchValue matchValue;

	public enum MatchValue
	{
		Yellow,
		Blue,
		Magenta,
		Indigo,
		Green,
		Teal,
		Red,
		Cyan,
		Wild
	}

	public void Init(Board board)
	{
		motherBoard = board;
	}

	public void SetCoord(int x, int y)
	{
		xIndex = x;
		yIndex = y;
	}

	public void Move(int destX, int destY, float timeToMove)
	{

		if (!isMoving)
		{

			StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
		}
	}


	IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
	{
		Vector3 startPosition = transform.position;

		bool reachedDestination = false;

		float elapsedTime = 0f;

		isMoving = true;

		while (!reachedDestination)
		{
			if (Vector3.Distance(transform.position, destination) < 0.01f)
			{

				reachedDestination = true;

				if (motherBoard != null)
				{
					motherBoard.PlaceGamePiece(this, (int)destination.x, (int)destination.y);

				}

				break;
			}

			elapsedTime += Time.deltaTime;

			float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);
			UseCustomInterpulation(t);

			transform.position = Vector3.Lerp(startPosition, destination, t);

			yield return null;
		}

		isMoving = false;
	}

	float UseCustomInterpulation(float t)
  {
		switch (interpolation)
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
