using MBOptionScreen.Settings;
using MBOptionScreen.Attributes;
using MBOptionScreen.Attributes.v2;

namespace AdonnaysTroopChanger
{
    public class ATCSettings : AttributeSettings<ATCSettings>
    {

        //public const string InstanceID = "AdonnaysTroopChanger";
        public override string ModName => ("Adonnay\'s Troop Changer " + SubModule.version);
        public override string ModuleFolderName => SubModule.ModuleFolderName;
        public override string Id { get; set; } = "Adonnay.AdonnaysTroopChanger_v1";




        [SettingPropertyBool(displayName: "Enable Mod Scan Functionality", Order = 0, RequireRestart = false, HintText = "Enables ATC to scan for files following the required naming pattern *ATC.modconfig.xml.")]
        [SettingPropertyGroup("Basic ATC settings")]
        public bool EnableModScan { get; set; } = false;

        [SettingPropertyBool(displayName: "Debuginfo: Recruit Spawning", Order = 1,  RequireRestart = false, HintText = "Logs detailed information in the ATC.debug.log about the spawning of recruits (settlement/culture/faction/recruit).")]
        [SettingPropertyGroup("Basic ATC settings")]
        public bool DebugRecruitSpawn { get; set; } = false;

        [SettingPropertyBool(displayName: "Debuginfo: Configuration", Order = 2, RequireRestart = false, HintText = "Logs detailed information in the ATC.debug.log related to the configuration and how it is read.")]
        [SettingPropertyGroup("Basic ATC settings")]
        public bool DebugConfigRead { get; set; } = false;

        [SettingPropertyBool(displayName: "Debuginfo: AI Recruiting", Order = 3, RequireRestart = false, HintText = "Logs detailed information in the ATC.debug.log about AI Lords aquiring new troops from settlements.")]
        [SettingPropertyGroup("Basic ATC settings")]
        public bool DebugAIRecruiting { get; set; } = false;



        [SettingPropertyBool(displayName: "Enable Faction Troops in Conquered Settlements", RequireRestart = false, HintText = "Allows to spawn faction troops (and their replacements) in conquered settlements.")]
        [SettingPropertyGroup("Faction Troops in Conquered Settlements", 1, true)]
        public bool EnableCCC { get; set; } = false;

  
        [SettingPropertyInteger(displayName: "Amount of Faction Troops", minValue: 1, maxValue: 100, RequireRestart = false, HintText = "Maximum amount of faction troops spawned in conquered settlements (in % - calculated with 100% Loyalty).")]
        [SettingPropertyGroup("Faction Troops in Conquered Settlements")]
        public int CCCAmount { get; set; } = 50;

        [SettingPropertyBool(displayName: "Clanmates Can Recruit Custom Troops", RequireRestart = false, HintText = "Allow clan mates (companions) to recruit everything the player can. This overrules the playeronly configuration!")]
        [SettingPropertyGroup("Custom Unit Recruiting Options")]
        public bool ClanCanRecruit { get; set; } = true;

        [SettingPropertyFloatingInteger(displayName: "TEST: Change Volunteer Spawning", minValue: 0.1f, maxValue: 2f, RequireRestart = false, HintText = "Reduce or increase the chance to spawn new recruits, applies to every settlement. Default (vanilla) spawn rate = 1.0!")]
        [SettingPropertyGroup("Custom Unit Recruiting Options")]
        public float RecruitSpawnFactor { get; set; } = 1.0f;
    }
}
