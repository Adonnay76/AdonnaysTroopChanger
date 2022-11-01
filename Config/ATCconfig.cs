using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using AdonnaysTroopChanger.Settings;

namespace AdonnaysTroopChanger.XMLReader
{
    public class ATCConfig
    {
        public static bool IsFileLoaded { get; set; }

        private static ATCConfig _instance = null;
        public static List<ATCMapFaction> factionList = new List<ATCMapFaction>();
        public static List<ATCClan> clanList = new List<ATCClan>();

        private static int _percent = 100;
        private static int _spawnChance = 5;
        private static int _atPower = 200;
        private static int _spawnChanceCap = 20;
        private static bool _playerOnly = false;
        private static bool _clanOnly = false;
        private static bool _aiOnly = false;
        private static string _replaceWith = null;


        public static ATCConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ATCConfig();
                    if (_instance == null)
                        throw new Exception("Unable to find ATCConfig instance in Loader!");
                }
                return _instance;
            }
        }

        public void LoadXML(string xmlpath)
        {
            if (xmlpath != "" && xmlpath != null)
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
                    return;
                }

                XmlElement root = doc.DocumentElement;

                if (root.Name == "ATCConfig")
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

                                if (t.Name == "basicTroops")
                                {
                                    troops = culture.GetBasicTroops();
                                }
                                else
                                {
                                    troops = culture.GetEliteTroops();
                                    try { _spawnChance = t.GetAttribute("spawnChance") != null ? Convert.ToInt32(t.GetAttribute("spawnChance")) : _spawnChance = 5; } catch { _spawnChance = 5; }
                                    try { _atPower = t.GetAttribute("atPower") != null ? Convert.ToInt32(t.GetAttribute("atPower")) : _atPower = 200; } catch { _atPower = 200; }
                                    try { _spawnChanceCap = t.GetAttribute("spawnChanceCap") != null ? Convert.ToInt32(t.GetAttribute("spawnChanceCap")) : _spawnChanceCap = 20; } catch { _spawnChanceCap = 20; }
                                    troops.SpawnChance = _spawnChance;
                                    troops.SpawnChanceCap = _spawnChanceCap;
                                    troops.atPower = _atPower;
                                }


                                foreach (XmlElement v in t.SelectNodes("*")) //ChildNodes)
                                {
                                    try { _percent = v.GetAttribute("percent") != null ? Convert.ToInt32(v.GetAttribute("percent")) : _percent = 100; } catch { _percent = 100; }
                                    try { _playerOnly = v.GetAttribute("playeronly") != null ? Convert.ToBoolean(v.GetAttribute("playeronly")) : _playerOnly = false; } catch { _playerOnly = false; }
                                    try { _clanOnly = v.GetAttribute("clanonly") != null ? Convert.ToBoolean(v.GetAttribute("clanonly")) : _clanOnly = false; } catch { _clanOnly = false; }
                                    try { _aiOnly = v.GetAttribute("AIonly") != null ? Convert.ToBoolean(v.GetAttribute("AIonly")) : _aiOnly = false; } catch { _aiOnly = false; }
                                    try { _replaceWith = v.GetAttribute("replacewith"); } catch { _replaceWith = null; }

                                    ATCVolunteer volunteer = troops.GetVolunteerByID(v.GetAttribute("id"));

                                    if ((volunteer == null) || (volunteer.ReplaceWith != _replaceWith))
                                    {
                                        volunteer = new ATCVolunteer()
                                        {
                                            VolunteerID = v.GetAttribute("id"),
                                            TroopPercent = _percent,
                                            PlayerOnly = _playerOnly,
                                            ClanOnly = _clanOnly,
                                            AIOnly = _aiOnly,
                                            ReplaceWith = _replaceWith
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

                    foreach (XmlElement f in root.SelectNodes("Clan"))
                    {

                        ATCClan clan = GetClan(f.GetAttribute("id"));

                        if (clan == null) //TroopConfig not found
                        {
                            clan = new ATCClan
                            {
                                ClanID = f.GetAttribute("id"),
                                Cultures = new List<ATCCulture>()
                            };

                            clanList.Add(clan);
                            SubModule.log.Add("NEW Configuration for <Clan> " + clan.ClanID + " added.");
                        }
                        else
                        {
                            SubModule.log.Add("... Configuration for <Clan> " + clan.ClanID + " already exists.");
                        }


                        foreach (XmlElement c in f.SelectNodes("Culture"))
                        {
                            ATCCulture culture = clan.GetCulture(c.GetAttribute("id"));  //check if culture alraedy exists

                            if (culture == null)
                            {
                                culture = new ATCCulture
                                {
                                    CultureID = c.GetAttribute("id"),
                                    BasicTroops = new ATCTroops { Volunteers = new List<ATCVolunteer>(), IsEliteTree = false },
                                    EliteTroops = new ATCTroops { Volunteers = new List<ATCVolunteer>(), IsEliteTree = true }
                                    //Volunteers = new List<ATCVolunteer>()
                                };
                                clan.Cultures.Add(culture);
                                SubModule.log.Add("NEW Configuration for   <Culture> " + culture.CultureID + " added.");
                            }
                            else
                            {
                                SubModule.log.Add("... Configuration for   <Culture> " + culture.CultureID + " already exists.");
                            }


                            foreach (XmlElement t in c.SelectNodes("*")) //basic and eliteTroops
                            {
                                ATCTroops troops;

                                if (t.Name == "basicTroops")
                                {
                                    troops = culture.GetBasicTroops();
                                }
                                else
                                {
                                    troops = culture.GetEliteTroops();
                                    try { _spawnChance = t.GetAttribute("spawnChance") != null ? Convert.ToInt32(t.GetAttribute("spawnChance")) : _spawnChance = 5; } catch { _spawnChance = 5; }
                                    try { _atPower = t.GetAttribute("atPower") != null ? Convert.ToInt32(t.GetAttribute("atPower")) : _atPower = 200; } catch { _atPower = 200; }
                                    try { _spawnChanceCap = t.GetAttribute("spawnChanceCap") != null ? Convert.ToInt32(t.GetAttribute("spawnChanceCap")) : _spawnChanceCap = 20; } catch { _spawnChanceCap = 20; }
                                    troops.SpawnChance = _spawnChance;
                                    troops.SpawnChanceCap = _spawnChanceCap;
                                    troops.atPower = _atPower;
                                }


                                foreach (XmlElement v in t.SelectNodes("*")) //ChildNodes)
                                {
                                    try { _percent = v.GetAttribute("percent") != null ? Convert.ToInt32(v.GetAttribute("percent")) : _percent = 100; } catch { _percent = 100; }
                                    try { _playerOnly = v.GetAttribute("playeronly") != null ? Convert.ToBoolean(v.GetAttribute("playeronly")) : _playerOnly = false; } catch { _playerOnly = false; }
                                    try { _clanOnly = v.GetAttribute("clanonly") != null ? Convert.ToBoolean(v.GetAttribute("clanonly")) : _clanOnly = false; } catch { _clanOnly = false; }
                                    try { _aiOnly = v.GetAttribute("AIonly") != null ? Convert.ToBoolean(v.GetAttribute("AIonly")) : _aiOnly = false; } catch { _aiOnly = false; }
                                    try { _replaceWith = v.GetAttribute("replacewith"); } catch { _replaceWith = null; }

                                    ATCVolunteer volunteer = troops.GetVolunteerByID(v.GetAttribute("id"));

                                    if ((volunteer == null) || (volunteer.ReplaceWith != _replaceWith))
                                    {
                                        volunteer = new ATCVolunteer()
                                        {
                                            VolunteerID = v.GetAttribute("id"),
                                            TroopPercent = _percent,
                                            PlayerOnly = _playerOnly,
                                            ClanOnly = _clanOnly,
                                            AIOnly = _aiOnly,
                                            ReplaceWith = _replaceWith
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
                else
                {
                    //Load legacy config files and convert
                    SubModule.log.Add("ERROR: No valid <ATCConfig> found!");
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

                    XElement eliteTroopsElement = new XElement("eliteTroops");
                    eliteTroopsElement.Add(new XAttribute("spawnChance", c.EliteTroops.SpawnChance));
                    eliteTroopsElement.Add(new XAttribute("atPower", c.EliteTroops.atPower));
                    eliteTroopsElement.Add(new XAttribute("spawnChanceCap", c.EliteTroops.SpawnChanceCap));

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

            foreach (ATCClan cl in clanList)
            {
                XElement clanElement = new XElement("Clan", new XAttribute("id", cl.ClanID));

                foreach (ATCCulture c in cl.Cultures)
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

                    XElement eliteTroopsElement = new XElement("eliteTroops");
                    eliteTroopsElement.Add(new XAttribute("spawnChance", c.EliteTroops.SpawnChance));
                    eliteTroopsElement.Add(new XAttribute("atPower", c.EliteTroops.atPower));
                    eliteTroopsElement.Add(new XAttribute("spawnChanceCap", c.EliteTroops.SpawnChanceCap));

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
                    clanElement.Add(cultureElement);
                }
                root.Add(clanElement);
            }

            File.WriteAllText(xmlpath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
            File.AppendAllText(xmlpath, Convert.ToString(root));
        }


        public static ATCMapFaction GetFaction(string factionID)
        {
            return factionList.Find(x => x.FactionID == factionID);
        }

        public static ATCClan GetClan(string clanID)
        {
            return clanList.Find(x => x.ClanID == clanID);
        }



        public static CharacterObject GetFactionRecruit(Hero notable, MobileParty recruitingParty = null)
        {
            string recruitType = null;
            CharacterObject basicRecruit = null;
            CultureObject culture;

            if (recruitingParty != null)
                recruitType = "default";

            // We need a culture as fallback scenario 
            if (notable.CurrentSettlement != null)
            { culture = notable.CurrentSettlement.Culture; }
            else
            { culture = notable.MapFaction.Culture; }


            if (basicRecruit == null)
            {
                string clanID;

                if (recruitingParty != null)
                {
                    if (recruitingParty.Party.Owner?.Clan == Hero.MainHero.Clan)
                    {
                        clanID = "player_clan";
                        recruitType = "player_clan";
                    }
                    else
                    {
                        clanID = recruitingParty.Party.Owner?.Clan.StringId;
                    }
                }
                else
                {
                    if (notable.CurrentSettlement?.OwnerClan == Hero.MainHero.Clan)
                    { clanID = "player_clan"; }
                    else
                    { clanID = notable.CurrentSettlement?.OwnerClan?.StringId; }
                }

                if (IsClanConfigured(clanID))
                {

                    foreach (ATCClan cl in ATCConfig.clanList.Where(f => f.ClanID == clanID))
                    {
                        foreach (ATCCulture c in cl.Cultures.Where(c => c.CultureID == culture.StringId))
                        {

                            if (NotableShouldGiveEliteTroop(notable, c))
                            {
                                basicRecruit = c.EliteTroops.GetRandomVolunteer(culture.EliteBasicTroop.StringId, recruitType);
                                if (ATCSettings.DebugConfigRead)
                                    SubModule.log.Add(String.Concat("DEBUG: GetFactionRecruit called with ", clanID, "|", culture.StringId, " --> configuration found! Returning \"", basicRecruit.StringId, "\"."));
                            }
                            else
                            {
                                basicRecruit = c.BasicTroops.GetRandomVolunteer(culture.BasicTroop.StringId, recruitType);
                                if (ATCSettings.DebugConfigRead)
                                    SubModule.log.Add(String.Concat("DEBUG: GetFactionRecruit called with ", clanID, "|", culture.StringId, " --> configuration found found! Returning \"", basicRecruit.StringId, "\"."));
                            }
                        }
                        if (basicRecruit == null)  //no specific entry found, try default
                        {
                            foreach (ATCCulture c in cl.Cultures.Where(c => c.CultureID == "default"))
                            {
                                if (NotableShouldGiveEliteTroop(notable, c))
                                {
                                    basicRecruit = c.EliteTroops.GetRandomVolunteer(culture.EliteBasicTroop.StringId, recruitType);
                                    if (ATCSettings.DebugConfigRead)
                                        SubModule.log.Add(String.Concat("DEBUG: GetFactionRecruit called with ", clanID, "|", culture.StringId, " --> configuration NOT found, but ", clanID, "|default", " configuration found! Returning \"", basicRecruit.StringId, "\"."));
                                }
                                else
                                {
                                    basicRecruit = c.BasicTroops.GetRandomVolunteer(culture.BasicTroop.StringId, recruitType);
                                    if (ATCSettings.DebugConfigRead)
                                        SubModule.log.Add(String.Concat("DEBUG: GetFactionRecruit called with ", clanID, "|", culture.StringId, " --> configuration NOT found, but ", clanID, "|default", " configuration found! Returning \"", basicRecruit.StringId, "\"."));

                                }
                            }
                        }
                    }
                }
            }


            if (basicRecruit == null)
            {
                string factionID;

                if (recruitingParty != null)
                {
                    if (recruitingParty.MapFaction == Hero.MainHero.MapFaction && recruitingParty.MapFaction?.Leader == Hero.MainHero)
                    {
                        factionID = "player_faction";
                    }
                    else
                    {
                        factionID = recruitingParty.MapFaction.StringId;
                    }
                }
                else
                {
                    if (notable.CurrentSettlement?.Owner?.MapFaction == Hero.MainHero.MapFaction && notable.CurrentSettlement?.Owner?.MapFaction?.Leader == Hero.MainHero)
                    {
                        // needed for populating player faction cities through UpdateVolunteersOfNotables where we have no recruiting party
                        factionID = "player_faction";
                    }
                    else
                    {
                        factionID = notable.MapFaction.StringId;
                    }
                }

                if (IsFactionConfigured(factionID))
                {
                    foreach (ATCMapFaction f in ATCConfig.factionList.Where(f => f.FactionID == factionID))
                    {
                        foreach (ATCCulture c in f.Cultures.Where(c => c.CultureID == culture.StringId))
                        {

                            if (NotableShouldGiveEliteTroop(notable, c))
                            {
                                basicRecruit = c.EliteTroops.GetRandomVolunteer(culture.EliteBasicTroop.StringId, recruitType);
                                if (ATCSettings.DebugConfigRead)
                                    SubModule.log.Add(String.Concat("DEBUG: GetFactionRecruit called with ", factionID, "|", culture.StringId, " --> configuration found! Returning \"", basicRecruit.StringId, "\"."));
                            }
                            else
                            {
                                basicRecruit = c.BasicTroops.GetRandomVolunteer(culture.BasicTroop.StringId, recruitType);
                                if (ATCSettings.DebugConfigRead)
                                    SubModule.log.Add(String.Concat("DEBUG: GetFactionRecruit called with ", factionID, "|", culture.StringId, " --> configuration found found! Returning \"", basicRecruit.StringId, "\"."));
                            }
                        }
                        if (basicRecruit == null)  //no specific entry found, try default
                        {
                            foreach (ATCCulture c in f.Cultures.Where(c => c.CultureID == "default"))
                            {
                                if (NotableShouldGiveEliteTroop(notable, c))
                                {
                                    basicRecruit = c.EliteTroops.GetRandomVolunteer(culture.EliteBasicTroop.StringId, recruitType);
                                    if (ATCSettings.DebugConfigRead)
                                        SubModule.log.Add(String.Concat("DEBUG: GetFactionRecruit called with ", factionID, "|", culture.StringId, " --> configuration NOT found, but ", factionID, "|default", " configuration found! Returning \"", basicRecruit.StringId, "\"."));
                                }
                                else
                                {
                                    basicRecruit = c.BasicTroops.GetRandomVolunteer(culture.BasicTroop.StringId, recruitType);
                                    if (ATCSettings.DebugConfigRead)
                                        SubModule.log.Add(String.Concat("DEBUG: GetFactionRecruit called with ", factionID, "|", culture.StringId, " --> configuration NOT found, but ", factionID, "|default", " configuration found! Returning \"", basicRecruit.StringId, "\"."));

                                }
                            }
                        }
                    }
                }

                if (basicRecruit == null)
                {
                    foreach (ATCMapFaction f in ATCConfig.factionList.Where(f => f.FactionID == "default"))
                    {
                        foreach (ATCCulture c in f.Cultures.Where(c => c.CultureID == culture.StringId))
                        {

                            if (NotableShouldGiveEliteTroop(notable, c))
                            {
                                basicRecruit = c.EliteTroops.GetRandomVolunteer(culture.EliteBasicTroop.StringId, recruitType);
                                //if (ATCSettings.Instance.DebugConfigRead)
                                if (ATCSettings.DebugConfigRead)
                                    SubModule.log.Add(String.Concat("DEBUG: GetFactionRecruit called with ", factionID, "|", culture.StringId, " --> configuration NOT found, but default|", culture.StringId, " found! Returning \"", basicRecruit.StringId, "\"."));
                            }
                            else
                            {
                                basicRecruit = c.BasicTroops.GetRandomVolunteer(culture.BasicTroop.StringId, recruitType);
                                //if (ATCSettings.Instance.DebugConfigRead)
                                if (ATCSettings.DebugConfigRead)
                                    SubModule.log.Add(String.Concat("DEBUG: GetFactionRecruit called with ", factionID, "|", culture.StringId, " --> configuration NOT found, but default|", culture.StringId, " found! Returning \"", basicRecruit.StringId, "\"."));
                            }
                        }
                    }
                }
            }



            if (basicRecruit == null)
            {
                if (NotableShouldGiveEliteTroop(notable, null))
                    basicRecruit = culture.EliteBasicTroop;
                else
                    basicRecruit = culture.BasicTroop;

                //if (ATCSettings.Instance.DebugConfigRead)
                if (ATCSettings.DebugConfigRead)
                {
                    SubModule.log.Add(String.Concat("DEBUG: GetFactionRecruit called with ", notable.MapFaction.StringId, "|", notable.CurrentSettlement?.Culture.StringId, " --> NO matching and NO \"default\" configuration found! Returning the culture's standard recruit \"", basicRecruit.StringId, "\"."));
                }
            }

            return basicRecruit;
        }



        public static bool NotableShouldGiveEliteTroop(Hero notable, ATCCulture cultureConfig = null)
        {
            //double notablePower = (notable.IsRuralNotable && notable.Power >= 200f) ? 1.5 : 0.5;
            float powerThreshold;
            float spawnChance;
            float spawnChanceCap;
            float factor;

            if (notable.CurrentSettlement == null)
                return false;

            if (ATCSettings.EliteOnlyInCastleVillages && notable.IsRuralNotable && notable.CurrentSettlement.Village.Bound.IsCastle)
            {
                return true;
            }


            if (cultureConfig == null)
            {
                powerThreshold = ATCSettings.ElitePowerThreshold;
                spawnChance = ATCSettings.EliteSpawnChance;
                spawnChanceCap = ATCSettings.EliteSpawnChanceCap;
            }
            else
            {
                powerThreshold = cultureConfig.EliteTroops.atPower;
                spawnChance = cultureConfig.EliteTroops.SpawnChance;
                spawnChanceCap = cultureConfig.EliteTroops.SpawnChanceCap;
            }

            if (notable.IsNotable)
            {
                factor = (powerThreshold * powerThreshold) / (Math.Max(50f, notable.Power) * Math.Max(50f, notable.Power));
            }
            else
            {
                factor = 2;
            }
            if (MBRandom.RandomInt(0, 100) <= (int)Math.Min(spawnChance * factor, spawnChanceCap))
            {
                return true;
            }


            return false;
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

                            _percentBasic += v.TroopPercent;
                        }

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
                        SubModule.log.Add(String.Concat("WARNING: Percentages of all basic volunteers for ", f.FactionID, "|", c.CultureID, " combined is ", _percentBasic, "%! Normalizing distribution."));
                        foreach (ATCVolunteer v in c.BasicTroops.Volunteers)
                        {
                            //tt.TroopPercent = tt.TroopPercent * 100 / _percent; //weighted on current percent
                            float _troopPercentOld = v.TroopPercent;
                            v.TroopPercent = Convert.ToInt32(Convert.ToDouble(v.TroopPercent) / Convert.ToDouble(_percentBasic) * 100); //100 / c.BasicTroops.Volunteers.Count;
                            SubModule.log.Add(String.Concat("Troop ", v.VolunteerID, " changed from ", _troopPercentOld, " to ", v.TroopPercent, "."));
                        }
                    }

                    if (_percentElite > 100)
                    {
                        SubModule.log.Add(String.Concat("WARNING: Percentages of all elite volunteers for ", f.FactionID, "|", c.CultureID, " combined is ", _percentElite, "%! Normalizing distribution."));
                        foreach (ATCVolunteer v in c.EliteTroops.Volunteers)
                        {
                            //tt.TroopPercent = tt.TroopPercent * 100 / _percent; //weighted on current percent
                            int _troopPercentOld = v.TroopPercent;
                            v.TroopPercent = Convert.ToInt32(Convert.ToDouble(v.TroopPercent) / Convert.ToDouble(_percentElite) * 100); //100 / c.EliteTroops.Volunteers.Count;
                            SubModule.log.Add(String.Concat("Troop ", v.VolunteerID, " changed from ", _troopPercentOld, " to ", v.TroopPercent, "."));
                        }
                    }
                }
            }
        }

        public static bool IsFactionConfigured(string factionID)
        {
            return GetFaction(factionID) != null;
        }

        public static bool IsClanConfigured(string clanID)
        {
            return GetClan(clanID) != null;
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

    public class ATCClan
    {
        public string ClanID { get; internal set; }
        public List<ATCCulture> Cultures { get; set; }


        public ATCCulture GetCulture(string stringID)
        {
            return Cultures.Find(x => x.CultureID == stringID);
        }

    }


    public class ATCCulture
    {
        public string CultureID { get; set; }
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
        public int SpawnChance;
        public int atPower;
        public int SpawnChanceCap;
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
