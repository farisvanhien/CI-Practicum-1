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
                            int index = horizontalBlock * 27 + verticalBlock * 3 + verticalNumber * 9 + horizontalNumber;

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
                            int index = horizontalBlock * 27 + verticalBlock * 3 + verticalNumber * 9 + horizontalNumber;

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
            

            // hill-climb
            
            for (int k = 0; k < 9; k++)
            {
                string output = "";
                for (int l = 0; l < 9; l++)
                {
                    output += sudoku[k * 9 + l];
                }
                Console.WriteLine(output);
            }
            Console.ReadKey();
        }
    }
}
