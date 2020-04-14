ATC or Adonnays Troop Changer lets you add (not replace) multiple custom troop trees to a culture. You can determine 
how many recruits will be changed to one of your confiured custom troops and whether the AI lords are allowed to
recruit them or not.

Version 1.1.5 BETA
--------------------
- Added 


Version 1.1.4 BETA
--------------------
- accidentally still had the prefix for the EliteBasicTroop getter in there which caused some crashes
- Added a debug log file (ATC.debug.log) in the /Config subfolder so gather information if the <debug> flags are set. No more alert messages.
- Added faction recruits to conquered settlements based on their loyalty (capped at 50% max for now)

	
Version 1.1.3 BETA
--------------------
- moved from the two prefixes for BasicTroop (get) and EliteBasicTroop (get) to a central prefix of UpdateVolunteersofNotables which gives much more freedom on how to populate the notable's recruit rosters with fresh volunteers


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

The new ATC.config.xml structure contains the <debugInfo> node.


Version 1.0.7
--------------------
Changelog:
- Initial Release
