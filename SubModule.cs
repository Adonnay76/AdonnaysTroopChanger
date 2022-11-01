using HarmonyLib;
using System;
using System.IO;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ModuleManager;
using AdonnaysTroopChanger.XMLReader;
using AdonnaysTroopChanger.Settings;
using AdonnaysTroopChanger.Models;
using System.Windows.Forms;



namespace AdonnaysTroopChanger
{

    public class SubModule : MBSubModuleBase
    {

        public static readonly string version = "1.8.1";

        public static readonly string ModuleFolderName = "AdonnaysTroopChanger";

        public static Random rng = new Random();
        public static Logfile log;
        public static bool disableATC = false;

        private string[] foreignConfigs;
        private string[] localATConfigs;
        private static bool mergeConfigs = false;



        private string configPath;
        public static string configFile;
        public static string mergedFile;
        private string logFile;

        public SubModule()
        {
            configPath = String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "\\Mount and Blade II Bannerlord\\ATC\\");
            configFile = String.Concat(configPath, "ATC.settings.xml");
            mergedFile = String.Concat(configPath, "ATC.config.merged.xml");
            logFile = String.Concat(configPath, "ATC.debug.log");
        }

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

            log = new Logfile(logFile);

            Harmony h = new Harmony("mod.bannerlord.adonnay");
            h.PatchAll();
        }


        public override void OnGameInitializationFinished(Game game)
        {

            base.OnGameInitializationFinished(game);

            ATCSettings.Initialize(configFile);
            log.Initialize();


        

            if (Campaign.Current == null)
                return;

            string[] modNames = Campaign.Current.SandBoxManager.ModuleManager.ModuleNames;

            
            //if (!ATCSettings.Instance.EnableModScan)
            if (!ATCSettings.EnableModScan)
            {
                localATConfigs = Directory.GetFiles(configPath, "*ATC.modconfig.xml", SearchOption.AllDirectories);

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

                    //string modPath = String.Concat(modulePath, modName, "/");
                    string modPath = ModuleHelper.GetModuleFullPath(modName);
                    try
                    {
                        foreignConfigs = Directory.GetFiles(modPath, "*ATC.modconfig.xml", SearchOption.AllDirectories);
                    
                
                        foreach (string file in foreignConfigs)
                        {
                            ATCConfig.Instance.LoadXML(file);
                            mergeConfigs = true;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (!mergeConfigs)
                {
                    log.Add("ERROR: Mod Scan enabled but no *ATC.modconfig.xml found in any of the /Modules subfolders!");
                    log.Add("ERROR: Please check if your troop mods are ATC 1.5.x compatible or create your own *ATC.modconfig.xml.");
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

            InformationManager.DisplayMessage(new InformationMessage("ATC " + version + " loaded successfully!", Colors.Green));
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);

            if (!(game.GameType is Campaign))
                return;

            if(!disableATC)
                AddModels(gameStarter as CampaignGameStarter);
        }

        private void AddModels(CampaignGameStarter gameStarter)
        {
            if (gameStarter != null)
            {
                gameStarter.AddModel(new ATCVolunteerProductionModel());
            }
        }

    }
}








