using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

namespace Cryptonite
{
    class FilesSensor
    {
        #region Variables
        private List<KeyValuePair<string, string>> keyList;
        private List<String> externalFolders;
        private List<String> hostDrives;
        
        private TimeSpan sleepTime;
        private CancellationTokenSource Token;
        private DetectionEvent dEvent;
        #endregion

        #region Public Variables
        internal readonly String SettingsFile = Directory.GetCurrentDirectory() + @"\\sensor.data";
        #endregion


        #region Contractors
        internal FilesSensor()
        {
            // Setups
            keyList = new List<KeyValuePair<string, string>>();
            hostDrives = new List<string>();
            sleepTime = new TimeSpan(0, 0, 1);
            Token = new CancellationTokenSource();

            externalFolders = new List<string>()
            {
           // @"users\" + HostDetails.CurrentUser + @"\my documents" ,
            @"users\public\documents",
            @"users\" + HostDetails.CurrentUser + @"\desktop",
            "not real"
            };

            // Start Working
            if (Debug.DeleteSettingsFile)
                File.Delete(SettingsFile);
            if (Debug.LoadSettings) 
                LoadSettings();

            makeFiles();

            dEvent = new DetectionEvent(this);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Thread main function - Start running
        /// </summary>
        internal void Start()
        {
            while (!Token.IsCancellationRequested)
            {
                Thread.Sleep(sleepTime);
                testFiles();
            }
        }

        /// <summary>
        /// Stop Running
        /// </summary>
        internal void Stop()
        {
            Token.Cancel();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Base function to create all the files using setup data
        /// </summary>
        private void makeFiles()
        {
            getHostDrives();
            createBaseFiles();
            sprayFiles();
        }
        
        /// <summary>
        /// Get all logical drives on the host
        /// </summary>
        private void getHostDrives()
        {
            DriveInfo[] driveList = DriveInfo.GetDrives();
            foreach (DriveInfo drive in driveList)
            {
                if (drive.IsReady == true)
                {
                    hostDrives.Add(drive.ToString());
                }
            }
        }

        /// <summary>
        /// Create all the keys that all the paths for the class sensors files
        /// </summary>
        private void createBaseFiles() 
        {
            if (keyList.Count() > 0) return;
            
            foreach (string drive in hostDrives)
            {
                string newFileName = drive + Helper.randString() + ".docx";
                KeyValuePair<string, string> newKVP = new KeyValuePair<string, string>(newFileName, "null");
                keyList.Add(newKVP);

                foreach (string external in externalFolders)
                {
                    string baseFolder = drive + external;
                    if (Directory.Exists(baseFolder) == false) continue;
                    newFileName = baseFolder + @"\" + Helper.randString() + ".docx";
                    newKVP = new KeyValuePair<string, string>(newFileName, "null");
                    keyList.Add(newKVP);
                }
            }
        }

        /// <summary>
        /// Try to create new file
        /// </summary>
        private Boolean createFile(KeyValuePair<string, string> fileObject, out string key)
        {
            key = "";
            bool hasError = false;
            if (File.Exists(fileObject.Key))
            {
                key = fileObject.Value;
                return true;
            }

            try
            {
                key = Helper.randString();

                if (!Debug.CreateFiles) return true;

                using (FileStream newFile = File.Create(fileObject.Key))
                {
                    Helper.Print("Writing Key: {0} To File: {1}", key, fileObject.Key);
                    Byte[] newBytes = Helper.GetBytes(key);
                    newFile.Write(newBytes, 0, newBytes.Length);
                    File.SetAttributes(fileObject.Key, FileAttributes.Hidden);
                }

                DirectoryInfo dInfo = new DirectoryInfo(fileObject.Key);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                dInfo.SetAccessControl(dSecurity);
    
            }

            catch (DirectoryNotFoundException)
            {
                Helper.PrintError(ExitCodes.NOT_EXISTS_DIRECTORY, "Trying to write file {0} in a non exists directory", fileObject.Key);
                hasError = true;
            }
            catch (UnauthorizedAccessException)
            {
                Helper.PrintError(ExitCodes.CANT_MAKE_FILE, "Can't Create File: {0}", fileObject.Key);
                hasError = true;
            }

            if (hasError == true)
            {
                keyList.Remove(fileObject);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create all the files missing in system (that you have active key on them)
        /// </summary>
        /// <returns>Amount of files created</returns>
        private Int16 sprayFiles()
        {
            Int16 newFilesCounter = 0;
            List<KeyValuePair<string, string>> newFilesToMake = keyList.Where(X => X.Value == "null").ToList();
            foreach (KeyValuePair<string, string> newPlace in newFilesToMake)
            {
                string keyValue = "";
                Boolean makeFile = createFile(newPlace, out keyValue);
                if (makeFile == true)
                {
                    string tmpKey = newPlace.Key;
                    keyList.Remove(newPlace);
                    keyList.Add(new KeyValuePair<string, string>(tmpKey, keyValue));
                    newFilesCounter++;

                    if (Debug.OneFileLimit && newFilesCounter == 1) goto finish;
                }
            }
        finish:
            saveSettings();
            return newFilesCounter;
        }
        
        /// <summary>
        ///  Run single batch of testing for changed files
        /// </summary>
        /// <returns>True if something changed, false if not</returns>
        /// 
        private Boolean testFiles() 
        {
            List<KeyValuePair<string, string>> filesToTest = keyList.Where(X => X.Value != "null").ToList();
            if (filesToTest.Count() == 0)
            {
                Helper.Print("No Files to Test");
                makeFiles();
                return false;
            }

            foreach (KeyValuePair<string, string> newFile in filesToTest)
            {
                if (File.Exists(newFile.Key) == false)
                {
                    Helper.Print("File Not Found: {0}", newFile.Key);
                    keyList.Remove(newFile);
                    continue;
                }

                try
                {
                    using (FileStream stream = new FileStream(newFile.Key, FileMode.Open, FileAccess.Read))
                    {
                        Byte[] textToLook = Helper.GetBytes(newFile.Value);
                        Byte[] readByte = new Byte[textToLook.Length];
                        stream.Read(readByte, 0, textToLook.Length);
                        //Helper.Print("Reading Text: {0} From File: {1}", GetString(readByte), newFile.Key);
                        string extractedText = Helper.GetString(readByte);
                        if (newFile.Value == extractedText)
                            Helper.Print("File Test {0} Successfull", newFile.Key);
                        else
                        {
                            // Helper.Print("File Test {0} NOT SUCCESSFULL", newFile.Key);
                            Detect(new DetectionEventsArgs( 
                                newFile.Key,
                                newFile.Value,
                                extractedText
                                ));

                            keyList.Remove(newFile);
                            return false;
                        }
                    }
                }
                catch (IOException)
                {
                    continue;
                }
            }

            return true; 
        }

        /// <summary>
        /// Load settings from previous run
        /// </summary>
        /// <returns>True if load complated</returns>
        private Boolean LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                string[] lines = File.ReadAllLines(SettingsFile);
                foreach (string line in lines)
                {
                    string[] lineSplit = line.Split(',');
                    KeyValuePair<string, string> newKey = new KeyValuePair<string, string>(lineSplit[0], lineSplit[1]);
                    keyList.Add(newKey);
                }
                Thread.Sleep(100);
                return true;
            }
            else
            {
                File.Create(SettingsFile).Close();
                Thread.Sleep(100);
                return true;
            }
        }

        /// <summary>
        /// Save settings
        /// </summary>
        /// <returns>True if save complated</returns>
        private Boolean saveSettings() 
        {
            List<string> textToWrite = new List<string>();
            foreach (KeyValuePair<string,string> pair in keyList)
            {
                string newLine = pair.Key + "," + pair.Value;
                textToWrite.Add(newLine);
            }

            try
            {
                File.WriteAllLines(SettingsFile, textToWrite);
            }
            catch (IOException)
            {
                Thread.Sleep(100);
                Helper.PrintError(ExitCodes.CANT_SAVE_SETTINGS_FILES, "Failed to save settings.files, trying again");
                try
                {
                    File.WriteAllLines(SettingsFile, textToWrite);
                }
                catch (IOException)
                {
                    Helper.Exit(ExitCodes.CANT_SAVE_SETTINGS_FILES_QUIT);
                }
            }
            return true; 
        }  

        /// <summary>
        /// Previde unique string
        /// </summary>
        /// <returns>Unique string</returns>
        #endregion

        

        #region Events
        internal delegate void OnDetection(object sender, DetectionEventsArgs e);
        internal event OnDetection Detection;
        protected virtual void Detect(DetectionEventsArgs e)
        {
            if (Detection != null) Detection(this, e);
        }

        #endregion
    }

    
}
