//
// StringFinder.cs
//
// Authors:
//  Anthony Lu <anthlu@outlook.com>
//

using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;

namespace StringFinder
{
    class Program
    {
        // Custom file types filter.
        static string[] disabledTypes = new string[] { };

        static void Main(string[] args)
        {
            if (args.Length == 0 || args.Length > 7)
            {
                Help();
                Environment.Exit(0);
            }
            else
            {
                // Convert all command to the lower case.
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("/"))
                    {
                        args[i] = args[i].ToLower();
                    }
                }

                if (args[0] == "/?" || args[0] == "-h" || args[0] == "--help" || args[0] == "/h")
                {
                    Help();
                }
                else
                {
                    try
                    {
                        switch (args[0])
                        {
                            case "/e":
                                string[] encodingType = {
                                    "ascii",
                                    "default",
                                    "bigendianunicode",
                                    "unicode",
                                    "utf32",
                                    "utf7",
                                    "utf8"
                                };

                                string value = args[1].ToLower();
                                value = args[1].Replace("-", ""); // If users input "utf-8", replace it with "utf8".

                                switch (args[1])
                                {
                                    case "/x":
                                        if (args[2] == "/s")
                                        {
                                            SearchString(args[3], args[4], "default", disabledTypes);
                                        }
                                        else
                                        {
                                            SearchString(args[4], args[5], "default", ConvertToStrArray(args[2], disabledTypes));
                                        }
                                        break;

                                    case "/s":
                                        SearchString(args[2], args[3], "default", disabledTypes);
                                        break;

                                    default:
                                        // If encoding list contains the encoding user given.
                                        if (Array.IndexOf(encodingType, value) > -1)
                                        {
                                            switch (args[2])
                                            {
                                                case "/x":
                                                    if (args[3] == "/s")
                                                    {
                                                        SearchString(args[4], args[5], value, disabledTypes);
                                                    }
                                                    else
                                                    {
                                                        SearchString(args[5], args[6], value, ConvertToStrArray(args[3], disabledTypes));
                                                    }
                                                    break;

                                                case "/s":
                                                    SearchString(args[3], args[4], "default", disabledTypes);
                                                    break;

                                                default:
                                                    PrintError(args[2]);
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Error: Unsupported encoding.");
                                        }
                                        break;
                                }
                                break;

                            case "/x":
                                if (args[1] == "/s")
                                {
                                    SearchString(args[2], args[3], "default", disabledTypes);
                                }
                                else
                                {
                                    SearchString(args[3], args[4], "default", ConvertToStrArray(args[1], disabledTypes));
                                }
                                break;

                            case "/s":
                                SearchString(args[1], args[2], "default", disabledTypes);
                                break;

                            default:
                                PrintError(args[0]);
                                break;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("Error: No encoding, file types, directory or file specified.");
                    }
                }
            }
        }

        static void Help()
        {
            Console.WriteLine("Search the string in the specified directory or file and return the file path which containing the string. Except encrypted files and special encoding files.\n");
            Console.WriteLine("Usage: sfr [[/e <encoding>] [/x <file types>] /s <string> <path>]\n");
            Console.WriteLine("/h       Display help. This is the same as not typing any options.");
            Console.WriteLine("/e       Specifies the encoding to parse files.");
            Console.WriteLine("/x       Specifies which file types are not searched. Use the comma as a delimiter, such as \"exe,dll\".");
            Console.WriteLine("/s       Specifies string to find.");
        }

        static void PrintError(string command)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No executable found matching command \"{0}\"", command);
            Console.ResetColor();
        }

        /// <summary>
        /// Read bytes from specified file.
        /// </summary>
        /// <param name="path">Specified path to read bytes.</param>
        /// <returns>A byte array containing data read from specified file.</returns>
        static byte[] ReadFileBytes(string path)
        {
            using (FileStream fsRead = new FileStream(path, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fsRead))
                {
                    byte[] bytes = br.ReadBytes(Convert.ToInt32(fsRead.Length));

                    return bytes;
                }
            }
        }

        /// <summary>
        /// Converts bytes to string with specified encoding.
        /// </summary>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.</param>
        /// <param name="encoding">Specified encoding to parse.</param>
        /// <returns>A string converted from bytes array.</returns>
        static string ConvertBytesToString(byte[] bytes, string encoding)
        {
            switch (encoding)
            {
                case "ascii":
                    return Encoding.ASCII.GetString(bytes);
                case "default":
                    return Encoding.Default.GetString(bytes);
                case "bigendianunicode":
                    return Encoding.BigEndianUnicode.GetString(bytes);
                case "unicode":
                    return Encoding.Unicode.GetString(bytes);
                case "utf32":
                    return Encoding.UTF32.GetString(bytes);
                case "utf7":
                    return Encoding.UTF7.GetString(bytes);
                case "utf8":
                    return Encoding.UTF8.GetString(bytes);
                default:
                    return Encoding.Default.GetString(bytes);
            }
        }

        /// <summary>
        /// Convert string(the comma as a delimiter) to string array.
        /// </summary>
        /// <param name="fileType">File types string.</param>
        /// <returns>A string array contains file types.</returns>
        static string[] ConvertToStrArray(string fileType, string[] disabledTypes)
        {
            string[] strArray = new string[fileType.Count(c => c == ',') + 1 + disabledTypes.Length];

            for (int i = 0; i < disabledTypes.Length; i++)
            {
                strArray[i] = disabledTypes[i];
            }

            for (int i = disabledTypes.Length; i < strArray.Length; i++)
            {
                if (!fileType.Contains(","))
                {
                    strArray[i] = fileType;
                    break;
                }

                strArray[i] = fileType.Substring(0, fileType.IndexOf(','));

                fileType = fileType.Substring(fileType.IndexOf(',') + 1, fileType.Length - strArray[i].Length - 1);
            }
            return strArray;
        }

        /// <summary>
        /// Check if the disabled file type list contains the type of file of given path.
        /// </summary>
        /// <param name="path">Specified directory or file path.</param>
        /// <param name="fileTypesArray">disabled file types array.</param>
        /// <returns>true if the disabled file type list contains the type of file of given path; otherwise, false.</returns>
        static bool IsDisabledFileType(string path, string[] fileTypesArray)
        {
            string[] fileTypeArray = fileTypesArray;
            if (path.Substring(path.LastIndexOf('\\') + 1, path.Length - path.LastIndexOf('\\') - 1).Contains("."))
            {
                if (Array.IndexOf(fileTypeArray, path.Substring(path.LastIndexOf('.') + 1)) > -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Search the string in the specified directory or file.
        /// </summary>
        /// <param name="strToFind">String to find.</param>
        /// <param name="path">Specified directory or file path.</param>
        /// <param name="encoding">Specified encoding to parse target file.</param>
        /// <param name="disabledFileType">"Specified which file types are not searched."</param>
        static void SearchString(string strToFind, string path, string encoding, string[] disabledFileTypeArray)
        {
            string filePath, fileStr;

            try
            {
                Console.WriteLine("Searching...");

                FileAttributes fileAttr = File.GetAttributes(@path);

                if ((fileAttr & FileAttributes.Directory) == FileAttributes.Directory)
                {

                    DirectoryInfo dirInfo = new DirectoryInfo(@path);

                    FileInfo[] fi = dirInfo.GetFiles("*", SearchOption.AllDirectories);

                    int j = 0;

                    for (int i = 0; i < fi.Length; i++)
                    {
                        filePath = fi[i].FullName;

                        if (false == IsDisabledFileType(filePath, disabledFileTypeArray))
                        {
                            fileStr = ConvertBytesToString(ReadFileBytes(filePath), encoding);

                            if (!(fileStr.IndexOf(strToFind) == -1))
                            {
                                Console.WriteLine(filePath);
                                j++;
                            }
                        }
                    }
                    if (j != 0)
                    {
                        Console.WriteLine("\n{0} file(s) may contain \"{1}\".\n", j, strToFind);
                    }
                    else
                    {
                        Console.WriteLine("No files containing \"{0}\" were found.\n\n\nTry to specify a different encoding to parse the files or they are encrypted or special encoding files.", strToFind);
                    }

                }
                else
                {
                    filePath = path;

                    if (false == IsDisabledFileType(filePath, disabledFileTypeArray))
                    {
                        fileStr = ConvertBytesToString(ReadFileBytes(filePath), encoding);

                        if (!(fileStr.IndexOf(strToFind) == -1))
                        {
                            Console.WriteLine("This file containing \"{0}\".\n", strToFind);
                        }
                    }
                    else
                    {
                        Console.WriteLine("This file does not contain \"{0}\".\n\n\nTry to specify a different encoding to parse the file or it was encrypted or special encoding files.", strToFind);
                    }

                }
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Error: Path is null.");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Error: Path contains invalid characters such as \", <, >, or |.");
            }
            catch (SecurityException)
            {
                Console.WriteLine("Error: Permission denied.");
            }
            catch (PathTooLongException)
            {
                Console.WriteLine("Error: The specified path, file name, or both exceed the system-defined maximum length.");
            }
            catch (NotSupportedException)
            {
                Console.WriteLine("Error: Path is in an invalid format.");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Error: File not found.");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Error: Directory not found.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Error: Permission denied.");
            }
            catch (IOException)
            {
                Console.WriteLine("Error: An I/O error occurs.");
            }
        }
    }
}
