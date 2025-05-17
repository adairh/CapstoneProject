using UnityEngine;

public enum MoveDirection
{
    Up,
    Left,
    Down,
    Right
}

public class InputManager : MonoBehaviour
{
    private MatchGameManager gm;

    private void Start()
    {
        gm = FindObjectOfType<MatchGameManager>();
    }

    private void Update()
    {
        if (gm.State == GameState.GameOver)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            gm.Move(MoveDirection.Right);
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            gm.Move(MoveDirection.Up);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            gm.Move(MoveDirection.Left);
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            gm.Move(MoveDirection.Down);
    }
}