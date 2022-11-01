using System;
using System.Collections.Generic;
using HarmonyLib;
using AdonnaysTroopChanger.XMLReader;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using AdonnaysTroopChanger.Settings;

namespace AdonnaysTroopChanger
{
    public static class Factions
    {
        public static List<String> factionList = new List<string>();
    }

         
[HarmonyPatch(typeof(RecruitmentCampaignBehavior), "GetRecruitVolunteerFromIndividual")]
    public class PatchRecruitActionFromIndividual
    {
        static void Prefix(ref MobileParty side1Party, ref CharacterObject subject, ref Hero individual, int bitCode)
        {

            

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


                CharacterObject newRecruit;

                newRecruit = ATCConfig.GetFactionRecruit(individual, side1Party);
                
                int tier = newRecruit.Tier;
                for (int i = tier; i < subject.Tier; i++)
                {
                    if (newRecruit.UpgradeTargets == null || newRecruit.UpgradeTargets.Length == 0)
                        break;
                    
                    newRecruit = newRecruit.UpgradeTargets[MBRandom.RandomInt(0, newRecruit.UpgradeTargets.Length)];
                }
                individual.VolunteerTypes[bitCode] = subject = newRecruit;

                if (ATCSettings.DebugAIRecruiting)
                {
                    SubModule.log.Add(String.Concat("DEBUG: GetRecruitVolunteerFromIndividual called for ", side1Party.Party.Owner?.Name, "(", side1Party.MapFaction.StringId, ")", " in ", individual.CurrentSettlement.Name, "(", individual.CurrentSettlement.Culture.StringId, "). Received: ", newRecruit.StringId));
                }
            }
        }
    }



    [HarmonyPatch(typeof(MobileParty), "FillPartyStacks")]
    public class PatchFillPartyStacks
    {

        static bool Prefix(MobileParty __instance, ref PartyTemplateObject pt)
        {

            if (ATCSettings.EnableCustomTemplates)
            {
                if (__instance?.LeaderHero != null)
                    pt = ModifyPartyTemplate(__instance);

            }

            return true;

        }


        static PartyTemplateObject ModifyPartyTemplate(MobileParty party)
        {
            PartyTemplateObject ct = (PartyTemplateObject)Campaign.Current.ObjectManager.GetObject("PartyTemplate", "atc_party_template_dummy");

            int _stacksCount = party.LeaderHero.Clan.DefaultPartyTemplate.Stacks.Count;

            ct.Stacks.Clear();

            if (ATCSettings.DebugAIRecruiting || ATCSettings.DebugAIRecruiting)
                SubModule.log.Add(String.Concat("DEBUG: Building PartyTemplateStacks for ", party.LeaderHero.Name, "(", party.LeaderHero.MapFaction.StringId, "|", party.LeaderHero.Culture, ")."));

            for (int i = 0; i < _stacksCount; i++)
            {
                CharacterObject characterObject = (CharacterObject)ATCConfig.GetFactionRecruit(party.LeaderHero, party);
                int _rnd = MBRandom.RandomInt(7);
                for (int j = 0; j < _rnd; j++)
                {
                    if (characterObject.UpgradeTargets != null && characterObject.UpgradeTargets?.Length != 0)
                        characterObject = characterObject.UpgradeTargets[MBRandom.RandomInt(characterObject.UpgradeTargets.Length)];
                }
                
                if (ATCSettings.DebugAIRecruiting)
                    SubModule.log.Add(String.Concat("DEBUG: Troop added to spawning party: ", characterObject.StringId));

                ct.Stacks.Add(new PartyTemplateStack(characterObject, party.LeaderHero.Clan.DefaultPartyTemplate.Stacks[i].MinValue, party.LeaderHero.Clan.DefaultPartyTemplate.Stacks[i].MaxValue));
                //ct.Stacks.Remove(hero.Clan.DefaultPartyTemplate.Stacks[i]);
            }
            
            return ct;
        }

    }

}

