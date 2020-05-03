using System;
using TaleWorlds.CampaignSystem;
using Helpers;
using AdonnaysTroopChanger.XMLReader;
using TaleWorlds.Core;


namespace AdonnaysTroopChanger.Bahaviors
{
    public class AdRecruitingBehavior : CampaignBehaviorBase
    {
        //private CharacterObject _selectedTroop;
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(this.DailyTick));
        }

        public void DailyTick()
        {
            this.UpdateVolunteersOfNotables();
            this.GiveDailyXpToNpcLords();
        }

        public override void SyncData(IDataStore dataStore)
        {
            //dataStore.SyncData<CharacterObject>("_selectedTroop", ref this._selectedTroop);
        }

        public void UpdateVolunteersOfNotables()
        {

            foreach (Settlement settlement in Campaign.Current.Settlements)
            {
                
                string factionID;
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
        }



        private bool IsPlayerClanConfigured()
        {
            return ATCConfig.GetFaction("player_clan") != null;
        }
        
        private bool IsPlayerKingdomConfigured()
        {
            return ATCConfig.GetFaction("player_faction") != null;
        }
        
        private void SortVolunteers(CharacterObject[] volunteers)
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



        private void GiveDailyXpToNpcLords()
        {
            //foreach (MobileParty mobileParty in MobileParty.All)
            //{
            //    if (mobileParty.IsLordParty && (mobileParty.Army == null || mobileParty.Army.LeaderParty != MobileParty.MainParty) && mobileParty.MapEvent == null && (mobileParty.Party.Owner == null || mobileParty.Party.Owner.Clan != Clan.PlayerClan))
            //    {
            //        for (int i = 0; i < mobileParty.MemberRoster.Count; i++)
            //        {
            //            TroopRosterElement troopRosterElement = mobileParty.MemberRoster.data[i];
            //            if (!troopRosterElement.Character.IsHero)
            //            {
            //                int xp = troopRosterElement.Xp;
            //                mobileParty.MemberRoster.SetElementXp(i, xp + troopRosterElement.Number * (5 + troopRosterElement.Character.Level));
            //            }
            //        }
            //    }
            //}
        }


    }
}
