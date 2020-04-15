using HarmonyLib;
using System;
using System.IO;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using AdonnaysTroopChanger.XMLReader;
using System.Collections.Generic;




namespace AdonnaysTroopChanger
{

    public class SubModule : MBSubModuleBase
    {
        public static readonly string ModuleFolderName = "AdonnaysTroopChanger";
        public static Random rng = new Random();
        //public static string caller;
        private static bool modsFound = false;
        public static Logfile log;
        private string[] foreignConfigs;


        private readonly string modulePath = String.Concat(BasePath.Name, "Modules/");

        private readonly string configFile = String.Concat(BasePath.Name, "Modules/AdonnaysTroopChanger/Config/ATC.config.xml");
        private readonly string mergedFile = String.Concat(BasePath.Name, "Modules/AdonnaysTroopChanger/Config/ATC.config.merged.xml");

        protected override void OnSubModuleLoad()
        {

            base.OnSubModuleLoad();

            log = new Logfile();


            //if (File.Exists(mergedFile))
            //{
            //    ATCconfig.Instance.LoadXML(mergedFile);
            //}
            //else 
            if (File.Exists(configFile))
            {
                ATCconfig.Instance.LoadXML(configFile);

                foreignConfigs = Directory.GetFiles(modulePath, "*ATC.modconfig.xml", SearchOption.AllDirectories);
                foreach (string file in foreignConfigs)
                {
                    ATCconfig.Instance.LoadXML(file);
                    modsFound = true;
                }

            }

            Harmony h = new Harmony("mod.bannerlord.adonnay");
            h.PatchAll();
        }


        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {

            if (!ATCconfig.IsFileLoaded)
            {
                InformationManager.DisplayMessage(new InformationMessage("ATC config not found! Please check /Config/ATC.debug.log!", new Color(1, 0, 0)));
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("ATC config found!", new Color(0, 1, 0)));
                log.Add("ATC config found!");
                ATCconfig.Parse();
            }
        }

        public override void OnGameLoaded(Game game, object initializerObject)
        {
            ATCconfig.ValidateTroops();

            if (modsFound)
                ATCconfig.Instance.SaveMergedXML(mergedFile);
        }

    }
}








