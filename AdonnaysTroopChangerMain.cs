using HarmonyLib;
using System;
using System.Linq;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using AdonnaysTroopChanger.XMLReader;



namespace AdonnaysTroopChanger
{

    public class AdonnaysTroopChangerMain : MBSubModuleBase
    {
        //public XmlSerializer serializer = new XmlSerializer(typeof(ATCTroops));
        public static Random rng = new Random();
        public static string caller;

        protected override void OnSubModuleLoad()
        {
            //Harmony.DEBUG = true;
            Harmony h = new Harmony("de.adonnay.troopchanger");
            h.PatchAll();


            //System.Collections.Generic.List<string> list = Directory.GetFiles(String.Concat(BasePath.Name, "Modules/"), "ATC.config.xml", SearchOption.AllDirectories).ToList();
            //for (int i = 0; i < list.Count; i++)
            //{
            //    string f = list[i];
            //    ATCconfig.Instance.LoadXML(f);
            //}

            string configfile = String.Concat(BasePath.Name, "Modules/AdonnaysTroopChanger/Config/ATC.config.xml");

            if (File.Exists(configfile))
            {
                ATCconfig.Instance.LoadXML(configfile);
            }

            base.OnSubModuleLoad();

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            if (!ATCconfig.IsFileLoaded)
            {
                InformationManager.DisplayMessage(new InformationMessage("ATC.config.XML not found!", new Color(1, 0, 0)));

                string configfile = String.Concat(BasePath.Name, "Modules/AdonnaysTroopChanger/Config/EXAMPLE_ATC.config.xml");
                if (File.Exists(configfile))
                {
                    InformationManager.DisplayMessage(new InformationMessage("...trying to load EXAMPLE_ATC.config.XML", new Color(1, 1, 0)));
                    ATCconfig.Instance.LoadXML(configfile);

                    if(ATCconfig.IsFileLoaded)
                        InformationManager.DisplayMessage(new InformationMessage("...EXAMPLE ATC.config.xml found!", new Color(0, 1, 0)));
                }
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("ATC config found!", new Color(0, 1, 0)));
                ATCconfig.Parse();
            }
        }


    }
}


namespace AdonnaysTroopChanger.Patches
{
    //**************************************************************************************************************************************
    //
    //  BASE TROOP REPLACEMENT 
    //
    //**************************************************************************************************************************************
    [HarmonyPatch(typeof(CultureObject), "BasicTroop", MethodType.Getter)]
    public class PatchBasicTroop
    {
        static void Postfix(ref CharacterObject __result)
        {
            if (AdonnaysTroopChangerMain.caller != "self")  // workaround to make sure we don't trip over our own feet
            {
                int rngvalue;
                string _resultID = __result.StringId;
                foreach (TroopConfig tc in ATCconfig.troopConfig.Where(tc => tc.SourceID == _resultID))
                {

                    rngvalue = AdonnaysTroopChangerMain.rng.Next(0, 100);
                    int prevPercent = 0;

                    // Debug Message to show the actual RNG value
                    if (ATCconfig.ShowRNGValue)
                        InformationManager.DisplayMessage(new InformationMessage("RNG Value used: " + rngvalue));

                    foreach (TargetTroop tt in tc.targetTroops)
                    {
                        if (rngvalue <= (tt.TroopPercent + prevPercent))
                        {
                            CharacterObject newTroop = CharacterObject.Find(tt.TroopID);
                            if (newTroop != null)
                            {
                                __result = newTroop;

                                //Debug Message to show the replaced troop
                                if (ATCconfig.ShowReplacementMsg)
                                    InformationManager.DisplayMessage(new InformationMessage(tc.SourceID + " changed to " + __result.StringId + " (" + rngvalue + ")"));
                                break; //target troop found, we can exit
                            }
                            else
                            {
                                InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));
                            }
                        }
                        prevPercent += tt.TroopPercent;
                    }

                }
            }
            else
            {
                AdonnaysTroopChangerMain.caller = "";
            }
        }
    }


    //**************************************************************************************************************************************
    //
    //  ELITE TROOP REPLACEMENT 
    //
    //**************************************************************************************************************************************
    [HarmonyPatch(typeof(CultureObject), "EliteBasicTroop", MethodType.Getter)]
    public class PatchEliteBasicTroop
    {
        static void Postfix(ref CharacterObject __result)
        {
            if (AdonnaysTroopChangerMain.caller != "self")  // workaround to make sure we don't trip over our own feet
            {
                int rngvalue;
                string _resultID = __result.StringId;
                foreach (TroopConfig tc in ATCconfig.troopConfig.Where(tc => tc.SourceID == _resultID))
                {

                    rngvalue = AdonnaysTroopChangerMain.rng.Next(0, 100);
                    int prevPercent = 0;

                    // Debug Message to show the actual RNG value
                    if (ATCconfig.ShowRNGValue)
                        InformationManager.DisplayMessage(new InformationMessage("RNG Value used: " + rngvalue));

                    foreach (TargetTroop tt in tc.targetTroops)
                    {
                        if (rngvalue <= (tt.TroopPercent + prevPercent))
                        {
                            CharacterObject newTroop = CharacterObject.Find(tt.TroopID);
                            if (newTroop != null)
                            {
                                __result = newTroop;

                                //Debug Message to show the replaced troop
                                if (ATCconfig.ShowReplacementMsg)
                                    InformationManager.DisplayMessage(new InformationMessage(tc.SourceID + " changed to " + __result.StringId + " (" + rngvalue + ")"));
                                break; //target troop found, we can exit
                            }
                            else
                            {
                                InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));
                            }
                        }
                        prevPercent += tt.TroopPercent;
                    }

                }
            }
            else
            {
                AdonnaysTroopChangerMain.caller = "";
            }
        }
    }

    //[HarmonyPatch(typeof(RecruitVolunteerVM), "OnRecruitMoveToCart")]
    //public class PatchRecruitVolunteerTroop
    //{
    //    static void Prefix(ref RecruitVolunteerTroopVM troop)
    //    {
    //        InformationManager.DisplayMessage(new InformationMessage("RecruitAction"));
    //    }
    //}


    [HarmonyPatch(typeof(RecruitAction), "GetRecruitVolunteerFromIndividual")]
    public class PatchRecruitActionFromIndividual
    {
        static void Prefix(ref MobileParty side1Party, ref CharacterObject subject, ref Hero individual, int bitCode)
        {
            //get BaseTroop id
            string _subjectRootID = Helpers.CharacterHelper.FindUpgradeRootOf(subject).StringId;
            AdonnaysTroopChangerMain.caller = "self";
            CharacterObject _settlementBaseTroop = individual.CurrentSettlement.Culture.BasicTroop;
            AdonnaysTroopChangerMain.caller = "self";
            CharacterObject _settlementEliteTroop = individual.CurrentSettlement.Culture.EliteBasicTroop;


            bool isEliteTroop = false;
            if (_settlementEliteTroop.StringId == _subjectRootID)
                isEliteTroop = true;
            bool _getout = false;
            
            CharacterObject replacementTroop = null;

            foreach (TroopConfig tc in ATCconfig.troopConfig.Where(tc => (tc.SourceID == _settlementBaseTroop.StringId) || (tc.SourceID ==_settlementEliteTroop.StringId)))
            {
                //foreach (TargetTroop tt in tc.targetTroops.Where(tt => tt.PlayerOnly == true))                
                foreach (TargetTroop tt in tc.targetTroops.Where(tt => tt.TroopID == _subjectRootID)) 
                {
                    //Only processe for troops with playeronly flag = TRUE
                    if (tt.PlayerOnly) 
                    {
                        
                        if (tt.KingdomOnly && (side1Party.MapFaction == Hero.MainHero.MapFaction))
                            break; //allow recruitment

                        if (isEliteTroop)
                            replacementTroop = _settlementEliteTroop;
                        else
                            replacementTroop = _settlementBaseTroop; 
                        
                        if (replacementTroop == null)
                            InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!", new Color(1, 0, 0)));
                         
                        _getout = true;
                        break;
                    }

                    //Only processe for troops with cultureonly flag = FALSE
                    else if (tt.CultureOnly)
                    {
                        AdonnaysTroopChangerMain.caller = "self";
                        if (side1Party.Leader.Culture.BasicTroop.StringId == tc.SourceID)   //basic_troop = <source_troop> bedeutet gleiche Kultur
                        {
                            //do nothing, the party is allowed to recruit the soldier due to matching culture
                            if (ATCconfig.ShowPlayeronlyMsg)
                                InformationManager.DisplayMessage(new InformationMessage(side1Party.Leader.Name + " (" + side1Party.Leader.Culture.Name + ")" + " recruited a " + subject.Name, new Color(0, 1, 0)));
                            replacementTroop = null;
                            _getout = true;
                            break;
                        }
                        else
                        {
                            if (isEliteTroop)
                                replacementTroop = _settlementEliteTroop;
                            else
                                replacementTroop = _settlementBaseTroop;

                            if (replacementTroop == null)
                                InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));
                        }
                    }

                    //Only processe for troops with kingdomonly flag = FALSE
                    else if (tt.KingdomOnly)
                    {
                        AdonnaysTroopChangerMain.caller = "self";
                        if (side1Party.MapFaction.BasicTroop.StringId == tc.SourceID)   //basic_troop = <source_troop> bedeutet gleiche Kultur
                        {
                            //do nothing, the party is allowed to recruit the soldier due to matching culture
                            if (ATCconfig.ShowPlayeronlyMsg)
                                InformationManager.DisplayMessage(new InformationMessage(side1Party.Leader.Name + " (" + side1Party.MapFaction.Name + ")" + " recruited a " + subject.Name, new Color(0, 1, 0)));
                            replacementTroop = null;
                            _getout = true;
                            break;
                        }
                        else
                        {
                            if (isEliteTroop)
                                replacementTroop = _settlementEliteTroop;
                            else
                                replacementTroop = _settlementBaseTroop;

                            if (replacementTroop == null)
                                InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));

                        }
                    }

                }

                if (_getout)
                    break;
            }

            if (replacementTroop != null)
            {
                if (ATCconfig.ShowPlayeronlyMsg)
                    InformationManager.DisplayMessage(new InformationMessage("A " + subject.Name + " refused to join " + side1Party.Leader.Culture.Name, new Color(1, 1, 0)));

                individual.VolunteerTypes[bitCode] = subject = replacementTroop;
            }


            _settlementBaseTroop = null;
            _settlementEliteTroop = null;
        }

        //static void Postfix(ref MobileParty side1Party, ref CharacterObject subject, ref Hero individual, int bitCode)
        //{
        //    //InformationManager.DisplayMessage(new InformationMessage("RecruitAction"));
        //}
    }


    //[HarmonyPatch(typeof(RecruitAction), "GetRecruitVolunteerFromIndividualToGarrison")]
    //public class PatchRecruitActionToGarrison
    //{
    //    static void Prefix(ref MobileParty side1Party, ref CharacterObject subject, ref Hero individual, int bitCode)
    //    {
    //        InformationManager.DisplayMessage(new InformationMessage("RecruitAction"));
    //    }

    //    static void Postfix(ref MobileParty side1Party, ref CharacterObject subject, ref Hero individual, int bitCode)
    //    {
    //        InformationManager.DisplayMessage(new InformationMessage("RecruitAction"));
    //    }
    //}

}

