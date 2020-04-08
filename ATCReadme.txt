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

