using MBOptionScreen.Settings;
using MBOptionScreen.Attributes;

namespace AdonnaysTroopChanger
{
    public class ATCSettings : AttributeSettings<ATCSettings>
    {

        //public const string InstanceID = "AdonnaysTroopChanger";
        public override string ModName => ("Adonnay\'s Troop Changer " + SubModule.version);
        public override string ModuleFolderName => SubModule.ModuleFolderName;
        public override string Id { get; set; } = "Adonnay.AdonnaysTroopChanger_v1";




        [SettingProperty("Enable Mod Scan Functionality", false, "Enables ATC to scan for files following the required naming pattern *ATC.modconfig.xml.")]
        [SettingPropertyGroup("Basic ATC settings")]
        public bool EnableModScan { get; set; } = false;
        
        [SettingProperty("Debuginfo: Playeronly Flag", false, "Logs detailed information in the ATC.debug.log about the behavior of the playeronly flag.")]
        [SettingPropertyGroup("Basic ATC settings")]
        public bool DebugPlayerOnlyFlag { get; set; } = false;

        [SettingProperty("Debuginfo: Show Replacements", false, "Logs detailed information in the ATC.debug.log which basic troop has been replaced with a custom troop.")]
        [SettingPropertyGroup("Basic ATC settings")]
        public bool DebugReplacementMsg { get; set; } = false;
    


        [SettingProperty("Enable Faction Troops in Conquered Settlements", false, "Allows to spawn faction troops (and their replacements) in conquered settlements.")]
        [SettingPropertyGroup("Faction Troops in Conquered Settlements", true)]
        public bool EnableCCC { get; set; } = true;

  
        [SettingProperty("Max Amount of Faction Troops", 1, 100, false, "Maximum amount of faction troops spawned in conquered settlements (in % - calculated with 100% Loyalty).")]
        [SettingPropertyGroup("Faction Troops in Conquered Settlements")]
        public int CCCAmount { get; set; } = 50;

        [SettingProperty("Clanmates Can Recruit Custom Troops", 1, 100, false, "Allow clan mates (companions) to recruit <target_troops>. This overrules all other options like playeronly or kingdomonly!")]
        [SettingPropertyGroup("Custom Unit Recruiting Options")]
        public bool ClanCanRecruit { get; set; } = true;

        [SettingProperty("TEST: Reduce Volunteer Spawning", 0.1f, 1f, false, "EXPERIMENTAL! Reduces the chance to spawn new recruits. That might help against waves and waves of enemies and make victories finally mean something!")]
        [SettingPropertyGroup("Custom Unit Recruiting Options")]
        public float RecruitSpawnFactor { get; set; } = 0.5f;
    }
}
