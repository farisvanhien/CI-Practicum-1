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

            // initiate the not-fixed
            for(int i = 0; i < 3; i++)
            {
                List<int> numbers = new List<int>();
                for (int j = 0; j < 3; j++)
                {
                    for(int k = 0; k < 3; k++)
                    {
                        if(sudoku[i*3+j*9+k] != 0)
                        {
                            numbers.Add(sudoku[i * 3 + j * 9 + k]);
                        }
                    }
                }
                int index = r.Next(9) + 1;
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if(!start[i * 3 + j * 9 + k]) // vrij
                        {
                            if (!numbers.Contains(index)) // index bestaat nog niet
                            {
                                sudoku[i * 3 + j * 9 + k] = index;
                            }
                            else // bestaat wel
                            {
                                while (numbers.Contains(index))
                                {
                                    index++;
                                    if (index > 9) index = 1;
                                }
                                sudoku[i * 3 + j * 9 + k] = index;
                            }
                            index++;
                            if (index > 9) index = 1;
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
