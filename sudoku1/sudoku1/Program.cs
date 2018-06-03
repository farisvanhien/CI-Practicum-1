using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

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
        static int localMaxScore;
        static bool done;

        int GETPLATEAUS;
        int totalPlateaus;

        int localMaxDuplicate;

        #region Fields
        Random r = new Random();
        Stopwatch stopwatch = new Stopwatch();

        static int[] sudoku = new int[81];             // the sudoku grid 
        static bool[] fixedNumbers = new bool[81];     // is true if the number is fixed

        static int index;
        static int blockIndex;
        static int swapIndex;

        static int[][] rows = new int[9][];            // jagged array that keep track of how many times a number is in a row
        static int[][] columns = new int[9][];         // jagged array that keep track of how many times a number is in a column

        static int[] rowScores = new int[9];
        static int[] columnScores = new int[9];

        static int randomBlock;
        static int bestScore;

        static bool[] didScoreChange = new bool[9];   // an array that sets a element true if the score didn't increase after swapping
        int counter;    // a counter that keeps track of the amount of elements set to true in didScoreChange

        static int start;

        static bool notCorrectNumbers;
        static bool globalMax;

        static int[] lastLocalMax = new int[81];

        static List<int> numbers = new List<int>(9);

        static List<int> bestSwap1 = new List<int>(15);
        static List<int> bestSwap2 = new List<int>(15);

        HashSet<string> localMax = new HashSet<string>();
        #endregion

        public Sudoku()
        {
            #region Input
            // init jagged arrays
            for (int i = 0; i < 9; i++)
            {
                rows[i] = new int[9];
                columns[i] = new int[9];
            }

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
                    notCorrectNumbers = false;
                    // fill the grid with the given input
                    for (int rows = 0; rows < 9; rows++)
                    {
                        string[] input = Console.ReadLine().Split();
                        for (int cols = 0; cols < 9; cols++)
                        {
                            if (int.Parse(input[cols]) > 9 || int.Parse(input[cols]) < 0)
                                notCorrectNumbers = true;
                            sudoku[rows * 9 + cols] = int.Parse(input[cols]);
                            if (sudoku[rows * 9 + cols] != 0)
                            {
                                fixedNumbers[rows * 9 + cols] = true;
                            }
                            else fixedNumbers[rows * 9 + cols] = false;
                        }
                    }
                    if (notCorrectNumbers)
                        throw new Exception();
                    break;
                }
                catch
                {
                    if (notCorrectNumbers)
                    {
                        Console.WriteLine("Please enter numbers from 0 to 9, where 0 represents a empty space\n");
                    }
                    else
                    {
                        Console.WriteLine("Please enter the sudoku in the right format\n");
                    }
                }
            }

            //check blocks
            for (int verticalBlock = 0; verticalBlock < 3; verticalBlock++)
            {
                for (int horizontalBlock = 0; horizontalBlock < 3; horizontalBlock++)
                {
                    numbers.Clear();
                    for (int verticalNumber = 0; verticalNumber < 3; verticalNumber++)
                    {
                        for (int horizontalNumber = 0; horizontalNumber < 3; horizontalNumber++)
                        {
                            index = verticalBlock * 27 + horizontalBlock * 3 + verticalNumber * 9 + horizontalNumber;

                            if (fixedNumbers[index])
                            {
                                numbers.Add(sudoku[index]);
                            }
                        }
                    }

                    // check if numbers exists twice in a block
                    if (numbers.Count != numbers.Distinct().Count())
                    {
                        Console.Clear();
                        Console.WriteLine("This sudoku has the same number twice in block {0}", (horizontalBlock + 1) + (verticalBlock * 3));
                        Exit();
                    }
                }
            }

            //check rows
            for (int row = 0; row < 9; row++)
            {
                numbers.Clear();
                for (int column = 0; column < 9; column++)
                {
                    index = row * 9 + column;
                    if (fixedNumbers[index])
                    {
                        numbers.Add(sudoku[index]);
                    }
                }

                // check if numbers exists twice in a row
                if (numbers.Count != numbers.Distinct().Count())
                {
                    Console.Clear();
                    Console.WriteLine("This sudoku has the same number twice in row {0}", (row + 1));
                    Exit();
                }
            }

            //check columns
            for (int column = 0; column < 9; column++)
            {
                numbers.Clear();
                for (int row = 0; row < 9; row++)
                {
                    index = row * 9 + column;
                    if (fixedNumbers[index])
                    {
                        numbers.Add(sudoku[index]);
                    }
                }

                // check if numbers exists twice in a column
                if (numbers.Count != numbers.Distinct().Count())
                {
                    Console.Clear();
                    Console.WriteLine("This sudoku has the same number twice in column {0}", (column + 1));
                    Exit();
                }
            }

            Console.Clear();
            Console.WriteLine("Solving sudoku...\n");

            #endregion

            #region Initialize

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

                            if (fixedNumbers[index])
                            {
                                numbers.Add(sudoku[index]); // add fixed numbers to a list so we only get unique numbers in a block
                            }
                        }
                    }

                    for (int verticalNumber = 0; verticalNumber < 3; verticalNumber++)
                    {
                        for (int horizontalNumber = 0; horizontalNumber < 3; horizontalNumber++)
                        {
                            index = horizontalBlock * 27 + verticalBlock * 3 + verticalNumber * 9 + horizontalNumber;

                            if (!fixedNumbers[index])  // free space
                            {
                                int random = r.Next(9) + 1;  // get a random number

                                while (numbers.Contains(random))    // only place unique numbers
                                {
                                    random++;
                                    if (random > 9) random = 1;
                                }
                                sudoku[index] = random;     // place the number
                                numbers.Add(random);        // add it to the list to avoid getting the same number in a block
                            }
                        }
                    }
                }
            }

            index = 0;
            // fill rows and columns
            for (int number = 0; number < 9; number++)
            {
                int modulus = index % 9;
                start = index - modulus;
                do
                    rows [number] [sudoku [start++] - 1]++;    // if a number exists in a row +1 to the index of that number - 1
                while (start % 9 != 0);

                start = modulus;
                while (start < 81)
                {
                    columns[number][sudoku[start] - 1]++;   // if a number exists in a column +1 to the index of that number - 1
                    start += 9;
                }

                index += 10;
            }

            // calculate starting row- and columnscores
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (rows[i][j] == 0)
                        rowScores[i]++;

                    if (columns[i][j] == 0)
                        columnScores[i]++;
                }
            }
            #endregion

            #region ILS
            for (int i = 0; i < 125_000_000; i++) // iteration criterion; takes less than 2 minutes
            {
                if (i % 1_000_000 == 0) Console.WriteLine(i + " " + stopwatch.ElapsedMilliseconds / 1000f);   // TODO: REMOVE WHEN DONE

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
                        if (rowNumber == 2 && columnNumber == 2)    // don't need to check swaps if it's the last number in the column
                            break;

                        index = blockIndex + rowNumber * 9 + columnNumber;

                        if (fixedNumbers[index])    // if the number is fixed than go to the next number
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

                                if (fixedNumbers[swapIndex])    // if the swap number is fixed that go to the next swap number
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

                // if a score is found with a better score than the original, then swap
                if (bestSwap1.Count > 0)
                    swap();
                else if (didScoreChange[randomBlock] == false)  // if you don't swap than add it to the array
                {
                    didScoreChange[randomBlock] = true;
                    counter++;
                }

                //if (GETPLATEAUS > 0)
                //{
                //    int score = 0;
                //    foreach (int row in rowScores)
                //    {
                //        score += row;
                //    }
                //    foreach (int column in columnScores)
                //    {
                //        score += column;
                //    }

                //    if (localMaxScore == score)
                //    {
                //        if (localMax.Add(string.Join("", sudoku)))
                //            totalPlateaus++;
                //    }
                //    else done = true;

                //}
                //// if all blocks have been checked and the score didn't change
                //// then check if you have the global max, else randomwalk
                if (counter == 9 /*|| done*/)
                {
                //    if (GETPLATEAUS == 0)
                //    {
                //        // try to add sudoku, else duplicate counter +1
                //        if (!localMax.Add(string.Join("", sudoku)))  // remember that plateaus can be seen as an unique localMax
                //            localMaxDuplicate++;

                //        localMaxScore = 0;
                //        foreach (int row in rowScores)
                //        {
                //            localMaxScore += row;
                //        }
                //        foreach (int column in columnScores)
                //        {
                //            localMaxScore += column;
                //        }
                //    }

                //    GETPLATEAUS++;
                //    Array.Clear(walkrandom, 0, 9);
                //    counter = 0;
                //    if (GETPLATEAUS > 7 || done)
                //    {
                //        GETPLATEAUS = 0;
                //        done = false;
                        // calculate the score

                    globalMax = true;
                    foreach (int row in rowScores)
                    {
                        if (row > 0)
                        {
                            globalMax = false;
                            break;
                        }
                    }
                    if (globalMax)  // no need to check if globalmax isnt possible
                        foreach (int column in columnScores)
                        {
                            if (column > 0)
                            {
                                globalMax = false;
                                break;
                            }
                        }

                    if (globalMax)    // check if you got the global max
                    {
                        //Console.Clear();                                              TODO: UNCOMMENT WHEN SPEEDTESTING IS DONE
                        Console.WriteLine($"I have found the solution in {stopwatch.ElapsedMilliseconds / 1000f} seconds and {i} states!");
                        printSudoku(sudoku);
                        Console.WriteLine("duplicates = " + localMaxDuplicate); //TODO: REMOVE WHEN DONE TESTING
                        Console.WriteLine("uniques = " + (localMax.Count() - totalPlateaus)); //TODO: REMOVE WHEN DONE TESTING
                        Console.WriteLine("ratio = " + localMaxDuplicate / (float)(localMax.Count - totalPlateaus));  //TODO: REMOVE WHEN DONE TESTING
                        Exit();
                    }

                    // try to add sudoku, else duplicate counter +1
                    //if (!localMax.Add(string.Join("", sudoku)))  // remember that plateaus can be seen as an unique localMax
                    //    localMaxDuplicate++;

                    if (i > 10_000_000)
                        randomWalk(4);
                    else if (i > 1_000_000)
                        randomWalk(5);
                    else if (i > 250_000)
                        randomWalk(6);
                    //else if (counter == 3) // walk more if more duplicate localmaxima are found, to get out of a group of localmaxima
                    //{
                    //    randomWalk(6);
                    //    counter = 0;
                    //} 
                    else
                    {
                        //if (sudoku.SequenceEqual(lastLocalMax))
                        //    counter++;
                        //Array.Copy(sudoku, lastLocalMax, 81);
                        randomWalk(7);
                    }
                    Array.Clear(didScoreChange, 0, 9);
                    counter = 0;
                }
            }
            //}

            // no solution found within a time limit
            //Console.Clear();                                      TODO: UNCOMMENT WHEN SPEED TESTING IS DONE
            Console.WriteLine($"I could not find the solution in {stopwatch.ElapsedMilliseconds / 1000f} seconds :(");
            Console.WriteLine("duplicates = " + localMaxDuplicate); //TODO: REMOVE WHEN DONE TESTING
            Console.WriteLine("uniques = " + localMax.Count()); //TODO: REMOVE WHEN DONE TESTING
            Console.WriteLine("ratio = " + localMaxDuplicate / (float)localMax.Count);  //TODO: REMOVE WHEN DONE TESTING
            Exit();
            #endregion
        }

        #region Methods
        
        void getRandomBlock()
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

        int getScore(int rowNumber, int columnNumber, int swaprowNumber, int swapcolumnNumber)
        {
            int columnsOffset = columnOffsetTable();// instead of randomBlock % 3 * 3 for speed
            int rowsOffset = rowOffsetTable();      // instead of randomBlock / 3 * 3 for speed

            if (rowNumber == swaprowNumber)
                return swapScore(columns, columnNumber + columnsOffset, swapcolumnNumber + columnsOffset);
            if (columnNumber == swapcolumnNumber)
                return swapScore(rows, rowNumber + rowsOffset, swaprowNumber + rowsOffset);

            return swapScore(rows, rowNumber + rowsOffset, swaprowNumber + rowsOffset) + swapScore(columns, columnNumber + columnsOffset, swapcolumnNumber + columnsOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int swapScore(int[][] matrix, int num1, int num2)
        {
            int score = 0;
            // number1 = the first row or column
            // number2 = the second row or column

            if (matrix[num1][sudoku[swapIndex] - 1] == 0) // if the number doesn't exist in the column/row where he is going => score++
                score++;

            if (matrix[num2][sudoku[swapIndex] - 1] == 1) // if the number that leaves only existed ones in the row/column, so non are left => score--
                score--;

            if (matrix[num2][sudoku[index] - 1] == 0)    // if the number doesn't exist in the column/row where he is going => score++
                score++;

            if (matrix[num1][sudoku[index] - 1] == 1)    // if the number that leaves only existed ones in the row/column, so non are left => score--
                score--;

            return score;   // a higher score is better
        }
        
        void swap()
        {
            int random;

            if (bestScore == 0) // if no swap gives the a better score than the original sudoku, then it has a chance that it won't swap
            { 
                if (didScoreChange[randomBlock] == false) // bestScore == 0, so the score doesn't change => add the block to the array to determine when to randomwalk
                {
                    didScoreChange[randomBlock] = true;
                    counter++;
                }
                random = r.Next(bestSwap1.Count + 1);
                if (random == bestSwap1.Count)
                    return; // don't swap
            }
            else
            {
                Array.Clear(didScoreChange, 0, 9);
                counter = 0;
                random = r.Next(bestSwap1.Count);
            }

            swapNumbers(ref sudoku[bestSwap1[random]], ref sudoku[bestSwap2[random]]);

            updateScore(bestSwap1[random], bestSwap2[random]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void swapNumbers(ref int number1, ref int number2)
        {
            int temp = number1;
            number1 = number2;
            number2 = temp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void updateScore(int index1, int index2) // this method is similar to when we first calculate the scores
        {
            int row1 = index1 / 9;  // get the start of the row
            int row2 = index2 / 9;

            int column1 = index1 % 9;   // get the start of the column
            int column2 = index2 % 9;

            bool notSameRow = row1 != row2;             // to make sure that we don't do unnecessary updates
            bool notSameColumn = column1 != column2;    // to make sure that we don't do unnecessary updates

            if (notSameRow)
            {
                // reset rowScores
                rowScores[row1] = 0;
                rowScores[row2] = 0;

                // clear the rows
                for (int i = 0; i < 9; i++)
                {
                    rows[row1][i] = 0;
                    rows[row2][i] = 0;
                }

                start = index1 - column1;
                do
                    rows [row1] [sudoku [start++] - 1]++;   // +1 on the index of the number found -1 and go to the next number (startrow++)
                while (start % 9 != 0);

                start = index2 - column2;
                do
                    rows [row2] [sudoku [start++] - 1]++;   // +1 on the index of the number found -1 and go to the next number (startrow++)
                while (start % 9 != 0);
            }

            if (notSameColumn)
            {
                //reset columnscores
                columnScores[column1] = 0;
                columnScores[column2] = 0;

                // clear the columns
                for (int i = 0; i < 9; i++)
                {
                    columns[column1][i] = 0;
                    columns[column2][i] = 0;
                }

                start = column1;
                while (start < 81)
                {
                    columns[column1][sudoku[start] - 1]++;
                    start += 9;
                }

                start = column2;
                while (start < 81)
                {
                    columns[column2][sudoku[start] - 1]++;
                    start += 9;
                }
            }

            // calculate the scores
            for (int i = 0; i < 9; i++)
            {
                if (notSameRow)
                {
                    if (rows[row1][i] == 0)
                        rowScores[row1]++;

                    if (rows[row2][i] == 0)
                        rowScores[row2]++;
                }

                if (notSameColumn)
                {
                    if (columns[column1][i] == 0)
                        columnScores[column1]++;

                    if (columns[column2][i] == 0)
                        columnScores[column2]++;
                }
            }
        }
        
        void randomWalk(int number)
        {
            int index1 = 0;
            int index2 = 0;

            bool swapped;

            for (int i = 0; i < number; i++)
            {
                getRandomBlock();
                swapped = false;
                for (int j = 0; j < 50; j++)    // give a max to the total tries, because if a block has 8 or 9 fixed numbers then no swap is possible
                {
                    index1 = blockIndex + r.Next(3) * 9 + r.Next(3);
                    index2 = blockIndex + r.Next(3) * 9 + r.Next(3);
                    if (index1 != index2 && !fixedNumbers[index1] && !fixedNumbers[index2])
                    {
                        swapped = true;
                        break;  // if the the indeces are not the same and both are not fixed, then stop generating random indeces
                    }
                }
                if (swapped)
                {
                    swapNumbers(ref sudoku[index1], ref sudoku[index2]);

                    updateScore(index1, index2);
                }
            }
        }

        static void printSudoku(int[] Sudoku)
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
            Console.WriteLine();
        }

        static void Exit()
        {
            Console.WriteLine("\nPress F5 to solve a new sudoku\nPress Esc to exit");
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

        static int columnOffsetTable()
        {
            switch (randomBlock)
            {
                case 0:
                case 3:
                case 6:
                    return 0;
                case 1:
                case 4:
                case 7:
                    return 3;
                case 2:
                case 5:
                case 8:
                    return 6;
                default:
                    return -1;
            }
        }

        static int rowOffsetTable()
        {
            if (randomBlock < 3)
                return 0;
            if (randomBlock > 5)
                return 6;
            return 3;
        }
        #endregion
    }
}