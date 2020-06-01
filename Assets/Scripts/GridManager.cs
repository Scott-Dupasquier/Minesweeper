using System; // Math
using System.Collections;
using System.Collections.Generic;
using System.Drawing; // Point
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

// This class is where most events are controlled
// The grid manager maintains all cells as a whole
public class GridManager : MonoBehaviour
{
    // This grid
    public GameObject gridPanel;
    public GameObject BGWinPanel;
    public GameObject WinPanel;
    public GameObject cell;
    public GameObject[,] grid;
    public TMP_Text bombsRemainingText;
    public TMP_Text timerText;
    public TMP_Text winText;
    public Button restartButton;
    
    // Initialize grid variables
    private int rows;
    private int cols;
    // Max expand size on initial click
    private int maxExpand;

    // buffer size on the side of the grid
    private int buffer;
    private float cellSize;

    // Keep track of if the game is over
    private bool finished;
    private bool runTimer;
    private bool populated;

    // Total amount of bombs
    private int numBombs;
    // Amount of bombs the player has marked
    private int bombsMarked;
    private float timer;

    void Awake()
    {
        // Set some initial variables based on the decided difficults
        var difficulty = PlayerPrefs.GetString("difficulty");

        // maxExpand determines the amount of cells to be revealed on first click.
        // A higher number means a higher change for a large expansion
        if (difficulty == "easy")
        {
            rows = 8;
            cols = 8;
            numBombs = 10;
            maxExpand = 1;
        }
        else if (difficulty == "intermediate")
        {
            rows = 16;
            cols = 16;
            numBombs = 40;
            maxExpand = 2;
        }
        else
        {
            rows = 16;
            cols = 30;
            numBombs = 99;
            maxExpand = 6;
        }
    }

    void Start()
    {
        // Make empty grid
        grid = new GameObject[rows, cols];

        // Buffer for the grid (in pixels)
        buffer = 10;

        // Initialize basic variables
        finished = false;
        runTimer = false;
        populated = false;
        bombsMarked = 0;
        bombsRemainingText.text = numBombs.ToString() + "/" + numBombs.ToString();
        timer = 0;
        timerText.text = "0:00";

        BGWinPanel.SetActive(false);
        WinPanel.SetActive(false);

        // Set cell parent to Grid so it shows up on screen
        cell.transform.SetParent(GameObject.FindGameObjectWithTag("Grid").transform, false);
        GetGridSize();
        GenerateGrid();

        // Check for a win regularly
        InvokeRepeating("CheckWin", 1.0f, 0.3f);
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

            // Update the face to reflect the users action
            if (Input.GetMouseButtonDown(0))
            {
                restartButton.GetComponent<RestartButton>().Hold();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                restartButton.GetComponent<RestartButton>().Release();
            }
        }
    }

    // Called when the game ends in any way
    public void GameOver(string condition)
    {
        // Game has ended, reveal the board & stop timer
        if (condition != "win")
        {
            restartButton.GetComponent<RestartButton>().Death();
        }
        else
        {
            // Reveal panels
            BGWinPanel.SetActive(true);
            WinPanel.SetActive(true);

            winText.text = "You beat the game in " + ToTime(timer) + ", well done!";
            restartButton.GetComponent<RestartButton>().Win();

            // Any cells that the player didn't mark should be marked as safe
            MarkAllSafe();
        }

        // Update variables accordingly
        finished = true;
        runTimer = false;
    }

    public bool GetFinished()
    {
        return finished;
    }

    // Adjust the bomb amount by adding or removing a bomb
    public void AdjustBombAmt(int num)
    {
        bombsMarked += num;
        bombsRemainingText.text = (numBombs - bombsMarked).ToString() + "/" + numBombs.ToString();
    }

    // Called by a cell manager when a bomb is hit and we need to reveal the board
    public void RevealCells(int col, int row)
    {
        // Initial list is the selected cell
        List<GameObject> cellsToExpand = new List<GameObject> {grid[row, col]};
        StartCoroutine(Explode(new List<Point>(), cellsToExpand));
    }

    // Called by a cell manager when a 0 is revealed by the player
    public void RevealZeros(int col, int row)
    {
        // Initial list is the selected cell
        List<GameObject> cellsToExpand = new List<GameObject> {grid[row, col]};
        ExpandZeros(new List<Point>(), cellsToExpand);
    }

    // Player wants to close the win statement box
    public void CloseWin()
    {
        BGWinPanel.SetActive(false);
        WinPanel.SetActive(false);
    }

    // Called at the start to determine how large the grid is which determines cell size
    private void GetGridSize()
    {
        var rectTransform = transform.GetComponent<RectTransform>();

        // The cell size is the minimum of the height/width minus buffer * 2 (buffer for each side)
        // divided by the number of rows or columns
        cellSize = Math.Min((rectTransform.rect.width - (buffer * 2)) / cols, (rectTransform.rect.height - (buffer * 2)) / rows);
    }

    // Creates all the individual cells in the grid
    private void GenerateGrid()
    {
        for (int col = 0; col < cols; ++col)
        {
            for (int row = 0; row < rows; ++row)
            {
                // Make a new cell object based on the precalculated cell size
                var newCell = Instantiate(cell, new Vector2(buffer + (col * cellSize), -buffer - (row * cellSize)), Quaternion.identity);
                newCell.transform.SetParent(GameObject.FindGameObjectWithTag("Grid").transform, false);
                
                // Pass along cells info to its cell manager
                newCell.GetComponent<CellManager>().InitializeValues(cellSize, row, col);
                grid[row, col] = newCell;
            }
        }

        // Resize the grid bg to properly fit
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (buffer * 2) + (cellSize * rows));
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (buffer * 2) + (cellSize * cols));
    }

    // Takes the initial click and expands a safe zone, then fills rest of the board
    public void PopulateBoard(int col, int row)
    {
        if (populated)
        {
            // Don't populate twice
            return;
        }

        // Update some variables
        runTimer = true;
        populated = true;
        
        // Variables to be used for random selection
        System.Random r = new System.Random();
        int randX;
        int randY;
        int bombsPlaced = 0;

        // List of neighbouring points
        List<Point> nbrs;

        // Make an initial area size to be marked safe
        var safeSize = r.Next(1, maxExpand);
        List<Point> safe = new List<Point>();
        List<Point> possible = new List<Point>();
        Point point;

        do
        {
            // Mark initial as safe
            safe.Add(new Point(col, row));

            // Want to add all new neighbours as expansion possibilities
            nbrs = GetNeighbours(col, row);
            foreach (Point nbr in nbrs)
            {
                if (!possible.Contains(nbr))
                {
                    possible.Add(nbr);
                }
            }

            // Now choose the next safe cell
            point = possible[r.Next(0, possible.Count)];
            col = point.X;
            row = point.Y;
        } while (safe.Count < safeSize);

        // Place bombs now that a safe area is marked
        while (bombsPlaced < numBombs)
        {
            // Choose a cell to plant a bomb
            randX = r.Next(0, cols);
            randY = r.Next(0, rows);
            point = new Point(randX, randY);

            var currentCell = grid[randY, randX];
            var cManager = currentCell.GetComponent<CellManager>();

            // Make sure the cell is empty and not in safe or possible lists.
            // The reason it can't be in possible is due to safe cells needing a value
            // of 0, meaning all neighbouring cells (ie the ones in possible) can't be bombs
            if (cManager.GetValue() == 0 && !possible.Contains(point) && !safe.Contains(point))
            {
                // Space unoccupied, plant bomb
                cManager.SetValue(-1);
                ++bombsPlaced;
            }
        }

        int bombsTouching;
        // Update all cells to show how many bombs they're touching
        for (col = 0;  col < cols; ++col)
        {
            for (row = 0; row < rows; ++row)
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

                        // Check if cell contains a bomb and increment bombsTouching if so
                        if (nbrCell.GetComponent<CellManager>().GetValue() == -1)
                        {
                            ++bombsTouching;
                        }
                    } catch(Exception e) 
                    {
                        Debug.Log(e);
                    }
                }

                // Report the information to the cell manager
                cManager.SetValue(bombsTouching);
            }
        }
    }

    /*
       0 0 0    (x-1,y-1) (x,y-1) (x+1,y-1)
       0 1 0    (x-1,y)   (x,y)   (x+1,y)
       0 0 0    (x-1,y+1) (x,y+1) (x+1,y+1)
     */
     // Get all neighbours of a particular cell
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
                else if (y + row >= 0 && y + row < rows && x + col >= 0 && x + col < cols) // Boundary checks
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

    // Explode out from an area
    private IEnumerator Explode(List<Point> expanded, List<GameObject> cellsToExpand)
    {
        // squareSize is relative to explosion area
        var expandSize = 1;

        while (cellsToExpand.Count != 0)
        {
            // Get the current cell we're expanding and it's information
            var currentCell = cellsToExpand[0];
            var cManager = currentCell.GetComponent<CellManager>();
            var row = cManager.GetRow();
            var col = cManager.GetCol();
            var coords = new Point(col, row);

            // Add cell coords to ones we've expanded
            expanded.Add(coords);
            cellsToExpand.Remove(currentCell);

            // Add all new neighbours to cellsToExpand
            foreach (Point p in GetNeighbours(col, row))
            {
                var nbrCell = grid[p.Y, p.X];

                // Find out if the neighbour is new to us
                if (!expanded.Contains(p) && !cellsToExpand.Contains(nbrCell))
                {
                    cellsToExpand.Add(nbrCell);
                }
            }

            // Reveal cell
            cManager.Reveal();

            // Next area we want expanded
            var x = Math.Min(cols, col + expandSize) - Math.Max(0, col - expandSize);
            var y = Math.Min(rows, expandSize + row) - Math.Max(0, row - expandSize); 
            if (expanded.Count >= x * y)
            {
                // Wait a short amount of time to create the expansion effect
                yield return new WaitForSeconds(Math.Min(1/expandSize, 0.05f));
                expandSize += 1;
            }
        }
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

    // Called repeatedly to check if a win has occurred
    private void CheckWin()
    {
        if (finished)
        {
            // Game already over
            return;
        }

        // Find out how many cells are still unrevealed
        var unrevealed = 0;
        for (int row = 0; row < rows; ++row)
        {
            for (int col = 0; col < cols; ++col)
            {
                var cell = grid[row, col];
                if (!cell.GetComponent<CellManager>().GetRevealed())
                {
                    ++unrevealed;
                }
            }
        }

        // The game is won when amount of unrevealed cells matches the amount of bombs
        if (unrevealed == numBombs)
        {
            GameOver("win");
        }
    }

    // Any cells the player didn't mark during the game should have a flag on it
    private void MarkAllSafe()
    {
        // Any un-revealed cells at this point are bombs
        for (int row = 0; row < rows; ++row)
        {
            for (int col = 0; col < cols; ++col)
            {
                var cManager = grid[row, col].GetComponent<CellManager>();
                if (!cManager.GetRevealed())
                {
                    cManager.MarkSafe();
                }
            }
        }
    }
}
