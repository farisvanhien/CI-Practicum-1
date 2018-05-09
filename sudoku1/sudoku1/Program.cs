using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace sudoku1
{
    class Program
    {
        static void Main(string[] args)
        {
            Sudoku sudoku = new Sudoku();
        }
    }
    class Sudoku
    {
        
        Random r = new Random();
        int threads;

        int[] sudoku = new int[81];
        bool[] start = new bool[81];
        
        int index;
        int indexHill;
        int swapIndex;

        int[,] rows = new int[9, 9];
        int[,] columns = new int[9, 9];

        int[] rowScores = new int[9];
        int[] columnScores = new int[9];

        int[] lastRowScores = new int[9];
        int[] lastColumnScores = new int[9];

        int counter = 0;

        int randomBlock;

        List<int> best1 = new List<int>();
        List<int> best2 = new List<int>();

        public Sudoku()
        {
            Console.Title = "SudokuSolver 9000";
            Stopwatch stopwatch = new Stopwatch();

            // make grid with input
            Console.WriteLine("Welcome to SudokuSolver 9000");
            Console.WriteLine("Please enter your sudoku in this format:");
            Console.Write("1 0 0 0 0 0 0 0 0\n0 2 0 0 0 0 0 0 0\n0 0 3 0 0 0 0 0 0\n0 0 0 4 7 9 0 0 0\n0 0 0 2 5 8 0 0 0\n0 0 0 1 3 6 0 0 0\n0 0 0 0 0 0 7 0 0\n0 0 0 0 0 0 0 8 0\n0 0 0 0 0 0 0 0 9\n");
            Console.WriteLine("");

            while (true)
            {
                try
                {
                    for (int rows = 0; rows < 9; rows++)
                    {
                        string[] input = Console.ReadLine().Split();
                        for (int cols = 0; cols < 9; cols++)
                        {
                            sudoku[rows * 9 + cols] = int.Parse(input[cols]);
                            if (sudoku[rows * 9 + cols] != 0)
                            {
                                start[rows * 9 + cols] = true;
                            }
                            else start[rows * 9 + cols] = false;
                        }
                    }
                    break;
                }
                catch
                {
                    Console.WriteLine("Please enter the sudoku in the right format");
                }
            }

            Console.WriteLine("");
            Console.WriteLine("How many threads do you want me to use?");
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out threads))
                {
                    break;
                }
                else Console.WriteLine("Please enter a single integer");
            }

            Console.WriteLine("");
            Console.WriteLine("Solving Sudoku...");
            Console.WriteLine("");
            stopwatch.Start();

            Parallel.For(0, threads, (thread, status) => {
                 
                 for (int horizontalBlock = 0; horizontalBlock < 3; horizontalBlock++)
                 {
                    // initiate the not-fixed
                    for (int verticalBlock = 0; verticalBlock < 3; verticalBlock++)
                     {
                         List<int> numbers = new List<int>();
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

                 indexHill = 0;

                 for (int number = 0; number < 9; number++)
                 {
                     int modulus = indexHill % 9;
                     int startRow = indexHill - modulus;
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

                     indexHill += 10;
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

                 for (uint i = 0; i < uint.MaxValue; i++) // give up; no solution
                {
                     getRandomBlock();

                     indexHill = index;

                     best1.Clear();
                     best2.Clear();

                     int bestScore = 0;

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

                             indexHill = index + rowNumber * 9 + columnNumber;

                             if (start[indexHill])
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
                                     swapIndex = index + swaprowNumber * 9 + swapcolumnNumber;

                                     if (start[swapIndex])
                                     {
                                         swapcolumnNumber++;
                                         continue;
                                     }

                                     int score = getScore(rowNumber, columnNumber, swaprowNumber, swapcolumnNumber);

                                     if (score > bestScore)
                                     {
                                         bestScore = score;
                                         best1.Clear();
                                         best2.Clear();
                                         best1.Add(indexHill);
                                         best2.Add(swapIndex);
                                     }
                                     else if (score == bestScore)
                                     {
                                         best1.Add(indexHill);
                                         best2.Add(swapIndex);
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

                     if (best1.Count > 0)
                         swap(bestScore);
                     else counter++;

                     if (counter == 50)
                     {
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
                            Console.WriteLine("I have found the solution in {0} seconds!", stopwatch.ElapsedMilliseconds / 1000f);
                             printSudoku();
                             break;
                         }
                            
                        // else randomwalk
                        randomWalk();
                        counter = 0;
                     }
                 }
                 
                 status.Stop();
                 Console.WriteLine("I could not find the solution :(");
                 Console.WriteLine("");
                 Console.WriteLine("Press Escape to exit");
                 while (true)
                 {
                     if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                         Environment.Exit(0);
                 }
             });

            
        }


        public void printSudoku()
        {
            Console.WriteLine("");
            for (int k = 0; k < 9; k++)
            {
                string output = "";
                for (int l = 0; l < 9; l++)
                {
                    if (l % 3 == 0 && l % 9 != 0)
                        output += "|";
                    else output += " ";
                    output += sudoku[k * 9 + l];

                }
                if (k % 3 == 0 && k != 0)
                    Console.WriteLine(" -----+-----+-----");

                Console.WriteLine(output);
            }
            Console.WriteLine("");
            Console.WriteLine("Press Escape to exit");
            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                    Environment.Exit(0);
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
            else if(columnNumber == swapcolumnNumber)
            {
                score = swapScore(rows, rowNumber + rowsOffset, swaprowNumber + rowsOffset);
            }
            else
            {
                score = swapScore(rows, rowNumber + rowsOffset, swaprowNumber + rowsOffset) + swapScore(columns, columnNumber + columnsOffset, swapcolumnNumber + columnsOffset);
            }

            return score;
        }

        public int swapScore(int[,] first, int number1, int number2)
        {
            int score = 0;
            int a = sudoku[swapIndex] - 1;
            int b = sudoku[indexHill] - 1;
            if (first[number1, a] == 0)
            {
                score++;
            }
            if (first[number2, a] == 1)
            {
                score--;
            }
            if (first[number2, b] == 0)
            {
                score++;
            }
            if (first[number1, b] == 1)
            {
                score--;
            }
            return score;
        }

        public void swap(int bestScore)
        {
            int random;

            if (bestScore == 0)
            {
                random = r.Next(best1.Count + 1);
                if (random == best1.Count)
                    return;
            }
            else
            {
                random = r.Next(best1.Count);
            }                

            int temp = sudoku[best1[random]];
            sudoku[best1[random]] = sudoku[best2[random]];
            sudoku[best2[random]] = temp;

            updateScore(best1[random], best2[random]);
        }

        public void didScoreChange()
        {
            if (Enumerable.SequenceEqual(rowScores, lastRowScores) && Enumerable.SequenceEqual(columnScores, lastColumnScores))
            {
                counter++;
            }
            else
            {
                rowScores.CopyTo(lastRowScores, 0);
                columnScores.CopyTo(lastColumnScores, 0);
                counter = 0;
            }
        }

        public void updateScore(int index1, int index2)
        {
            didScoreChange();
            
            int row1 = index1 / 9;
            int row2 = index2 / 9;

            int column1 = index1 % 9;
            int column2 = index2 % 9;

            rowScores[row1] = 0;
            rowScores[row2] = 0;
            columnScores[column1] = 0;
            columnScores[column2] = 0;

            Array.Clear(rows, row1 * 9, 9);
            Array.Clear(rows, row2 * 9, 9);

            Array.Clear(columns, column1 * 9, 9);
            Array.Clear(columns, column2 * 9, 9);

            if(row1 != row2)
            {
                int startRow = index1 - column1;
                rows[row1, sudoku[startRow] - 1]++;
                startRow++;
                while (startRow % 9 != 0)
                {
                    rows[row1, sudoku[startRow] - 1]++;
                    startRow++;
                }
            }

            if (column1 != column2)
            {
                int startColumn = column2;
                while (startColumn < 81)
                {
                    columns[column1, sudoku[startColumn] - 1]++;
                    startColumn += 9;
                }
            }

            if(row1 != row2)
            {
                int startRow = index2 - column2;
                rows[row2, sudoku[startRow] - 1]++;
                startRow++;
                while (startRow % 9 != 0)
                {
                    rows[row2, sudoku[startRow] - 1]++;
                    startRow++;
                }
            }
            
            if(column1 != column2)
            {
                int startColumn = column2;
                while (startColumn < 81)
                {
                    columns[column2, sudoku[startColumn] - 1]++;
                    startColumn += 9;
                }
            }

            for (int i = 0; i < 9; i++)
            {
                if (row1 != row2)
                {
                    if (rows[row1, i] == 0)
                        rowScores[row1]++;

                    if (rows[row2, i] == 0)
                        rowScores[row2]++;
                }

                if (column1 != column2)
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
            randomBlock = r.Next(9);  // get a random number
            switch (randomBlock) // get the correct index
            {
                case 0:
                    index = 0;
                    break;
                case 1:
                    index = 3;
                    break;
                case 2:
                    index = 6;
                    break;
                case 3:
                    index = 27;
                    break;
                case 4:
                    index = 30;
                    break;
                case 5:
                    index = 33;
                    break;
                case 6:
                    index = 54;
                    break;
                case 7:
                    index = 57;
                    break;
                case 8:
                    index = 60;
                    break;
            }
        }

        public void randomWalk()
        {
            int random1row;
            int random1column;

            int random2row;
            int random2column;

            int index1;
            int index2;

            int i = 0;
            while(i < 20)
            {
                getRandomBlock();

                while (true)
                {
                    random1row = r.Next(3);
                    random1column = r.Next(3);
                    random2row = r.Next(3);
                    random2column = r.Next(3);
                    index1 = index + random1row * 9 + random1column;
                    index2 = index + random2row * 9 + random2column;
                    if (index1 != index2 && !start[index1] && !start[index2])
                    {
                        break;
                    }
                }

                int temp = sudoku[index1];
                sudoku[index1] = sudoku[index2];
                sudoku[index2] = temp;
                i++;

                updateScore(index1, index2);
            }
        }
    }
}