public class LevelMoves : Level
{
    public int numMoves;
    public int targetScore;

    private int movesUsed = 0;

    protected override void Start()
    {
        base.Start();

        hud.SetLevelType();
        hud.SetTarget(targetScore);
        hud.SetRemaining(numMoves);
    }

    public override void OnMove()
    {
        movesUsed++;
        hud.SetRemaining(numMoves - movesUsed);

        if (numMoves == movesUsed)
        {
            if (currentScore >= targetScore)
                GameWin();
            else
                GameLose();
        }
    }

    public override void AddBonusMoves(int amount)
    {
        numMoves += amount;
        hud.SetRemaining(numMoves - movesUsed);
    }
}
