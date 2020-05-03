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

        private static int _percent         = 100;
        private static bool _playerOnly     = false;
        private static bool _clanOnly       = false;
        private static bool _aiOnly         = false;
        private static string _replaceWith  = null;

        
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

                if(root.Name == "ATCConfig")
                {
                    foreach (XmlElement f in root.SelectNodes("MapFaction")) 
                    {
                   
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
                                                      

                        foreach (XmlElement c in f.SelectNodes("Culture")) 
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


                            foreach (XmlElement t in c.SelectNodes("*")) //basic and eliteTroops
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


                                foreach (XmlElement v in t.SelectNodes("*")) //ChildNodes)
                                { 
                                    try { _percent      = v.GetAttribute("percent") != null ? Convert.ToInt32(v.GetAttribute("percent")) : _percent = 100; } catch { _percent = 100; }
                                    try { _playerOnly   = v.GetAttribute("playeronly") != null ? Convert.ToBoolean(v.GetAttribute("playeronly")): _playerOnly = false; } catch { _playerOnly = false; }
                                    try { _clanOnly     = v.GetAttribute("clanonly") != null ? Convert.ToBoolean(v.GetAttribute("clanonly")) : _clanOnly = false; } catch { _clanOnly = false; }
                                    try { _aiOnly       = v.GetAttribute("AIonly") != null ? Convert.ToBoolean(v.GetAttribute("AIonly")) : _aiOnly =false; } catch { _aiOnly = false; }
                                    try { _replaceWith  = v.GetAttribute("replacewith"); } catch { _replaceWith = null; }

                                    ATCVolunteer volunteer = troops.GetVolunteerByID(v.GetAttribute("id"));

                                    if (volunteer == null)
                                    {
                                        volunteer = new ATCVolunteer()
                                        {
                                            VolunteerID     = v.GetAttribute("id"),
                                            TroopPercent    = _percent,
                                            PlayerOnly      = _playerOnly,
                                            ClanOnly        = _clanOnly,
                                            AIOnly          = _aiOnly,
                                            ReplaceWith     = _replaceWith
                                        };
                                        troops.Volunteers.Add(volunteer);
                                        if (!troops.IsEliteTree)
                                            SubModule.log.Add("NEW Configuration for     <volunteer> (basic) " + volunteer.VolunteerID + " added with " + volunteer.TroopPercent + "%.");
                                        else
                                            SubModule.log.Add("NEW Configuration for     <volunteer> (elite) " + volunteer.VolunteerID + " added with " + volunteer.TroopPercent + "%.");
                                    }
                                    else
                                    {
                                        if (!troops.IsEliteTree)
                                            SubModule.log.Add("... Configuration for     <volunteer> (basic)" + volunteer.VolunteerID + " already exists, skipping.");
                                        else
                                            SubModule.log.Add("... Configuration for     <volunteer> (elite)" + volunteer.VolunteerID + " already exists, skipping.");
                                    }
                                }
                            }
                        }
                    }
                }
                else if(root.Name == "ATCTroops")
                {
                    //Load legacy config files and convert
                    SubModule.log.Add("WARNING: Legacy file structure detected! It is advised to move the configuration to the new 1.4.x structure!");

                    foreach(XmlElement s in root.SelectNodes("source_troop"))
                    {
                        ATCLegacyConfig legacyConfig = new ATCLegacyConfig() { FactionIDs = new List<string>(), TargetTroops = new List<ATCVolunteer>() };
                        string sourceTroop = s.GetAttribute("id");

                        // Yes, I hardcoded this... sue me
                        switch (sourceTroop)
                        {
                            case "player_clan_basic":
                                legacyConfig.FactionIDs.Add("player_clan");
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;

                            case "player_clan_elite":
                                legacyConfig.FactionIDs.Add("player_clan");
                                legacyConfig.IsElite = true;
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;


                            case "player_kingdom_basic":
                                legacyConfig.FactionIDs.Add("player_faction");
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;

                            case "player_kingdom_elite":
                                legacyConfig.FactionIDs.Add("player_faction");
                                legacyConfig.IsElite = true;
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;


                            case "imperial_recruit":
                                legacyConfig.FactionIDs.Add("empire");
                                legacyConfig.FactionIDs.Add("empire_w");
                                legacyConfig.FactionIDs.Add("empire_s");
                                foreach(XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;

                            case "imperial_vigla_recruit":
                                legacyConfig.FactionIDs.Add("empire");
                                legacyConfig.FactionIDs.Add("empire_w");
                                legacyConfig.FactionIDs.Add("empire_s");
                                legacyConfig.IsElite = true;
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;


                            case "sturgian_recruit":
                                legacyConfig.FactionIDs.Add("sturgia");
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;

                            case "sturgian_warrior_son":
                                legacyConfig.FactionIDs.Add("sturgia");
                                legacyConfig.IsElite = true;
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;


                            case "aserai_recruit":
                                legacyConfig.FactionIDs.Add("aserai");
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;

                            case "aserai_youth":
                                legacyConfig.FactionIDs.Add("aserai");
                                legacyConfig.IsElite = true;
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;

                            case "vlandian_recruit":
                                legacyConfig.FactionIDs.Add("vlandia");
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;

                            case "vlandian_squire":
                                legacyConfig.FactionIDs.Add("vlandia");
                                legacyConfig.IsElite = true;
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;


                            case "battanian_volunteer":
                                legacyConfig.FactionIDs.Add("battania");
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;

                            case "battanian_highborn_youth":
                                legacyConfig.FactionIDs.Add("battania");
                                legacyConfig.IsElite = true;
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;


                            case "khuzait_nomad":
                                legacyConfig.FactionIDs.Add("khuzait");
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;

                            case "khuzait_noble_son":
                                legacyConfig.FactionIDs.Add("khuzait");
                                legacyConfig.IsElite = true;
                                foreach (XmlElement t in s.SelectNodes("target_troop"))
                                {
                                    legacyConfig.TargetTroops.Add(legacyConfig.GetATCVolunteerFromLegacyFile(t));
                                }
                                break;

                        }

                        foreach (string f in legacyConfig.FactionIDs)
                        {
                            ATCMapFaction faction = GetFaction(f);

                            if (faction == null) //TroopConfig not found
                            {
                                faction = new ATCMapFaction
                                {
                                    FactionID = f,
                                    Cultures = new List<ATCCulture>() 
                                };

                                factionList.Add(faction);
                                SubModule.log.Add("NEW Configuration for <MapFaction> " + f + " added.");
                            }
                            else
                            {
                                SubModule.log.Add("... Configuration for <MapFaction> " + f + " already exists.");
                            }

                            
                            ATCCulture culture = faction.GetCulture(legacyConfig.Culture);  //check if culture alraedy exists

                            if (culture == null)
                            {
                                culture = new ATCCulture
                                {
                                    CultureID = legacyConfig.Culture,
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


                            foreach (ATCVolunteer t in legacyConfig.TargetTroops) //basic and eliteTroops
                            {
                                ATCTroops troops;

                                if (!legacyConfig.IsElite)
                                {
                                    troops = culture.GetBasicTroops();
                                }
                                else
                                {
                                    troops = culture.GetEliteTroops();
                                }


                                ATCVolunteer volunteer = troops.GetVolunteerByID(t.VolunteerID);

                                if (volunteer == null)
                                {
                                    volunteer = new ATCVolunteer()
                                    {
                                        VolunteerID     = t.VolunteerID,
                                        TroopPercent    = t.TroopPercent,
                                        PlayerOnly      = t.PlayerOnly,
                                        ClanOnly        = t.ClanOnly,
                                        AIOnly          = t.AIOnly,
                                        ReplaceWith     = t.ReplaceWith
                                    };
                                    troops.Volunteers.Add(volunteer);
                                    if (!troops.IsEliteTree)
                                        SubModule.log.Add("NEW Configuration for     <volunteer> (basic) " + volunteer.VolunteerID + " added with " + volunteer.TroopPercent + "%.");
                                    else
                                        SubModule.log.Add("NEW Configuration for     <volunteer> (elite) " + volunteer.VolunteerID + " added with " + volunteer.TroopPercent + "%.");
                                }
                                else
                                {
                                    if (!troops.IsEliteTree)
                                        SubModule.log.Add("... Configuration for     <volunteer> (basic)" + volunteer.VolunteerID + " already exists, skipping.");
                                    else
                                        SubModule.log.Add("... Configuration for     <volunteer> (elite)" + volunteer.VolunteerID + " already exists, skipping.");
                                }
                            } 
                        }
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
                        
                        if (c.BasicTroops.Volunteers[i].PlayerOnly)
                            volunteerElement.Add(new XAttribute("playeronly", c.BasicTroops.Volunteers[i].PlayerOnly));

                        if (c.BasicTroops.Volunteers[i].ClanOnly)
                            volunteerElement.Add(new XAttribute("clanonly", c.BasicTroops.Volunteers[i].ClanOnly));

                        if (c.BasicTroops.Volunteers[i].AIOnly) 
                            volunteerElement.Add(new XAttribute("AIonly", c.BasicTroops.Volunteers[i].AIOnly));

                        if (c.BasicTroops.Volunteers[i].ReplaceWith != null && c.BasicTroops.Volunteers[i].ReplaceWith != "")
                            volunteerElement.Add(new XAttribute("replacewith", c.BasicTroops.Volunteers[i].ReplaceWith));
                        basicTroopsElement.Add(volunteerElement);
                    }
                    cultureElement.Add(basicTroopsElement);

                    XElement eliteTroopsElement = new XElement("EliteTroops");
                    for (int i = 0; i < c.EliteTroops.Volunteers.Count; i++)
                    {
                        XElement volunteerElement = new XElement("volunteer", new XAttribute("id", c.EliteTroops.Volunteers[i].VolunteerID));
                        volunteerElement.Add(new XAttribute("percent", c.EliteTroops.Volunteers[i].TroopPercent));
                        
                        if (c.EliteTroops.Volunteers[i].PlayerOnly)
                            volunteerElement.Add(new XAttribute("playeronly", c.EliteTroops.Volunteers[i].PlayerOnly));

                        if (c.EliteTroops.Volunteers[i].ClanOnly)
                            volunteerElement.Add(new XAttribute("clanonly", c.EliteTroops.Volunteers[i].ClanOnly));

                        if (c.EliteTroops.Volunteers[i].AIOnly)
                            volunteerElement.Add(new XAttribute("AIonly", c.EliteTroops.Volunteers[i].AIOnly));

                        if (c.EliteTroops.Volunteers[i].ReplaceWith != null && c.EliteTroops.Volunteers[i].ReplaceWith != "")
                            volunteerElement.Add(new XAttribute("replacewith", c.EliteTroops.Volunteers[i].ReplaceWith));
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



        public static CharacterObject GetBasicRecruit(string factionID, CultureObject culture, string recruitType = null)
        {
            CharacterObject basicRecruit = null;
            
            foreach(ATCMapFaction f in ATCConfig.factionList.Where(f => f.FactionID == factionID))
            {
                foreach (ATCCulture c in f.Cultures.Where(c => c.CultureID == culture.StringId))
                {
                    basicRecruit = c.BasicTroops.GetRandomVolunteer(culture.BasicTroop.StringId, recruitType);
                    if (ATCSettings.Instance.DebugConfigRead)
                    {
                        SubModule.log.Add(String.Concat("DEBUG: GetBasicRecruit called with ", factionID, "|", culture.StringId, " --> ", c.CultureID ," configuration found! Returning \"", basicRecruit.StringId, "\"."));
                    }
                }
                if (basicRecruit == null)  //no specific entry found, try default
                {
                    foreach (ATCCulture c in f.Cultures.Where(c => c.CultureID == "default"))
                    {
                        basicRecruit = c.BasicTroops.GetRandomVolunteer(culture.BasicTroop.StringId, recruitType);
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


        public static CharacterObject GetEliteRecruit(string factionID, CultureObject culture, string recruitType = null)
        {
            CharacterObject eliteRecruit = null;

            foreach (ATCMapFaction f in ATCConfig.factionList.Where(f => f.FactionID == factionID))
            {
                foreach (ATCCulture c in f.Cultures.Where(c => c.CultureID == culture.StringId))
                {
                    eliteRecruit = c.EliteTroops.GetRandomVolunteer(culture.EliteBasicTroop.StringId, recruitType);
                    if (ATCSettings.Instance.DebugConfigRead)
                    {
                        SubModule.log.Add(String.Concat("DEBUG: GetEliteRecruit called with ", factionID, "|", culture.StringId, " --> ", c.CultureID, " configuration found! Returning \"", eliteRecruit.StringId, "\"."));
                    }
                }
                if (eliteRecruit == null)  //no specific entry found, try default
                {
                    foreach (ATCCulture c in f.Cultures.Where(c => c.CultureID == "default"))
                    {
                        eliteRecruit = c.EliteTroops.GetRandomVolunteer(culture.EliteBasicTroop.StringId, recruitType);
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
            //Clean Up Target Troops
            foreach (ATCMapFaction f in factionList.ToArray())
            {
                foreach (ATCCulture c in f.Cultures.ToArray())
                {
                    int _percentBasic = 0;
                    int _percentElite = 0;
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

                    foreach (ATCVolunteer v in c.EliteTroops.Volunteers.ToArray())
                    {

                        if (CharacterObject.Find(v.VolunteerID) == null)
                        {
                            SubModule.log.Add(String.Concat("ERROR: ", v.VolunteerID, " is no valid troop ID (or mod is disabled)! Removing that element to prevent the game from crashing!"));
                            c.EliteTroops.Volunteers.Remove(v);
                        }
                        else
                        {
                            if (!CharacterObject.Find(v.VolunteerID).IsBasicTroop)
                            {
                                SubModule.log.Add(String.Concat("WARNING: ", v.VolunteerID, " is not configured as base troop (is_basic_troop = true)!"));
                            }
                            

                            // Check for flag combinations
                            if (v.AIOnly && v.PlayerOnly)
                            {
                                SubModule.log.Add("WARNING: " + f.FactionID + "|" + c.CultureID + " --> playeronly and aionly are mutually exclusive, you cannot set both to \"true\"! Setting AIOnly to \"false\"!");
                                v.AIOnly = false;
                            }
                            if (v.AIOnly && v.ClanOnly)
                            {
                                SubModule.log.Add("WARNING: " + f.FactionID + "|" + c.CultureID + " --> clanonly and aionly are mutually exclusive, you cannot set both to \"true\"! Setting ClanOnly to \"false\"!");
                                v.ClanOnly = false;
                            }
                            if (v.ClanOnly && v.PlayerOnly)
                            {
                                SubModule.log.Add("WARNING: " + f.FactionID + "|" + c.CultureID + " --> playeronly and clanonly are mutually exclusive, you cannot set both to \"true\"! Setting PlayerOnly to \"false\"!");
                                v.PlayerOnly = false;
                            }


                            // Check for missing replacewith string
                            if ((v.PlayerOnly || v.ClanOnly) && v.ReplaceWith == null)
                            {
                                SubModule.log.Add("WARNING: playeronly or clanonly are \"true\" but no replacewith is set! This may spawn unwanted basic, non-custom troops!");
                            }

                        }
                        _percentElite += v.TroopPercent;
                    }

                    // removed because no <..Troops> tag must be left out!
                    //if (c.BasicTroops.Volunteers.Count == 0 && c.EliteTroops.Volunteers.Count == 0)
                    //{
                    //    f.Cultures.Remove(c);
                    //}

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

    public class ATCLegacyConfig
    {
        public List<string> FactionIDs { get; set; }
        public string Culture { get; set; } = "default";
        public bool IsElite { get; set; } = false;
        public List<ATCVolunteer> TargetTroops { get; set; }

        public ATCVolunteer GetATCVolunteerFromLegacyFile(XmlElement t)
        { 
            int  _percent;
            bool _playerOnly;
            bool _clanOnly;
            bool _aiOnly;

            try { _percent = Convert.ToInt32(t.GetAttribute("percent")); } catch { _percent = 100; }
            try { _playerOnly = Convert.ToBoolean(t.GetAttribute("playeronly")); } catch { _playerOnly = false; }
            try { _clanOnly = Convert.ToBoolean(t.GetAttribute("clanonly")); } catch { _clanOnly = false; }
            try { _aiOnly = Convert.ToBoolean(t.GetAttribute("AIonly")); } catch { _aiOnly = false; }
            
            ATCVolunteer volunteer = new ATCVolunteer()
            {
                VolunteerID     = t.GetAttribute("id"),
                TroopPercent    = _percent,
                PlayerOnly      = _playerOnly,
                ClanOnly        = _clanOnly,
                AIOnly          = _aiOnly,
                ReplaceWith     = null
            };

        return volunteer;
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

        public CharacterObject GetRandomVolunteer(string basicRecruitID, string recruitType)
        {
            int _rng = SubModule.rng.Next(0, 100);
            int _prevPercent = 0;
            string replaceID = basicRecruitID; //fallback
            CharacterObject replacementTroop = null;

            foreach (ATCVolunteer v in Volunteers)
            {
                if (_rng <= (v.TroopPercent + _prevPercent))
                {
                    //recruitType != null means we have a recruit action, not a spawning action and we only replace units
                    //on recruiting, NOT on spawning
                    if (recruitType == "default") 
                    {
                        if (v.PlayerOnly || v.ClanOnly)
                        {
                            if (v.ReplaceWith != null)
                                replaceID = v.ReplaceWith;
                        }
                        else
                        {
                            replaceID = v.VolunteerID;
                        }
                    }
                    else if (recruitType == "player_clan")
                    {
                        if (v.PlayerOnly)
                        {
                            if (v.ReplaceWith != null)
                                replaceID = v.ReplaceWith;
                        }
                        else
                        {
                            replaceID = v.VolunteerID;
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
        public bool ClanOnly { get; set; }
        public bool AIOnly { get; set; }
        public string ReplaceWith { get; set; }

    }

}
