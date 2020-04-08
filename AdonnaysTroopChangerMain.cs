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

        protected override void OnSubModuleLoad()
        {
            //Harmony.DEBUG = true;
            Harmony h = new Harmony("de.adonnay.troopchanger");
            h.PatchAll();

            string configfile = String.Concat(BasePath.Name, "Modules/AdonnaysTroopChanger/Config/ATC.config.xml");

            if (File.Exists(configfile))
            {
                ATCconfig.Instance.LoadXML(configfile);
            }

        }

    }
}


namespace AdonnaysTroopChanger.Patches
{
    [HarmonyPatch(typeof(CultureObject), "BasicTroop", MethodType.Getter)]
    public class PatchBasicTroop
    {
        static void Postfix(ref CharacterObject __result)
        {
            int rngvalue;
            foreach (TroopConfig tc in ATCconfig.troopConfig)
            {
                if (__result.IsBasicTroop & __result.StringId == tc.SourceID)
                {
                    rngvalue = AdonnaysTroopChangerMain.rng.Next(0, 100);
                    int prevPercent = 0;
                    foreach (TargetTroop tt in tc.targetTroops)
                    {
                        if (rngvalue < (tt.TroopPercent + prevPercent))
                        {
                            CharacterObject newTroop = CharacterObject.Find(tt.TroopID);
                            if (newTroop != null) 
                            { 
                                __result = newTroop;
                                InformationManager.DisplayMessage(new InformationMessage("Volunteer Patched to: " + __result.StringId));
                                break; //target troop found, we can exit
                            }
                            else
                            {
                                InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));
                            }
                        }
                        prevPercent = tt.TroopPercent;
                    }

                    break; //target troop found, we can exit
                }
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
            //InformationManager.DisplayMessage(new InformationMessage("RecruitAction"));
            foreach (TroopConfig tc in ATCconfig.troopConfig)
            {
                foreach (TargetTroop tt in tc.targetTroops.Where(tt => tt.PlayerOnly == true))
                {
                    if (Helpers.CharacterHelper.FindUpgradeRootOf(subject).StringId == tt.TroopID)
                    {
                        CharacterObject basicTroop = new CharacterObject();
                        basicTroop = CharacterObject.Find(tc.SourceID);
                        if (basicTroop != null)
                        {
                            InformationManager.DisplayMessage(new InformationMessage(basicTroop.StringId + " recruited instead of " + subject.StringId));

                            individual.VolunteerTypes[bitCode] = subject = basicTroop;
                            break;
                            }
                        else
                        {
                            InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));
                        }
                    }
                }
            }
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

