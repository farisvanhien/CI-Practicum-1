using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        int[] sudoku = new int[81];
        bool[] start = new bool[81];
        Random r = new Random();

        int indexHill;
        int swapIndex;

        // index 0 = number 1... etc
        int[] row1 = new int[9];
        int[] column1 = new int[9];

        int[] row2 = new int[9];
        int[] column2 = new int[9];

        int[] row3 = new int[9];
        int[] column3 = new int[9];


        List<int> best1 = new List<int>();
        List<int> best2 = new List<int>();

        public Sudoku()
        {

            int index = 0;

            // make grid with input
            for(int rows = 0; rows < 9; rows++)
            {
                string[] input = Console.ReadLine().Split();
                for(int cols = 0; cols < 9; cols++)
                {
                    sudoku[rows * 9 + cols] = int.Parse(input[cols]);
                    if(sudoku[rows * 9 + cols] != 0)
                    {
                        start[rows * 9 + cols] = true;
                    }
                    else start[rows * 9 + cols] = false;
                }
            }

            for(int horizontalBlock = 0; horizontalBlock < 3; horizontalBlock++)
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

            Console.WriteLine("Randomly filled in:");
            printSudoku();

            
            int randomBlock = r.Next(9) + 1;  // get a random number
            switch (randomBlock) // get the correct index
            {
                case 1:
                    index = 0;
                    break;
                case 2:
                    index = 3;
                    break;
                case 3:
                    index = 6;
                    break;
                case 4:
                    index = 27;
                    break;
                case 5:
                    index = 30;
                    break;
                case 6:
                    index = 33;
                    break;
                case 7:
                    index = 54;
                    break;
                case 8:
                    index = 57;
                    break;
                case 9:
                    index = 60;
                    break;
            }


            indexHill = index;
            Console.WriteLine("startscore of block {0} is {1}", randomBlock, blockScore(index));
            Console.WriteLine("");


            // hebben we dit eigenlijk wel nodig?

            /*
            for (int number = 0; number < 9; number++) // calculate the starting scores of a block
            {
                switch (number)
                {
                    case 0:
                        scores[0] = 18 - row1dis - column1dis;
                        break;
                    case 1:
                        scores[1] = 18 - row1dis - column2dis;
                        break;
                    case 2:
                        scores[2] = 18 - row1dis - column3dis;
                        break;
                    case 3:
                        scores[3] = 18 - row2dis - column1dis;
                        break;
                    case 4:
                        scores[4] = 18 - row2dis - column2dis;
                        break;
                    case 5:
                        scores[5] = 18 - row2dis - column3dis;
                        break;
                    case 6:
                        scores[6] = 18 - row3dis - column1dis;
                        break;
                    case 7:
                        scores[7] = 18 - row3dis - column2dis;
                        break;
                    case 8:
                        scores[8] = 18 - row3dis - column3dis;
                        break;
                }
            }
            */

            
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

                    while(swaprowNumber < 3)
                    {
                        while(swapcolumnNumber < 3)
                        {
                            swapIndex = index + swaprowNumber * 9 + swapcolumnNumber;

                            if (start[swapIndex])
                            {
                                swapcolumnNumber++;
                                continue;
                            }

                            int score = getScore(rowNumber, columnNumber, swaprowNumber, swapcolumnNumber, column1, column2, column3, row1, row2, row3);

                            if(score >= bestScore)
                            {
                                bestScore = score;
                                best1.Clear();
                                best2.Clear();
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

            swap();

            Console.WriteLine("Answer:");
            printSudoku();

            Console.WriteLine("score of block {0} is {1}", randomBlock, blockScore(index));
            Console.ReadKey();
            
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
        }
        public int getScore(int rowNumber, int columnNumber, int swaprowNumber, int swapcolumnNumber, int[] column1, int[] column2, int[] column3, int[] row1, int[] row2, int[] row3)
        {
            int score = 0;
            // dit kan wss beter...
            // mss met switch case?
            if (rowNumber == 0)
            {
                if (columnNumber == 0)
                {
                    if (swaprowNumber == 0)
                    {
                        if (swapcolumnNumber == 1)           // 0,0 en 0,1
                        {
                            score = swapScore(column1, column2);
                        }
                        else if (swapcolumnNumber == 2)      // 0,0 en 0,2
                        {
                            score = swapScore(column1, column3);
                        }
                    }
                    else if (swaprowNumber == 1)
                    {
                        if (swapcolumnNumber == 0)          // 0,0 en 1,0
                        {
                            score = swapScore(row1, row2);
                        }
                        else if (swapcolumnNumber == 1)     // 0,0 en 1,1
                        {
                            score = swapScore(row1, row2) + swapScore(column1, column2);
                        }
                        else                                // 0,0 en 1,2
                        {
                            score = swapScore(row1, row2) + swapScore(column1, column3);
                        }
                    }
                    else
                    {
                        if (swapcolumnNumber == 0)          // 0,0 en 2,0   
                        {
                            score = swapScore(row1, row3);
                        }
                        else if (swapcolumnNumber == 1)     // 0,0 en 2,1
                        {
                            score = swapScore(row1, row3) + swapScore(column1, column2);
                        }
                        else                                // 0,0 en 2,2
                        {
                            score = swapScore(row1, row3) + swapScore(column1, column3);
                        }
                    }
                }
                else if (columnNumber == 1)
                {
                    if (swaprowNumber == 0)
                    {
                        if (swapcolumnNumber == 2)          // 0,1 en 0,2
                        {
                            score = swapScore(column2, column3);
                        }
                    }
                    else if (swaprowNumber == 1)
                    {
                        if (swapcolumnNumber == 0)          // 0,1 en 1,0
                        {
                            score = swapScore(column2, column1) + swapScore(row1, row2);
                        }
                        else if (swapcolumnNumber == 1)     // 0,1 en 1,1
                        {
                            score = swapScore(row1, row2);
                        }
                        else                                // 0,1 en 1,2
                        {
                            score = swapScore(column2, column3) + swapScore(row1, row2);
                        }
                    }
                    else
                    {
                        if (swapcolumnNumber == 0)          // 0,1 en 2,0
                        {
                            score = swapScore(column2, column1) + swapScore(row1, row3);
                        }
                        else if (swapcolumnNumber == 1)     // 0,1 en 2,1
                        {
                            score = swapScore(row1, row3);
                        }
                        else                                // 0,1 en 2,2
                        {
                            score = swapScore(column2, column3) + swapScore(row1, row2);
                        }
                    }
                }
                else
                {
                    if (swaprowNumber == 1)
                    {
                        if (swapcolumnNumber == 0)          // 0,2 en 1,0
                        {
                            score = swapScore(column3, column1) + swapScore(row1, row2);
                        }
                        else if (swapcolumnNumber == 1)     // 0,2 en 1,1
                        {
                            score = swapScore(column3, column2) + swapScore(row1, row2);
                        }
                        else                                // 0,2 en 1,2
                        {
                            score = swapScore(row1, row2);
                        }
                    }
                    else if (swaprowNumber == 2)
                    {
                        if (swapcolumnNumber == 0)          // 0,2 en 2,0
                        {
                            score = swapScore(column3, column1) + swapScore(row1, row3);
                        }
                        else if (swapcolumnNumber == 1)     // 0,2 en 2,1
                        {
                            score = swapScore(column3, column2) + swapScore(row1, row3);
                        }
                        else                                // 0,2 en 2,2
                        {
                            score = swapScore(row1, row3);
                        }
                    }
                }
            }
            else if (rowNumber == 1)
            {
                if (columnNumber == 0)
                {
                    if (swaprowNumber == 1)
                    {
                        if (swapcolumnNumber == 1)          // 1,0 en 1,1
                        {
                            score = swapScore(column1, column2);
                        }
                        else if (swapcolumnNumber == 2)     // 1,0 en 1,2
                        {
                            score = swapScore(column1, column3);
                        }

                    }
                    else if (swaprowNumber == 2)
                    {
                        if (swapcolumnNumber == 0)          // 1,0 en 2,0
                        {
                            score = swapScore(row2, row3);
                        }
                        else if (swapcolumnNumber == 1)     // 1,0 en 2,1
                        {
                            score = swapScore(column1, column2) + swapScore(row2, row3);
                        }
                        else                                // 1,0 en 2,2
                        {
                            score = swapScore(column1, column3) + swapScore(row2, row3);
                        }
                    }
                }
                else if (columnNumber == 1)
                {
                    if (swaprowNumber == 1)
                    {
                        if (swapcolumnNumber == 2)          // 1,1 en 1,2
                        {
                            score = swapScore(column2, column3);
                        }
                    }
                    else if (swaprowNumber == 2)
                    {
                        if (swapcolumnNumber == 0)          // 1,1 en 2,0
                        {
                            score = swapScore(column2, column1) + swapScore(row2, row3);
                        }
                        else if (swapcolumnNumber == 1)     // 1,1 en 2,1
                        {
                            score = swapScore(row2, row3);
                        }
                        else                                // 1,1 en 2,2
                        {
                            score = swapScore(column2, column3) + swapScore(row2, row3);
                        }
                    }
                }
                else
                {
                    if (swaprowNumber == 2)
                    {
                        if (swapcolumnNumber == 0)          // 1,2 en 2,0
                        {
                            score = swapScore(column3, column1) + swapScore(row2, row3);
                        }
                        else if (swapcolumnNumber == 1)     // 1,2 en 2,1
                        {
                            score = swapScore(column3, column2) + swapScore(row2, row3);
                        }
                        else                                // 1,2 en 2,2
                        {
                            score = swapScore(row2, row3);
                        }
                    }
                }
            }
            else
            {
                if (columnNumber == 0)
                {
                    if (swaprowNumber == 2)
                    {
                        if (swapcolumnNumber == 1)          // 2,0 en 2,1
                        {
                            score = swapScore(column1, column2);
                        }
                        else if (swapcolumnNumber == 2)     // 2,0 en 2,2
                        {
                            score = swapScore(column1, column3);
                        }
                    }
                }
                else if (columnNumber == 1)
                {
                    if (swaprowNumber == 2)
                    {
                        if (swapcolumnNumber == 2)          // 2,1 en 2,2
                        {
                            score = swapScore(column2, column3);
                        }
                    }
                }
            }

            return score;
        }

        public int swapScore(int[] first, int[] swapped)
        {
            int score = 0;
            if (first[sudoku[swapIndex] - 1] == 0)
            {
                score++;
            }
            if (swapped[sudoku[swapIndex] - 1] == 0)
            {
                score--;
            }
            if (swapped[sudoku[indexHill] - 1] == 0)
            {
                score++;
            }
            if (first[sudoku[indexHill] - 1] == 0)
            {
                score--;
            }
            return score;
        }

        public void swap()
        {
            int random = r.Next(best1.Count);

            int temp = sudoku[best1[random]];
            sudoku[best1[random]] = sudoku[best2[random]];
            sudoku[best2[random]] = temp;
        }

        public int blockScore(int index)
        {

            indexHill = index;
            for (int number = 0; number < 3; number++)
            {
                if (number == 0)
                {
                    int modulus = indexHill % 9;
                    int startRow = indexHill - modulus;
                    row1[sudoku[startRow] - 1]++;
                    startRow++;
                    while (startRow % 9 != 0)
                    {
                        row1[sudoku[startRow] - 1]++;
                        startRow++;
                    }
                    int startColumn = modulus;
                    while (startColumn < 81)
                    {
                        column1[sudoku[startColumn] - 1]++;
                        startColumn += 9;
                    }
                }
                else if (number == 1)
                {
                    int modulus = indexHill % 9;
                    int startRow = indexHill - modulus;
                    row2[sudoku[startRow] - 1]++;
                    startRow++;
                    while (startRow % 9 != 0)
                    {
                        row2[sudoku[startRow] - 1]++;
                        startRow++;
                    }
                    int startColumn = modulus;
                    while (startColumn < 81)
                    {
                        column2[sudoku[startColumn] - 1]++;
                        startColumn += 9;
                    }
                }
                else
                {
                    int modulus = indexHill % 9;
                    int startRow = indexHill - modulus;
                    row3[sudoku[startRow] - 1]++;
                    startRow++;
                    while (startRow % 9 != 0)
                    {
                        row3[sudoku[startRow] - 1]++;
                        startRow++;
                    }
                    int startColumn = modulus;
                    while (startColumn < 81)
                    {
                        column3[sudoku[startColumn] - 1]++;
                        startColumn += 9;
                    }
                }

                indexHill += 10;

            }

            int[] scores = new int[9];

            int row1score = 0;
            int column1score = 0;

            int row2score = 0;
            int column2score = 0;

            int row3score = 0;
            int column3score = 0;

            foreach (int i in row1)
                if (i == 0)
                    row1score++;

            foreach (int i in column1)
                if (i == 0)
                    column1score++;

            foreach (int i in row2)
                if (i == 0)
                    row2score++;

            foreach (int i in column2)
                if (i == 0)
                    column2score++;

            foreach (int i in row3)
                if (i == 0)
                    row3score++;

            foreach (int i in column3)
                if (i == 0)
                    column3score++;

            return row1score + row2score + row3score + column1score + column2score + column3score;
        }
    }
}
