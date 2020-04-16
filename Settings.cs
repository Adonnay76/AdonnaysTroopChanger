using ModLib;
using ModLib.Attributes;
using System.Xml.Serialization;

namespace AdonnaysTroopChanger
{
    public class Settings : SettingsBase
    {

        public const string InstanceID = "AdonnaysTroopChanger";
        public override string ModName => "Adonnay\'s Troop Changer";
        public override string ModuleFolderName => SubModule.ModuleFolderName;

        [XmlElement]
        public override string ID { get; set; } = InstanceID;

        public static Settings Instance
        {
            get
            {
                return (Settings)SettingsDatabase.GetSettings(InstanceID);
            }
        }



        #region Basic Settings
        [XmlElement]
        [SettingProperty("Enable Mod Scan Functionality", "Enables ATC to scan for files following the required naming pattern *ATC.modconfig.xml.")]
        [SettingPropertyGroup("Basic ATC settings")]
        public bool EnableModScan { get; set; } = false;
        
        [XmlElement]
        [SettingProperty("Debuginfo: Playeronly Flag", "Logs detailed information in the ATC.debug.log about the behavior of the playeronly flag.")]
        [SettingPropertyGroup("Basic ATC settings")]
        public bool DebugPlayerOnlyFlag { get; set; } = false;

        [XmlElement]
        [SettingProperty("Debuginfo: Show Replacements", "Logs detailed information in the ATC.debug.log which basic troop has been replaced with a custom troop.")]
        [SettingPropertyGroup("Basic ATC settings")]
        public bool DebugReplacementMsg { get; set; } = false;
        #endregion


        #region CCC
        [XmlElement]
        [SettingProperty("Enable Faction Troops in Conquered Settlements", "Allows to spawn faction troops (and their replacements) in conquered settlements.")]
        [SettingPropertyGroup("Faction Troops in Conquered Settlements", true)]
        public bool EnableCCC { get; set; } = true;

        [XmlElement]
        [SettingProperty("Max Amount of Faction Troops", 1, 100, "Maximum amount of faction troops spawned in conquered settlements (in % - calculated with 100% Loyalty).")]
        [SettingPropertyGroup("Faction Troops in Conquered Settlements")]
        public int CCCAmount { get; set; } = 50;
        #endregion
    }
}
