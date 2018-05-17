using System;
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
        #region Fields
        Random r = new Random();
        Stopwatch stopwatch = new Stopwatch();

        int[] sudoku = new int[81];             // the sudoku grid 
        bool[] fixedNumbers = new bool[81];     // is true if the number is fixed

        int index;
        int blockIndex;
        int swapIndex;

        int[][] rows = new int[9][];            // jagged array that keep track of how many times a number is in a row
        int[][] columns = new int[9][];         // jagged array that keep track of how many times a number is in a column

        int[] rowScores = new int[9];
        int[] columnScores = new int[9];

        int randomBlock;
        int bestScore;

        int localMaxDuplicate = 0;

        bool notCorrectNumbers;

        List<int> numbers = new List<int>(9);   // a list with multiple uses

        List<int> bestSwap1 = new List<int>();
        List<int> bestSwap2 = new List<int>();

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
            for(int row = 0; row < 9; row++)
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
                int startRow = index - modulus;
                rows[number][sudoku[startRow] - 1]++;   // if a number exists in a row +1 to the index of that number - 1
                startRow++;
                while (startRow % 9 != 0)
                {
                    rows[number][sudoku[startRow] - 1]++;
                    startRow++;
                }
                int startColumn = modulus;
                while (startColumn < 81)
                {
                    columns[number][sudoku[startColumn] - 1]++; // if a number exists in a column +1 to the index of that number - 1
                    startColumn += 9;
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

            numbers.Clear(); // to be able to reuse numbers in ILS
            #endregion

            #region ILS
            for (int i = 0; i < 123456789; i++) // iteration criterion; takes about 2.5 minutes at 2.9 GHz in Release
            {
                if (i % 1000000 == 0) Console.WriteLine(i + " " + stopwatch.ElapsedMilliseconds / 1000f);   // TODO: REMOVE WHEN DONE

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
                        if (rowNumber == 3 && columnNumber == 3)    // don't need to check swaps if it's the last number in the column
                        {
                            columnNumber++;
                            break;
                        }

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
                else if(!numbers.Contains(randomBlock)) 
                    numbers.Add(randomBlock);   // if you don't swap, then add the block to a list

                // if all blocks have been checked and the score didn't change
                // then check if you have the global max, else randomwalk
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
                        Console.WriteLine("duplicates = " + localMaxDuplicate); //TODO: REMOVE WHEN DONE TESTING
                        Console.WriteLine("uniques = " + localMax.Count()); //TODO: REMOVE WHEN DONE TESTING
                        Console.WriteLine("ratio = " + localMaxDuplicate / (float)localMax.Count);  //TODO: REMOVE WHEN DONE TESTING
                        Console.WriteLine("states = " + i); //TODO: REMOVE WHEN DONE TESTING
                        Exit();
                    }

                    // try to add sudoku, else duplicate counter +1
                    if(!localMax.Add(string.Join("", sudoku)))  // remember that plateaus can be seen as an unique localMax
                        localMaxDuplicate++;    

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
            Console.WriteLine("I could not find the solution in {0} seconds :(", stopwatch.ElapsedMilliseconds / 1000f);
            Console.WriteLine("duplicates = " + localMaxDuplicate); //TODO: REMOVE WHEN DONE TESTING
            Console.WriteLine("uniques = " + localMax.Count()); //TODO: REMOVE WHEN DONE TESTING
            Console.WriteLine("ratio = " + localMaxDuplicate / (float)localMax.Count);  //TODO: REMOVE WHEN DONE TESTING
            Exit();
            #endregion
        }

        #region Methods

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

        public int swapScore(int[][] matrix, int number1, int number2)
        {
            int score = 0;
            // number1 = the first row or column
            // number2 = the second row or column
            if (matrix[number1][sudoku[swapIndex] - 1] == 0) // if the number doesn't exist in the column/row where he is going => score++
            {
                score++;
            }
            if (matrix[number2][sudoku[swapIndex] - 1] == 1) // if the number that leaves only existed ones in the row/column, so non are left => score--
            {
                score--;
            }
            if (matrix[number2][sudoku[index] - 1] == 0)    // if the number doesn't exist in the column/row where he is going => score++
            {
                score++;
            }
            if (matrix[number1][sudoku[index] - 1] == 1)    // if the number that leaves only existed ones in the row/column, so non are left => score--
            {
                score--;
            }
            return score;   // a higher score is better
        }

        public void swap()
        {
            int random;

            if (bestScore == 0) // if no swap gives the a better score than the original sudoku, then it has a chance that it won't swap
            {
                if (!numbers.Contains(randomBlock))
                    numbers.Add(randomBlock);   // bestScore == 0, so the score doesn't change => add the block to the list to determine when to randomwalk

                random = r.Next(bestSwap1.Count + 1);
                if (random == bestSwap1.Count)  
                    return; // don't swap
            }
            else
            {
                numbers.Clear();    // score did change, so counter resets
                random = r.Next(bestSwap1.Count);
            }

            swapNumbers(ref sudoku[bestSwap1[random]], ref sudoku[bestSwap2[random]]);

            updateScore(bestSwap1[random], bestSwap2[random]);
        }

        public void swapNumbers(ref int number1, ref int number2)
        {
            int temp = number1;
            number1 = number2;
            number2 = temp;
        }

        public void updateScore(int index1, int index2) // this method is similar to when we first calculate the scores
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
                for(int i = 0; i < 9; i++)
                {
                    rows[row1][i] = 0;
                    rows[row2][i] = 0;
                }
                
                int startRow = index1 - column1;
                rows[row1][sudoku[startRow] - 1]++;
                startRow++;
                while (startRow % 9 != 0)
                {
                    rows[row1][sudoku[startRow] - 1]++;
                    startRow++;
                }

                startRow = index2 - column2;
                rows[row2][sudoku[startRow] - 1]++;
                startRow++;
                while (startRow % 9 != 0)
                {
                    rows[row2][sudoku[startRow] - 1]++;
                    startRow++;
                }
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

                int startColumn = column1;
                while (startColumn < 81)
                {
                    columns[column1][sudoku[startColumn] - 1]++;
                    startColumn += 9;
                }

                startColumn = column2;
                while (startColumn < 81)
                {
                    columns[column2][sudoku[startColumn] - 1]++;
                    startColumn += 9;
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

        public void randomWalk(int number)
        {
            int random1row;
            int random1column;

            int random2row;
            int random2column;

            int index1 = 0;
            int index2 = 1;

            for(int i = 0; i < number; i++)
            {
                getRandomBlock();
                int j;
                for (j = 0; j < 50; j++)    // give a max to the total tries, because if a block has 8 or 9 fixed numbers then no swap is possible
                {
                    random1row = r.Next(3);
                    random1column = r.Next(3);
                    random2row = r.Next(3);
                    random2column = r.Next(3);
                    index1 = blockIndex + random1row * 9 + random1column;
                    index2 = blockIndex + random2row * 9 + random2column;
                    if (index1 != index2 && !fixedNumbers[index1] && !fixedNumbers[index2])
                    {
                        break;  // if the the indeces are not the same and both are not fixed, then stop generating random indeces
                    }
                }
                if(j < 50)
                {
                    swapNumbers(ref sudoku[index1], ref sudoku[index2]);

                    updateScore(index1, index2);
                }
                
            }
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
            Console.WriteLine();
        }

        public void Exit()
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
#endregion
    }
}