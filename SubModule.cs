using HarmonyLib;
using System;
using System.IO;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using AdonnaysTroopChanger.XMLReader;




namespace AdonnaysTroopChanger
{

    public class SubModule : MBSubModuleBase
    {
        public static readonly string ModuleFolderName = "AdonnaysTroopChanger";
        public static Random rng = new Random();
        public static string caller;
        public static Logfile log;

        protected override void OnSubModuleLoad()
        {

            base.OnSubModuleLoad();

            log = new Logfile();

            string configfile = String.Concat(BasePath.Name, "Modules/AdonnaysTroopChanger/Config/ATC.config.xml");

            if (File.Exists(configfile))
            {
                ATCconfig.Instance.LoadXML(configfile);
            }
           
            Harmony h = new Harmony("mod.bannerlord.adonnay");
            h.PatchAll();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            if (!ATCconfig.IsFileLoaded)
            {
                //InformationManager.DisplayMessage(new InformationMessage("ATC.config.XML not found!", new Color(1, 0, 0)));
                log.Add("ATC.config.XML not found!");

                string configfile = String.Concat(BasePath.Name, "Modules/AdonnaysTroopChanger/Config/EXAMPLE_ATC.config.xml");
                if (File.Exists(configfile))
                {
                    //InformationManager.DisplayMessage(new InformationMessage("...trying to load EXAMPLE_ATC.config.XML", new Color(1, 1, 0)));
                    log.Add("...trying to load EXAMPLE_ATC.config.XML");
                    ATCconfig.Instance.LoadXML(configfile);

                    if (ATCconfig.IsFileLoaded)
                    {
                        //InformationManager.DisplayMessage(new InformationMessage("...EXAMPLE_ATC.config.xml found!", new Color(0, 1, 0)));
                        log.Add("...EXAMPLE_ATC.config.xml found!");
                    }
                    else
                    {
                        //InformationManager.DisplayMessage(new InformationMessage("...EXAMPLE_ATC.config.XML not found!", new Color(1, 0, 0)));
                        log.Add("...EXAMPLE_ATC.config.XML not found!");
                    }
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage("...EXAMPLE_ATC.config.XML not found!", new Color(1, 0, 0)));
                    log.Add("...EXAMPLE_ATC.config.XML not found!");
                }
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("ATC config found!", new Color(0, 1, 0)));
                log.Add("ATC config found!");
                ATCconfig.Parse();
            }
        }
    }
}








