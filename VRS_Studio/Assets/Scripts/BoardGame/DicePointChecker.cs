using UnityEngine;

public class DicePointChecker : MonoBehaviour
{
    GameManager GameManager { get { return GameManager.instance; } }

    int sumpoint = 0;

    private void Update()
    {
        for (int a = 0; a < GameManager.Dice.Length; a++)
        {
            if (GameManager.Dice[a].GetComponent<SmallObjectProperty>().canCauclate == false) return;
        }
        if (GameManager.gameMode == GameManager.GameMode.DiceMode)
        {
            if (GameManager.gameState == GameManager.GameState.Start)
                GameManager.CompareDicePoint(sumpoint);
            else if (GameManager.gameState == GameManager.GameState.Invalid)
                GameManager.RobotDicePoint = sumpoint;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.name == "WhiteDice") 
            sumpoint += int.Parse(other.gameObject.name);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.parent.name == "WhiteDice")
            sumpoint -= int.Parse(other.gameObject.name);
    }
}