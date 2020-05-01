using System;
using System.Linq;
using HarmonyLib;
using AdonnaysTroopChanger.XMLReader;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Actions;
using Helpers;
using System.Collections.Generic;

namespace AdonnaysTroopChanger
{

    [HarmonyPatch(typeof(RecruitCampaignBehavior), "UpdateVolunteersOfNotables")]
    public class PatchNotableRecruits
    {
        static bool Prefix()
        {
            if (SubModule.disableATC)
                return true;


            string factionID;

            foreach (Settlement settlement in Campaign.Current.Settlements)
            {

                if (settlement.OwnerClan == Hero.MainHero.Clan && IsPlayerClanConfigured())
                {
                    factionID = "player_clan";
                }
                else if (settlement.MapFaction == Hero.MainHero.MapFaction && Hero.MainHero.MapFaction.Leader == Hero.MainHero && IsPlayerKingdomConfigured())
                {
                    factionID = "player_faction";
                }
                else
                {
                    factionID = settlement.MapFaction.StringId;
                }



                if ((settlement.IsTown && !settlement.Town.IsRebeling) || (settlement.IsVillage && !settlement.Village.Bound.Town.IsRebeling))
                {
                    foreach (Hero notable in settlement.Notables)
                    {
                        if (notable.CanHaveRecruits)
                        {
                            bool troopAdded = false;
                            CultureObject cultureObject = (notable.CurrentSettlement != null) ? notable.CurrentSettlement.Culture : notable.Clan.Culture;

                            // Change cultureObject based on town's loyalty
                            if (ATCSettings.Instance.EnableCCC)
                            {
                                if (settlement.IsTown)
                                {
                                    if (MBRandom.RandomInt(0, 100) <= (ATCSettings.Instance.CCCAmount * notable.CurrentSettlement.Town.Loyalty / 100))
                                    {
                                        cultureObject = (notable.CurrentSettlement != null) ? notable.CurrentSettlement.MapFaction.Culture : notable.Clan.Culture;
                                    }
                                }
                                else if (settlement.IsVillage)
                                {
                                    if (MBRandom.RandomInt(0, 100) <= (ATCSettings.Instance.CCCAmount * notable.CurrentSettlement.Village.Bound.Town.Loyalty / 100))
                                    {
                                        cultureObject = (notable.CurrentSettlement != null) ? notable.CurrentSettlement.MapFaction.Culture : notable.Clan.Culture;
                                    }
                                }
                            }

                            CharacterObject basicTroop = cultureObject.BasicTroop;
                            double num = (notable.IsRuralNotable && notable.Power >= 200) ? 1.0 : 0.5;
                            float num2 = 200f;
                            for (int i = 0; i < 6; i++)
                            {
                                if (MBRandom.RandomFloat < (Campaign.Current.Models.VolunteerProductionModel.GetDailyVolunteerProductionProbability(notable, i, settlement) * ATCSettings.Instance.RecruitSpawnFactor))
                                {
                                    if (notable.VolunteerTypes[i] == null)
                                    {
                                        if (ATCSettings.Instance.DebugRecruitSpawn)
                                            SubModule.log.Add(String.Concat("DEBUG: UpdateVolunteersOfNotables getting recruit for settlement ", notable.CurrentSettlement.Name, "(", notable.CurrentSettlement.MapFaction.StringId, "|", notable.CurrentSettlement.Culture.StringId, ")."));
                                        notable.VolunteerTypes[i] = ATCConfig.GetBasicRecruit(factionID, cultureObject);
                                        troopAdded = true;
                                    }
                                    else
                                    {
                                        float num3 = num2 * num2 / (Math.Max(50f, (float)notable.Power) * Math.Max(50f, (float)notable.Power));
                                        int level = notable.VolunteerTypes[i].Level;
                                        if (MBRandom.RandomInt((int)Math.Max(2.0, (double)((float)level * num3) * num * 1.5)) == 0 && notable.VolunteerTypes[i].UpgradeTargets != null && notable.VolunteerTypes[i].Level < 20)
                                        {
                                            if (notable.VolunteerTypes[i] == basicTroop && HeroHelper.HeroShouldGiveEliteTroop(notable))
                                            {
                                                notable.VolunteerTypes[i] = ATCConfig.GetEliteRecruit(factionID, cultureObject);
                                                if (ATCSettings.Instance.DebugRecruitSpawn)
                                                    SubModule.log.Add(String.Concat("DEBUG: UpdateVolunteersOfNotables upgrading basic recruit in settlement ", notable.CurrentSettlement.Name, "(", notable.CurrentSettlement.MapFaction.StringId, "|", notable.CurrentSettlement.Culture.StringId, ") to elite troop: \"", notable.VolunteerTypes[i].StringId, "\"."));
                                                troopAdded = true;
                                            }
                                            else
                                            {
                                                notable.VolunteerTypes[i] = notable.VolunteerTypes[i].UpgradeTargets[MBRandom.RandomInt(notable.VolunteerTypes[i].UpgradeTargets.Length)];
                                                troopAdded = true;
                                            }
                                        }
                                    }
                                }
                            }

                            if (troopAdded)
                            {
                                SortVolunteers(notable.VolunteerTypes);
                            }
                                

                        }
                    }
                }

            }

            return false;  //skips original method
        }



        private static bool IsPlayerClanConfigured()
        {
            return ATCConfig.GetFaction("player_clan") != null;
        }


        private static bool IsPlayerKingdomConfigured()
        {
            return ATCConfig.GetFaction("player_faction") != null;
        }


        private static void SortVolunteers(CharacterObject[] volunteers)
        {
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < 6; k++)
                {
                    if (volunteers[k] != null)
                    {
                        int l = k + 1;
                        while (l < 6)
                        {
                            if (volunteers[l] != null)
                            {
                                if ((float)volunteers[k].Level + (volunteers[k].IsMounted ? 0.5f : 0f) > (float)volunteers[l].Level + (volunteers[l].IsMounted ? 0.5f : 0f))
                                {
                                    CharacterObject characterObject = volunteers[k];
                                    volunteers[k] = volunteers[l];
                                    volunteers[l] = characterObject;
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
            string _recruitType;

            //SubModule.log.Add(side1Party.Name + "(" + side1Party.MapFaction.Name + ") recruits " + subject.Name + " from " + individual.CurrentSettlement.Name + "(Faction: " + individual.CurrentSettlement.MapFaction.Name + ", Culture: " + individual.CurrentSettlement.Culture.Name + ")");

            if (!SubModule.disableATC)
            {

                string factionID;

                if (individual.CurrentSettlement == null)  //don't know if this can happen, but it was checked in the oringial code so better to be safe than sorry
                    return;
                
                if (individual.CurrentSettlement.OwnerClan == Hero.MainHero.Clan)
                {
                    factionID = "player_clan";
                }
                else if (individual.CurrentSettlement.MapFaction == Hero.MainHero.MapFaction)
                {
                    factionID = "player_faction";
                }
                else
                {
                    factionID = individual.CurrentSettlement.MapFaction.StringId;
                }

                if (side1Party.Leader.HeroObject.Clan == Hero.MainHero.Clan)
                {
                    _recruitType = "player_clan";
                }
                else
                {
                    _recruitType = "default"; //for all other AI Lords
                }
                

                if (HeroHelper.HeroShouldGiveEliteTroop(individual))
                { 
                    if (ATCSettings.Instance.DebugAIRecruiting)
                    {
                        SubModule.log.Add(String.Concat("DEBUG: GetRecruitVolunteerFromIndividual called for ", side1Party.Leader.HeroObject.Name, "(", factionID, ")", " trying to recruit ", subject.StringId, " in ", individual.CurrentSettlement.Name, "(", individual.CurrentSettlement.Culture.StringId, ")."));
                    }
                    individual.VolunteerTypes[bitCode] = subject = ATCConfig.GetEliteRecruit(factionID, individual.CurrentSettlement.Culture, _recruitType);
                }
                else
                {
                    if (ATCSettings.Instance.DebugAIRecruiting)
                    {
                        SubModule.log.Add(String.Concat("DEBUG: GetRecruitVolunteerFromIndividual called for ", side1Party.Leader.HeroObject.Name, " (", factionID, ")", " trying to recruit ", subject.StringId, " in ", individual.CurrentSettlement.Name, "(", individual.CurrentSettlement.Culture.StringId, ")."));
                    }
                    individual.VolunteerTypes[bitCode] = subject = ATCConfig.GetBasicRecruit(factionID, individual.CurrentSettlement.Culture, _recruitType);
                }


                //if (ATCSettings.Instance.ClanCanRecruit && side1Party.Leader.HeroObject.Clan == Hero.MainHero.Clan)
                //{
                //    sameClan = true;
                //}

                //string _subjectRootID = null;
                //CharacterObject _settlementBaseTroop = null;
                //CharacterObject _settlementEliteTroop = null;
                ////get BaseTroop id
                //try
                //{
                //    _subjectRootID = Helpers.CharacterHelper.FindUpgradeRootOf(subject).StringId;
                //    _settlementBaseTroop = individual.CurrentSettlement.Culture.BasicTroop;
                //    _settlementEliteTroop = individual.CurrentSettlement.Culture.EliteBasicTroop;
                //}
                //catch
                //{
                //    //InformationManager.DisplayMessage(new InformationMessage("Initialization failed for GetRecruit Prefix", new Color(1, 0, 0)));
                //    SubModule.log.Add("ERROR: GetRecruitVolunteerFromIndividual -> Initialization failed!");
                //    return;
                //}


                //bool isEliteTroop = false;
                //if (_settlementEliteTroop.StringId == _subjectRootID)
                //    isEliteTroop = true;
                //bool _getout = false;

                //CharacterObject replacementTroop = null;

                //foreach (TroopConfig tc in ATCconfig.troopConfig.Where(tc => (tc.SourceID == _settlementBaseTroop.StringId || tc.SourceID == _settlementEliteTroop.StringId)))
                //{
                //    //foreach (TargetTroop tt in tc.targetTroops.Where(tt => tt.PlayerOnly == true))                
                //    foreach (VolunteerType tt in tc.TargetTroops.Where(tt => tt.TroopID == _subjectRootID))
                //    {
                //        //Only processe for troops with playeronly flag = TRUE
                //        if (tt.PlayerOnly)
                //        {

                //            if ((tt.KingdomOnly && (side1Party.MapFaction == Hero.MainHero.MapFaction)) || sameClan)
                //                break; //allow recruitment

                //            if (isEliteTroop)
                //                replacementTroop = _settlementEliteTroop;
                //            else
                //                replacementTroop = _settlementBaseTroop;

                //            if (replacementTroop == null)
                //                SubModule.log.Add("ERROR: GetRecruitVolunteerFromIndividual -> " + tt.TroopID + " invalid!");
                //            //InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!", new Color(1, 0, 0)));

                //            _getout = true;
                //            break;
                //        }

                //        //Only processe for troops with cultureonly flag = FALSE
                //        else if (tt.CultureOnly)
                //        {
                //            //SubModule.caller = "self"; - OBSOLETE
                //            if (side1Party.Leader.Culture.BasicTroop.StringId == tc.SourceID)   //basic_troop = <source_troop> bedeutet gleiche Kultur
                //            {
                //                //do nothing, the party is allowed to recruit the soldier due to matching culture
                //                if (ATCSettings.Instance.DebugPlayerOnlyFlag)
                //                    SubModule.log.Add("GetRecruitVolunteerFromIndividual -> " + side1Party.Leader.Name + "(" + side1Party.Leader.Culture.Name + ")" + " recruited a " + subject.Name);
                //                //InformationManager.DisplayMessage(new InformationMessage(side1Party.Leader.Name + " (" + side1Party.Leader.Culture.Name + ")" + " recruited a " + subject.Name, new Color(0, 1, 0)));
                //                replacementTroop = null;
                //                _getout = true;
                //                break;
                //            }
                //            else
                //            {
                //                if (isEliteTroop)
                //                    replacementTroop = _settlementEliteTroop;
                //                else
                //                    replacementTroop = _settlementBaseTroop;

                //                if (replacementTroop == null)
                //                    SubModule.log.Add("ERROR: GetRecruitVolunteerFromIndividual -> " + tt.TroopID + " invalid!");
                //                //InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));
                //            }
                //        }

                //        //Only processe for troops with kingdomonly flag = FALSE
                //        else if (tt.KingdomOnly)
                //        {
                //            //SubModule.caller = "self"; - OBSOLETE
                //            if (side1Party.MapFaction.BasicTroop.StringId == tc.SourceID)   //basic_troop = <source_troop> bedeutet gleiche Kultur
                //            {
                //                //do nothing, the party is allowed to recruit the soldier due to matching culture
                //                if (ATCSettings.Instance.DebugPlayerOnlyFlag)
                //                    SubModule.log.Add("GetRecruitVolunteerFromIndividual -> " + side1Party.Leader.Name + " (" + side1Party.MapFaction.Name + ")" + " recruited a " + subject.Name);
                //                //InformationManager.DisplayMessage(new InformationMessage(side1Party.Leader.Name + " (" + side1Party.MapFaction.Name + ")" + " recruited a " + subject.Name, new Color(0, 1, 0)));
                //                replacementTroop = null;
                //                _getout = true;
                //                break;
                //            }
                //            else
                //            {
                //                if (isEliteTroop)
                //                    replacementTroop = _settlementEliteTroop;
                //                else
                //                    replacementTroop = _settlementBaseTroop;

                //                if (replacementTroop == null)
                //                    SubModule.log.Add("ERROR: GetRecruitVolunteerFromIndividual -> " + tt.TroopID + " invalid!");
                //                //InformationManager.DisplayMessage(new InformationMessage(tt.TroopID + " invalid!"));

                //            }
                //        }

                //    }

                //    if (_getout)
                //        break;
                //}

                //if (replacementTroop != null)
                //{
                //    if (ATCSettings.Instance.DebugPlayerOnlyFlag)
                //        SubModule.log.Add("GetRecruitVolunteerFromIndividual -> A " + subject.Name + " refused to join " + side1Party.Leader.Culture.Name);
                //    //InformationManager.DisplayMessage(new InformationMessage("A " + subject.Name + " refused to join " + side1Party.Leader.Culture.Name, new Color(1, 1, 0)));

                //    individual.VolunteerTypes[bitCode] = subject = replacementTroop;
                //}


                //_settlementBaseTroop = null;
                //_settlementEliteTroop = null;
            }
        }
    }


    //[HarmonyPatch(typeof(RecruitVolunteerVM), "OnRecruitMoveToCart")]
    //public class PatchOnRecruitMoveToCart
    //{
    //    static bool Prefix(ref RecruitVolunteerVM __instance, ref RecruitVolunteerTroopVM troop)
    //    {
    //        InformationManager.DisplayMessage(new InformationMessage(" refuses to join!"));
    //        __instance.ExecuteRemoveFromCart(troop);
    //        return false;
    //    }
    //}

    //[HarmonyPatch(typeof(RecruitVolunteerVM))]
    //[HarmonyPatch("RecruitVolunteerVM", MethodType.StaticConstructor)]
    //public class PatchPlayerRecruit
    //{
    //    static void Prefix(Hero owner, List<CharacterObject> troops, Action<RecruitVolunteerVM, RecruitVolunteerTroopVM> onRecruit, Action<RecruitVolunteerVM, RecruitVolunteerTroopVM> onRemoveFromCart)
    //    {
    //        foreach (CharacterObject troop in troops)
    //        {
    //            InformationManager.DisplayMessage(new InformationMessage(troop.StringId));
    //        }

    //    }
    //}


}

