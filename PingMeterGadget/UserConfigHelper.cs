using System;
using System.IO;
using System.Web.Script.Serialization;

namespace PingMeterGadget
{
    class UserConfigHelper
    {
        /// <summary>Full path to savefile.</summary>
        private string filename;
        /// <summary>Directory where savefile should be (used to create such directory if it doesn't exists)</summary>
        private string dir;
        /// <summary>Object where values provided by user are stored. Will get JSONified.</summary>
        public UserConfig cfg;

        /// <summary>
        /// Initializes config object and tries to load calues from hard drive.
        /// </summary>
        /// <param name="prefix">Prefix added to each configuration file</param>
        /// <param name="instanceID">Current instance's ID</param>
        /// <param name="directory">Root directory, if "" then %AppData% is being used</param>
        /// <param name="directory2">Application's folder's name</param>
        /// <param name="extension">Extension of savefile</param>
        public UserConfigHelper(string prefix, string instanceID,
            string directory = "", // will become Environment.SpecialFolder.ApplicationData
            string directory2 = ProgramConfig.defaultFoldername, 
            string extension = ProgramConfig.defaultFileExtension)
        {
            if (directory == "") directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            dir = directory + "\\" + directory2;
            filename = dir + "\\" + prefix + instanceID + "." + extension;
            if (File.Exists(filename))
            {
                cfg = (new JavaScriptSerializer()).Deserialize<UserConfig>(File.ReadAllText(filename));
            }
            else
            {
                cfg = new UserConfig();
                Save(); // will create new folder and file
            }
        }

        /// <summary>Saves current configuration object to hard drive.</summary>
        public void Save()
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(filename, (new JavaScriptSerializer()).Serialize(cfg));
        }
    }
}
