#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
#endregion

namespace RemoveRevitBackups
{
    [Transaction(TransactionMode.Manual)]
    public class cmdRemoveRevitBackups : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            // Set Varaibles
            int counter = 0;
            string logPath = "";

            // Create List for Log File
            List<string> deletedFileLog = new List<string>();
            deletedFileLog.Add("The following backup files have been deleted:");

            FolderBrowserDialog selectFolder = new FolderBrowserDialog();
            selectFolder.ShowNewFolderButton = false;

            //Open Folder Dialog And Only Run Code If a Folder Is Selected
            if(selectFolder.ShowDialog() == DialogResult.OK)
            {
                // Get The Selected Folder Path
                string directory = selectFolder.SelectedPath;

                // Get All Files From Selected Folder
                string[] files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

                // Loop Through Files
                foreach(string file in files)
                {
                    // Check if the File Is a Revit File
                    if(Path.GetExtension(file) == ".rvt" || Path.GetExtension(file) == ".rfa")
                    {
                        // Get The Last 9 CHaractrs Of Filename To Check If Backup
                        string checkString = file.Substring(file.Length - 9, 9);

                        if(checkString.Contains(".0") == true)
                        {
                            // Add Filename To The List
                            deletedFileLog.Add(file);

                            // Delete File
                            File.Delete(file);

                            // Increment Counter
                            counter++;
                        }
                    }
                }
                // Output The Log File
                if(counter > 0)
                {
                    logPath = WriteListToTxt(deletedFileLog, directory);
                }
            }

            // Alert The User
            TaskDialog td = new TaskDialog("Complete");
            td.MainInstruction = "Deleted " + counter.ToString() + " backup files";
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Click to view lo file");
            td.CommonButtons = TaskDialogCommonButtons.Ok;

            TaskDialogResult result = td.Show();

            if(result == TaskDialogResult.CommandLink1)
            {
                Process.Start(logPath);
            }

            return Result.Succeeded;
        }
        internal string WriteListToTxt(List<string> strinList, string filePath)
        {
            string fileName = "_DeleteBackupFiles.txt";
            string fullPath = filePath + @"\" + fileName;

            File.WriteAllLines(fullPath, strinList);

            return fullPath;
        }
    }
}
