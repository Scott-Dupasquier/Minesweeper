using System; // Math
using System.Collections;
using System.Collections.Generic;
using System.Drawing; // Point
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class GridManager : MonoBehaviour
{
        // This grid
    public GameObject gridPanel;
    public GameObject cell;
    public GameObject[,] grid;
    public TMP_Text bombsRemainingText;
    public TMP_Text timerText;
    public Button restartButton;
    
    // Initialize grid variables
    private int rows;
    private int cols;
    private int[,] board;

    // buffer size on the side of the grid
    private int buffer;
    private float cellSize;

    // Keep track of if the game is over
    private bool finished;
    private bool runTimer;
    private bool firstClick;

    private const int numBombs = 99;
    private int bombsMarked;
    // toReveal contains the amount of safe spaces that still need to be revealed
    private int toReveal;
    private float timer;
    // Start is called before the first frame update
    void Start()
    {
        rows = 16;
        cols = 31;
        board = new int[rows, cols];
        grid = new GameObject[rows, cols];

        buffer = 10;

        finished = false;
        runTimer = false;
        firstClick = true;
        bombsMarked = 0;
        bombsRemainingText.text = numBombs.ToString() + "/" + numBombs.ToString();
        toReveal = (rows * cols) - numBombs;
        timer = 0;
        timerText.text = "0:00";

        // Set cell parent to Grid so it shows up on screen
        cell.transform.SetParent(GameObject.FindGameObjectWithTag("Grid").transform, false);
        GetGridSize();
        GenerateGrid();

        if (PlayerPrefs.HasKey("row"))
        {
            PlayerPrefs.DeleteKey("row");
            PlayerPrefs.DeleteKey("col");
        }
    }

    // Update is called once per frame
    void Update()
    {        
        if (!finished)
        {
            // Update timer first
            if (runTimer)
            {
                timer += Time.deltaTime;
                timerText.text = ToTime(timer);
            }

            if (Input.GetMouseButtonDown(0))
            {
                restartButton.GetComponent<RestartButton>().Hold();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                restartButton.GetComponent<RestartButton>().Release();
            }

            if (PlayerPrefs.HasKey("row"))
            {
                var row = PlayerPrefs.GetInt("row");
                var col = PlayerPrefs.GetInt("col");
                List<GameObject> cellsToExpand = new List<GameObject> {grid[row, col]};
                ExpandZeros(new List<Point>(), cellsToExpand);
                PlayerPrefs.DeleteKey("row");
                PlayerPrefs.DeleteKey("col");
            }

            if (PlayerPrefs.HasKey("finished"))
            {
                // Game has ended, reveal the board & stop timer
                if (PlayerPrefs.GetString("finished") != "win")
                {
                    RevealCells();
                    restartButton.GetComponent<RestartButton>().Death();
                }
                else
                {
                    timerText.text = "You win!";
                    restartButton.GetComponent<RestartButton>().Win();
                }

                PlayerPrefs.DeleteKey("finished");
                finished = true;
                runTimer = false;
            }

            // When toReveal == 0, the player has won
            if (toReveal == 0)
            {
                PlayerPrefs.SetString("finished", "win");
            }
        }
    }

    // On first click in grid start the timer and populate the board
    public void FirstClick()
    {
        if (!firstClick)
        {
            // Only needs to be done once
            return;
        }

        runTimer = true;

        firstClick = false;
    }

    public void AdjustBombAmt(int num)
    {
        Debug.Log("true");
        bombsMarked += num;
        bombsRemainingText.text = (numBombs - bombsMarked).ToString() + "/" + numBombs.ToString();
    }

    private void RevealCells()
    {
        for (int row = 0; row < rows; ++row)
        {
            for (int col = 0; col < cols; ++col)
            {
                var cManager = grid[row, col].GetComponent<CellManager>();
                if (!cManager.GetRevealed())
                {
                    cManager.Reveal();
                }
            }
        }
    }

    private void GetGridSize()
    {
        var rectTransform = transform.GetComponent<RectTransform>();
        cellSize = Math.Min((rectTransform.rect.width - (buffer * 2)) / cols, (rectTransform.rect.height - (buffer * 2)) / rows);
    }

    private void GenerateGrid()
    {
        for (int col = 0; col < cols; ++col)
        {
            for (int row = 0; row < rows; ++row)
            {
                var newCell = Instantiate(cell, new Vector2(buffer + (col * cellSize), -buffer - (row * cellSize)), Quaternion.identity);
                newCell.transform.SetParent(GameObject.FindGameObjectWithTag("Grid").transform, false);
                
                newCell.GetComponent<CellManager>().InitializeValues(cellSize, row, col);
                grid[row, col] = newCell;
            }
        }

        var rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (buffer * 2) + (cellSize * rows));
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (buffer * 2) + (cellSize * cols));

        PopulateBoard();
    }

    private void PopulateBoard()
    {
        System.Random r = new System.Random();
        int randX;
        int randY;
        int bombsPlaced = 0;

        while (bombsPlaced < numBombs)
        {
            randX = r.Next(0, cols);
            randY = r.Next(0, rows);

            var currentCell = grid[randY, randX];
            var cManager = currentCell.GetComponent<CellManager>();

            if (cManager.GetValue() == 0)
            {
                // Space unoccupied, plant bomb
                cManager.SetValue(-1);
                board[randY, randX] = -1;
                ++bombsPlaced;
            }
        }

        List<Point> nbrs;
        int bombsTouching;
        // Update all cells to show how many bombs they're touching
        for (int col = 0;  col < cols; ++col)
        {
            for (int row = 0; row < rows; ++row)
            {
                var currentCell = grid[row, col];
                var cManager = currentCell.GetComponent<CellManager>();
                // If cell is a bomb amount they're touching doesn't matter
                if (cManager.GetValue() == -1)
                {
                    continue;
                }

                bombsTouching = 0;

                // Get all neighbouring cells
                nbrs = GetNeighbours(col, row);
                foreach(Point p in nbrs)
                {
                    try
                    {
                        var nbrCell = grid[p.Y, p.X];
                        if (nbrCell.GetComponent<CellManager>().GetValue() == -1)
                        {
                            ++bombsTouching;
                        }
                    } catch(Exception e) 
                    {
                        Debug.Log(e);
                    }
                }
                cManager.SetValue(bombsTouching);
                board[row, col] = bombsTouching;
            }
        }
    }

    /*
       0 0 0    (x-1,y-1) (x,y-1) (x+1,y-1)
       0 1 0    (x-1,y)   (x,y)   (x+1,y)
       0 0 0    (x-1,y+1) (x,y+1) (x+1,y+1)
     */
    private List<Point> GetNeighbours(int x, int y)
    {
        // Return an array of 2D coordinates of the cells neighbours
        List<Point> nbrs = new List<Point>();

        for (int col = -1; col < 2; ++col)
        {
            for (int row = -1; row < 2; ++row)
            {
                // Don't want to add center cell to list
                if (col == 0 && row == 0)
                {
                    continue;
                }
                else if (y + row >= 0 && y + row < rows && x + col >= 0 && x + col < cols)
                {
                    // Neighbour exists, add to list
                    Point nbr = new Point(x + col, y + row);
                    nbrs.Add(nbr);
                }
            }
        }

        return nbrs;
    }

    // ExpandZeros expands all adjacent cells next to connected zeros
    private void ExpandZeros(List<Point> expanded, List<GameObject> cellsToExpand)
    {
        // No more cells, recursion finished
        if (cellsToExpand.Count == 0)
        {
            return;
        }

        // Get the current cell we're expanding and it's information
        var currentCell = cellsToExpand[0];
        var cManager = currentCell.GetComponent<CellManager>();
        var row = cManager.GetRow();
        var col = cManager.GetCol();
        var coords = new Point(col, row);

        // Add cell coords to ones we've expanded
        expanded.Add(coords);
        cellsToExpand.Remove(currentCell);
        
        // Only add neighbours for expansion if this is also a 0 cell
        if (cManager.GetValue() == 0)
        {
            // Add all new neighbours to cellsToExpand
            foreach (Point p in GetNeighbours(col, row))
            {
                var nbrCell = grid[p.Y, p.X];

                // Find out if the neighbour also has 0 bombs and is new to us
                if (!expanded.Contains(p) && !cellsToExpand.Contains(nbrCell))
                {
                    cellsToExpand.Add(nbrCell);
                }
            }
        }

        // Lock so it doesn't trigger re-expansion later
        cManager.SetLock(true);
        // Reveal cell
        cManager.Reveal();
        cManager.SetLock(false);

        // Recurse
        ExpandZeros(expanded, cellsToExpand);
    }

    // ToTime converts a float of seconds and milliseconds into minutes and seconds
    // time should be in the format of 112.48729 (seconds.milliseconds)
    private string ToTime(float time)
    {
        // Round down, don't care about milliseconds
        var secs = Math.Floor(time);
        var mins = Math.Floor(secs / 60);
        secs %= 60;
        return mins.ToString() + ":" + secs.ToString().PadLeft(2, '0');
    }

    // Test function for printing out board
    void PrintBoard()
    {
        string board_str;

        for (int row = 0; row < rows; ++row)
        {
            board_str = "";
            for (int col = 0; col < cols; ++col)
            {
                if (board[row, col] != -1)
                {
                    board_str += " ";
                }
                board_str += board[row, col] + " ";
            }
            Debug.Log(board_str);
        }
    }
}
