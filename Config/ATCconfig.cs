using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using TaleWorlds.Library;

namespace AdonnaysTroopChanger.XMLReader
{
    public class ATCConfig
    {
        public static bool IsFileLoaded { get; set; }

        private static ATCConfig _instance = null;
        public static List<ATCMapFaction> factionList = new List<ATCMapFaction>();

        private static int _percent = 100;
        private static bool _playerOnly = false;
        private static bool _aionly = false;
        private static string _replacewith = null;

        
        public static ATCConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ATCConfig();
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

                foreach (XmlElement f in root.SelectNodes("*"))  //Factions
                {
                    switch (f.Name)
                    {

                        case "MapFaction":

                            ATCMapFaction faction = GetFaction(f.GetAttribute("id"));

                            if (faction == null) //TroopConfig not found
                            {
                                faction = new ATCMapFaction
                                {
                                    FactionID = f.GetAttribute("id"),
                                    Cultures = new List<ATCCulture>() 
                                };

                                factionList.Add(faction);
                                SubModule.log.Add("NEW Configuration for <MapFaction> " + faction.FactionID + " added.");
                            }
                            else
                            {
                                SubModule.log.Add("... Configuration for <MapFaction> " + faction.FactionID + " already exists.");
                            }
                                                      

                            foreach (XmlElement c in f.ChildNodes)    //Cultures
                            {
                                ATCCulture culture = faction.GetCulture(c.GetAttribute("id"));  //check if culture alraedy exists

                                if (culture == null)
                                {
                                    culture = new ATCCulture
                                    {
                                        CultureID = c.GetAttribute("id"),
                                        BasicTroops = new ATCTroops { Volunteers = new List<ATCVolunteer>(), IsEliteTree = false },
                                        EliteTroops = new ATCTroops { Volunteers = new List<ATCVolunteer>(), IsEliteTree = true }
                                        //Volunteers = new List<ATCVolunteer>()
                                    };
                                    faction.Cultures.Add(culture);
                                    SubModule.log.Add("NEW Configuration for   <Culture> " + culture.CultureID + " added.");
                                }
                                else
                                {
                                    SubModule.log.Add("... Configuration for   <Culture> " + culture.CultureID + " already exists.");
                                }


                                foreach (XmlElement t in c.ChildNodes)
                                {
                                    ATCTroops troops;

                                    if(t.Name == "basicTroops")
                                    {
                                        troops = culture.GetBasicTroops();
                                    }
                                    else
                                    {
                                        troops = culture.GetEliteTroops();
                                    }

                                    //if(troops == null)
                                    //{
                                    //    troops = new ATCTroops
                                    //    {
                                    //        Volunteers = new List<ATCVolunteer>()
                                    //    };
                                    //}


                                    foreach (XmlElement v in t.ChildNodes)
                                    { 
                                        try { _percent = Convert.ToInt32(v.GetAttribute("percent")); } catch { _percent = 100; }
                                        try { _playerOnly = Convert.ToBoolean(v.GetAttribute("playeronly")); } catch { _playerOnly = false; }
                                        try { _aionly = Convert.ToBoolean(v.GetAttribute("AIonly")); } catch { _aionly = false; }
                                        try { _replacewith = v.GetAttribute("replacewith"); } catch { _replacewith = null; }

                                        ATCVolunteer volunteer = troops.GetVolunteerByID(v.GetAttribute("id"));

                                        if (volunteer == null)
                                        {
                                            volunteer = new ATCVolunteer()
                                            {
                                                VolunteerID = v.GetAttribute("id"),
                                                TroopPercent = _percent,
                                                PlayerOnly = _playerOnly,
                                                ReplaceWith = _replacewith,
                                                AIOnly = _aionly,
                                            };
                                            troops.Volunteers.Add(volunteer);
                                            SubModule.log.Add("NEW Configuration for     <volunteer> " + volunteer.VolunteerID + " added.");
                                        }
                                        else
                                        {
                                            SubModule.log.Add("... Configuration for     <volunteer> " + volunteer.VolunteerID + " already exists, skipping.");
                                        }
                                    }

                                }

                            }
                            break;
                    }
                }
            }
        }


        public void SaveMergedXML(string xmlpath)
        {
            SubModule.log.Add("Configuration(s) found! Writing ATC.config.merged.xml");

            XElement root = new XElement("ATCConfig");           

            foreach (ATCMapFaction f in factionList)
            {
                XElement factionElement = new XElement("MapFaction", new XAttribute("id", f.FactionID));

                foreach (ATCCulture c in f.Cultures)
                {
                    XElement cultureElement = new XElement("Culture", new XAttribute("id", c.CultureID));

                    XElement basicTroopsElement = new XElement("basicTroops");
                    for (int i = 0; i < c.BasicTroops.Volunteers.Count; i++)
                    {
                        XElement volunteerElement = new XElement("volunteer", new XAttribute("id", c.BasicTroops.Volunteers[i].VolunteerID));
                        volunteerElement.Add(new XAttribute("percent", c.BasicTroops.Volunteers[i].TroopPercent));
                        volunteerElement.Add(new XAttribute("playeronly", c.BasicTroops.Volunteers[i].PlayerOnly));
                        volunteerElement.Add(new XAttribute("replacewith", c.BasicTroops.Volunteers[i].ReplaceWith));
                        volunteerElement.Add(new XAttribute("AIonly", c.BasicTroops.Volunteers[i].AIOnly));
                        basicTroopsElement.Add(volunteerElement);
                    }
                    cultureElement.Add(basicTroopsElement);

                    XElement eliteTroopsElement = new XElement("EliteTroops");
                    for (int i = 0; i < c.EliteTroops.Volunteers.Count; i++)
                    {
                        XElement volunteerElement = new XElement("volunteer", new XAttribute("id", c.EliteTroops.Volunteers[i].VolunteerID));
                        volunteerElement.Add(new XAttribute("percent", c.EliteTroops.Volunteers[i].TroopPercent));
                        volunteerElement.Add(new XAttribute("playeronly", c.EliteTroops.Volunteers[i].PlayerOnly));
                        volunteerElement.Add(new XAttribute("replacewith", c.EliteTroops.Volunteers[i].ReplaceWith));
                        volunteerElement.Add(new XAttribute("AIonly", c.EliteTroops.Volunteers[i].AIOnly));
                        eliteTroopsElement.Add(volunteerElement);
                    }
                    cultureElement.Add(eliteTroopsElement);
                    factionElement.Add(cultureElement);
                }
                root.Add(factionElement);
                
            }

            File.WriteAllText(xmlpath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
            File.AppendAllText(xmlpath, Convert.ToString(root));
        }


        public static ATCMapFaction GetFaction(string factionID)
        {
            return factionList.Find(x => x.FactionID == factionID);
        }



        public static CharacterObject GetBasicRecruit(string factionID, CultureObject culture, bool recruitAction = false)
        {
            CharacterObject basicRecruit = null;
            
            foreach(ATCMapFaction f in ATCConfig.factionList.Where(f => f.FactionID == factionID))
            {
                foreach (ATCCulture c in f.Cultures.Where(c => c.CultureID == culture.StringId))
                {
                    basicRecruit = c.BasicTroops.GetRandomVolunteer(culture.BasicTroop.StringId, recruitAction);
                    if (ATCSettings.Instance.DebugConfigRead)
                    {
                        SubModule.log.Add(String.Concat("DEBUG: GetBasicRecruit called with ", factionID, "|", culture.StringId, " --> ", c.CultureID ," configuration found! Returning \"", basicRecruit.StringId, "\"."));
                    }
                }
                if (basicRecruit == null)  //no specific entry found, try default
                {
                    foreach (ATCCulture c in f.Cultures.Where(c => c.CultureID == "default"))
                    {
                        basicRecruit = c.BasicTroops.GetRandomVolunteer(culture.BasicTroop.StringId, recruitAction);
                        if (ATCSettings.Instance.DebugConfigRead)
                        {
                            SubModule.log.Add(String.Concat("DEBUG: GetBasicRecruit called with ", factionID, "|", culture.StringId, " --> \"default\" configuration found! Returning \"", basicRecruit.StringId, "\"."));
                        }
                    }
                }
            }

            if (basicRecruit == null)
            {
                basicRecruit = culture.BasicTroop;
                if (ATCSettings.Instance.DebugConfigRead)
                {
                    SubModule.log.Add(String.Concat("DEBUG: GetBasicRecruit called with ", factionID, "|", culture.StringId, " --> NO matching and NO \"default\" configuration found! Returning the culture's standard recruit \"", basicRecruit.StringId, "\"."));
                }
            }

            return basicRecruit;
        }


        public static CharacterObject GetEliteRecruit(string factionID, CultureObject culture, bool recruitAction = false)
        {
            CharacterObject eliteRecruit = null;

            foreach (ATCMapFaction f in ATCConfig.factionList.Where(f => f.FactionID == factionID))
            {
                foreach (ATCCulture c in f.Cultures.Where(c => c.CultureID == culture.StringId))
                {
                    eliteRecruit = c.EliteTroops.GetRandomVolunteer(culture.EliteBasicTroop.StringId, recruitAction);
                    if (ATCSettings.Instance.DebugConfigRead)
                    {
                        SubModule.log.Add(String.Concat("DEBUG: GetEliteRecruit called with ", factionID, "|", culture.StringId, " --> ", c.CultureID, " configuration found! Returning \"", eliteRecruit.StringId, "\"."));
                    }
                }
                if (eliteRecruit == null)  //no specific entry found, try default
                {
                    foreach (ATCCulture c in f.Cultures.Where(c => c.CultureID == "default"))
                    {
                        eliteRecruit = c.EliteTroops.GetRandomVolunteer(culture.EliteBasicTroop.StringId, recruitAction);
                        if (ATCSettings.Instance.DebugConfigRead)
                        {
                            SubModule.log.Add(String.Concat("DEBUG: GetEliteRecruit called with ", factionID, "|", culture.StringId, " --> \"default\" configuration found! Returning \"", eliteRecruit.StringId, "\"."));
                        }
                    }
                }
            }

            if (eliteRecruit == null)
            {
                eliteRecruit = culture.EliteBasicTroop;
                if (ATCSettings.Instance.DebugConfigRead)
                {
                    SubModule.log.Add(String.Concat("DEBUG: GetEliteRecruit called with ", factionID, "|", culture.StringId, " --> NO matching and NO \"default\" configuration found! Returning the culture's standard recruit \"", eliteRecruit.StringId, "\"."));
                }
            }

            return eliteRecruit; 
        }



        public static void ValidateTroops()
        {
            // Clean Up Source Troops
            //foreach (ATCMapFaction f in factionList.ToArray())
            //{
            //    //ignore special tokens that are NOT troops
            //    if (f.FactionID == "player_clan_basic" || f.FactionID == "player_clan_elite" ||
            //        f.FactionID == "player_kingdom_basic" || f.FactionID == "player_kingdom_elite")
            //    {
            //        return;
            //    }

            //    if (MBObjectManager.Instance.GetObject( .Find(f.FactionID) == null)
            //    {
            //        SubModule.log.Add("ERROR: " + f.FactionID + " is no valid <source_troop>! Removing that element to prevent the game from crashing!");
            //        factionList.Remove(f);
            //    }
            //}

            //Clean Up Target Troops
            foreach (ATCMapFaction f in factionList.ToArray())
            {
                int _percentBasic = 0;
                int _percentElite = 0;
                foreach (ATCCulture c in f.Cultures.ToArray())
                {
                    foreach (ATCVolunteer v in c.BasicTroops.Volunteers.ToArray())
                    {
                    
                        if (CharacterObject.Find(v.VolunteerID) == null)
                        {
                            SubModule.log.Add(String.Concat("ERROR: ", v.VolunteerID, " is no valid troop ID (or mod is disabled)! Removing that element to prevent the game from crashing!"));
                            c.BasicTroops.Volunteers.Remove(v);
                        }
                        else
                        {
                            if (!CharacterObject.Find(v.VolunteerID).IsBasicTroop)
                            {
                                SubModule.log.Add(String.Concat("WARNING: ", v.VolunteerID, " is not configured as base troop (is_basic_troop = true)!"));
                            }
                        }
                        _percentBasic += v.TroopPercent;
                    }

                    foreach (ATCVolunteer v in c.EliteTroops.Volunteers)
                    {

                        if (CharacterObject.Find(v.VolunteerID) == null)
                        {
                            SubModule.log.Add(String.Concat("ERROR: ", v.VolunteerID, " is no valid troop ID (or mod is disabled)! Removing that element to prevent the game from crashing!"));
                            c.BasicTroops.Volunteers.Remove(v);
                        }
                        else
                        {
                            if (!CharacterObject.Find(v.VolunteerID).IsBasicTroop)
                            {
                                SubModule.log.Add(String.Concat("WARNING: ", v.VolunteerID, " is not configured as base troop (is_basic_troop = true)!"));
                            }
                        }
                        _percentElite += v.TroopPercent;
                    }

                    //Remove Target Troop when no Source Troops remain
                    if (c.BasicTroops.Volunteers.Count == 0 && c.EliteTroops.Volunteers.Count == 0)
                    {
                        f.Cultures.Remove(c);
                    }

                    if (_percentBasic > 100)
                    {
                        SubModule.log.Add(String.Concat("WARNING: Percentages of all basic volunteers for ", f.FactionID, "|", c.CultureID, " combined is > 100%; Normalizing distribution."));
                        foreach (ATCVolunteer v in c.BasicTroops.Volunteers)
                        {
                            //tt.TroopPercent = tt.TroopPercent * 100 / _percent; //weighted on current percent
                            v.TroopPercent = 100 / c.BasicTroops.Volunteers.Count;
                        }
                    }

                    if (_percentElite > 100)
                    {
                        SubModule.log.Add(String.Concat("WARNING: Percentages of all elite volunteers for ", f.FactionID, "|", c.CultureID, " combined is > 100%; Normalizing distribution."));
                        foreach (ATCVolunteer v in c.EliteTroops.Volunteers)
                        {
                            //tt.TroopPercent = tt.TroopPercent * 100 / _percent; //weighted on current percent
                            v.TroopPercent = 100 / c.EliteTroops.Volunteers.Count;
                        }
                    }
                }
            }
        }
    }



    public class ATCMapFaction
    {
        public string FactionID { get; internal set; }
        public List<ATCCulture> Cultures { get; set; }


        public ATCCulture GetCulture(string stringID)
        {
            return Cultures.Find(x => x.CultureID == stringID);
        }

    }


    public class ATCCulture
    {
        public string CultureID { get; set;  }
        public ATCTroops BasicTroops { get; set; }
        public ATCTroops EliteTroops { get; set; }

        public ATCTroops GetBasicTroops()
        {
            return BasicTroops;
        }

        public ATCTroops GetEliteTroops()
        {
            return EliteTroops;
        }


    }


    public class ATCTroops
    {
        public List<ATCVolunteer> Volunteers { get; set; }
        public bool IsEliteTree { get; set; }

        public List<ATCVolunteer> GetVolunteers()
        {
            return Volunteers;
        }

        public ATCVolunteer GetVolunteerByID(string stringID)
        {
            return Volunteers.Find(x => x.VolunteerID == stringID);
        }

        public CharacterObject GetRandomVolunteer(string basicRecruitID, bool recruitAction)
        {
            int _rng = SubModule.rng.Next(0, 100);
            int _prevPercent = 0;
            string replaceID;
            CharacterObject replacementTroop = null;

            foreach (ATCVolunteer v in Volunteers)
            {
                if (_rng <= (v.TroopPercent + _prevPercent))
                {
                    if (recruitAction && v.PlayerOnly == true)
                    {
                        if (v.ReplaceWith != null)
                        {
                            replaceID = v.ReplaceWith;
                        }
                        else
                        {
                            replaceID = basicRecruitID;
                        }
                    }
                    else
                    {
                        replaceID = v.VolunteerID;
                    }

                    try
                    {
                        replacementTroop = CharacterObject.Find(replaceID);
                        break;
                    }
                    catch
                    {
                        SubModule.log.Add(String.Concat("GetRandomVolunteer -> ", v.VolunteerID, " is no valid troop ID!"));
                        //InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));
                    }

                }
                _prevPercent += v.TroopPercent;
            }

            if (replacementTroop == null)
                replacementTroop = CharacterObject.Find(basicRecruitID);

            return replacementTroop;

        }

    }



    public class ATCVolunteer
    {
        public string VolunteerID { get; set; }
        public int TroopPercent { get; set; }
        public bool PlayerOnly { get; set; }
        public string ReplaceWith { get; set; }
        public bool AIOnly { get; set; }

    }

}
