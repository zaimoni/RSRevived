ALPHA 9 CHANGES
---------------

Undead hunger and rotting.
Living sanity and insanity, cannibalism.
Medics can try to revive recently deceased people.

FIXED BUGS
1 (Rules) Cannot fire at or make an enemy of an indirect enemy.
2 (Item) Scent killer is not equipable anymore and the NPCs never use them.
3 (Options) Supplies drop option effect is reversed.
4 (Options) Some options are irrelevant to VTG mode but still enabled and used in computing difficulty.
5 (Wtf) Can drag more than one corpse but not really, causing bugs.
6 (Inventory) Partial stacks do not stack correctly in some situations : more is added than what is dropped.
7 (Murder) Cops can murder without consequences. Consequences will never be the same.


NEW GAMEPLAY : UNDEAD ROT
- Most undeads slowly rot away and have to eat flesh, similar to how livings have to eat food.
- Bite/eat people/corpses to recover points.
- When Hungry an undead may loose a skill each turn.
- When Starved an undead may loose 1 HP per turn.

NEW GAMEPLAY : SANITY
- Livings should try to keep their Sanity.
- Sanity slowly decays.
- There are 3 levels of Sanity :
	- Sane : nothing happens.
	- Disturbed : nightmares.
	- Insane : nightmares plus insanity sometimes taking control of the living actions and doing random things.
- Livings seeing or doing disturbing actions take a sanity hit.
	- butchering a corpse.
	- eating a corpse (living cannibalism is worse).
	- eating someone alive.
	- zombification and corpse rising.
	- death of follower/leader with whom the living had a bond (see below)
- Recover sanity by:
	- forming and keeping a bond with followers/leaders (see below)
	- using entertainment items (see blow).
	- taking some meds (yellow pills).
	- killing undeads.

NEW GAMEPLAY : CANNIBALISM
- Livings can eat corpses when starving or insane.
- Eating corpse recovers a bit of food points but transmit infection and is disturbing for sanity.
- Nutrition value is low, Light Eater helps a bit.

NEW GAMEPLAY : REVIVE
- Can try to revive corpses.
- Need : medic skill, a medikit, the corpse must be fresh.
- Chances depends on the skill level and the state of the corpse.
- A revive attempt consumes the medikit even if failed.
- Big boost in trust if not enemies.

NEW ITEMS : ENTERTAINMENT
- Using recovers sanity.
- Have a chance to become "boring" after each use.
- A "boring" item has no value anymore to the character who found it boring (but may still have value for other people).
- Books : good entertainment value.
- Magazines : stackable but low value and discarded once read.

FOLLOWERS
- Bond:
	- when max trust is reached, a "bond" is formed between the leader and the follower.
	- a bond helps both livings recovering sanity, it is reassuring to know there is someone you can count on in this insane world.
	- however, when the "bonded one" dies, the living takes a sanity hit.
- Followers remember the trust they had in their previous leader(s). Eg: leave a follower behind and he will remember the trust he had in you if you lead him again.

GAMEPLAY
- Trees are now breakable and give wood.
- Wood from broken objects is not limited to one full stack of planks. Eg: trees = 10 planks (4,4,2).
- Trap: small actors (rats) have 90% chance to avoid triggering a trap.
- Murder: killing an agressor of a leader/mate count as self-defence and is not a murder.
- Indirect enemies: extended to include relations with respective leaders.

SKILLS
- New Z-Skill : Z-Light Eater, similar to living Light-Eater.
- Modified skill zombification : Light Eater becomes Z-Light Eater; Z-Eater has to be learned as zombie.
- New Skill : Strong Psyche - stay sane longer and recover more san.

AI
- Followers: When fleeing/retreating, try to avoid stepping into their leader line of fire.
- Improved speed advantage evaluation : intelligent NPCs are much better at controlling range when they have a ranged weapon or are fighting a slower enemy (less stupid deaths).
- Starved or Insane hungry livings will eat corpses.
- Some livings use entertainment items.
- Intelligent NPCs will avoid stepping into traps that could kill instantly them unless starving or courageous/wreckless (gangs).
- Civilians will temporarily loose interest in an item stack they can't reach (eg:blocked by deadly traps).
- Not afraid anymore of stepping on empty cans ^^
- Not interested in ranged weapons if already have one.
- Civilians will trade with each other, but they won't annoy the player and remember with who they traded recently as to not spam trade offers.
- Civilians with medic skill will revive others.

MODDING
- Meds can recover San.
- Items_Entertainment.csv.

Have fun!
And remember, die with a smile!
