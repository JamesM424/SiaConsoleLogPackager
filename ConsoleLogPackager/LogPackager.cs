using System;
using System.IO;                    //To get AppDev folders
using System.IO.Compression;       //To zip the file
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;         //for JsonObject

namespace ConsoleLogPackager
{
    class LogPackager
    {
        static void Main(string[] args)
            
        {
           
            // Keep the console window open in debug mode.
            Console.WriteLine("This program is to package all Sia logs into a Zipped file for easy posting.");
            Console.WriteLine("Press Y to continue. \n");

            string result = Console.ReadLine();
            //Console.WriteLine(result);
            //Console.ReadLine();
            
            if (result.Equals ("y", StringComparison.OrdinalIgnoreCase ) || result.Equals ("yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine ("yes");
                //Console.ReadLine();

                Start();

            }
            else
            {
                Console.WriteLine("Not Y.  We will exit.");
                Console.ReadLine();
                return;
            }
            result = "";  //re-init result
            Console.WriteLine("Do you want to perform other operations?\nPress Y to continue. \n");
            result = Console.ReadLine();
            if (result.Equals("y", StringComparison.OrdinalIgnoreCase) || result.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("yes");
                //Console.ReadLine();

                Otheroperations();
             }
            else
            {
                Console.WriteLine("Not Y.  We will exit.");
                Console.ReadLine();
                return;

            }

            Console.WriteLine("There are no more features.\n  Press any key to exit\n");
            Console.ReadLine();


        }


        public static void Start()
        //We've received authorization to start so... lets begin
        {
            // First, lets get the environment variables started
            //string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //string folderpath = appdata + "\\Sia-UI";
            string folderpath = " ";        //set to null to start
            
            //instead, lets read the JSON so we can accomodate people that may have specified a special location
            folderpath = LoadJson(folderpath);

            //We need to check to see if file exists before we get here apparently...
            //Check dir exists code was here.  Moved to LoadJson because in some other changes, we read there first now

            //We will need someplace to copy the files too, that is easily visible for an end user
            string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string LogDir = (desktopDir + "\\SiaLogs");

            if (Directory.Exists (LogDir))
            {
                Console.WriteLine("Preexisting SiaLogs folder exists on the desktop.  It's OK.");
            }
            else
            {
                Directory.CreateDirectory(LogDir);
            }
            
            //Now copy all log files from AppData to Desktop SiaLogs
            string sourceFolder = folderpath;
            List<string> allFiles = new List<string>();
            AddFileNamesToList(sourceFolder, allFiles);
            foreach (string fileName in allFiles)
            {
                //Are we going to want copies of the .JSONs as well?  Hmm...  Probably wouldn't hurt, and not much extra space either but not yet for security.  Maybe levels?
                if (fileName.Contains(".log"))
                {
                    //slight problem here as fileName reports the full path.  Need to do some fixup.  I'll just make simpleFile as it may be useful in the future
                    string simpleFile = (" ");
                    simpleFile = System.IO.Path.GetFileName(fileName);
                    
                    string destFile = System.IO.Path.Combine(LogDir, simpleFile);
                    //make true to just over-write older files
                    System.IO.File.Copy(fileName, destFile, true);

                    ////THIS WILL BE WHERE WE COPY EVERYTHING
                }
            }
            
            //Now zip it all up
            String zippath = (desktopDir + "\\SiaLogs.zip");

            if (File.Exists(zippath))
            {
                //we need to delete if an oder version exists as this will be a hard fault
                File.Delete(zippath);
            }

            ZipFile.CreateFromDirectory(LogDir, zippath, CompressionLevel.Fastest, true);
            
            //And, we can tell them and exit
            Console.WriteLine("Zip file is on your desktop.  Thank you. \n");
            //Console.WriteLine("Press any key to exit.  Or that X button");
            //System.Console.ReadKey();
            return;
         }


        public static void AddFileNamesToList(string sourceDir, List<string> allFiles)
        {
            string[] fileEntries = Directory.GetFiles(sourceDir);
            foreach (string fileName in fileEntries)
            {
                allFiles.Add(fileName);
            }

            //Recursion for subdirs    
            string[] subdirectoryEntries = Directory.GetDirectories(sourceDir);
            foreach (string item in subdirectoryEntries)
            {
                // Avoid "reparse points"
                if ((File.GetAttributes(item) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    AddFileNamesToList(item, allFiles);
                }
            }

        }
        

        static string LoadJson(string SiaPath)
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folderpath = appdata + "\\Sia-UI";
            string file = (folderpath + "\\config.json");

            //We need to check to see if file exists before we get here apparently...
            if (Directory.Exists(folderpath))
            {
                Console.WriteLine("Directory exists.  Continuing... \n");
            }
            else
            {
                Console.WriteLine("Sia Directory not found!  We will exit.");
                Console.ReadLine();
                Environment.Exit(2);
                //ruthless exit, but don't feel like making it harder than it needs to be
            }


            using (StreamReader r = new StreamReader(file))
            {
                string json = r.ReadToEnd();
                JsonTextReader reader = new JsonTextReader(new StringReader(json));
                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        //Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);  //legacy, doesn't need to be here
                    }
                    else
                    {
                        //Console.WriteLine("Token: {0}", reader.TokenType);  //legacy, doesn't need to be here
                    }
                }

                JObject o = JObject.Parse(json);

                //Console.WriteLine(json);      //for testing
                string datadir = (string)o["datadir"];

                string name = (string)o["siad"]["datadir"];         //nested JSON

                SiaPath = name;
                return SiaPath;

            }

        }
        public static void Otheroperations()

        {
            string folderpath = " ";        //set to null to begin with
            string pathtodelete = "";
            folderpath = LoadJson(folderpath);

            Console.WriteLine("Do you want to delete the consensus?\n");
            string ooResult = Console.ReadLine();
            if (ooResult.Equals("y", StringComparison.OrdinalIgnoreCase) || ooResult.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("yes");
                pathtodelete = folderpath + "\\consensus";
                if (Directory.Exists(pathtodelete))
                {
                    Console.WriteLine ("deleting " + (pathtodelete) + "\n");
                    Directory.Delete(pathtodelete, true);
                }
                //reset to null
                pathtodelete = "";
            }
            ooResult = "";
            Console.WriteLine("Do you want to delete the transaction pool?\n");
            ooResult = Console.ReadLine();
            if (ooResult.Equals("y", StringComparison.OrdinalIgnoreCase) || ooResult.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("yes");
                pathtodelete = folderpath + "\\transactionpool";
                if (Directory.Exists(pathtodelete))
                {
                    Console.WriteLine("deleting " + (pathtodelete) + "\n");
                    Directory.Delete(pathtodelete, true);
                }
                //reset to null
                pathtodelete = "";
             }
            ooResult = "";
            Console.WriteLine("Do you want to delete the gateway?\n");
            ooResult = Console.ReadLine();
            if (ooResult.Equals("y", StringComparison.OrdinalIgnoreCase) || ooResult.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("yes");
                pathtodelete = folderpath + "\\gateway";
                if (Directory.Exists(pathtodelete))
                {
                    Console.WriteLine("deleting " + (pathtodelete) + "\n");
                    Directory.Delete(pathtodelete, true);
                }
                //reset to null
                pathtodelete = "";
            }
            ooResult = "";
            Console.WriteLine("Do you want to delete the host folder?\n");
            ooResult = Console.ReadLine();
            if (ooResult.Equals("y", StringComparison.OrdinalIgnoreCase) || ooResult.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("yes");
                pathtodelete = folderpath + "\\host";
                if (Directory.Exists(pathtodelete))
                {
                    Console.WriteLine("deleting " + (pathtodelete) + "\n");
                    Directory.Delete(pathtodelete, true);
                }
                //reset to null
                pathtodelete = "";
            }
            ooResult = "";
            Console.WriteLine("Do you want to delete the renter folder?\n");
            ooResult = Console.ReadLine();
            if (ooResult.Equals("y", StringComparison.OrdinalIgnoreCase) || ooResult.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("yes");
                pathtodelete = folderpath + "\\renter";
                if (Directory.Exists(pathtodelete))
                {
                    Console.WriteLine("deleting " + (pathtodelete) + "\n");
                    Directory.Delete(pathtodelete, true);
                }
                //reset to null
                pathtodelete = "";
            }
            ooResult = "";
            Console.WriteLine("Do you want to delete the wallet data?\n");
            ooResult = Console.ReadLine();
            if (ooResult.Equals("y", StringComparison.OrdinalIgnoreCase) || ooResult.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("yes");
                pathtodelete = folderpath + "\\wallet";
                if (Directory.Exists(pathtodelete))
                {
                    Console.WriteLine("deleting " + (pathtodelete) + "\n");
                    Directory.Delete(pathtodelete, true);
                }
                //reset to null
                pathtodelete = "";
            }
        }
    }

}

