using HarmonyLib;
using System;
using System.IO;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using AdonnaysTroopChanger.XMLReader;
using System.Windows.Forms;




namespace AdonnaysTroopChanger
{

    public class SubModule : MBSubModuleBase
    {

        public static readonly string version = "1.4.0.5";

        public static readonly string ModuleFolderName = "AdonnaysTroopChanger";
        public static Random rng = new Random();
        public static Logfile log;
        public static bool disableATC = false;

        private string[] foreignConfigs;
        private string[] localATConfigs;
        private static bool mergeConfigs = false;

        private readonly string modulePath = String.Concat(BasePath.Name, "Modules/");
        private readonly string modATCPath = String.Concat(BasePath.Name, "Modules/AdonnaysTroopChanger/");
        private readonly string configPath = String.Concat(BasePath.Name, "Modules/AdonnaysTroopChanger/Config/");
        private readonly string mergedFile = String.Concat(BasePath.Name, "Modules/AdonnaysTroopChanger/Config/ATC.config.merged.xml");

        protected override void OnSubModuleLoad()
        {

            base.OnSubModuleLoad();

            if (!Directory.Exists(configPath))
            {
                try
                {
                    Directory.CreateDirectory(configPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
                

            log = new Logfile();

            Harmony h = new Harmony("mod.bannerlord.adonnay");
            h.PatchAll();

        }


        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {

     
        }

        public override void OnGameInitializationFinished(Game game)
        {

            base.OnGameInitializationFinished(game);

            log.Initialize();

            if (Campaign.Current == null)
                return;

            string[] modNames = Campaign.Current.SandBoxManager.ModuleManager.ModuleNames;

            if (!ATCSettings.Instance.EnableModScan)
            {
                localATConfigs = Directory.GetFiles(modATCPath, "*ATC.modconfig.xml", SearchOption.AllDirectories);

                if (localATConfigs.Length != 0)
                {
                    foreach (string file in localATConfigs)
                    {
                        ATCConfig.Instance.LoadXML(file);
                        mergeConfigs = true;
                    }
                }
                else
                {
                    log.Add("ERROR: Mod Scan disabled and no *ATC.modconfig.xml found in /AdonnaysTroopChanger/Config!");
                    log.Add("ERROR: Either allow ATC to scan for mods in Mod Options -> Enable Mod Scan (recommended for beginners!) or create your own ATC.modconfig.xml (recommended for advanced users).");
                    log.Add("FATAL ERROR: ATC Disabled!");
                    disableATC = true;
                }
            }
            else
            {
                foreach(string modName in modNames)
                {
                    string modPath = String.Concat(modulePath, modName, "/");
                    foreignConfigs = Directory.GetFiles(modPath, "*ATC.modconfig.xml", SearchOption.AllDirectories);
                
                    foreach (string file in foreignConfigs)
                    {
                        ATCConfig.Instance.LoadXML(file);
                        mergeConfigs = true;
                    }
                }

                if (!mergeConfigs)
                {
                    log.Add("ERROR: Mod Scan enabled but no *ATC.modconfig.xml found in any of the /Modules subfolders!");
                    log.Add("ERROR: Please check if your troop mods are ATC 1.3.x compatible or create your own *ATC.modconfig.xml.");
                    log.Add("ERROR: Workaround: If the troop mod came with an 1.1.x ATC.config.xml, just rename that to ATC.modconfig.xml.");
                    log.Add("FATAL ERROR: ATC Disabled!");
                    disableATC = true;
                }
            }


            if (SubModule.disableATC)
                return;

            ATCConfig.ValidateTroops();

            if (mergeConfigs)
            {
                ATCConfig.Instance.SaveMergedXML(mergedFile);
            }
        }
    }





}








