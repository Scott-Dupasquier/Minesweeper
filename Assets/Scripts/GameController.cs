using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Drawing; // Point
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Board variables
    private int x_dimension;
    private int y_dimension;
    private int[,] board; // 2D game board array

    // Bomb variables
    private const int num_bombs = 99; // Total number of bombs on board
    private int num_cleared; // Amount of bombs user has marked

    // Start is called before the first frame update
    void Start()
    {
        // Need to initialize game board
        x_dimension = 31;
        y_dimension = 16;
        board = new int[y_dimension,x_dimension];
        PopulateBoard();

        num_cleared = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /* Populate the game board with bombs, then update
       non-bomb cells to store how many bombs they touch
     */
    private void PopulateBoard()
    {
        System.Random r = new System.Random();
        int rand_x;
        int rand_y;
        int bombs_placed = 0;

        while (bombs_placed < num_bombs)
        {
            rand_x = r.Next(0, x_dimension);
            rand_y = r.Next(0, y_dimension);
            if (board[rand_y, rand_x] == 0)
            {
                // Space unoccupied, plant bomb
                board[rand_y, rand_x] = -1;
                ++bombs_placed;
            }
        }

        List<Point> nbrs;
        int bombs_touching;
        // Update all cells to show how many bombs they're touching
        for (int row = 0; row < y_dimension; ++row)
        {
            for (int col = 0; col < x_dimension; ++col)
            {
                // If cell is a bomb amount they're touching doesn't matter
                if (board[row, col] == -1)
                {
                    continue;
                }

                bombs_touching = 0;

                // Get all neighbouring cells
                nbrs = GetNeighbours(col, row);
                foreach(Point p in nbrs)
                {
                    try
                    {
                        if (board[p.Y, p.X] == -1)
                        {
                            ++bombs_touching;
                        }
                    } catch(Exception e) 
                    {
                        Debug.Log("(" + p.X.ToString() + ", " + p.Y.ToString() + ")");
                    }
                }
                board[row, col] = bombs_touching;
            }
        }

        PrintBoard();
    }

    /* Function to be moved into Prefab script when possible
       0 0 0    (x-1,y-1) (x,y-1) (x+1,y-1)
       0 1 0    (x-1,y)   (x,y)   (x+1,y)
       0 0 0    (x-1,y+1) (x,y+1) (x+1,y+1)
     */
    private List<Point> GetNeighbours(int x, int y)
    {
        // Return an array of 2D coordinates of the cells neighbours
        List<Point> nbrs = new List<Point>();

        for (int row = -1; row < 2; ++row)
        {
            for (int col = -1; col < 2; ++col)
            {
                // Don't want to add center cell to list
                if (col == 0 && row == 0)
                {
                    continue;
                }
                else if (y + row >= 0 && y + row < y_dimension && x + col >= 0 && x + col < x_dimension)
                {
                    // Neighbour exists, add to list
                    Point nbr = new Point(x + col, y + row);
                    nbrs.Add(nbr);
                }
            }
        }

        return nbrs;
    }


    /* TEST FUNCTIONS, REMOVE AFTER */
    void PrintBoard()
    {
        string board_str;

        using (StreamWriter sw = File.CreateText("board.txt"))
        {
            for (int row = 0; row < y_dimension; ++row)
            {
                board_str = "";
                for (int col = 0; col < x_dimension; ++col)
                {
                    if (board[row, col] != -1)
                    {
                        board_str += " ";
                    }
                    board_str += board[row, col] + " ";
                }
                sw.WriteLine(board_str);
            }
        }
    }
}
