binaryonly ZIPs are meant to overwrite a stable release with compatible CSV and sound files.
As of 0.9.1, the minimum required .NET framework is 4.6.x

REVIVED 0.9.9 CHANGES
------------------
* keypress buffer of size one implemented.  Only the last keypress survives.
* new command line option --socrates-daimon.  Enables cheat commands; cf the RS Revived Manual.
* new command line option --PC.  Cf. the RS Revived manual for details.
* Savefile format has been broken
* Vintage games no longer overwrite options that are forced (the forcing is handled elsewhere)
* PC zombies are on the same skill upgrade options as NPCs.
* hours until needing to sleep is correctly reported (will not jump around at sunrise/sunset)
* Waiting now guarantees maximum realistic energy rating on the next turn.
* You may stop running even if too tired to run.

AI overhaul includes but is not limited to
* Followers should have far less difficulty finding their leader now.
* The police are learning what organized force means.
** They now can sweep the districts (cheating eidetic memory, but mostly not using the exact location of unsighted z).  
   This includes limited cross-map pathfinding, but it does account for non-vintage mode sewers being unclearable.
*** CHAR building codes have requirements on the police accessibility of residential basements.  You plausibly have 
    three to four game hours to get any survivalist grenades before the police do.
**** There were RNG side effects.  If you use a seed from 0.9.8
***** The overall map layout is the same.
***** All items should be where they were.  Random quantities are expected to be different.
***** A noticeable minority of livings will have different name/gender.
***** The type of z of the day-zero cold start are completely inconsistent.  Positions are consistent.
** If you have an active police radio, the chatter may be informative.
** Threat tracking and newly interesting locations to see have transparent overlays for police.  The minimap also reports on these, 
   with inconsistent color coding to the overlays.

REVIVED 0.9.8 CHANGES
------------------
* savefile corruption fix
* bluffing being a cop in single-PC mode fixed
* pointless retreating by firearms users mitigated
* CivilianAI and GangAI theoretical bugfix regarding left-hand items
** Triggered by NPC civilian having two or more of: cell phone, flashlight, stench killer.  As bikers/gangsters
   do not use stench killer, it cannot trigger the theoretical bug.

REVIVED 0.9.7 CHANGES
------------------
* keep the player count accurate during reincarnation
* counter-adjust the drop/pickup loop fixes of 0.9.3
* It is possible for civilians (and survivors, technically) to bluff 
  being cops well enough to become one for the duration of the apocalypse.
  The Prisoner Who Should Not Be currently will not divulge the location 
  of the CHAR base to a cop.  See the RS Revived manual for details.

REVIVED 0.9.6 CHANGES
------------------
* The simulation thread should not attempt reincarnation.

REVIVED 0.9.5 CHANGES
------------------
* deal with crash bug when doing an impossible reincarnation option
* enable the close button, and don't backtalk when Task Manager force-closes the game.
* Waiting for a keypress, once the simulation thread stops, should read very close to 0% CPU in Task Manager.

REVIVED 0.9.4 CHANGES
------------------
* failed item pickup travel loop suppressed
* crash bug system involving sound fixed (reincarnation proven trigger) 

REVIVED 0.9.3 CHANGES
------------------
* drop/pickup loop issues suppressed more
* Line of Sight handling of cardinal direction visibility blocking less restrictive

REVIVED 0.9.2 CHANGES
------------------
* Civilian AI item handling now "stable". Also, drop/pickup loop involving food and an inferior item fixed.
* The player may put items into containers.  AI...not yet.
* The player may choose which item to take from a stack in a container
* Being told the location of the CHAR underground base should be slightly safer
* Speculative attempt to prevent crash from AI picking up activated traps
* Require killing an actor, to remove him from the map.  Should mitigate/prevent re-killing by starvation.
* new command: Item Info, default command Shift-I to match City Info default command I.  Presentation is awful.
  The civilian AI is scheduled to use this information as well, but doesn't do so yet.

REVIVED 0.9.1 CHANGES
------------------
* hungry civilians do not push anything jumpable (this includes wrecked cars)
* living AI self-awareness of stamina costs closer to correct.
* energy stat now displayed to player (AI was using it)
* giving an item that doesn't fully fit in the destination, won't destroy it.
* equipping a flashlight, should immediately give you enhanced visibility 
  range at night
* Sound has been restored.
* AI item juggling is in the middle of an overhaul.
** Police radios are now an implicit item for cops.
* The midnight invasions actually are at midnight.
* The civilian followers of a cop should no longer be terminated by their 
  leader for killing their leader's enemies.
* crash reports should be informative, even when they are triggered in 
  districts without players.
* new command-line option --seed=... Cf. the RSRevived Manual for details.
* new command-line option --subway-cop Cf. the RSRevived Manual for details.
* Antiviral pills have been banished from Classic mode; infection in 
  the game mode is now required.

Savefiles are incompatible with Alpha 9 fork RC 6.  Please start a new game.

NOTE: the .NET version has been re-targeted from 3.5 to 4.6.1 for technically motivated reasons.
This shouldn't be a problem on Win7 or higher; download from Microsoft if it is a problem.

ALPHA 9 FORK RC 6 CHANGES
------------------

* (critical) Mitigate/prevent multi-threading crashes on district change
* make trading with the player work better
* Text correction for expired/spoiled food (icon and nutrition were correct)
* Don't like your skill choices?  Get wiser ones.

ALPHA 9 FORK RC 5 CHANGES
------------------

Recharging flashlights and trackers actually works as advertised for RC 1 (maximum 8 turns)

ALPHA 9 FORK RC 4 CHANGES
------------------

Trading with player doesn't crash (instance of systematic error in precondition verification [RC 3], then
some other issues)


ALPHA 9 FORK RC 2 CHANGES
------------------

Sleeping now doesn't crash (Line of Sight specification error)


ALPHA 9 FORK RC 1 CHANGES
------------------

Recharging: nothing takes more than 8 turns to recharge.  If it has really long life (cell phones,
small flashlights) it's efficient.  Short life items are not penalized.

Line of fire/line of sight display stops at the blocked point, not the target.  This is a 
side effect of changing the line of fire/line of sight to intelligently swerve to avoid 
being blocked.

Giving items no longer merges against ground inventory.

The civilian and gang AIs have had some micro-optimizations regarding flashlight usage and related
situations.

Trading is in the middle of a rewrite.  Further changes require very invasive restructuring so
they haven't been attempted.

The alpha 9 savefile viewer should work as-is with this release of alpha fork.
