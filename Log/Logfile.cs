using System.IO;
using System;
using TaleWorlds.Library;

namespace AdonnaysTroopChanger
{
    public class Logfile
    {
        private readonly string filename = String.Concat(BasePath.Name, "Modules/AdonnaysTroopChanger/Config/ATC.debug.log");

        public Logfile()
        {
            File.WriteAllText(filename, "Log Start - ATC version used " + SubModule.version);
        }

        public void Add(string log)
        {
            File.AppendAllText(filename, "\r\n" + $"{DateTime.Now.ToString("MM/dd/yyyy HH:mm")} : " + log);
        }

        public void Initialize()
        {
            File.AppendAllText(filename, "\r\n");
            File.AppendAllText(filename, "\r\n");
            File.AppendAllText(filename, "\r\nInitializing ATC... ");
            File.AppendAllText(filename, "\r\n----------------------");
            File.AppendAllText(filename, "\r\nScan Mode:            " + ATCSettings.Instance.EnableModScan);
            File.AppendAllText(filename, "\r\nDebug Recruit Spawn:  " + ATCSettings.Instance.DebugRecruitSpawn);
            File.AppendAllText(filename, "\r\nDebug Configuration:  " + ATCSettings.Instance.DebugConfig);
            File.AppendAllText(filename, "\r\n");
            File.AppendAllText(filename, "\r\nCulture Change:       " + ATCSettings.Instance.EnableCCC);
            File.AppendAllText(filename, "\r\nCulture Change Amt:   " + ATCSettings.Instance.CCCAmount);
            File.AppendAllText(filename, "\r\nClan Recruit:         " + ATCSettings.Instance.ClanCanRecruit);
            File.AppendAllText(filename, "\r\nRecruit Spawn Factor: " + ATCSettings.Instance.RecruitSpawnFactor);
            File.AppendAllText(filename, "\r\n");

        }
    }
}
