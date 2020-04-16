using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using HarmonyLib;
using AdonnaysTroopChanger.XMLReader;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Actions;
using Helpers;

namespace AdonnaysTroopChanger
{

    [HarmonyPatch(typeof(RecruitCampaignBehavior), "UpdateVolunteersOfNotables")]
    public class PatchNotableRecruits
    {
        static bool Prefix()
        {
            if (SubModule.disableATC)
                return true;

            foreach (Settlement settlement in Campaign.Current.Settlements)
            {
                if ((settlement.IsTown && !settlement.Town.IsRebeling) || (settlement.IsVillage && !settlement.Village.Bound.Town.IsRebeling))
                {
                    foreach (Hero hero in settlement.Notables)
                    {
                        if (hero.CanHaveRecruits)
                        {
                            bool flag = false;
                            CultureObject cultureObject = (hero.CurrentSettlement != null) ? hero.CurrentSettlement.Culture : hero.Clan.Culture;

                            // Change cultureObject based on town's loyalty
                            if (Settings.Instance.EnableCCC)
                            {

                                if (settlement.IsTown)
                                {
                                    if (MBRandom.RandomInt(0, 100) <= (Settings.Instance.CCCAmount * hero.CurrentSettlement.Town.Loyalty / 100))
                                    {
                                        cultureObject = (hero.CurrentSettlement != null) ? hero.CurrentSettlement.MapFaction.Culture : hero.Clan.Culture;
                                    }
                                }
                                else if (settlement.IsVillage)
                                {
                                    if (MBRandom.RandomInt(0, 100) <= (Settings.Instance.CCCAmount * hero.CurrentSettlement.Village.Bound.Town.Loyalty / 100))
                                    {
                                        cultureObject = (hero.CurrentSettlement != null) ? hero.CurrentSettlement.MapFaction.Culture : hero.Clan.Culture;
                                    }
                                }
                            }

                            CharacterObject basicTroop = cultureObject.BasicTroop;
                            double num = (hero.IsRuralNotable && hero.Power >= 200) ? 1.0 : 0.5;
                            float num2 = 200f;
                            for (int i = 0; i < 6; i++)
                            {
                                if (MBRandom.RandomFloat < Campaign.Current.Models.VolunteerProductionModel.GetDailyVolunteerProductionProbability(hero, i, settlement))
                                {
                                    if (hero.VolunteerTypes[i] == null)
                                    {
                                        hero.VolunteerTypes[i] = ATCconfig.GetReplacement(basicTroop);
                                        if (Settings.Instance.DebugReplacementMsg && hero.CurrentSettlement.Culture.Name != hero.CurrentSettlement.MapFaction.Culture.Name)
                                            SubModule.log.Add("UpdateVolunteersOfNotables -> Settlement " + hero.CurrentSettlement.Name + "(Culture: " + hero.CurrentSettlement.Culture.Name + ", Owner: " + hero.CurrentSettlement.MapFaction.Name + ") received new recruit: " + hero.VolunteerTypes[i].Name);
                                        flag = true;
                                    }
                                    else
                                    {
                                        float num3 = num2 * num2 / (Math.Max(50f, (float)hero.Power) * Math.Max(50f, (float)hero.Power));
                                        int level = hero.VolunteerTypes[i].Level;
                                        if (MBRandom.RandomInt((int)Math.Max(2.0, (double)((float)level * num3) * num * 1.5)) == 0 && hero.VolunteerTypes[i].UpgradeTargets != null && hero.VolunteerTypes[i].Level < 20)
                                        {
                                            if (hero.VolunteerTypes[i] == basicTroop && HeroHelper.HeroShouldGiveEliteTroop(hero))
                                            {
                                                hero.VolunteerTypes[i] = ATCconfig.GetReplacement(cultureObject.EliteBasicTroop);
                                                flag = true;
                                            }
                                            else
                                            {
                                                hero.VolunteerTypes[i] = hero.VolunteerTypes[i].UpgradeTargets[MBRandom.RandomInt(hero.VolunteerTypes[i].UpgradeTargets.Length)];
                                                flag = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (flag)
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    for (int k = 0; k < 6; k++)
                                    {
                                        if (hero.VolunteerTypes[k] != null)
                                        {
                                            int l = k + 1;
                                            while (l < 6)
                                            {
                                                if (hero.VolunteerTypes[l] != null)
                                                {
                                                    if ((float)hero.VolunteerTypes[k].Level + (hero.VolunteerTypes[k].IsMounted ? 0.5f : 0f) > (float)hero.VolunteerTypes[l].Level + (hero.VolunteerTypes[l].IsMounted ? 0.5f : 0f))
                                                    {
                                                        CharacterObject characterObject = hero.VolunteerTypes[k];
                                                        hero.VolunteerTypes[k] = hero.VolunteerTypes[l];
                                                        hero.VolunteerTypes[l] = characterObject;
                                                        break;
                                                    }
                                                    break;
                                                }
                                                else
                                                {
                                                    l++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

    }


    //public class ATCRecruitAction : RecruitAction
    //{

    //    public static new void GetRecruitVolunteerFromIndividual(MobileParty side1Party, CharacterObject subject, Hero individual, int bitCode)
    //    {

    //    }

    //}



    [HarmonyPatch(typeof(RecruitAction), "GetRecruitVolunteerFromIndividual")]
    public class PatchRecruitActionFromIndividual
    {
        static void Prefix(ref MobileParty side1Party, ref CharacterObject subject, ref Hero individual, int bitCode)
        {
            if (!SubModule.disableATC)
            {

                string _subjectRootID = null;
                CharacterObject _settlementBaseTroop = null;
                CharacterObject _settlementEliteTroop = null;
                //get BaseTroop id
                try
                {
                    _subjectRootID = Helpers.CharacterHelper.FindUpgradeRootOf(subject).StringId;
                    _settlementBaseTroop = individual.CurrentSettlement.Culture.BasicTroop;
                    _settlementEliteTroop = individual.CurrentSettlement.Culture.EliteBasicTroop;
                }
                catch
                {
                    //InformationManager.DisplayMessage(new InformationMessage("Initialization failed for GetRecruit Prefix", new Color(1, 0, 0)));
                    SubModule.log.Add("ERROR: GetRecruitVolunteerFromIndividual -> Initialization failed!");
                    return;
                }


                bool isEliteTroop = false;
                if (_settlementEliteTroop.StringId == _subjectRootID)
                    isEliteTroop = true;
                bool _getout = false;

                CharacterObject replacementTroop = null;

                foreach (TroopConfig tc in ATCconfig.troopConfig.Where(tc => (tc.SourceID == _settlementBaseTroop.StringId) || (tc.SourceID == _settlementEliteTroop.StringId)))
                {
                    //foreach (TargetTroop tt in tc.targetTroops.Where(tt => tt.PlayerOnly == true))                
                    foreach (TargetTroop tt in tc.TargetTroops.Where(tt => tt.TroopID == _subjectRootID))
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
                                SubModule.log.Add("ERROR: GetRecruitVolunteerFromIndividual -> " + tt.TroopID + " invalid!");
                            //InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!", new Color(1, 0, 0)));

                            _getout = true;
                            break;
                        }

                        //Only processe for troops with cultureonly flag = FALSE
                        else if (tt.CultureOnly)
                        {
                            //SubModule.caller = "self"; - OBSOLETE
                            if (side1Party.Leader.Culture.BasicTroop.StringId == tc.SourceID)   //basic_troop = <source_troop> bedeutet gleiche Kultur
                            {
                                //do nothing, the party is allowed to recruit the soldier due to matching culture
                                if (Settings.Instance.DebugPlayerOnlyFlag)
                                    SubModule.log.Add("GetRecruitVolunteerFromIndividual -> " + side1Party.Leader.Name + "(" + side1Party.Leader.Culture.Name + ")" + " recruited a " + subject.Name);
                                //InformationManager.DisplayMessage(new InformationMessage(side1Party.Leader.Name + " (" + side1Party.Leader.Culture.Name + ")" + " recruited a " + subject.Name, new Color(0, 1, 0)));
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
                                    SubModule.log.Add("ERROR: GetRecruitVolunteerFromIndividual -> " + tt.TroopID + " invalid!");
                                //InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));
                            }
                        }

                        //Only processe for troops with kingdomonly flag = FALSE
                        else if (tt.KingdomOnly)
                        {
                            //SubModule.caller = "self"; - OBSOLETE
                            if (side1Party.MapFaction.BasicTroop.StringId == tc.SourceID)   //basic_troop = <source_troop> bedeutet gleiche Kultur
                            {
                                //do nothing, the party is allowed to recruit the soldier due to matching culture
                                if (Settings.Instance.DebugPlayerOnlyFlag)
                                    SubModule.log.Add("GetRecruitVolunteerFromIndividual -> " + side1Party.Leader.Name + " (" + side1Party.MapFaction.Name + ")" + " recruited a " + subject.Name);
                                //InformationManager.DisplayMessage(new InformationMessage(side1Party.Leader.Name + " (" + side1Party.MapFaction.Name + ")" + " recruited a " + subject.Name, new Color(0, 1, 0)));
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
                                    SubModule.log.Add("ERROR: GetRecruitVolunteerFromIndividual -> " + tt.TroopID + " invalid!");
                                //InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));

                            }
                        }

                    }

                    if (_getout)
                        break;
                }

                if (replacementTroop != null)
                {
                    if (Settings.Instance.DebugPlayerOnlyFlag)
                        SubModule.log.Add("GetRecruitVolunteerFromIndividual -> A " + subject.Name + " refused to join " + side1Party.Leader.Culture.Name);
                    //InformationManager.DisplayMessage(new InformationMessage("A " + subject.Name + " refused to join " + side1Party.Leader.Culture.Name, new Color(1, 1, 0)));

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
    }
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

//**************************************************************************************************************************************
//
//  BASE TROOP REPLACEMENT  - OBSOLETE
//
//**************************************************************************************************************************************
//[HarmonyPatch(typeof(CultureObject), "BasicTroop", MethodType.Getter)]
//public class PatchBasicTroop
//{
//    static void Postfix(ref CharacterObject __result)
//    {
//        if (AdonnaysTroopChangerMain.caller != "self")  // workaround to make sure we don't trip over our own feet
//        {
//            int rngvalue;
//            string _resultID = __result.StringId;
//            foreach (TroopConfig tc in ATCconfig.troopConfig.Where(tc => tc.SourceID == _resultID))
//            {

//                rngvalue = AdonnaysTroopChangerMain.rng.Next(0, 100);
//                int prevPercent = 0;

//                // Debug Message to show the actual RNG value
//                if (ATCconfig.ShowRNGValue)
//                    InformationManager.DisplayMessage(new InformationMessage("RNG Value used: " + rngvalue));

//                foreach (TargetTroop tt in tc.targetTroops)
//                {
//                    if (rngvalue <= (tt.TroopPercent + prevPercent))
//                    {
//                        CharacterObject newTroop = CharacterObject.Find(tt.TroopID);
//                        if (newTroop != null)
//                        {
//                            __result = newTroop;

//                            //Debug Message to show the replaced troop
//                            if (ATCconfig.ShowReplacementMsg)
//                                InformationManager.DisplayMessage(new InformationMessage(tc.SourceID + " changed to " + __result.StringId + " (" + rngvalue + ")"));
//                            break; //target troop found, we can exit
//                        }
//                        else
//                        {
//                            InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));
//                        }
//                    }
//                    prevPercent += tt.TroopPercent;
//                }

//            }
//        }
//        else
//        {
//            AdonnaysTroopChangerMain.caller = "";
//        }
//    }
//}


//**************************************************************************************************************************************
//
//  ELITE TROOP REPLACEMENT - OBSOLETE
//
//**************************************************************************************************************************************
//[HarmonyPatch(typeof(CultureObject), "EliteBasicTroop", MethodType.Getter)]
//public class PatchEliteBasicTroop
//{
//    static void Postfix(ref CharacterObject __result)
//    {
//        if (AdonnaysTroopChangerMain.caller != "self")  // workaround to make sure we don't trip over our own feet
//        {
//            int rngvalue;
//            string _resultID = __result.StringId;
//            foreach (TroopConfig tc in ATCconfig.troopConfig.Where(tc => tc.SourceID == _resultID))
//            {

//                rngvalue = AdonnaysTroopChangerMain.rng.Next(0, 100);
//                int prevPercent = 0;

//                // Debug Message to show the actual RNG value
//                if (ATCconfig.ShowRNGValue)
//                    InformationManager.DisplayMessage(new InformationMessage("RNG Value used: " + rngvalue));

//                foreach (TargetTroop tt in tc.targetTroops)
//                {
//                    if (rngvalue <= (tt.TroopPercent + prevPercent))
//                    {
//                        CharacterObject newTroop = CharacterObject.Find(tt.TroopID);
//                        if (newTroop != null)
//                        {
//                            __result = newTroop;

//                            //Debug Message to show the replaced troop
//                            if (ATCconfig.ShowReplacementMsg)
//                                InformationManager.DisplayMessage(new InformationMessage(tc.SourceID + " changed to " + __result.StringId + " (" + rngvalue + ")"));
//                            break; //target troop found, we can exit
//                        }
//                        else
//                        {
//                            InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));
//                        }
//                    }
//                    prevPercent += tt.TroopPercent;
//                }

//            }
//        }
//        else
//        {
//            AdonnaysTroopChangerMain.caller = "";
//        }
//    }
//}

//[HarmonyPatch(typeof(RecruitVolunteerVM), "OnRecruitMoveToCart")]
//public class PatchRecruitVolunteerTroop
//{
//    static void Prefix(ref RecruitVolunteerTroopVM troop)
//    {
//        InformationManager.DisplayMessage(new InformationMessage("RecruitAction"));
//    }
//}