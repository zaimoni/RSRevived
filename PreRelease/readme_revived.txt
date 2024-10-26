binaryonly ZIPs are meant to overwrite a stable release with compatible CSV and sound files.
As of 0.9.1, the minimum required .NET framework is 4.6.x

REVIVED 0.10.0 CHANGES
------------------
* New command Transfer Item, default key CTRL-T.  This allows transferring items between 
  any two ground inventories "in reach"
* minimum required .NET is now 8.0.
* recruiting followers is now easier.  See manual for details.
* RAPID fire mode is now more like a snap shot, or fanning, than a burst.  No more wasted shots.
* The city planners have improved zoning regulations for districts.
* Z-detectors can clear threat tracking caused by Z.
* New command Countermand PC, default key ALT-SHIFT-O.  Use this to inspect which self-orders are active, and cancel them.
* Some of the direction-choice commands now automatically act, if there is only a single relevant target.
* The SWAT team is now somewhat lazier, but more consistently equipped.  Odds are much better
  that there is a complete police uniform in the southernmost supply room of the police offices.
* Player waypoints are working; see manual for details.
* Item memory has been completely revamped, and it is possible to set a waypoint for a specific item memory location.
* Explicitly repeating a command 2-9 times is possible for some commands.  See manual for details.
* The main O)rdering followers UI has been reformatted; the top level is now on a right-aligned popup menu.
** As long as the follower is in communication, this order will work.  The historical mutual line of sight and mutual cell phones are special cases of this.
** Drop All items, is gone.
** "What's Up" reports on new-style Objectives, not old-style Orders.  Prior entries are old-style (and may be UI-glitchy)  They may be countermanded from here.
*** all PC self-orders are new-style objectives.
** Ordering followers to go somewhere, is done from the Far Look command.  (This sets an objective).  The follower will *not* stay there once arrived.
** Ordering followers to go pick up an item, is done from the Item Memory listings.  (This sets an objective.)
* There is a new stance, crouching.  Manual control of crouching, will require gameplay useful/fun enough to justify the extra keystrokes.
** Crouching allows using ground inventories as if they were on shop shelves.

2020-11-09 and earlier
* Cf. main manual's commands section for how self-orders work.
* Friends are now enemy detectors.
** Handwavium: you're reading off where they want to shoot or melee, from their body language.  You must be able to see them; detection isn't enough.
** Provisional implementation: zombies get the zombie detector icon; everyone else gets the blackops icon.
** this is very gamey currently:
*** the detectee only has to be targetable with the best ranged weapon.  It'll trigger even if that isn't in use.  You cannot read off an enemy at range five,
    from someone who only has a shotgun (typically range 3).
*** this does work for melee weapons (with targeting range 1, except for Father Time's Scythe which is range 2)
* Containers now have real inventory: pushing that store shelf takes the item on it, with it.
* new text label SIMULATING when it is not your turn.  Not 100% reliable; if things look weird try the R)un command to force a screen redraw.
** same screen space also has the SAVING label when saving a game.  This replaces wiping the displayed messages.
** same screen space will also tell you if far-walking will/has aborted: IN COMBAT, ENEMIES NEAR, WANTED ITEMS, or WANTED TRADES text.
*** These correspond to living AI objective/goal interrupt conditions.  Their accuracy for the player ranges from dubious to "not really".
* Vomit is no longer eternal.
* Safety from traps re-implementation, replacing RS Alpha 10's implementation, has landed
** It is now possible to learn how to be safe from a given trap, from overhearing explanations over the police radio.
** Avoiding a given trap, can also teach you how to be safe from it.
* Counted walk command now available; 1) through 9) steps.
* police radio range has been respecified as the minimap radius.  Reduction from being underground is not in (yet).
* Many RS Alpha 10 changes merged in
** This does include the park shed and residential outdoor rooms changes, and corresponding map objects.
** The safety from traps implementation is in, but due for a re-implementation (blocked by police radio range re-implementation)
** Revised BaseAI::SafetyFrom is not in yet (do want this)
** Revised trading UI is not in yet (do want this for player-NPC trading but not player-player trading)
** Item handling changes scheduled to be cherry-picked (full merge not practical)
** tracker clock not in yet (this affects the same trackers as a vaporware alarm feature)
** Hospital storage room nurse not in yet (undecided, interacts with vaporware hospital power-up storyline)
** disarming mechanic not in yet (like general idea, not comfortable with current implementation)
** Invulnerability as debug feature, and guarantee of PC sighting of NPCs will not be in.
* names better matched to stereotypical English gender
* key configuration files are not backward compatible.
* even more AI overhaul.
** Gangsters are now hostile to biker armor, as advertised (note: may not be staying as this contradicts RS Alpha 10 resolution to same issue)
* Closing the door behind you is now a free action.
* Windows and glass doors are no longer perfect ablative armor
* The CHAR Guard Manual is very interesting (0% boredom rate), but dry reading (only restores as much sanity as a magazine per reading).
** It is also informative in other ways
* map generation radically altered.
** survivalist weapon caches more usable (ammo is matched to ranged weapons in cache)
** The SWAT team starting at the police station on Day 0 Turn 0 has pre-looted the offices.
** Complete subway network at district size 40+
* It is rarely possible to see where a living is planning to move next.  (If you are about to shove him, you will see this.)
* player-player trading is now possible.  A similar implementation is used for player-NPC trading (no more roulette, if the target doesn't
  want to give up anything you want, you will know immediately)
** RS Alpha 10 negotiation has not yet been merged in; it is wanted.
* new (stub) command: Faction Info (default Shift-Ctrl-I).  See the main manual for details.
* new (stub) command: Set Waypoint (default Alt-Shift-I).  Use this to far-look (move the viewpoint without actually moving).
  See the main manual for details.
** W)alk and R)un work from here.
* Items will survive better if the method of death isn't particularly destructive to them.  (A slightly weaker version 
  of the change in Still Alive).
** the historical resistance of food and ammo to destruction has been retained.  Armor will survive better
   if the method of death is non-violent.  Other items will survive better if they aren't exposed to zombification.
* The surface maps and sewer maps have lost their peace walls. For example, you can
** see across district boundaries (so does your minimap)
** throw grenades across district boundaries
** fire at targets across district boundaries, etc.
** Unfortunately, the AI is just as unimpaired as you.

REVIVED 0.9.9.5 CHANGES
* NPC speech restored.
* bump to chat crash bug fixed
* some redraws added -- should have much more immediate feedback that you have moved/rested/recharged.
  There are most likely additional coverage holes (that is, won't guarantee that all times lag is happening
  that your displayed energy level is 0 or less).

REVIVED 0.9.9.4 CHANGES
------------------
* off-by-one offset error in displaying item counts fixed (e.g., 10 looked like 1)
* getting partial item stacks from containers should work reliably now.
* In spite of critically misleading profiling, it does appear that the speed issues in 0.9.9 onwards were from
  stacking enhanced pathfinding on top of disallowing districts to wildly diverge in game time.  A mitigation has
  gone in, but anyone relying on letting other districts get 20+ turns behind will have to play an earlier version.

REVIVED 0.9.9.3 CHANGES
------------------
* hard crash in release mode fixed

REVIVED 0.9.9.2 CHANGES
------------------
* Self-defense when killing a cop, or a follower of a cop, will not prevent murder charges.


REVIVED 0.9.9.1 CHANGES
------------------
* CivilianAI/GangAI no longer hoard canned food
* hard crash in release-mode melee behavior fixed
* going to bed had pathing difficulties.

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
