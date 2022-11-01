using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace AdonnaysTroopChanger.Settings
{
    public static class ATCSettings
    {
        public static bool EnableModScan              = true;
        public static bool EnableCustomTemplates      = true;
        public static bool DebugConfigRead            = true;
        public static bool DebugRecruitSpawn          = true;
        public static bool DebugAIRecruiting          = true;
        public static float RecruitSpawnFactor        = 1.0f;
        public static bool  EliteOnlyInCastleVillages = true;
        public static float EliteSpawnChance          = 5.0f;
        public static float ElitePowerThreshold       = 200.0f;
        public static float EliteSpawnChanceCap       = 20.0f;
        public static int RecruitMaxUpgradeTier       = 4;
        public static int LevelRecruitsUpToTier       = 3;
        public static int MaxLogSizeInKB              = 500;


        public static void Initialize(string file)
        {
            if (file != "" && file != null)
            {
                XmlDocument doc = new XmlDocument();

                try
                {
                    if (File.Exists(file))
                    {
                        SubModule.log.Add("Loading " + file);
                        doc.Load(file);
                    }
                    else
                    {
                        SubModule.log.Add("No Settings file found, creating new ATC.settings.xml!");
                        CreateSettingsFile(file);
                    }

                }
                catch
                {
                    SubModule.log.Add("Loading Failed! Creating new settings file!");
                    CreateSettingsFile(file);
                    return;
                }

                XmlElement root = doc.DocumentElement;


                try { EnableModScan             = Convert.ToBoolean(root.SelectSingleNode("EnableModScan").InnerText); }                catch { SubModule.log.Add("WARNING: Option EnableModScan not found in ATC.settings.xml, assuming default value 'TRUE'.");               EnableModScan = true; }
                try { EnableCustomTemplates     = Convert.ToBoolean(root.SelectSingleNode("EnableCustomTemplates").InnerText); }        catch { SubModule.log.Add("WARNING: Option EnableCustomTemplates not found in ATC.settings.xml, assuming default value 'TRUE'.");       EnableCustomTemplates = true; }
                try { DebugConfigRead           = Convert.ToBoolean(root.SelectSingleNode("DebugConfigRead").InnerText); }              catch { SubModule.log.Add("WARNING: Option DebugConfigRead not found in ATC.settings.xml, assuming default value 'FALSE'.");            DebugConfigRead = false; }
                try { DebugRecruitSpawn         = Convert.ToBoolean(root.SelectSingleNode("DebugRecruitSpawn").InnerText); }            catch { SubModule.log.Add("WARNING: Option DebugRecruitSpawn not found in ATC.settings.xml, assuming default value 'FALSE'.");          DebugRecruitSpawn = false; }
                try { DebugAIRecruiting         = Convert.ToBoolean(root.SelectSingleNode("DebugAIRecruiting").InnerText); }            catch { SubModule.log.Add("WARNING: Option DebugAIRecruiting not found in ATC.settings.xml, assuming default value 'FALSE'.");          DebugAIRecruiting = false; }
                try { RecruitSpawnFactor        = (float)Convert.ToDouble(root.SelectSingleNode("RecruitSpawnFactor").InnerText); }     catch { SubModule.log.Add("WARNING: Option RecruitSpawnFactor not found in ATC.settings.xml, assuming default value '1.0'.");           RecruitSpawnFactor = 1.0f; }
                try { EliteOnlyInCastleVillages = Convert.ToBoolean(root.SelectSingleNode("DebugAIRecruiting").InnerText); }            catch { SubModule.log.Add("WARNING: Option EliteOnlyInCastleVillages not found in ATC.settings.xml, assuming default value 'FALSE'.");  EliteOnlyInCastleVillages = true; }
                try { EliteSpawnChance          = (float)Convert.ToDouble(root.SelectSingleNode("EliteSpawnChance").InnerText); }       catch { SubModule.log.Add("WARNING: Option EliteSpawnChance not found in ATC.settings.xml, assuming default value '5.0'.");             EliteSpawnChance = 5.0f; }
                try { ElitePowerThreshold       = (float)Convert.ToDouble(root.SelectSingleNode("ElitePowerThreshold").InnerText); }    catch { SubModule.log.Add("WARNING: Option ElitePowerThreshold not found in ATC.settings.xml, assuming default value '200'.");          ElitePowerThreshold = 200.0f; }
                try { EliteSpawnChanceCap       = (float)Convert.ToDouble(root.SelectSingleNode("EliteSpawnChanceCap").InnerText); }    catch { SubModule.log.Add("WARNING: Option EliteSpawnChanceCap not found in ATC.settings.xml, assuming default value '20.0'.");         EliteSpawnChanceCap = 20.0f; }
                try { RecruitMaxUpgradeTier     = Convert.ToInt32(root.SelectSingleNode("RecruitMaxUpgradeTier").InnerText); }          catch { SubModule.log.Add("WARNING: Option RecruitMaxUpgradeTier not found in ATC.settings.xml, assuming default value '5'.");          RecruitMaxUpgradeTier = 4; }
                try { LevelRecruitsUpToTier     = Convert.ToInt32(root.SelectSingleNode("LevelRecruitsUpToTier").InnerText); }          catch { SubModule.log.Add("WARNING: Option LevelRecruitsUpToTier not found in ATC.settings.xml, assuming default value '3'.");          LevelRecruitsUpToTier = 1; }
                try { MaxLogSizeInKB            = Convert.ToInt32(root.SelectSingleNode("MaxLogSizeInKB").InnerText); }                 catch { SubModule.log.Add("WARNING: Option MaxLogSizeInKB not found in ATC.settings.xml, assuming default value '500'.");               MaxLogSizeInKB = 500; }

                try { 
                    if (root.SelectSingleNode("EnableCultureBlending") != null)
                        SubModule.log.Add("INFO: EnableCultureBlending is no longer supported. You can remove this from your ATC.settings.xml."); }
                catch { }

                try
                {
                    if (root.SelectSingleNode("CultureBlendingAmount") != null)
                        SubModule.log.Add("INFO: CultureBlendingAmount is no longer supported. You can remove this from your ATC.settings.xml.");
                }
                catch { }

            }
        }

        private static void CreateSettingsFile(string file)
        {
            XElement root = new XElement("ATCSettings");

            root.Add(new XElement("EnableModScan", EnableModScan));
            root.Add(new XElement("EnableCustomTemplates", EnableModScan));
            root.Add(new XElement("DebugConfigRead", DebugConfigRead));
            root.Add(new XElement("DebugRecruitSpawn",DebugRecruitSpawn));
            root.Add(new XElement("DebugAIRecruiting", DebugAIRecruiting));
            root.Add(new XElement("RecruitSpawnFactor", RecruitSpawnFactor));
            root.Add(new XElement("EliteOnlyInCastleVillages", EliteOnlyInCastleVillages));
            root.Add(new XElement("EliteSpawnChance", EliteSpawnChance));
            root.Add(new XElement("ElitePowerThreshold", ElitePowerThreshold));
            root.Add(new XElement("EliteSpawnChanceCap", EliteSpawnChanceCap));
            root.Add(new XElement("RecruitMaxUpgradeTier", RecruitMaxUpgradeTier));
            root.Add(new XElement("LevelRecruitsUpToTier", LevelRecruitsUpToTier));
            root.Add(new XElement("MaxLogSizeInKB", MaxLogSizeInKB));


        File.WriteAllText(file, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
            File.AppendAllText(file, Convert.ToString(root));

        }



    }
}
