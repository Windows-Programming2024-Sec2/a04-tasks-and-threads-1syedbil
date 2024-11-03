using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace WindowsProg_A4
{
    internal class Program
    {
        static void Main(string[] args)
        {
            switch(ValidateCmdLineArgs(args))
            {
                case 0:
                    return;
                case 1:

                default:
                    return;
            }
        }

        static int ValidateCmdLineArgs(string[] args) 
        {
            Regex regex = new Regex("[A-Za-z0-9]+\\.txt");

            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("\nError: You must have at least 1 argument and no more than 2 arguments.\n");
                return 0;
            }
            else if (args[0] == "/?")
            {
                Usage();
                return 0;
            }
            else if (!regex.IsMatch(args[0]))
            {
                Usage();
                return 0;
            }
            else if (!Int32.TryParse(args[1], out int i))
            {
                Usage();
                return 0;
            }
            else if (Int32.TryParse(args[1], out int j))  
            {
                if (j < 1000 || j > 20000000)
                {
                    Usage();
                    return 0;
                }
            }
            return 1;
        }

        static void Usage()
        {
            Console.WriteLine("\nThe first argument must be a file name with the .txt extension. The second argument must be the total number of characters that the file can hold. Must be between 1000 and 20000000\n");
        }
    }
}
