using System;
using HarmonyLib;
using AdonnaysTroopChanger.XMLReader;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.Core;
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
                                            if (HeroHelper.HeroShouldGiveEliteTroop(notable))
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
                if (individual.CurrentSettlement == null)  //don't know if this can happen, but it was checked in the oringial code so better to be safe than sorry
                    return;

                //Special Check for "Improved Garrisons" as their recruiting parties do not have Heros attached to them
                if (side1Party.Party.Owner == null)
                {
                    SubModule.log.Add("WARNING: GetRecruitVolunteerFromIndividual skipped because recruiting party does not have a Clan!");
                    return;
                }


                Clan _recruitingClan;
                TextObject _recruiterName;
                //if (side1Party.Leader.HeroObject != null)
                //{
                //    _recruitingClan = side1Party.Leader.HeroObject.Clan;
                //    _recruiterName = side1Party.Leader.HeroObject.Name;
                //}
                //else
                //{
                _recruitingClan = side1Party.Party.Owner.Clan;
                _recruiterName = side1Party.Party.Owner.Name;
                //}



                string _factionID;
                if (_recruitingClan == Hero.MainHero.Clan)
                {
                    _factionID = "player_clan";
                    _recruitType = "player_clan";  //for clanonly flag
                }
                else if (side1Party.MapFaction == Hero.MainHero.MapFaction)
                {
                    _factionID = "player_faction";
                    _recruitType = "default"; //for all other AI Lords
                }
                else
                {
                    _factionID = individual.CurrentSettlement.MapFaction.StringId;
                    _recruitType = "default"; //for all other AI Lords
                }

                

                if (HeroHelper.HeroShouldGiveEliteTroop(individual))
                { 
                    if (ATCSettings.Instance.DebugAIRecruiting)
                    {
                        SubModule.log.Add(String.Concat("DEBUG: GetRecruitVolunteerFromIndividual called for ", _recruiterName, "(", _factionID, ")", " trying to recruit ", subject.StringId, " in ", individual.CurrentSettlement.Name, "(", individual.CurrentSettlement.Culture.StringId, ")."));
                    }
                    individual.VolunteerTypes[bitCode] = subject = ATCConfig.GetEliteRecruit(_factionID, individual.CurrentSettlement.Culture, _recruitType);
                }
                else
                {
                    if (ATCSettings.Instance.DebugAIRecruiting)
                    {
                        SubModule.log.Add(String.Concat("DEBUG: GetRecruitVolunteerFromIndividual called for ", _recruiterName, " (", _factionID, ")", " trying to recruit ", subject.StringId, " in ", individual.CurrentSettlement.Name, "(", individual.CurrentSettlement.Culture.StringId, ")."));
                    }
                    individual.VolunteerTypes[bitCode] = subject = ATCConfig.GetBasicRecruit(_factionID, individual.CurrentSettlement.Culture, _recruitType);
                }
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

