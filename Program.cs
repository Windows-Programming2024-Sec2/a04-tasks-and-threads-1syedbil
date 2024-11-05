/*
 * FILE          : Program.cs
 * PROJECT       : WindowsProg_A4
 * PROGRAMMER    : Bilal Syed
 * FIRST VERSION : 2024-11-02
 * DESCRIPTION   : This program is meant to demonstrate task parallelism. It first takes command line arguments from
 *                 the user for the name of a text file and the size of the file. Then it validates these arguments.
 *                 Once the arguments are valid, the program runs 25 idential tasks in a loop. These 25 identical tasks
 *                 each write one line of random data (GUID) to the file. The loop's end is determined by another task 
 *                 which checks the file's size periodically every 100 milliseconds. The size checking task runs in 
 *                 parallel with the 25 writing tasks. So, as the file is being written to, the file's size is being
 *                 checked at the same time. Once the size checking task finds that the maximum file size has been reached
 *                 or exceeded, any remaining writing tasks will be completed and then all tasks will come to an end. 
 *                 At which point the program will end as well.
 */

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
        static async Task Main(string[] args)
        {
            switch (ValidateCmdLineArgs(args))
            {
                case 0:
                    return;
                case 1:
                    int checkResult = CheckFile(args[0]);

                    if (checkResult == 0)
                    {
                        Console.WriteLine("\nExiting...");
                        return;
                    }
                    else if (checkResult == 1)
                    {
                        try
                        {
                            File.WriteAllText(args[0], string.Empty);
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine("Exception caught: {0}\n", e);
                            return;
                        }
                        finally
                        {
                            Console.WriteLine("File created.\n\nWriting to file...\n");
                        }
                    }
                    else if (checkResult == Constants.TWO)
                    {
                        try
                        {
                            File.WriteAllText(args[0], string.Empty);
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine("Exception caught: {0}\n", e);
                            return;
                        }
                        finally
                        {
                            Console.WriteLine("\nOverwritting file...\n");
                        }
                    }
                    break;
                default:
                    return;
            }

            int maxFileSize = 0;
            Int32.TryParse(args[1], out maxFileSize);

            Task fileSizeTask = CheckFileSize(args[0], maxFileSize);

            while (!fileSizeTask.IsCompleted)
            {
                FileInfo file = new FileInfo(args[0]);
                List<Task> writingTasks = new List<Task>();

                if (file.Length >= maxFileSize)
                {
                    break;
                }

                for (int i = 0; i < Constants.NUM_WRITING_TASKS; i++)
                {
                    writingTasks.Add(WriteToFile(args[0]));
                }

                await Task.WhenAll(writingTasks);
            }

            await fileSizeTask;
        }

        /*
         * METHOD      : CheckFileSize()
         * DESCRIPTION : Periodically checks the size of the specified file every 100 milliseconds to ensure it does not exceed the maximum size.
         * PARAMETERS  : string fileName - the name of the file that is having its size checked
         *               int maxFileSize - the maximum allowable file size, which must be within the range of 1000 to 20000000
         * RETURNS     : Task - an asynchronous task
         */
        static async Task CheckFileSize(string fileName, int maxFileSize)
        {
            bool maxSizeReached = false;

            while (!maxSizeReached)
            {
                await Task.Delay(Constants.HUNDRED);

                FileInfo file = new FileInfo(fileName);
                Console.WriteLine("Current File Size: " + file.Length);

                if (file.Length >= maxFileSize)
                {
                    Console.WriteLine("Final File Size: " + file.Length);
                    maxSizeReached = true;
                }
            }
        }

        /*
         * METHOD      : WriteToFile()
         * DESCRIPTION : Appends a randomly generated GUID string as a new line to the specified file.
         * PARAMETERS  : string fileName - the name of the file that is being appended to
         * RETURNS     : Task - an asynchronous task
         */
        static async Task WriteToFile(string fileName)
        {
            string data = Guid.NewGuid().ToString();
            StreamWriter sw = null;

            try
            {
                sw = File.AppendText(fileName);
            }
            catch (IOException e)
            {
                Console.WriteLine("Exception caught: {0}\n", e);
                return;
            }
            finally
            {
                using (sw)
                {
                    await sw.WriteLineAsync(data);
                }
            }
        }

        /*
         * METHOD      : ValidateCmdLineArgs()
         * DESCRIPTION : Validates the command line arguments inputted by the user. Checks for things like the formatting
         *               of the file name, the number of command line arguments, and to see if the value of the file size
         *               is within the range. Also makes sure that the file size is an integer.
         * PARAMETERS  : string[] args - array of the command line arguments as strings
         * RETURNS     : int - returns 0 if inputs are invalid, returns 1 if inputs are valid
         */
        static int ValidateCmdLineArgs(string[] args)
        {
            Regex regex = new Regex("[A-Za-z0-9._-]+\\.txt\\z");

            if (args.Length < 1 || args.Length > Constants.TWO)
            {
                Console.WriteLine("\nError: You must have at least 1 argument and no more than 2 arguments.\n");
                return 0;
            }
            else if (args[0] == "/?" && args.Length == 1)
            {
                Usage();
                return 0;
            }
            else if (!regex.IsMatch(args[0]))
            {
                Console.WriteLine("\nError: Incorrect input for the argument(s).");
                Usage();
                return 0;
            }
            else if (!Int32.TryParse(args[1], out int i))
            {
                Console.WriteLine("\nError: Incorrect input for the argument(s).");
                Usage();
                return 0;
            }
            else if (Int32.TryParse(args[1], out int j))
            {
                if (j < Constants.MIN_SIZE || j > Constants.MAX_SIZE)
                {
                    Console.WriteLine("\nError: Incorrect input for the argument(s).");
                    Usage();
                    return 0;
                }
            }
            return 1;
        }

        /*
         * METHOD      : CheckFile()
         * DESCRIPTION : Checks the file name inputted by the user from the command line to see if a file with that name already
         *               exists in the current directory. If the file does already exist then it asks the user if they want to 
         *               overrwrite the file. If the file doesn't exist then it just leaves the function without doing anything else.
         * PARAMETERS  : string fileName - the name of the file that is having its existence in the current directory verrified
         * RETURNS     : int - returns 1 if the file does not exists, returns 2 if the user wants the file to be overwritten, returns
         *                     0 if the user does not want the file to be overwritten
         */
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
                    Console.WriteLine("\nThe " + fileName + " file already exists in the current directory. Would you like to overrwrite it?\n");
                    Console.WriteLine("\t1. Yes\n\t2. No\n\nEnter the number corresponding to your choice:");
                    string input = Console.ReadLine();
                    Int32.TryParse(input, out userChoice);
                    while (userChoice != 1 && userChoice != Constants.TWO)
                    {
                        Console.WriteLine("Error: Please enter a number corresponding to the menu choices.");
                        input = Console.ReadLine();
                        Console.WriteLine();
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
                return Constants.TWO;
            }
            else if (userChoice == Constants.TWO)
            {
                return 0;
            }

            return Constants.NEG_ONE;
        }

        /*
         * METHOD      : Usage()
         * DESCRIPTION : Explains to the user what they are supposed to input for the two command line arguments.
         * PARAMETERS  : None
         * RETURNS     : void
         */
        static void Usage()
        {
            Console.WriteLine("\nThe first argument must be a file name with the .txt extension. The second argument must be the maximum size of the file. Its value must be an integer between 1000 and 20000000.\n");
        }
    }

    /*
     * NAME    : Constants
     * PURPOSE : This is a static class that defines constant values used throughout the program.
     */
    static class Constants
    {
        public const int NEG_ONE = -1;
        public const int TWO = 2;
        public const int NUM_WRITING_TASKS = 25;
        public const int HUNDRED = 100;
        public const int MIN_SIZE = 1000;
        public const int MAX_SIZE = 20000000;
    }
}
