using System.IO;
using System;
using AdonnaysTroopChanger.Settings;
using System.Windows.Forms;
using TaleWorlds.ModuleManager;

namespace AdonnaysTroopChanger
{
    public class Logfile
    {
        private FileInfo fileInfo;
        private bool done = false;
        private string localLogFile;
        
        public Logfile(string logFile)
        {
            localLogFile = logFile;
            File.WriteAllText(logFile, "Log Start - ATC version used " + SubModule.version);
        }

        public void Add(string log)
        {
            if (!done)
            {
                if (fileInfo == null)
                { 
                    fileInfo = new FileInfo(localLogFile);
                }

                fileInfo.Refresh();

            
                if (fileInfo.Length <= ATCSettings.MaxLogSizeInKB * 1024)
                {
                    File.AppendAllText(localLogFile, "\r\n" + $"{DateTime.Now.ToString("MM/dd/yyyy HH:mm")} : " + log);
                }
                else
                {
                    File.AppendAllText(localLogFile, "\r\nMaximum file Size of " + fileInfo.Length / 1024 + " KB reached!");
                    done = true;
                }
            }
        }

        public void Initialize()
        {
            File.AppendAllText(localLogFile, "\r\n");
            File.AppendAllText(localLogFile, "\r\nATC Path     :" + ModuleHelper.GetModuleFullPath("AdonnaysTroopChanger"));
            File.AppendAllText(localLogFile, "\r\nConfig File  :" + SubModule.configFile);
            File.AppendAllText(localLogFile, "\r\nMerged Config:" + SubModule.mergedFile);
            File.AppendAllText(localLogFile, "\r\n");
            File.AppendAllText(localLogFile, "\r\n");
            File.AppendAllText(localLogFile, "\r\nInitializing ATC... ");
            File.AppendAllText(localLogFile, "\r\n----------------------");
            File.AppendAllText(localLogFile, "\r\nMod Scan Enabled:        " + (ATCSettings.EnableModScan ? "Yes" : "No"));
            File.AppendAllText(localLogFile, "\r\nCustom Party Templates:  " + (ATCSettings.EnableCustomTemplates ? "Yes" : "No"));
            File.AppendAllText(localLogFile, "\r\nDebug Recruit Spawn:     " + (ATCSettings.DebugRecruitSpawn ? "Yes" : "No"));
            File.AppendAllText(localLogFile, "\r\nDebug Configuration:     " + (ATCSettings.DebugConfigRead ? "Yes" : "No"));
            File.AppendAllText(localLogFile, "\r\nDebug AI Recruiting:     " + (ATCSettings.DebugAIRecruiting ? "Yes" : "No"));
            File.AppendAllText(localLogFile, "\r\nElite Spawn Chance:      " + ATCSettings.EliteSpawnChance);
            File.AppendAllText(localLogFile, "\r\nElite Spawn Chance Cap:  " + ATCSettings.EliteSpawnChanceCap);
            File.AppendAllText(localLogFile, "\r\nElite Power Threshold:   " + ATCSettings.ElitePowerThreshold);
            File.AppendAllText(localLogFile, "\r\nRecruit Spawn Factor:    " + ATCSettings.RecruitSpawnFactor);
            File.AppendAllText(localLogFile, "\r\nMaximum Log Size in KB:  " + ATCSettings.MaxLogSizeInKB);
            File.AppendAllText(localLogFile, "\r\n");

            

        }
    }
}