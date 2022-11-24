using System;
using System.Collections.Generic;
using System.Linq;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D<short>;

#nullable enable

namespace djack.RogueSurvivor.Data
{
    internal interface IMap
    {
        public short Height { get; }
        public short Width { get; }
        public Point Origin { get; }
        public string MapName { get; }

        public bool HasExitAt(in Point pos);
        Actor? GetActorAt(Point pos);
        Inventory? GetItemsAt(Point pos);
        MapObject? GetMapObjectAt(Point pos);
        TileModel GetTileModelAt(Point pt);

        int TurnOrderFor(Actor a);

        Dictionary<Point, List<Corpse>>? FilterCorpses(Predicate<Corpse> ok);

        // cheat map similar to savefile viewer
        void DaimonMap(OutTextFile dest)
        {
            if (!Engine.Session.Get.CMDoptionExists("socrates-daimon")) return;
            dest.WriteLine(MapName + "<br>");
            // XXX since the lock at the district level causes deadlocking, we may be inconsistent for simulation districtions
            // we typically have one of actors or items here...full map has motivation
            var inv_data = new List<string>();
            string[] actor_headers = { "pos", "name", "Priority", "AP", "HP", "Inventory" };  // XXX would be function-static in C++
            List<string> actor_data = new List<string>();
            string[][] ascii_map = new string[Height][];

            void _process_inv(Inventory? inv, int x, int y) {
                if (null != inv && !inv.IsEmpty) {
                    string p_txt = '(' + x.ToString() + ',' + y.ToString() + ')';
                    foreach (Item it in inv.Items) {
                        inv_data.Add("<tr class='inv'><td>" + p_txt + "</td><td>" + it.ToString() + "</td></tr>");
                    }
                    ascii_map[y][x] = "&"; // Angband/Nethack pile.
                }
            }

            foreach (short y in Enumerable.Range(0, Height)) {
                ascii_map[y] = new string[Width];
                foreach (short x in Enumerable.Range(0, Width)) {
                    // XXX does not handle transparent walls or opaque non-walls
                    Point pt = new Point(x, y) + Origin;
                    var tile = GetTileModelAt(pt);
                    if (null == tile) {
                        ascii_map[y][x] = " ";
                        continue;
                    }
                    ascii_map[y][x] = (tile.IsWalkable ? "." : "#");    // typical floor tile if walkable, typical wall otherwise
                    if (HasExitAt(pt)) ascii_map[y][x] = ">";                  // downwards exit
#region map objects
                    const string tree_symbol = "&#x2663;"; // unicode: card suit club looks enough like a tree
                    const string car_symbol = "<span class='car'>&#x1F698;</span>";   // unicode: oncoming car
                    const string drawer_symbol = "&#x2584;";    // unicode: block elements
                    const string shop_shelf_symbol = "&#x25A1;";    // unicode: geometric shapes
                    const string large_fortification_symbol = "<span class='lfort'>&#x25A6;</span>";    // unicode: geometric shapes
                    const string power_symbol = "&#x2B4D;";    // unicode: misc symbols & arrows
                    const string closed_gate = "<span class='lfort'>&#x2630;</span>";    // unicode: misc symbols (I Ching heaven)
                    const string iron_fence = "<span class='lfort'>&#x2632;</span>";    // unicode: misc symbols (I Ching fire)
                    const string open_gate = "<span class='lfort'>&#x2637;</span>";    // unicode: misc symbols (I Ching earth)
                    const string chair = "<span class='chair'>&#x2441;</span>";    // unicode: OCR chair
                    var tmp_obj = GetMapObjectAt(pt);  // micro-optimization target (one Point temporary involved)
                    if (null != tmp_obj) {
                        if (tmp_obj.IsCouch) {
                            ascii_map[y][x] = "="; // XXX no good icon for bed...we have no rings so this is not-awful
                        } else if (MapObject.IDs.TREE == tmp_obj.ID) {
                            ascii_map[y][x] = tree_symbol;
                        } else if (MapObject.IDs.CAR1 == tmp_obj.ID) {
                            ascii_map[y][x] = car_symbol; // unicode: oncoming car
                        } else if (MapObject.IDs.CAR2 == tmp_obj.ID) {
                            ascii_map[y][x] = car_symbol; // unicode: oncoming car
                        } else if (MapObject.IDs.CAR3 == tmp_obj.ID) {
                            ascii_map[y][x] = car_symbol; // unicode: oncoming car
                        } else if (MapObject.IDs.CAR4 == tmp_obj.ID) {
                            ascii_map[y][x] = car_symbol; // unicode: oncoming car
                        } else if (MapObject.IDs.DRAWER == tmp_obj.ID) {
                            ascii_map[y][x] = drawer_symbol;
                        } else if (MapObject.IDs.SHOP_SHELF == tmp_obj.ID) {
                            ascii_map[y][x] = shop_shelf_symbol;
                        } else if (MapObject.IDs.LARGE_FORTIFICATION == tmp_obj.ID) {
                            ascii_map[y][x] = large_fortification_symbol;
                        } else if (MapObject.IDs.CHAR_POWER_GENERATOR == tmp_obj.ID) {
                            ascii_map[y][x] = ((tmp_obj as Engine.MapObjects.PowerGenerator).IsOn ? "<span class='power' style='background:green'>" : "<span class='power' style='background:red'>") + power_symbol + " </span>";
                        } else if (MapObject.IDs.IRON_GATE_CLOSED == tmp_obj.ID) {
                            ascii_map[y][x] = closed_gate;
                        } else if (MapObject.IDs.IRON_FENCE == tmp_obj.ID || MapObject.IDs.WIRE_FENCE == tmp_obj.ID) {
                            ascii_map[y][x] = iron_fence;
                        } else if (MapObject.IDs.IRON_GATE_OPEN == tmp_obj.ID) {
                            ascii_map[y][x] = open_gate;
                        } else if (MapObject.IDs.CHAIR == tmp_obj.ID) {
                            ascii_map[y][x] = chair;
                        } else if (MapObject.IDs.CHAR_CHAIR == tmp_obj.ID) {
                            ascii_map[y][x] = chair;
                        } else if (MapObject.IDs.HOSPITAL_CHAIR == tmp_obj.ID) {
                            ascii_map[y][x] = chair;
                        } else if (tmp_obj.IsTransparent && !tmp_obj.IsWalkable) {
                            ascii_map[y][x] = "|"; // gate; iron wall
                        } else {
                            if (tmp_obj is Engine.MapObjects.DoorWindow tmp_door) {
                                if (tmp_door.IsBarricaded) {
                                    ascii_map[y][x] = large_fortification_symbol; // no good icon...pretend it's a large fortification since it would have to be torn down to be passed through
                                } else if (tmp_door.IsClosed) {
                                    ascii_map[y][x] = "+"; // typical closed door
                                } else if (tmp_door.IsOpen) {
                                    ascii_map[y][x] = "'"; // typical open door
                                } else /* if (tmp_door.IsBroken */ {
                                    ascii_map[y][x] = "'"; // typical broken door
                                }
                            }
                        }
                        _process_inv(tmp_obj.NonEmptyInventory, x, y);
                    }
#endregion
                    _process_inv(GetItemsAt(pt), x, y);
#region actors
                    var a = GetActorAt(pt);
                    if (null != a && !a.IsDead) {
                        string p_txt = '(' + x.ToString() + ',' + y.ToString() + ')';
                        string a_str = ((int)a.Faction.ID).ToString(); // default to the faction numeral
                        string pos_css = "";
                        if (a.Controller is PlayerController) {
                            a_str = "@";
                            pos_css = " style='background:lightgreen'";
                        }
                        switch (a.Model.ID) {
                            case Gameplay.GameActors.IDs.UNDEAD_SKELETON:
                                a_str = "<span style='background:orange'>s</span>"; break;
                            case Gameplay.GameActors.IDs.UNDEAD_RED_EYED_SKELETON:
                                a_str = "<span style='background:red'>s</span>"; break;
                            case Gameplay.GameActors.IDs.UNDEAD_RED_SKELETON:
                                a_str = "<span style='background:darkred'>s</span>"; break;
                            case Gameplay.GameActors.IDs.UNDEAD_ZOMBIE:
                                a_str = "<span style='background:orange'>S</span>"; break;
                            case Gameplay.GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE:
                                a_str = "<span style='background:red'>S</span>"; break;
                            case Gameplay.GameActors.IDs.UNDEAD_DARK_ZOMBIE:
                                a_str = "<span style='background:darkred'>S</span>"; break;
                            case Gameplay.GameActors.IDs.UNDEAD_ZOMBIE_MASTER:
                                a_str = "<span style='background:orange'>Z</span>"; break;
                            case Gameplay.GameActors.IDs.UNDEAD_ZOMBIE_LORD:
                                a_str = "<span style='background:red'>Z</span>"; break;
                            case Gameplay.GameActors.IDs.UNDEAD_ZOMBIE_PRINCE:
                                a_str = "<span style='background:darkred'>Z</span>"; break;
                            case Gameplay.GameActors.IDs.UNDEAD_MALE_ZOMBIFIED:
                            case Gameplay.GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED:
                                a_str = "<span style='background:orange'>d</span>"; break;
                            case Gameplay.GameActors.IDs.UNDEAD_MALE_NEOPHYTE:
                            case Gameplay.GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE:
                                a_str = "<span style='background:red'>d</span>"; break;
                            case Gameplay.GameActors.IDs.UNDEAD_MALE_DISCIPLE:
                            case Gameplay.GameActors.IDs.UNDEAD_FEMALE_DISCIPLE:
                                a_str = "<span style='background:darkred'>d</span>"; break;
                            case Gameplay.GameActors.IDs.UNDEAD_RAT_ZOMBIE:
                                a_str = "<span style='background:orange'>r</span>"; break;
                            case Gameplay.GameActors.IDs.MALE_CIVILIAN:
                            case Gameplay.GameActors.IDs.FEMALE_CIVILIAN:
                                a_str = "<span style='background:lightgreen'>" + a_str + "</span>"; break;
                            case Gameplay.GameActors.IDs.FERAL_DOG:
                                a_str = "<span style='background:lightgreen'>C</span>"; break;    // C in Angband, Nethack
                            case Gameplay.GameActors.IDs.CHAR_GUARD:
                                a_str = "<span style='background:darkgray;color:white'>" + a_str + "</span>"; break;
                            case Gameplay.GameActors.IDs.ARMY_NATIONAL_GUARD:
                                a_str = "<span style='background:darkgreen;color:white'>" + a_str + "</span>"; break;
                            case Gameplay.GameActors.IDs.BIKER_MAN:
                                a_str = "<span style='background:darkorange;color:white'>" + a_str + "</span>"; break;
                            case Gameplay.GameActors.IDs.POLICEMAN:
                            case Gameplay.GameActors.IDs.POLICEWOMAN:
                                a_str = "<span style='background:lightblue'>" + a_str + "</span>"; break;
                            case Gameplay.GameActors.IDs.GANGSTA_MAN:
                                a_str = "<span style='background:red;color:white'>" + a_str + "</span>"; break;
                            case Gameplay.GameActors.IDs.BLACKOPS_MAN:
                                a_str = "<span style='background:black;color:white'>" + a_str + "</span>"; break;
                            case Gameplay.GameActors.IDs.SEWERS_THING:
                            case Gameplay.GameActors.IDs.JASON_MYERS:
                                a_str = "<span style='background:darkred;color:white'>" + a_str + "</span>"; break;
                        }
                        var actor_stats = new List<string> { " " };

                        if (a.Model.Abilities.HasToEat) {
                            if (a.IsStarving) actor_stats.Add("<span style='background-color:black; color:red'>H</span>");
                            else if (a.IsHungry) actor_stats.Add("<span style='background-color:black; color:yellow'>H</span>");
                            else if (a.IsAlmostHungry) actor_stats.Add("<span style='background-color:black; color:green'>H</span>");
                        } else if (a.Model.Abilities.IsRotting) {
                            if (a.IsRotStarving) actor_stats.Add("<span style='background-color:black; color:red'>H</span>");
                            else if (a.IsRotHungry) actor_stats.Add("<span style='background-color:black; color:yellow'>R</span>");
                            else if (a.IsAlmostRotHungry) actor_stats.Add("<span style='background-color:black; color:green'>R</span>");
                        }
                        if (a.Model.Abilities.HasSanity) {
                            if (a.IsInsane) actor_stats.Add("<span style='background-color:black; color:red'>I</span>");
                            else if (a.IsDisturbed) actor_stats.Add("<span style='background-color:black; color:yellow'>I</span>");
                        }
                        if (a.Model.Abilities.HasToSleep) {
                            if (a.IsExhausted) actor_stats.Add("<span style='background-color:black; color:red'>Z</span>");
                            else if (a.IsSleepy) actor_stats.Add("<span style='background-color:black; color:yellow'>Z</span>");
                            else if (a.IsAlmostSleepy) actor_stats.Add("<span style='background-color:black; color:green'>Z</span>");
                        }
                        if (a.IsSleeping) actor_stats.Add("<span style='background-color:black; color:cyan'>Z</span>");
                        if (0 < a.MurdersCounter) actor_stats.Add("<span style='background-color:black; color:red'>M</span>");
                        if (0 < a.CountFollowers) actor_stats.Add("<span style='background-color:black; color:cyan'>L</span>");
                        if (null != a.LiveLeader) actor_stats.Add("<span style='background-color:black; color:cyan'>F:" + a.LiveLeader.Name + "</span>");

                        actor_data.Add("<tr><td" + pos_css + ">" + p_txt + "</td><td>" + a.UnmodifiedName + string.Concat(actor_stats) + "</td><td>" + TurnOrderFor(a).ToString() + "</td><td>" + a.ActionPoints.ToString() + "</td><td>" + a.HitPoints.ToString() + "</td><td class='inv'>" + (null == a.Inventory ? "" : (a.Inventory.IsEmpty ? "" : a.Inventory.ToString())) + "</td></tr>");
                        ascii_map[y][x] = a_str;
                    }
#endregion
                }
            }

            static bool is_problem_corpse(Corpse c) { return 0 < c.DeadGuy.InfectionPercent; }
            var corpse_catalog = FilterCorpses(is_problem_corpse);
            if (null != corpse_catalog) dest.WriteLine("<pre>Problematic corpses:\n" + corpse_catalog.to_s() + "</pre>");

            if (0 >= inv_data.Count && 0 >= actor_data.Count) return;
            if (0 < actor_data.Count) {
                dest.WriteLine("<table border=2 cellspacing=1 cellpadding=1 align=left>");
                dest.WriteLine("<tr><th>" + string.Join("</th><th>", actor_headers) + "</th></tr>");
                foreach (string s in actor_data) dest.WriteLine(s);
                dest.WriteLine("</table>");
            }
            if (0 < inv_data.Count) {
                dest.WriteLine("<table border=2 cellspacing=1 cellpadding=1 align=right>");
                foreach (string s in inv_data) dest.WriteLine(s);
                dest.WriteLine("</table>");
            }
            dest.WriteLine("<a name='" + MapName + "'></a>");
            dest.WriteLine("<pre style='clear:both'>");
            foreach (int y in Enumerable.Range(0, Height)) {
                dest.WriteLine(string.Concat(ascii_map[y]));
            }
            dest.WriteLine("</pre>");
        }
    }
}
