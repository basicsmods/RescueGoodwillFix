using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RescueGoodwillFix
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main()
        {
            var harmony = new Harmony("basics.rescuegoodwillfix");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.ExitMap))]
    public class Pawn_ExitMap_Patch
    {
        /*
        The decompiled code for this function includes

        	bool flag = ThingOwnerUtility.AnyParentIs<ActiveDropPodInfo>(this) || ThingOwnerUtility.AnyParentIs<TravelingTransportPods>(this);
			bool flag2 = this.IsCaravanMember() || this.teleporting || flag;
			bool flag3 = !flag2 || (!this.IsPrisoner && !this.IsSlave && !flag) || (this.guest != null && this.guest.Released);
			bool flag4 = flag3 && (this.IsPrisoner || this.IsSlave) && this.guest != null && this.guest.Released;
			if (flag3 && !flag2)
			{
				foreach (Thing thing in this.EquippedWornOrInventoryThings)
				{
					Precept_ThingStyle styleSourcePrecept = thing.GetStyleSourcePrecept();
					if (styleSourcePrecept != null)
					{
						styleSourcePrecept.Notify_ThingLost(thing, false);
					}
				}
			}
			if (base.Faction != null)
			{
				base.Faction.Notify_MemberExitedMap(this, flag4);
			}

        Within Faction.Notify_MemberExitedMap, flag4 is used to determine whether or not the player
        should get any goodwill. 
        This patch makes it so that instead of 
                base.Faction.Notify_MemberExitedMap(this, flag4);
        we have
                base.Faction.Notify_MemberExitedMap(this, flag3);
        which allows for goodwill from both prisoner release and friendly rescues. 

        I am unsure of whether or not there are extra side effects of this.
        */
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            int idx;

            var f_Notify_MemberExitedMap = AccessTools.Method(typeof(Faction), nameof(Faction.Notify_MemberExitedMap));
            idx = list.FirstIndexOf(instr => instr.Calls(f_Notify_MemberExitedMap));
            if (idx < 0 || idx >= list.Count)
                Log.Error("Cannot find call Faction::Notify_MemberExitedMap() in Pawn::ExitMap. $basics$");
            else
            {
				if (list[idx-1].opcode != OpCodes.Ldloc_S)
                    Log.Error("Previous instruction is not a ldloc_s even though it should have been. $basics$");
				else
				{
					list[idx - 1] = new CodeInstruction(OpCodes.Ldloc_3);
					Log.Message("Changed ldloc_s to ldloc_3. $basics$");
				}
            }
            return list;
        }
    }
}
