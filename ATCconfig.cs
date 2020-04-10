using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace AdonnaysTroopChanger.XMLReader
{
    public class ATCconfig
    {
        public static bool IsFileLoaded { get; set; }
        public static bool ShowReplacementMsg { get; set; }
        public static bool ShowRNGValue { get; set; }
        public static bool ShowPlayeronlyMsg { get; set; }

        private static ATCconfig _instance = null;
        public static List<TroopConfig> troopConfig = new List<TroopConfig>();


        private static int _percent = 100;
        private static bool _playerOnly = false;
        private static bool _aionly = false;
        private static bool _cultureOnly = false;
        private static bool _kingdomOnly = false;


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

                try
                { 
                    doc.Load(xmlpath);
                    IsFileLoaded = true;
                }                
                catch (FileNotFoundException)
                {
                    IsFileLoaded = false;
                }

                XmlElement root = doc.DocumentElement;

                foreach (XmlElement e in root.ChildNodes)
                {
                    switch (e.Name)
                    {
                        case "debugInfo":
                            foreach (XmlElement di in e.ChildNodes)
                            {
                                switch (di.Name)
                                {
                                    case "troop_replacement":
                                        ShowReplacementMsg = Convert.ToBoolean(di.FirstChild.Value);
                                        break;

                                    case "show_percentage":
                                        ShowRNGValue = Convert.ToBoolean(di.FirstChild.Value);
                                        break;

                                    case "playeronly_flag":
                                        ShowPlayeronlyMsg = Convert.ToBoolean(di.FirstChild.Value);
                                        break;
                                }
                            }

                            break;

                        case "source_troop":

                            TroopConfig sourceTroop = new TroopConfig
                            {
                                SourceID = e.GetAttribute("id"),
                                targetTroops = new List<TargetTroop>()
                            };

                            troopConfig.Add(sourceTroop);

                            foreach (XmlElement ec in e.ChildNodes)
                            {

                                try { _percent     = Convert.ToInt32(ec.GetAttribute("percent")); } catch { }
                                try { _playerOnly  = Convert.ToBoolean(ec.GetAttribute("playeronly")); } catch { }
                                try { _aionly      = Convert.ToBoolean(ec.GetAttribute("AIonly")); } catch { }
                                try { _cultureOnly = Convert.ToBoolean(ec.GetAttribute("cultureonly")); } catch { }
                                try { _kingdomOnly = Convert.ToBoolean(ec.GetAttribute("kingdomonly")); } catch { }
                     

                                sourceTroop.targetTroops.Add(new TargetTroop() { 
                                    TroopID         = ec.GetAttribute("id"), 
                                    TroopPercent    = _percent, 
                                    PlayerOnly      = _playerOnly,
                                    AIOnly          = _aionly,
                                    CultureOnly     = _cultureOnly,
                                    KingdomOnly     = _kingdomOnly,

                                });

                            }
                            break;
                    }
                }
            }
        }



        public static void Parse()
        {
            for (int i = 0; i < troopConfig.Count; i++)
            {
                foreach(TargetTroop tt in troopConfig[i].targetTroops)
                {
                    if (tt.PlayerOnly && tt.CultureOnly)
                        InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + ": playeronly = true, cultureonly ignored!", new Color(1, 1, 0)));

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
        public bool AIOnly { get; set; }
        public bool CultureOnly { get; set; }
        public bool KingdomOnly { get; set; }
    }

}
