using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public enum PieceType
    {
        EMPTY,
        NORMAL,
        BLOCKED,
        COUNT,
    };

    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    };





    public int xDim;
    public int yDim;
    public float fillTime;

    public PiecePrefab[] piecePrefabs;
    public GameObject backgroundPrefab;
    public Level level;
    public List<Vector2Int> blockedCells;
    public List<Vector2Int> icedCells;

    private Dictionary<PieceType, GameObject> piecePrefabDict;
    private GamePiece[,] pieces;

    private GamePiece pressedPiece;
    private GamePiece enteredPiece;

    public bool gameOver = false;

    public bool isPaused = false;

    private bool isFilling = false;

    private bool isSwapping = false;

    private int cascadeDepth = 0;
    private readonly HashSet<GamePiece> crackedThisPass = new HashSet<GamePiece>();

    public bool IsFilling
    {
        get { return isFilling; }
    }

    public int CascadeDepth
    {
        get { return cascadeDepth; }
    }
    // Start is called before the first frame update
    void Awake()
    {
        piecePrefabDict = new Dictionary<PieceType, GameObject>();
        for(int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        for (int x = 0; x < xDim; x++)
        {
            for(int y = 0; y < yDim; y++)
            {
                GameObject background = (GameObject)Instantiate(backgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent = transform;
            }
        }

        pieces = new GamePiece[xDim, yDim];

       
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y] == null)
                {
                    SpawnNewPiece(x, y, IsBlockedCell(x, y) ? PieceType.BLOCKED : PieceType.EMPTY);

                }
            }
        }
        StartCoroutine(InitialFillRoutine());


    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator InitialFillRoutine()
    {
        // The very first fill must not score free points, so it runs the same gravity/spawn
        // step as a normal refill but skips ClearAllValidMatches entirely, then fixes up any
        // matches that happened to land by re-rolling colors instead of clearing pieces.
        isFilling = true;

        while (FillStep())
        {
            yield return new WaitForSeconds(fillTime);
        }

        RemoveInitialMatches();
        isFilling = false;

        ApplyIcedCells();
    }

    private void RemoveInitialMatches()
    {
        bool foundMatch = true;
        int safetyPasses = 0;

        while (foundMatch && safetyPasses < 100)
        {
            foundMatch = false;
            safetyPasses++;

            for (int y = 0; y < yDim; y++)
            {
                for (int x = 0; x < xDim; x++)
                {
                    GamePiece piece = pieces[x, y];

                    if (!piece.IsColored())
                    {
                        continue;
                    }

                    if (GetMatch(piece, x, y) != null)
                    {
                        foundMatch = true;
                        RerollColorUntilNoMatch(piece, x, y);
                    }
                }
            }
        }
    }

    private void RerollColorUntilNoMatch(GamePiece piece, int x, int y)
    {
        int attempts = 0;

        do
        {
            piece.ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, 4));
            attempts++;
        }
        while (GetMatch(piece, x, y) != null && attempts < 20);
    }

    private void ApplyIcedCells()
    {
        if (icedCells == null)
        {
            return;
        }

        for (int i = 0; i < icedCells.Count; i++)
        {
            int x = icedCells[i].x;
            int y = icedCells[i].y;
            GamePiece piece = pieces[x, y];

            if (piece != null && piece.IsColored() && piece.IceComponent == null)
            {
                piece.gameObject.AddComponent<IcePiece>();
            }
        }
    }

    public IEnumerator Fill()
    {
        bool needsRefill = true;
        isFilling = true;
       // int _y = yDim - 1;
        while (needsRefill)
        {
            yield return new WaitForSeconds(fillTime);
            while (FillStep())
            {

                yield return new WaitForSeconds(fillTime);
               // _y--;
            }
            cascadeDepth++;
            needsRefill = ClearAllValidMatches();
        }

        isFilling = false;
    }

    public bool FillStep()
    {
     
        
        
        bool movedPiece = false;
      
        
        // We're looking for pieces we can move down and the bottom row can not be moved down
         for(int y = yDim - 2; y >= 0; y--)
             // Going from bottom to top (ignoring the bottom row)
         {
             for(int x = 0; x < xDim; x++)
             {
                 GamePiece piece = pieces[x, y];

                 if (piece.IsMovable())
                 {
                     GamePiece pieceBelow = pieces[x, y + 1];

                     if(pieceBelow.Type == PieceType.EMPTY)
                     {
                         Destroy(pieceBelow.gameObject);
                         piece.MovableComponent.Move(x, y + 1, fillTime);
                         pieces[x, y + 1] = piece;
                         SpawnNewPiece(x, y, PieceType.EMPTY);
                         movedPiece = true;
                     }
                 }
             }
         }

        // Diagonal fill: a cell directly beneath an immovable piece (a blocked stone,
        // or a still-frozen ice piece) can never be fed from straight above, so let a
        // neighboring piece that's stuck (its own column is occupied right below it)
        // spill sideways-down into it instead.
        for (int y = yDim - 2; y >= 0; y--)
        {
            for (int x = 0; x < xDim; x++)
            {
                if (pieces[x, y].Type == PieceType.EMPTY || pieces[x, y].IsMovable())
                {
                    continue;
                }

                GamePiece target = pieces[x, y + 1];

                if (target.Type != PieceType.EMPTY)
                {
                    continue;
                }

                for (int dx = -1; dx <= 1; dx += 2)
                {
                    int sx = x + dx;

                    if (sx < 0 || sx >= xDim)
                    {
                        continue;
                    }

                    GamePiece diagPiece = pieces[sx, y];

                    if (!diagPiece.IsMovable())
                    {
                        continue;
                    }

                    GamePiece belowDiagPiece = pieces[sx, y + 1];

                    if (belowDiagPiece.Type == PieceType.EMPTY)
                    {
                        // It can fall straight down in its own column, leave it for the vertical pass.
                        continue;
                    }

                    Destroy(target.gameObject);
                    diagPiece.MovableComponent.Move(x, y + 1, fillTime);
                    pieces[x, y + 1] = diagPiece;
                    SpawnNewPiece(sx, y, PieceType.EMPTY);
                    movedPiece = true;
                    break;
                }
            }
        }

        for (int x = 0; x < xDim; x++)
        {

            GamePiece pieceBelow = pieces[x, 0];

            if(pieceBelow.Type == PieceType.EMPTY)
            {
                Destroy(pieceBelow.gameObject);
                GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, -1), Quaternion.identity);
                newPiece.transform.parent = transform;

                /*
                if(x == 0 && _y == 0) {
                    pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                    pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                    pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                    pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, 1));
                    movedPiece = true;
                }
                
                 

                
                else
                {
                   
                }*/

                pieces[x, 0] = newPiece.GetComponent<GamePiece>();
                pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, 4));
                movedPiece = true;

            }
        }
       
        
        return movedPiece; 
    }
    private bool IsBlockedCell(int x, int y)
    {
        if (blockedCells == null)
        {
            return false;
        }

        for (int i = 0; i < blockedCells.Count; i++)
        {
            if (blockedCells[i].x == x && blockedCells[i].y == y)
            {
                return true;
            }
        }

        return false;
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - xDim / 2.0f + x,
            transform.position.y + yDim / 2.0f - y);
    }

    public GamePiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity);
        newPiece.transform.parent = transform;

        pieces[x, y] = newPiece.GetComponent<GamePiece>();
        pieces[x, y].Init(x, y, this, type);

        return pieces[x, y];
    }

    public bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
        return (piece1.X == piece2.X && (int)Mathf.Abs(piece1.Y - piece2.Y) == 1)
            || (piece1.Y == piece2.Y && (int)Mathf.Abs(piece1.X - piece2.X) == 1);
    }

    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        if (gameOver || isPaused || isSwapping)
        {
            return;
        }

        if (piece1.IsMovable() && piece2.IsMovable())
        {
            StartCoroutine(SwapPiecesRoutine(piece1, piece2));
        }
    }

    private IEnumerator SwapPiecesRoutine(GamePiece piece1, GamePiece piece2)
    {
        isSwapping = true;
        AudioManager.Play("piece_swap");

        int piece1X = piece1.X;
        int piece1Y = piece1.Y;
        int piece2X = piece2.X;
        int piece2Y = piece2.Y;

        pieces[piece1X, piece1Y] = piece2;
        pieces[piece2X, piece2Y] = piece1;

        bool isMatch = GetMatch(piece1, piece2X, piece2Y) != null || GetMatch(piece2, piece1X, piece1Y) != null;

        piece1.MovableComponent.Move(piece2X, piece2Y, fillTime);
        piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

        yield return new WaitForSeconds(fillTime);

        if (isMatch)
        {
            cascadeDepth = 0;
            ClearAllValidMatches();
            pressedPiece = null;
            enteredPiece = null;
            StartCoroutine(Fill());
            level.OnMove();
        }
        else
        {
            AudioManager.Play("piece_swap_invalid");

            pieces[piece1X, piece1Y] = piece1;
            pieces[piece2X, piece2Y] = piece2;

            piece1.MovableComponent.Move(piece1X, piece1Y, fillTime);
            piece2.MovableComponent.Move(piece2X, piece2Y, fillTime);

            yield return new WaitForSeconds(fillTime);

            pressedPiece = null;
            enteredPiece = null;
        }

        isSwapping = false;
    }

    public void PressPiece(GamePiece piece)
    {
        pressedPiece = piece;
    }

    public void EnterPiece(GamePiece piece)
    {
        enteredPiece = piece;
    }

    public void ReleasePiece()
    {
        if(IsAdjacent(pressedPiece, enteredPiece))
        {
            SwapPieces(pressedPiece, enteredPiece);
        }
    }

    public List<GamePiece> GetMatch(GamePiece piece, int newX, int newY)
    {
        if (piece.IsColored())
        {
            ColorPiece.ColorType color = piece.ColorComponent.Color;
            List<GamePiece> horizontalPieces = new List<GamePiece>();
            List<GamePiece> verticalPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            // Checking horizontally
            horizontalPieces.Add(piece);

            for(int dir = 0; dir <= 1; dir++)
            {
                for(int xOffset = 1; xOffset < xDim; xOffset++)
                {
                    int x;

                    if(dir == 0)
                    { //Left
                        x = newX - xOffset;
                    }
                    else
                    { //Right
                        x = newX + xOffset;
                    }

                    if(x < 0 || x >= xDim)
                    {
                        break;
                    }

                    if (pieces[x, newY].IsColored() && pieces[x, newY].ColorComponent.Color == color)
                    {
                        horizontalPieces.Add(pieces[x, newY]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if(horizontalPieces.Count >= 3)
            {
                for(int i = 0; i < horizontalPieces.Count; i++)
                {
                    matchingPieces.Add(horizontalPieces[i]);
                }
            }

            if(matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }



            //Checking vertically
            verticalPieces.Add(piece);
            for (int dir = 0; dir <= 1; dir++)
            {
                for (int yOffset = 1; yOffset < yDim; yOffset++)
                {
                    int y;

                    if (dir == 0)
                    { //Up
                        y = newY - yOffset;
                    }
                    else
                    { //Down
                        y = newY + yOffset;
                    }

                    if (y < 0 || y >= yDim)
                    {
                        break;
                    }

                    if (pieces[newX, y].IsColored() && pieces[newX, y].ColorComponent.Color == color)
                    {
                        verticalPieces.Add(pieces[newX, y]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (verticalPieces.Count >= 3)
            {
                for (int i = 0; i < verticalPieces.Count; i++)
                {
                    matchingPieces.Add(verticalPieces[i]);
                }
            }

            if (matchingPieces.Count >= 3)
            {
                return matchingPieces;
            }


        }

        return null;   

    }

    public bool ClearAllValidMatches()
    {
        bool needsRefill = false;
        crackedThisPass.Clear();

        for(int y = 0; y < yDim; y++)
        {
            for(int x = 0; x < xDim; x++)
            {
                if (pieces[x, y].IsClearable())
                {
                    List<GamePiece> match = GetMatch(pieces[x, y], x, y);
                    if(match != null)
                    {
                        for(int i = 0; i < match.Count; i++)
                        {
                            if (ClearPiece(match[i].X, match[i].Y))
                            {
                                needsRefill = true;
                            }
                        }
                    }
                }
            }
        }
        return needsRefill;
    }

    public bool ClearPiece(int x, int y)
    {
        GamePiece piece = pieces[x, y];

        if (!piece.IsClearable() || piece.ClearableComponent.IsCleared)
        {
            return false;
        }

        if (crackedThisPass.Contains(piece))
        {
            // Already cracked earlier in this same pass; wait for the next pass to actually clear it.
            return false;
        }

        if (piece.IsFrozen())
        {
            piece.IceComponent.Crack();
            crackedThisPass.Add(piece);
            AudioManager.Play("ice_crack");
            return false;
        }

        piece.ClearableComponent.Clear();
        AudioManager.Play("piece_match");
        SpawnNewPiece(x, y, PieceType.EMPTY);

        return true;
    }

    public void GameOver()
    {
        gameOver = true;
    }
}
