using System; // Math
using System.Collections;
using System.Collections.Generic;
using System.Drawing; // Point
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public GameObject cell;
    public GameObject[,] grid;
    
    // Initialize grid variables
    private int rows;
    private int cols;
    private int[,] board;

    // buffer size on the side of the grid
    private int buffer;
    private float cellSize;

    private const int numBombs = 99;
    // Start is called before the first frame update
    void Start()
    {
        rows = 16;
        cols = 31;
        board = new int[rows, cols];
        grid = new GameObject[rows, cols];

        buffer = 10;

        // Set cell parent to Grid so it shows up on screen
        cell.transform.SetParent(GameObject.FindGameObjectWithTag("Grid").transform, false);
        GetGridSize();
        GenerateGrid();
    }

    // Update is called once per frame
    void Update()
    {
        
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
                
                newCell.GetComponent<CellManager>().SetSize(cellSize);
                grid[row, col] = newCell;
            }
        }

        var rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (buffer * 2) + (cellSize * rows));

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

    // Test function for printing out board
    // void PrintBoard()
    // {
    //     string board_str;

    //     for (int row = 0; row < rows; ++row)
    //     {
    //         board_str = "";
    //         for (int col = 0; col < cols; ++col)
    //         {
    //             if (board[row, col] != -1)
    //             {
    //                 board_str += " ";
    //             }
    //             board_str += board[row, col] + " ";
    //         }
    //         Debug.Log(board_str);
    //     }
    // }
}
