﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace sudoku1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "SudokuSolver 9000";
            Sudoku sudoku = new Sudoku();
        }
    }
    class Sudoku
    {
        Random r = new Random();
        Stopwatch stopwatch = new Stopwatch();

        int[] sudoku = new int[81];
        bool[] start = new bool[81];

        int index;
        int blockIndex;
        int swapIndex;

        int[,] rows = new int[9, 9];
        int[,] columns = new int[9, 9];

        int[] rowScores = new int[9];
        int[] columnScores = new int[9];

        int localMaxDuplicate = 0;

        int randomBlock;

        int bestScore;

        bool notCorrectFormat;

        List<int> numbers = new List<int>(9);

        List<int> bestSwap1 = new List<int>();
        List<int> bestSwap2 = new List<int>();

        HashSet<string> localMax = new HashSet<string>();

        public Sudoku()
        {
            Console.WriteLine(
@"Welcome to SudokuSolver 9000
Please enter your sudoku in this format:
1 0 0 0 0 0 0 0 0
0 2 0 0 0 0 0 0 0
0 0 3 0 0 0 0 0 0
0 0 0 4 7 9 0 0 0
0 0 0 2 5 8 0 0 0
0 0 0 1 3 6 0 0 0
0 0 0 0 0 0 7 0 0
0 0 0 0 0 0 0 8 0
0 0 0 0 0 0 0 0 9
");

            while (true)
            {
                try
                {
                    notCorrectFormat = false;
                    for (int rows = 0; rows < 9; rows++)
                    {
                        string[] input = Console.ReadLine().Split();
                        for (int cols = 0; cols < 9; cols++)
                        {
                            if (int.Parse(input[cols]) > 9 || int.Parse(input[cols]) < 0)
                                notCorrectFormat = true;
                            sudoku[rows * 9 + cols] = int.Parse(input[cols]);
                            if (sudoku[rows * 9 + cols] != 0)
                            {
                                start[rows * 9 + cols] = true;
                            }
                            else start[rows * 9 + cols] = false;
                        }

                    }
                    if (notCorrectFormat)
                        throw new Exception();
                    break;
                }
                catch
                {
                    if (notCorrectFormat)
                    {
                        Console.WriteLine("Please enter numbers from 0 to 9, where 0 represents a empty space\n");
                    }
                    else
                    {
                        Console.WriteLine("Please enter the sudoku in the right format\n");
                    }
                }
            }
            Console.Clear();
            Console.WriteLine("Solving sudoku...\n");
            stopwatch.Start();

            for (int horizontalBlock = 0; horizontalBlock < 3; horizontalBlock++)
            {
                // initiate the not-fixed
                for (int verticalBlock = 0; verticalBlock < 3; verticalBlock++)
                {
                    numbers.Clear();
                    for (int verticalNumber = 0; verticalNumber < 3; verticalNumber++)
                    {
                        for (int horizontalNumber = 0; horizontalNumber < 3; horizontalNumber++)
                        {
                            index = horizontalBlock * 27 + verticalBlock * 3 + verticalNumber * 9 + horizontalNumber;

                            if (sudoku[index] != 0)
                            {
                                numbers.Add(sudoku[index]);
                            }
                        }
                    }

                    for (int verticalNumber = 0; verticalNumber < 3; verticalNumber++)
                    {
                        for (int horizontalNumber = 0; horizontalNumber < 3; horizontalNumber++)
                        {
                            index = horizontalBlock * 27 + verticalBlock * 3 + verticalNumber * 9 + horizontalNumber;

                            if (!start[index])  // free space
                            {
                                int random = r.Next(9) + 1;  // get a random number

                                while (numbers.Contains(random))    // only place unique numbers
                                {
                                    random++;
                                    if (random > 9) random = 1;
                                }
                                sudoku[index] = random;     // place the number
                                numbers.Add(random);        // add it to the list to avoid getting the same number
                            }
                        }
                    }
                }
            }

            index = 0;

            for (int number = 0; number < 9; number++)
            {
                int modulus = index % 9;
                int startRow = index - modulus;
                rows[number, sudoku[startRow] - 1]++;
                startRow++;
                while (startRow % 9 != 0)
                {
                    rows[number, sudoku[startRow] - 1]++;
                    startRow++;
                }
                int startColumn = modulus;
                while (startColumn < 81)
                {
                    columns[number, sudoku[startColumn] - 1]++;
                    startColumn += 9;
                }

                index += 10;
            }

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (rows[i, j] == 0)
                        rowScores[i]++;

                    if (columns[i, j] == 0)
                        columnScores[i]++;
                }
            }

            numbers.Clear(); // to be able to reuse numbers

            for (int i = 0; i < 123456789; i++) // give up; no solution
            {
                if (i % 1000000 == 0) Console.WriteLine(i + " " + stopwatch.ElapsedMilliseconds / 1000f);
                getRandomBlock();

                index = blockIndex;

                bestSwap1.Clear();
                bestSwap2.Clear();

                bestScore = 0;

                int columnNumber = 0;
                int rowNumber = 0;

                while (rowNumber < 3)
                {
                    while (columnNumber < 3)
                    {
                        if (rowNumber == 3 && columnNumber == 3)
                        {
                            columnNumber++;
                            break;
                        }

                        index = blockIndex + rowNumber * 9 + columnNumber;

                        if (start[index])
                        {
                            columnNumber++;
                            continue;
                        }

                        int swaprowNumber = rowNumber;
                        int swapcolumnNumber = columnNumber + 1;

                        while (swaprowNumber < 3)
                        {
                            while (swapcolumnNumber < 3)
                            {
                                swapIndex = blockIndex + swaprowNumber * 9 + swapcolumnNumber;

                                if (start[swapIndex])
                                {
                                    swapcolumnNumber++;
                                    continue;
                                }

                                int score = getScore(rowNumber, columnNumber, swaprowNumber, swapcolumnNumber);

                                if (score > bestScore)
                                {
                                    bestScore = score;
                                    bestSwap1.Clear();
                                    bestSwap2.Clear();
                                    bestSwap1.Add(index);
                                    bestSwap2.Add(swapIndex);
                                }
                                else if (score == bestScore)
                                {
                                    bestSwap1.Add(index);
                                    bestSwap2.Add(swapIndex);
                                }

                                swapcolumnNumber++;
                            }
                            swaprowNumber++;
                            swapcolumnNumber = 0;
                        }

                        columnNumber++;
                    }
                    columnNumber = 0;
                    rowNumber++;
                }

                // if a score is found with a better score than the original, thn swap
                if (bestSwap1.Count > 0)
                    swap();
                else if(!numbers.Contains(randomBlock))
                    numbers.Add(randomBlock);

                // if the score doenst change after swapping for 30 times in a row
                // then check if you have find the solution, otherwise randomwalk
                if (numbers.Count == 9)  
                {
                    // calculate the score
                    int score = 0;
                    foreach (int row in rowScores)
                    {
                        score += row;
                    }
                    foreach (int column in columnScores)
                    {
                        score += column;
                    }
                
                    if (score == 0)    // check if you got the global max
                    {
                        //Console.Clear();                                              TODO: UNCOMMENT WHEN SPEEDTESTING IS DONE
                        Console.WriteLine("I have found the solution in {0} seconds!", stopwatch.ElapsedMilliseconds / 1000f);
                        printSudoku(sudoku);
                        Console.WriteLine("duplicates = " + localMaxDuplicate);
                        Console.WriteLine("uniques = " + localMax.Count());
                        Console.WriteLine("ratio = " + localMaxDuplicate / (float)localMax.Count);
                        Console.WriteLine("states = " + i);
                        Exit();
                    }

                    // try to add sudoku, else duplicate counter +1
                    if(!localMax.Add(string.Join("", sudoku)))
                        localMaxDuplicate++;    // plateaus can be seen as localMaxDuplicate

                    // randomwalk, walk more if more duplicate localmaxima are found, to get out of a group of localmaxima
                    if (localMaxDuplicate % 100 == 0)
                        randomWalk(6);
                    else
                        randomWalk(3);
                    numbers.Clear();
                }
            }

            // no solution found within a time limit
            //Console.Clear();                                      TODO: UNCOMMENT WHEN SPEED TESTING IS DONE
            Console.WriteLine("I could not find the solution :(");
            Console.WriteLine("duplicates = " + localMaxDuplicate);
            Console.WriteLine("uniques = " + localMax.Count());
            Console.WriteLine("ratio = " + localMaxDuplicate / (float)localMax.Count);
            Exit();
        }


        public void printSudoku(int[] Sudoku)
        {
            Console.WriteLine();
            for (int k = 0; k < 9; k++)
            {
                string output = "";
                for (int l = 0; l < 9; l++)
                {
                    if (l % 3 == 0 && l % 9 != 0)
                        output += "|";
                    else output += " ";
                    output += Sudoku[k * 9 + l];

                }
                if (k % 3 == 0 && k != 0)
                    Console.WriteLine(" -----+-----+-----");

                Console.WriteLine(output);
            }
        }

        public void Exit()
        {
            Console.WriteLine("\nPress F5 to solve a new sudoku or press Esc to exit");
            ConsoleKeyInfo input;
            
            while (true)
            {
                input = Console.ReadKey(true);
                if (input.Key == ConsoleKey.Escape)
                    Environment.Exit(0);
                else if (input.Key == ConsoleKey.F5)
                {
                    Console.Clear();
                    Sudoku s = new Sudoku();
                }
            }
            
        }

        public int getScore(int rowNumber, int columnNumber, int swaprowNumber, int swapcolumnNumber)
        {
            int score = 0;
            int columnsOffset = (randomBlock % 3) * 3;
            int rowsOffset = (randomBlock / 3) * 3;

            if (rowNumber == swaprowNumber)
            {
                score = swapScore(columns, columnNumber + columnsOffset, swapcolumnNumber + columnsOffset);
            }
            else if (columnNumber == swapcolumnNumber)
            {
                score = swapScore(rows, rowNumber + rowsOffset, swaprowNumber + rowsOffset);
            }
            else
            {
                score = swapScore(rows, rowNumber + rowsOffset, swaprowNumber + rowsOffset) + swapScore(columns, columnNumber + columnsOffset, swapcolumnNumber + columnsOffset);
            }

            return score;
        }

        public int swapScore(int[,] matrix, int number1, int number2)
        {
            int score = 0;
            int a = sudoku[swapIndex] - 1;
            int b = sudoku[index] - 1;
            if (matrix[number1, a] == 0) // if the number doesn't exist in the column/row where he is going => score++
            {
                score++;
            }
            if (matrix[number2, a] == 1) // if the number that leaves only existed ones in the row/column, so non are left => score--
            {
                score--;
            }
            if (matrix[number2, b] == 0) // if the number doesn't exist in the column/row where he is going => score++
            {
                score++;
            }
            if (matrix[number1, b] == 1) // if the number that leaves only existed ones in the row/column, so non are left => score--
            {
                score--;
            }
            return score; // a higher score is better
        }

        public void swapNumbers(ref int number1, ref int number2)
        {
            int temp = number1;
            number1 = number2;
            number2 = temp;
        }

        public void swap()
        {
            int random;

            if (bestScore == 0) // if no swap gives the a better score than the original sudoku, then it has a change that it won't swap
            {
                if (!numbers.Contains(randomBlock))
                    numbers.Add(randomBlock);   // score didn't change so counter goes up

                random = r.Next(bestSwap1.Count + 1);
                if (random == bestSwap1.Count)
                    return;
            }
            else
            {
                numbers.Clear(); // score did change, so counter resets
                random = r.Next(bestSwap1.Count);
            }

            // swap two numbers
            swapNumbers(ref sudoku[bestSwap1[random]], ref sudoku[bestSwap2[random]]);

            updateScore(bestSwap1[random], bestSwap2[random]);
        }

        public void updateScore(int index1, int index2)
        {
            int row1 = index1 / 9;  //
            int row2 = index2 / 9;

            int column1 = index1 % 9;
            int column2 = index2 % 9;
            bool notSameRow = row1 != row2;
            bool notSameColumn = column1 != column2;

            if (notSameRow)
            {
                rowScores[row1] = 0;
                rowScores[row2] = 0;

                Array.Clear(rows, row1 * 9, 9);
                Array.Clear(rows, row2 * 9, 9);

                int startRow = index1 - column1;
                rows[row1, sudoku[startRow] - 1]++;
                startRow++;
                while (startRow % 9 != 0)
                {
                    rows[row1, sudoku[startRow] - 1]++;
                    startRow++;
                }

                startRow = index2 - column2;
                rows[row2, sudoku[startRow] - 1]++;
                startRow++;
                while (startRow % 9 != 0)
                {
                    rows[row2, sudoku[startRow] - 1]++;
                    startRow++;
                }
            }
        
            if (notSameColumn)
            {
                columnScores[column1] = 0;
                columnScores[column2] = 0;

                Array.Clear(columns, column1 * 9, 9);
                Array.Clear(columns, column2 * 9, 9);

                int startColumn = column1;
                while (startColumn < 81)
                {
                    columns[column1, sudoku[startColumn] - 1]++;
                    startColumn += 9;
                }

                startColumn = column2;
                while (startColumn < 81)
                {
                    columns[column2, sudoku[startColumn] - 1]++;
                    startColumn += 9;
                }
            }

            for (int i = 0; i < 9; i++)
            {
                if (notSameRow)
                {
                    if (rows[row1, i] == 0)
                        rowScores[row1]++;

                    if (rows[row2, i] == 0)
                        rowScores[row2]++;
                }

                if (notSameColumn)
                {
                    if (columns[column1, i] == 0)
                        columnScores[column1]++;

                    if (columns[column2, i] == 0)
                        columnScores[column2]++;
                }
            }
        }

        public void getRandomBlock()
        {
            randomBlock = r.Next(9);    // get a random number
            switch (randomBlock)        // get the correct index
            {
                case 0:
                    blockIndex = 0;
                    break;
                case 1:
                    blockIndex = 3;
                    break;
                case 2:
                    blockIndex = 6;
                    break;
                case 3:
                    blockIndex = 27;
                    break;
                case 4:
                    blockIndex = 30;
                    break;
                case 5:
                    blockIndex = 33;
                    break;
                case 6:
                    blockIndex = 54;
                    break;
                case 7:
                    blockIndex = 57;
                    break;
                case 8:
                    blockIndex = 60;
                    break;
            }
        }

        public void randomWalk(int number)
        {
            int random1row;
            int random1column;

            int random2row;
            int random2column;

            int index1;
            int index2;

            int i = 0;
            while (i < number)
            {
                getRandomBlock();

                while (true)
                {
                    random1row = r.Next(3);
                    random1column = r.Next(3);
                    random2row = r.Next(3);
                    random2column = r.Next(3);
                    index1 = blockIndex + random1row * 9 + random1column;
                    index2 = blockIndex + random2row * 9 + random2column;
                    if (index1 != index2 && !start[index1] && !start[index2])
                    {
                        break;  // if the the indeces are not the same and both are not fixed, then stop generating random indeces
                    }
                }

                swapNumbers(ref sudoku[index1], ref sudoku[index2]);
                i++;

                updateScore(index1, index2);
            }
        }
    }
}