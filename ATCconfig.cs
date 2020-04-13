using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
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

                            TroopConfig sourceTroop = GetTroopConfig(e.GetAttribute("id"));

                            if (sourceTroop == null) //TroopConfig not found
                            {
                                sourceTroop = new TroopConfig
                                {
                                    SourceID = e.GetAttribute("id"),
                                    targetTroops = new List<TargetTroop>()
                                };

                                troopConfig.Add(sourceTroop);
                            }
                                                      

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


        public static TroopConfig GetTroopConfig(string stringID)
        {
            return troopConfig.Find(x => x.SourceID == stringID);
        }

        public static CharacterObject GetReplacement(CharacterObject troop)
        {
            CharacterObject _replacement = troop;

            if (GetTroopConfig(troop.StringId) != null)
            {
                _replacement = GetTroopConfig(troop.StringId).GetReplacement(troop);
            }
            

            return _replacement;
            
        }

        public static void Parse()
        {
            for (int i = 0; i < troopConfig.Count; i++)
            {
                foreach (TargetTroop tt in troopConfig[i].targetTroops)
                {
                    if (tt.PlayerOnly && tt.CultureOnly)
                        SubModule.log.Add("Parse -> " + tt.TroopID + ": playeronly = true, cultureonly = true has no effect!");

                            //InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + ": playeronly = true, cultureonly ignored!", new Color(1, 1, 0)));
                }
            }

        }
    }



    public class TroopConfig
    {
        public string SourceID { get; internal set; }
        public List<TargetTroop> targetTroops;

        public CharacterObject GetReplacement(CharacterObject sourceTroop)
        {
            int _rng = SubModule.rng.Next(0, 100);
            int _prevPercent = 0;
            CharacterObject replacementTroop = null;

            foreach (TargetTroop tt in targetTroops)
            {
                if (_rng <= (tt.TroopPercent + _prevPercent))
                {
                    try
                    {
                        replacementTroop = CharacterObject.Find(tt.TroopID);
                    }
                    catch
                    {
                        SubModule.log.Add("GetReplacement -> " + tt.TroopID + " is no valid <target_troop> ID!");
                        //InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));
                    }

                    if (replacementTroop != sourceTroop)
                    {
                        //Debug Message to show the replaced troop
                        //if (ATCconfig.ShowReplacementMsg)
                        //    SubModule.log.Add("GetReplacement -> " + SourceID + " changed to " + replacementTroop.StringId + " (" + _rng + ")");
                        //InformationManager.DisplayMessage(new InformationMessage(SourceID + " changed to " + replacementTroop.StringId + " (" + _rng + ")"));
                        break; //target troop found, we can exit
                    }
                }
                _prevPercent += tt.TroopPercent;
            }

            if (replacementTroop != null)
                return replacementTroop;
            else
                return sourceTroop;

        }
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
