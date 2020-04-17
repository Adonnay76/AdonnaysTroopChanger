using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace AdonnaysTroopChanger.XMLReader
{
    public class ATCconfig
    {
        public static bool IsFileLoaded { get; set; }
        //public static bool EnableModScan { get; set; }
        //public static bool ShowReplacementMsg { get; set; }
        //public static bool ShowPlayeronlyMsg { get; set; }

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

        public void LoadXML(string xmlpath)
        {
            if (xmlpath != "")
            {
                XmlDocument doc = new XmlDocument();

                try
                {
                    SubModule.log.Add("Loading " + xmlpath);
                    doc.Load(xmlpath);
                    IsFileLoaded = true;
                }                
                catch 
                {
                    SubModule.log.Add("Loading Failed!");
                    IsFileLoaded = false;
                }

                XmlElement root = doc.DocumentElement;

                foreach (XmlElement e in root.ChildNodes)
                {
                    switch (e.Name)
                    {
                        case "EnableModScan":

                            //if (!xmlpath.Contains("ATC.config.xml"))
                            //{
                            SubModule.log.Add("WARNING: <EnableModScan> has been moved to the ingame Mod Options!");
                            break;
                            //}
                            //    else
                            //    {
                            //        EnableModScan = Convert.ToBoolean(e.FirstChild.Value);
                            //        SubModule.log.Add("MOD Parameter <EnableModScan> set to " + EnableModScan.ToString());
                            //    }

                            //break;

                        case "debugInfo":
                            //    if (!xmlpath.Contains("ATC.config.xml"))
                            //    {
                            SubModule.log.Add("WARNING: All <debug_info> flags have been moved to the ingame Mod Options!");
                            break;
                        //    }
                        //        foreach (XmlElement di in e.ChildNodes)
                        //    {
                        //        switch (di.Name)
                        //        {
                        //            case "troop_replacement":
                        //                ShowReplacementMsg = Convert.ToBoolean(di.FirstChild.Value);
                        //                SubModule.log.Add("Debug Parameter <troop_replacement> set to " + ShowPlayeronlyMsg.ToString());
                        //                break;

                        //            case "playeronly_flag":
                        //                ShowPlayeronlyMsg = Convert.ToBoolean(di.FirstChild.Value);
                        //                SubModule.log.Add("Debug Parameter <playeronly_flag> set to " + ShowPlayeronlyMsg.ToString());
                        //                break;
                        //        }
                        //    }

                        //    break;

                        case "source_troop":

                            TroopConfig sourceTroop = GetTroopConfig(e.GetAttribute("id"));

                            if (sourceTroop == null) //TroopConfig not found
                            {
                                sourceTroop = new TroopConfig
                                {
                                    SourceID = e.GetAttribute("id"),
                                    TargetTroops = new List<TargetTroop>()
                                };

                                troopConfig.Add(sourceTroop);
                                SubModule.log.Add("New <source_troop> " + sourceTroop.SourceID + " added.");
                            }
                            else
                            {
                                SubModule.log.Add("<source_troop> " + sourceTroop.SourceID + " already exists.");
                            }
                                                      

                            foreach (XmlElement ec in e.ChildNodes)
                            {

                                try { _percent     = Convert.ToInt32(ec.GetAttribute("percent")); } catch { }
                                try { _playerOnly  = Convert.ToBoolean(ec.GetAttribute("playeronly")); } catch { }
                                try { _aionly      = Convert.ToBoolean(ec.GetAttribute("AIonly")); } catch { }
                                try { _cultureOnly = Convert.ToBoolean(ec.GetAttribute("cultureonly")); } catch { }
                                try { _kingdomOnly = Convert.ToBoolean(ec.GetAttribute("kingdomonly")); } catch { }

                                TargetTroop targetTroop = sourceTroop.GetTargetTroopByID(ec.GetAttribute("id"));

                                if (targetTroop == null)
                                {
                                    targetTroop = new TargetTroop() 
                                    {
                                        TroopID = ec.GetAttribute("id"),
                                        TroopPercent = _percent,
                                        PlayerOnly = _playerOnly,
                                        AIOnly = _aionly,
                                        CultureOnly = _cultureOnly,
                                        KingdomOnly = _kingdomOnly,
                                    };
                                    sourceTroop.TargetTroops.Add(targetTroop);
                                    SubModule.log.Add("New <target_troop> " + targetTroop.TroopID + " added.");
                                }
                                else
                                {
                                    SubModule.log.Add("<target_troop> " + targetTroop.TroopID + " already exists, skipping.");
                                }

                            }
                            break;
                    }
                }
            }
        }


        public void SaveMergedXML(string xmlpath)
        {
            SubModule.log.Add("Multiple Configurations found! Writing ATC.config.merged.xml");

            XElement root = new XElement("ATCTroops");           

            foreach (TroopConfig tc in troopConfig)
            {
                XElement sourceTroopElement = new XElement("source_troop", new XAttribute("id", tc.SourceID));
                
                for (int i = 0; i < tc.TargetTroops.Count; i++)
                {
                    XElement targetTroopElement = new XElement("target_troop", new XAttribute("id", tc.TargetTroops[i].TroopID));
                    targetTroopElement.Add(new XAttribute("percent", tc.TargetTroops[i].TroopPercent));
                    targetTroopElement.Add(new XAttribute("playeronly", tc.TargetTroops[i].PlayerOnly));
                    targetTroopElement.Add(new XAttribute("AIonly", tc.TargetTroops[i].AIOnly));
                    targetTroopElement.Add(new XAttribute("cultureonly", tc.TargetTroops[i].CultureOnly));
                    targetTroopElement.Add(new XAttribute("kingdomonly", tc.TargetTroops[i].KingdomOnly));
                    sourceTroopElement.Add(targetTroopElement);
                }
                root.Add(sourceTroopElement);
                
            }

            File.WriteAllText(xmlpath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
            File.AppendAllText(xmlpath, Convert.ToString(root));
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
                foreach (TargetTroop tt in troopConfig[i].TargetTroops)
                {
                    if (tt.PlayerOnly && tt.CultureOnly)
                        SubModule.log.Add("Parse -> " + tt.TroopID + ": playeronly = true, cultureonly = true has no effect!");

                            //InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + ": playeronly = true, cultureonly ignored!", new Color(1, 1, 0)));
                }
            }

        }

        public static void ValidateTroops()
        {
            // Clean Up Source Troops
            foreach (TroopConfig tc in troopConfig.ToArray())
            {
                if(CharacterObject.Find(tc.SourceID) == null)
                { 
                    SubModule.log.Add("ERROR: " + tc.SourceID + " is no valid <source_troop>! Removing that element to prevent the game from crashing!");
                    troopConfig.Remove(tc);
                }
            }

            //Clean Up Target Troops
            foreach (TroopConfig tc in troopConfig.ToArray())
            {
                int _percent = 0;
                foreach (TargetTroop tt in tc.TargetTroops.ToArray())
                {
                    if(CharacterObject.Find(tt.TroopID) == null)
                    {
                        SubModule.log.Add("ERROR: " + tt.TroopID + " is no valid <target_troop> (or mod is disabled)! Removing that element to prevent the game from crashing!");
                        tc.TargetTroops.Remove(tt);
                    }
                    else
                    {
                        if (!CharacterObject.Find(tt.TroopID).IsBasicTroop)
                        {
                            SubModule.log.Add("WARNING: " + tt.TroopID + " is not configured as base troop (is_basic_troop = true)!");
                        }
                    }
                    _percent += tt.TroopPercent;
                }


                //Remove Target Troop when no Source Troops remain
                if(tc.TargetTroops.Count == 0)
                {
                    troopConfig.Remove(tc);
                }

                if(_percent > 100)
                {
                    SubModule.log.Add("WARNING: Percentages of all <target_troops> for " + tc.SourceID + " combind is > 100%; Normalizing distribution.");
                    foreach(TargetTroop tt in tc.TargetTroops)
                    {
                        //tt.TroopPercent = tt.TroopPercent * 100 / _percent; //weighted on current percent
                        tt.TroopPercent = 100 / tc.TargetTroops.Count;
                    }
                }
            }
        }
    }



    public class TroopConfig
    {
        public string SourceID { get; internal set; }
        public List<TargetTroop> TargetTroops { get; set; }


        public TargetTroop GetTargetTroopByID(string stringID)
        {
            return TargetTroops.Find(x => x.TroopID == stringID);
        }

        public CharacterObject GetReplacement(CharacterObject sourceTroop)
        {
            int _rng = SubModule.rng.Next(0, 100);
            int _prevPercent = 0;
            CharacterObject replacementTroop = null;

            foreach (TargetTroop tt in TargetTroops)
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
