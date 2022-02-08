using TMPro;

public class GoalsManager : Singleton<GoalsManager>
{
  int currentGoals = 0;

  public int CurrentGoals { get { return currentGoals; } }


  public TextMeshProUGUI numOfGoalsText;

  public void SetGoals(int numOfGoals)
  {
    currentGoals = numOfGoals;
    UpdateGoalsText(currentGoals);
  }

  public void UpdateGoalsText(int goalValue)
  {
    if (numOfGoalsText != null)
    {
      numOfGoalsText.text = (goalValue / 3).ToString();
    }
  }

  public void AddGoal(int value)
  {
    currentGoals += value;
    UpdateGoalsText(currentGoals);
  }

  public void OnGoalAchieved(int value)
  {
    currentGoals -= value;
    UpdateGoalsText(currentGoals);
  }
}
