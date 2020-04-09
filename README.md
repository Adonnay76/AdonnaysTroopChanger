ATC or Adonnays Troop Changer lets you add (not replace) multiple custom troop trees to a culture. You can determine 
how many recruits will be changed to one of your confiured custom troops and whether the AI lords are allowed to
recruit them or not.


Version 1.0.8
--------------------
Changelog:
- New flag "cultureonly" added to the ATC.config.xml - This allows to restrict custom troops to the culture of the replaced base recruit
- Modified debug messages to show names instead of technical IDs

Version 1.0.7b
--------------------
Changelog:
- Replacement method corrected to hopefully work more consistent with more than <target_troops> configured 
- Refined the debug messaging
- Debug messages can now be disabled in the ATC.config.xml

IF YOU HAVE AN OLDER ATC.config.xml (version 1.0.7). Please download the new config sample and either overwrite the existing one if you have not altered it or modify your custom ATC.config.xml according to the new structure.

The new ATC.config.xml structure contains the <debugInfo> node with the 3 possible fields shown below. The rest of the config file is unchanged.
<ATCTroops>
	<debugInfo>
		<troop_replacement>true</troop_replacement>
		<show_percentage>false</show_percentage>
		<playeronly_flag>false</playeronly_flag>
	</debugInfo>
	<source_troop id="sturgian_recruit">
		<target_troop id ="chael_nadra_recruit_f" percent="2" playeronly="true"/>  
		<target_troop id ="chael_nadra_recruit_m" percent="2" playeronly="true"/>  
	</source_troop>
	<source_troop id="battanian_volunteer">
		<target_troop id="chael_nadra_recruit_f" percent="2" playeronly="true"/>
		<target_troop id="chael_nadra_recruit_m" percent="2" playeronly="true"/>
	</source_troop>
	<source_troop id="vlandian_recruit">
		<target_troop id="chael_nadra_recruit_f" percent="2" playeronly="true"/>  
		<target_troop id="chael_nadra_recruit_m" percent="2" playeronly="true"/>  
	</source_troop>	
</ATCTroops>





Version 1.0.7
--------------------
Changelog:
- Initial Release
