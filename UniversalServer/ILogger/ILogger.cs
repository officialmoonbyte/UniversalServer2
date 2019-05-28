using System;
using System.IO;

namespace Moonbyte.Logging
{
    public class ILogger
    {
        //Generating a new string for the log file.
        public static string Log;

        /// <summary>
        /// Used to add a value to the log string.
        /// </summary>
        public static void AddToLog(string Header, string Value)
        {
            string value = "[" + DateTime.Now.ToString("HH:mm") + "] " + "[" + Header.ToUpper() + "] " + Value;

            //Check if Log is null, if it is not then makes a new line.
            if (Log != null) Log = Log + "\r\n" + value;

            //Cehck if log is null, if it is then set log to value
            if (Log == null) Log = value;

            //Prints value in console
            Console.WriteLine(value);
        }

        /// <summary>
        /// Adds a white space to the command list
        /// </summary>
        public static void AddWhitespace()
        {
            //Add the white space
            if (Log != null) Log += "\r\n";

            //Prints in console
            Console.WriteLine(" ");
        }

        /// <summary>
        /// Set the logging events for the server
        /// </summary>
        public static void SetLoggingEvents()
        {
            AppDomain.CurrentDomain.UnhandledException += ((obj, args) =>
            {
                UnhandledExceptionEventArgs e = args;

                ILogger.AddToLog("Current Domain Error", "Error with App Domain");

                Exception ex = (Exception)e.ExceptionObject;

                ILogger.AddToLog("Current Domain", "Message : " + ex.Message);
                ILogger.AddToLog("Current Domain Error", "StackTrace : " + ex.StackTrace);
                ILogger.AddToLog("Current Domain Error", "Source : " + ex.Source);

                ILogger.WriteLog();
            });
            AppDomain.CurrentDomain.ProcessExit += ((obj, args) =>
            {
                ILogger.WriteLog();
            });
        }

        /// <summary>
        /// Used to write to the log file
        /// </summary>
        public static void WriteLog()
        {

            //Get the execution directory
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

            //Check if the Log is null
            if (Log != null)
            {
                //Delete the log file if it exist.
                if (File.Exists(exeDirectory + "\\Log.log")) File.Delete(exeDirectory + "\\Log.log");

                //Creates the log file, and then close the file stream.
                File.Create(exeDirectory + "\\Log.log").Close();

                //Write to the log file.
                File.WriteAllText(exeDirectory + "\\Log.log", Log);
            }
        }

        public static void LogExceptions(Exception e)
        {
            ILogger.AddToLog("INFO", e.Message);
            ILogger.AddToLog("INFO", e.StackTrace);
        }

    }
}
