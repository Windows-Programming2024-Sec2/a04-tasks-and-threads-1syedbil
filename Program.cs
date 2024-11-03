using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices.WindowsRuntime;

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
                    int checkResult = CheckFile(args[0]);

                    if (checkResult == 0)
                    {
                        Console.WriteLine("Leaving.\n");
                        return;
                    }
                    else if (checkResult == 1)
                    {
                        Console.WriteLine("Continuing.\n");
                        return;
                    }
                    return;
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

        static int CheckFile(string fileName)
        {
            int userChoice = 0;
            bool fileExists = false;

            try
            {
                fileExists = File.Exists(fileName);
            }
            catch (IOException e)
            {
                Console.WriteLine("Exception caught: {0}\n", e);
                return 0;
            }
            finally
            {
                if (fileExists)
                {
                    Console.WriteLine("The " + fileName + " file already exists in the current directory. Would you like to overrwrite it?\n");
                    Console.WriteLine("\t1. Yes\n\t2. No\nEnter the number corresponding to your choice:");
                    string input = Console.ReadLine();
                    Int32.TryParse(input, out userChoice);
                    while (userChoice != 1 && userChoice != 2)
                    {
                        Console.WriteLine("Error: Please enter a number corresponding to the menu choices.\n");
                        input = Console.ReadLine();
                        Int32.TryParse(input, out userChoice);
                    }
                }
            }

            if (!fileExists)
            {
                return 1;
            }
            else if (userChoice == 1)
            {
                return 1;
            }
            else if (userChoice == 2)
            {
                return 0;
            }

            return -1;
        }

        static void Usage()
        {
            Console.WriteLine("\nThe first argument must be a file name with the .txt extension. The second argument must be the total number of characters that the file can hold. Must be between 1000 and 20000000\n");
        }
    }
}
