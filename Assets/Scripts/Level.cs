using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{

    public Grid grid;

    public Hud hud;

    public int score1Star;
    public int score2Star;
    public int score3Star;

    protected int currentScore;

    protected bool didWin;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        hud.SetScore(currentScore);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void GameWin()
    {
        grid.GameOver();
        didWin = true;
        StartCoroutine(WaitForGridFill());
    }

    public virtual void GameLose()
    {
        grid.GameOver();
        didWin = false;
        StartCoroutine(WaitForGridFill());

    }

    public virtual void OnMove()
    {
        Debug.Log("You Moved!");

    }

    // Overridden by move-limited levels to actually extend the move budget.
    public virtual void AddBonusMoves(int amount) { }

    public void ResumeAfterBonus()
    {
        grid.gameOver = false;
        didWin = false;
    }

    public virtual void OnPieceCleared(GamePiece piece)
    {
        int baseScore = 0;

        if (piece.ColorComponent.Color == ColorPiece.ColorType.RED)
        {
            baseScore = 100;
        }
        else if (piece.ColorComponent.Color == ColorPiece.ColorType.GREEN)
        {
            baseScore = 150;
        }
        else if (piece.ColorComponent.Color == ColorPiece.ColorType.BLUE)
        {
            baseScore = 200;
        }
        else if (piece.ColorComponent.Color == ColorPiece.ColorType.YELLOW)
        {
            baseScore = 250;
        }

        // Cascades triggered by gravity (not the player's direct swap) score with
        // diminishing returns so a single lucky chain can't snowball into a huge score.
        float cascadeMultiplier = Mathf.Max(0.2f, 1f / (piece.GridRef.CascadeDepth + 1));
        currentScore += Mathf.RoundToInt(baseScore * cascadeMultiplier);

        hud.SetScore(currentScore);
    }

    protected virtual IEnumerator WaitForGridFill()
    {
        while (grid.IsFilling)
        {
            yield return 0;
        }

        if (didWin)
        {
            hud.OnGameWin(currentScore);
        }
        else
        {
            hud.OnGameLose(currentScore);
        }
    }
}
