﻿using System; // Math
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
    private int[,] board;

    // buffer size on the side of the grid
    private int buffer;
    private float cellSize;

    // Keep track of if the game is over
    private bool finished;
    private bool runTimer;
    private bool populated;

    private const int numBombs = 99;
    private int bombsMarked;
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

    public void GameOver(string condition)
    {
        // Game has ended, reveal the board & stop timer
        if (condition != "win")
        {
            // RevealCells();
            restartButton.GetComponent<RestartButton>().Death();
        }
        else
        {
            // Reveal panels
            BGWinPanel.SetActive(true);
            WinPanel.SetActive(true);

            winText.text = "You beat the game in " + ToTime(timer) + ", well done!";
            restartButton.GetComponent<RestartButton>().Win();
        }

        finished = true;
        runTimer = false;
    }

    public bool GetFinished()
    {
        return finished;
    }

    public void AdjustBombAmt(int num)
    {
        bombsMarked += num;
        bombsRemainingText.text = (numBombs - bombsMarked).ToString() + "/" + numBombs.ToString();
    }

    // Called by a cell manager when a bomb is hit and we need to reveal the board
    public void RevealCells(int col, int row)
    {
        List<GameObject> cellsToExpand = new List<GameObject> {grid[row, col]};
        StartCoroutine(Explode(new List<Point>(), cellsToExpand));
    }

    // Called by a cell manager when a 0 is revealed by the player
    public void RevealZeros(int col, int row)
    {
        List<GameObject> cellsToExpand = new List<GameObject> {grid[row, col]};
        ExpandZeros(new List<Point>(), cellsToExpand);
    }

    // Player wants to close the win statement box
    public void CloseWin()
    {
        BGWinPanel.SetActive(false);
        WinPanel.SetActive(false);
    }

    private void GetGridSize()
    {
        var rectTransform = transform.GetComponent<RectTransform>();
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

        runTimer = true;
        populated = true;
        
        System.Random r = new System.Random();
        int randX;
        int randY;
        int bombsPlaced = 0;

        // List of neighbouring points
        List<Point> nbrs;

        // Make an initial area size to be marked safe
        var safeSize = r.Next(1, 6);
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

        while (bombsPlaced < numBombs)
        {
            randX = r.Next(0, cols);
            randY = r.Next(0, rows);
            point = new Point(randX, randY);

            var currentCell = grid[randY, randX];
            var cManager = currentCell.GetComponent<CellManager>();

            if (cManager.GetValue() == 0 && !possible.Contains(point))
            {
                // Space unoccupied, plant bomb
                cManager.SetValue(-1);
                board[randY, randX] = -1;
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

    private void CheckWin()
    {
        if (finished)
        {
            // Game already over
            return;
        }

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

        if (unrevealed == numBombs)
        {
            GameOver("win");
        }
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
