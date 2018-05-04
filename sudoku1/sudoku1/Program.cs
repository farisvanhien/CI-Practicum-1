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

            HashSet<int> row1 = new HashSet<int>();
            HashSet<int> column1 = new HashSet<int>();

            HashSet<int> row2 = new HashSet<int>();
            HashSet<int> column2 = new HashSet<int>();

            HashSet<int> row3 = new HashSet<int>();
            HashSet<int> column3 = new HashSet<int>();

            int randomHill = r.Next(9) + 1;  // get a random number

            switch (randomHill) // get the correct index
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

            int indexHill = index;

            for (int number = 0; number < 3; number++)
            {
                if(number == 0)
                {
                    int modulus = indexHill % 9;
                    int startRow = indexHill - modulus;
                    row1.Add(sudoku[startRow]);
                    startRow++;
                    while (startRow % 9 != 0)
                    {
                        row1.Add(sudoku[startRow]);
                        startRow++;
                    }
                    int startColumn = modulus;
                    while (startColumn < 81)
                    {
                        column1.Add(sudoku[startColumn]);
                        startColumn += 9;
                    }
                }
                else if (number == 1)
                {
                    int modulus = indexHill % 9;
                    int startRow = indexHill - modulus;
                    row2.Add(sudoku[startRow]);
                    startRow++;
                    while (startRow % 9 != 0)
                    {
                        row2.Add(sudoku[startRow]);
                        startRow++;
                    }
                    int startColumn = modulus;
                    while (startColumn < 82)
                    {
                        column2.Add(sudoku[startColumn]);
                        startColumn += 9;
                    }
                }
                else
                {
                    int modulus = indexHill % 9;
                    int startRow = indexHill - modulus;
                    row3.Add(sudoku[startRow]);
                    startRow++;
                    while (startRow % 9 != 0)
                    {
                        row3.Add(sudoku[startRow]);
                        startRow++;
                    }
                    int startColumn = modulus;
                    while (startColumn < 82)
                    {
                        column3.Add(sudoku[startColumn]);
                        startColumn += 9;
                    }
                }

                indexHill += 10;

            }

            int[] scores = new int[9];

            for (int number = 1; number < 10; number++) // calculate the starting scores of a block
            {
                switch (number)
                {
                    case 1:
                        scores[0] = 18 - row1.Count - column1.Count;
                        break;
                    case 2:
                        scores[1] = 18 - row1.Count - column2.Count;
                        break;
                    case 3:
                        scores[2] = 18 - row1.Count - column3.Count;
                        break;
                    case 4:
                        scores[3] = 18 - row2.Count - column1.Count;
                        break;
                    case 5:
                        scores[4] = 18 - row2.Count - column2.Count;
                        break;
                    case 6:
                        scores[5] = 18 - row2.Count - column3.Count;
                        break;
                    case 7:
                        scores[6] = 18 - row3.Count - column1.Count;
                        break;
                    case 8:
                        scores[7] = 18 - row3.Count - column2.Count;
                        break;
                    case 9:
                        scores[8] = 18 - row3.Count - column3.Count;
                        break;
                }
                
            }


            // hill-climb

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
            Console.ReadKey();
        }
    }
}
