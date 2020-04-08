using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using TaleWorlds.Core;

namespace AdonnaysTroopChanger.XMLReader
{
    public class ATCconfig
    {
        private static ATCconfig _instance = null;
        public static List<TroopConfig> troopConfig = new List<TroopConfig>();
        public static ATCconfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ATCconfig();
                    if (_instance == null)
                        throw new Exception("Unable to find settings in Loader");
                }
                return _instance;
            }
        }

        public void LoadXML(string xmlpath = "")
        {
            if (xmlpath != "")
            {
                XmlDocument doc = new XmlDocument();

                doc.Load(xmlpath);
                //XmlNode xmlNodes = xmlDocument.SelectSingleNode("ATCTroops");
                XmlElement root = doc.DocumentElement;

                foreach (XmlElement e in root.ChildNodes)
                {
                    switch (e.Name)
                    {
                        case "source_troop":

                            TroopConfig sourceTroop = new TroopConfig
                            {
                                SourceID = e.GetAttribute("id"),
                                targetTroops = new List<TargetTroop>()
                            };

                            troopConfig.Add(sourceTroop);

                            foreach (XmlElement ec in e.ChildNodes)
                            {
                                sourceTroop.targetTroops.Add(new TargetTroop() { TroopID = ec.GetAttribute("id"), TroopPercent = Convert.ToInt32(ec.GetAttribute("percent")), PlayerOnly = Convert.ToBoolean(ec.GetAttribute("playeronly")) });
                            }
                            break;
                    }
                }
            }
        }
    }



    public class TroopConfig
    {
        public string SourceID { get; internal set; }
        public List<TargetTroop> targetTroops;
    }

    public class TargetTroop
    {
        public string TroopID { get; set; }
        public int TroopPercent { get; set; }
        public bool PlayerOnly { get; set; }
    }
}
