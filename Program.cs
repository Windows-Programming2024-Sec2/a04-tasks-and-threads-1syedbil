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
            switch(ValidateCmdLineArgs(args))
            {
                case 0:
                    return;
                case 1:
                    int checkResult = CheckFile(args[0]);

                    if (checkResult == 0)
                    {
                        return;
                    }
                    else if (checkResult == 1)
                    {
                        try
                        {
                            using (File.CreateText(args[0])) { }
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine("Exception caught: {0}\n", e);
                            return;
                        }
                        finally
                        {
                            Console.WriteLine("File created.\n");
                        }
                    }
                    else if (checkResult == 2)
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
                            Console.WriteLine("File overwritten.\n");
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

                for (int i = 0; i < 25; i++)
                {
                    writingTasks.Add(WriteToFile(args[0]));
                }

                await Task.WhenAll(writingTasks);
            }

            await fileSizeTask;
        }

        static async Task CheckFileSize(string fileName, int maxFileSize)
        {
            bool maxSizeReached = false;

            while (!maxSizeReached)
            {
                await Task.Delay(100);

                FileInfo file = new FileInfo(fileName);
                Console.WriteLine("Current File Size: " + file.Length);

                if (file.Length >= maxFileSize)
                {
                    Console.WriteLine("Final File Size: " + file.Length);
                    maxSizeReached = true;
                }
            }
        }

        static async Task WriteToFile(string fileName)
        {
            string data = Guid.NewGuid().ToString();

            try
            {
                using (StreamWriter sw = File.AppendText(fileName))
                {
                    await sw.WriteLineAsync(data);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Exception caught: {0}\n", e);
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
                return 2;
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
