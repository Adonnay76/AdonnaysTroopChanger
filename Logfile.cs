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
            File.WriteAllText(filename, "Log Start");
        }

        public void Add(string log)
        {
            File.AppendAllText(filename, "\r\n" + $"{DateTime.Now.ToString("MM/dd/yyyy HH:mm")} : " + log);
        }

    }
}
