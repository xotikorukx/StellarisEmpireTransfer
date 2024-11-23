using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StellarisEmpireTransfer.Core
{
    public class EmpireManager
    {
        bool IsServer = false;
        bool isReading = false;
        bool isWriting = false;

        public EmpireManager(bool isServer)
        {
            IsServer = isServer;
        }

        public string ReadFile()
        {
            string empirePath;

            if (IsServer)
            {
                empirePath = "user_empire_designs_v3.4.txt";

                if (!File.Exists(empirePath))
                {
                    File.Create(empirePath);
                }
            }
            else
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                empirePath = $"{documentsPath}\\Paradox Interactive\\Stellaris\\user_empire_designs_v3.4.txt";
            }

            while (isReading || isWriting) { }

            isReading = true;

            StreamReader empiresFile;
            empiresFile = File.OpenText(empirePath);

            string fileData = empiresFile.ReadToEnd();
            empiresFile.Close();

            isReading = false;

            return fileData;
        }

        public void WriteFile(string empireData)
        {
            string empirePath;

            if (IsServer)
            {
                empirePath = "user_empire_designs_v3.4.txt";

                if (!File.Exists(empirePath))
                {
                    File.Create(empirePath);
                }
            }
            else
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                empirePath = $"{documentsPath}\\Paradox Interactive\\Stellaris\\user_empire_designs_v3.4.txt";
            }

            while (isReading || isWriting) { }

            isWriting = true;

            File.WriteAllText(empirePath, empireData);

            isWriting = false;
        }

        public string UpdateEmpire(string empireNameToUpdate, string updatedEmpireData, string existingEmpireData)
        {
            string fileString = "";

            List<string> EmpireNames = GetAllEmpireNames(existingEmpireData);

            bool exists = EmpireNames.IndexOf(empireNameToUpdate) > -1;

            foreach (string iEmpireName in EmpireNames)
            {
                if (iEmpireName == empireNameToUpdate) //If the empire exists, and we are overwriting it
                {
                    Console.WriteLine($" - Empire exists; overwriting");
                    fileString = $"{fileString}\n{updatedEmpireData}";
                }
                else
                { //If the empire is one we aren't touching
                    string iEmpireData = GetEmpireData(iEmpireName, existingEmpireData);
                    fileString = $"{fileString}\n{iEmpireData}";
                }
            }

            if (!exists)
            {
                Console.WriteLine($" - Empire NOT exists; appending");
                fileString = $"{existingEmpireData}\n{updatedEmpireData}";
            }

            return fileString.Trim();
        }

        public string DeleteEmpire(string empireNameToDelete, string existingEmpireData)
        {
            return UpdateEmpire(empireNameToDelete, "", existingEmpireData);
        }

        public string GetEmpireData(string empireName, string empireData)
        {
            StreamReader empiresFile = new StreamReader(GenerateStreamFromString(empireData));

            int depth = 0;

            string? readString;
            Regex empireNameRegex = new Regex(@"^"".*""=$", RegexOptions.IgnoreCase);
            Regex openBracketRegex = new Regex(@"^\s*{\s*$", RegexOptions.IgnoreCase);
            Regex closeBracketRegex = new Regex(@"^\s*}\s*$", RegexOptions.IgnoreCase);

            bool appendData = false;
            bool firstAppend = false;
            string thisEmpireData = "";

            do
            {
                readString = empiresFile.ReadLine();

                if (readString == null) break;

                if (openBracketRegex.IsMatch(readString))
                {
                    depth++;
                }
                else if (closeBracketRegex.IsMatch(readString))
                {
                    depth--;
                }
                else if (depth == 0 && empireNameRegex.IsMatch(readString) && empireName == readString.Replace("\"", "").Replace("=", ""))
                {
                    appendData = true;
                    firstAppend = true;
                }

                if (appendData)
                {
                    thisEmpireData = $"{thisEmpireData}\n{readString}";

                    if (firstAppend || depth > 0)
                    {
                        firstAppend = false;
                    }
                    else
                    {
                        return thisEmpireData;//.Replace("\t", "\n");
                    }
                }
            } while (readString != null);

            throw new Exception("Could not find matching empire.");
        }

        public List<string> GetAllEmpireNames(string empireData)
        {
            StreamReader empiresFile = new StreamReader(GenerateStreamFromString(empireData));

            List<string> empireNames = new List<string>();

            int depth = 0;

            string? readString;
            Regex empireNameRegex = new Regex(@"^"".*""=$", RegexOptions.IgnoreCase);
            Regex openBracketRegex = new Regex(@"^\s*{\s*$", RegexOptions.IgnoreCase);
            Regex closeBracketRegex = new Regex(@"^\s*}\s*$", RegexOptions.IgnoreCase);

            do
            {
                readString = empiresFile.ReadLine();

                if (readString == null) break;

                if (openBracketRegex.IsMatch(readString))
                {
                    depth++;
                }
                else if (closeBracketRegex.IsMatch(readString))
                {
                    depth--;
                }
                else if (depth == 0 && empireNameRegex.IsMatch(readString))
                {
                    empireNames.Add(readString.Replace("\"", "").Replace("=", ""));
                }
            } while (readString != null);

            return empireNames;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0; // Reset the stream position to the beginning
            return stream;
        }
    }
}
