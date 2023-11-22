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

            List<string> expectedInstructions = new List<string>{
                "ldarg.0 NULL",
				"call static System.Boolean RimWorld.Planet.WorldPawnsUtility::IsWorldPawn(Verse.Pawn p)",
				"brfalse.s Label0",
				"ldstr \"Called ExitMap() on world pawn \"",
				"ldarg.0 NULL",
				"call static System.String System.String::Concat(System.Object arg0, System.Object arg1)",
				"call static System.Void Verse.Log::Warning(System.String text)",
				"ret NULL",
				"ldarg.0 NULL [Label0]",
				"call RimWorld.Ideo Verse.Pawn::get_Ideo()",
				"dup NULL",
				"brtrue.s Label1",
				"pop NULL",
				"br.s Label2",
				"ldarg.0 NULL [Label1]",
				"ldarg.0 NULL",
				"call virtual Verse.Map Verse.Thing::get_Map()",
				"call System.Void RimWorld.Ideo::Notify_MemberLost(Verse.Pawn member, Verse.Map map)",
				"ldarg.1 NULL [Label2]",
				"brfalse.s Label3",
				"ldarg.0 NULL",
				"call static System.Boolean RimWorld.Planet.CaravanExitMapUtility::CanExitMapAndJoinOrCreateCaravanNow(Verse.Pawn pawn)",
				"brfalse.s Label4",
				"ldarg.0 NULL",
				"ldarg.2 NULL",
				"call static System.Void RimWorld.Planet.CaravanExitMapUtility::ExitMapAndJoinOrCreateCaravan(Verse.Pawn pawn, Verse.Rot4 exitDir)",
				"ret NULL",
				"ldarg.0 NULL [Label3, Label4]",
				"call static Verse.AI.Group.Lord Verse.AI.Group.LordUtility::GetLord(Verse.Pawn p)",
				"stloc.0 NULL",
				"ldloc.0 NULL",
				"brfalse.s Label5",
				"ldloc.0 NULL",
				"ldarg.0 NULL",
				"ldc.i4.6 NULL",
				"ldloca.s 5 (System.Nullable`1[Verse.DamageInfo])",
				"initobj System.Nullable`1[Verse.DamageInfo]",
				"ldloc.s 5 (System.Nullable`1[Verse.DamageInfo])",
				"callvirt System.Void Verse.AI.Group.Lord::Notify_PawnLost(Verse.Pawn pawn, Verse.AI.Group.PawnLostCondition cond, System.Nullable`1<Verse.DamageInfo> dinfo)",
				"ldarg.0 NULL [Label5]",
				"ldfld Verse.Pawn_CarryTracker Verse.Pawn::carryTracker",
				"brfalse Label6",
				"ldarg.0 NULL",
				"ldfld Verse.Pawn_CarryTracker Verse.Pawn::carryTracker",
				"callvirt Verse.Thing Verse.Pawn_CarryTracker::get_CarriedThing()",
				"brfalse Label7",
				"ldarg.0 NULL",
				"ldfld Verse.Pawn_CarryTracker Verse.Pawn::carryTracker",
				"callvirt Verse.Thing Verse.Pawn_CarryTracker::get_CarriedThing()",
				"isinst Verse.Pawn",
				"stloc.s 6 (Verse.Pawn)",
				"ldloc.s 6 (Verse.Pawn)",
				"brfalse.s Label8",
				"ldarg.0 NULL",
				"call virtual RimWorld.Faction Verse.Thing::get_Faction()",
				"brfalse.s Label9",
				"ldarg.0 NULL",
				"call virtual RimWorld.Faction Verse.Thing::get_Faction()",
				"ldloc.s 6 (Verse.Pawn)",
				"callvirt virtual RimWorld.Faction Verse.Thing::get_Faction()",
				"beq.s Label10",
				"ldarg.0 NULL",
				"call virtual RimWorld.Faction Verse.Thing::get_Faction()",
				"ldfld RimWorld.KidnappedPawnsTracker RimWorld.Faction::kidnapped",
				"ldloc.s 6 (Verse.Pawn)",
				"ldarg.0 NULL",
				"callvirt System.Void RimWorld.KidnappedPawnsTracker::Kidnap(Verse.Pawn pawn, Verse.Pawn kidnapper)",
				"br.s Label11",
				"ldarg.0 NULL [Label9, Label10]",
				"ldfld System.Boolean Verse.Pawn::teleporting",
				"brtrue.s Label12",
				"ldarg.0 NULL",
				"ldfld Verse.Pawn_CarryTracker Verse.Pawn::carryTracker",
				"ldfld Verse.ThingOwner`1<Verse.Thing> Verse.Pawn_CarryTracker::innerContainer",
				"ldloc.s 6 (Verse.Pawn)",
				"callvirt abstract virtual System.Boolean Verse.ThingOwner::Remove(Verse.Thing item)",
				"pop NULL",
				"ldloc.s 6 (Verse.Pawn) [Label12]",
				"ldc.i4.0 NULL",
				"ldarg.2 NULL",
				"callvirt System.Void Verse.Pawn::ExitMap(System.Boolean allowedToJoinOrCreateCaravan, Verse.Rot4 exitDir)",
				"br.s Label13",
				"ldarg.0 NULL [Label8]",
				"ldfld Verse.Pawn_CarryTracker Verse.Pawn::carryTracker",
				"callvirt Verse.Thing Verse.Pawn_CarryTracker::get_CarriedThing()",
				"ldc.i4.0 NULL",
				"callvirt virtual System.Void Verse.Thing::Destroy(Verse.DestroyMode mode)",
				"ldarg.0 NULL [Label11, Label13]",
				"ldfld System.Boolean Verse.Pawn::teleporting",
				"brfalse.s Label14",
				"ldloc.s 6 (Verse.Pawn)",
				"brtrue.s Label15",
				"ldarg.0 NULL [Label14]",
				"ldfld Verse.Pawn_CarryTracker Verse.Pawn::carryTracker",
				"ldfld Verse.ThingOwner`1<Verse.Thing> Verse.Pawn_CarryTracker::innerContainer",
				"callvirt virtual System.Void Verse.ThingOwner::Clear()",
				"ldarg.0 NULL [Label6, Label7, Label15]",
				"call static System.Boolean Verse.ThingOwnerUtility::AnyParentIs(Verse.Thing thing)",
				"brtrue.s Label16",
				"ldarg.0 NULL",
				"call static System.Boolean Verse.ThingOwnerUtility::AnyParentIs(Verse.Thing thing)",
				"br.s Label17",
				"ldc.i4.1 NULL [Label16]",
				"stloc.1 NULL [Label17]",
				"ldarg.0 NULL",
				"call static System.Boolean RimWorld.Planet.CaravanUtility::IsCaravanMember(Verse.Pawn pawn)",
				"brtrue.s Label18",
				"ldarg.0 NULL",
				"ldfld System.Boolean Verse.Pawn::teleporting",
				"br.s Label19",
				"ldc.i4.1 NULL [Label18]",
				"ldloc.1 NULL [Label19]",
				"or NULL",
				"stloc.2 NULL",
				"ldloc.2 NULL",
				"brfalse.s Label20",
				"ldarg.0 NULL",
				"call System.Boolean Verse.Pawn::get_IsPrisoner()",
				"brtrue.s Label21",
				"ldarg.0 NULL",
				"call System.Boolean Verse.Pawn::get_IsSlave()",
				"brtrue.s Label22",
				"ldloc.1 NULL",
				"brfalse.s Label23",
				"ldarg.0 NULL [Label21, Label22]",
				"ldfld RimWorld.Pawn_GuestTracker Verse.Pawn::guest",
				"brfalse.s Label24",
				"ldarg.0 NULL",
				"ldfld RimWorld.Pawn_GuestTracker Verse.Pawn::guest",
				"callvirt System.Boolean RimWorld.Pawn_GuestTracker::get_Released()",
				"br.s Label25",
				"ldc.i4.0 NULL [Label24]",
				"br.s Label26",
				"ldc.i4.1 NULL [Label20, Label23]",
				"stloc.3 NULL [Label25, Label26]",
				"ldloc.3 NULL",
				"brfalse.s Label27",
				"ldarg.0 NULL",
				"call System.Boolean Verse.Pawn::get_IsPrisoner()",
				"brtrue.s Label28",
				"ldarg.0 NULL",
				"call System.Boolean Verse.Pawn::get_IsSlave()",
				"brfalse.s Label29",
				"ldarg.0 NULL [Label28]",
				"ldfld RimWorld.Pawn_GuestTracker Verse.Pawn::guest",
				"brfalse.s Label30",
				"ldarg.0 NULL",
				"ldfld RimWorld.Pawn_GuestTracker Verse.Pawn::guest",
				"callvirt System.Boolean RimWorld.Pawn_GuestTracker::get_Released()",
				"br.s Label31",
				"ldc.i4.0 NULL [Label27, Label29, Label30]",
				"stloc.s 4 (System.Boolean) [Label31]",
				"ldloc.3 NULL",
				"brfalse.s Label32",
				"ldloc.2 NULL",
				"brtrue.s Label33",
				"ldarg.0 NULL",
				"call System.Collections.Generic.IEnumerable`1<Verse.Thing> Verse.Pawn::get_EquippedWornOrInventoryThings()",
				"callvirt abstract virtual System.Collections.Generic.IEnumerator`1<Verse.Thing> System.Collections.Generic.IEnumerable`1<Verse.Thing>::GetEnumerator()",
				"stloc.s 7 (System.Collections.Generic.IEnumerator`1[Verse.Thing])",
				"br.s Label34 [EX_BeginException]",
				"ldloc.s 7 (System.Collections.Generic.IEnumerator`1[Verse.Thing]) [Label36]",
				"callvirt abstract virtual Verse.Thing System.Collections.Generic.IEnumerator`1<Verse.Thing>::get_Current()",
				"stloc.s 8 (Verse.Thing)",
				"ldloc.s 8 (Verse.Thing)",
				"call static RimWorld.Precept_ThingStyle RimWorld.ThingStyleHelper::GetStyleSourcePrecept(Verse.Thing thing)",
				"stloc.s 9 (RimWorld.Precept_ThingStyle)",
				"ldloc.s 9 (RimWorld.Precept_ThingStyle)",
				"brfalse.s Label35",
				"ldloc.s 9 (RimWorld.Precept_ThingStyle)",
				"ldloc.s 8 (Verse.Thing)",
				"ldc.i4.0 NULL",
				"callvirt virtual System.Void RimWorld.Precept_ThingStyle::Notify_ThingLost(Verse.Thing thing, System.Boolean destroyed)",
				"ldloc.s 7 (System.Collections.Generic.IEnumerator`1[Verse.Thing]) [Label34, Label35]",
				"callvirt abstract virtual System.Boolean System.Collections.IEnumerator::MoveNext()",
				"brtrue.s Label36",
				"leave.s Label37",
				"ldloc.s 7 (System.Collections.Generic.IEnumerator`1[Verse.Thing]) [EX_BeginFinally]",
				"brfalse.s Label38",
				"ldloc.s 7 (System.Collections.Generic.IEnumerator`1[Verse.Thing])",
				"callvirt abstract virtual System.Void System.IDisposable::Dispose()",
				"endfinally NULL [Label38, EX_EndException]",
				"ldarg.0 NULL [Label32, Label33, Label37]",
				"call virtual RimWorld.Faction Verse.Thing::get_Faction()",
				"brfalse.s Label39",
				"ldarg.0 NULL",
				"call virtual RimWorld.Faction Verse.Thing::get_Faction()",
				"ldarg.0 NULL",
				"ldloc.s 4 (System.Boolean)",
				"callvirt System.Void RimWorld.Faction::Notify_MemberExitedMap(Verse.Pawn member, System.Boolean free)",
				"ldarg.0 NULL [Label39]",
				"call virtual RimWorld.Faction Verse.Thing::get_Faction()",
				"call static RimWorld.Faction RimWorld.Faction::get_OfPlayer()",
				"bne.un.s Label40",
				"ldarg.0 NULL",
				"call System.Boolean Verse.Pawn::get_IsSlave()",
				"brfalse.s Label41",
				"ldarg.0 NULL",
				"call RimWorld.Faction Verse.Pawn::get_SlaveFaction()",
				"brfalse.s Label42",
				"ldarg.0 NULL",
				"call RimWorld.Faction Verse.Pawn::get_SlaveFaction()",
				"call static RimWorld.Faction RimWorld.Faction::get_OfPlayer()",
				"beq.s Label43",
				"ldarg.0 NULL",
				"ldfld RimWorld.Pawn_GuestTracker Verse.Pawn::guest",
				"callvirt System.Boolean RimWorld.Pawn_GuestTracker::get_Released()",
				"brfalse.s Label44",
				"ldarg.0 NULL",
				"call RimWorld.Faction Verse.Pawn::get_SlaveFaction()",
				"ldarg.0 NULL",
				"ldloc.s 4 (System.Boolean)",
				"callvirt System.Void RimWorld.Faction::Notify_MemberExitedMap(Verse.Pawn member, System.Boolean free)",
				"ldarg.0 NULL [Label40, Label41, Label42, Label43, Label44]",
				"ldfld RimWorld.Pawn_Ownership Verse.Pawn::ownership",
				"ldnull NULL",
				"cgt.un NULL",
				"ldloc.s 4 (System.Boolean)",
				"and NULL",
				"brfalse.s Label45",
				"ldarg.0 NULL",
				"ldfld RimWorld.Pawn_Ownership Verse.Pawn::ownership",
				"callvirt System.Void RimWorld.Pawn_Ownership::UnclaimAll()",
				"ldarg.0 NULL [Label45]",
				"ldfld RimWorld.Pawn_GuestTracker Verse.Pawn::guest",
				"brfalse.s Label46",
				"ldarg.0 NULL",
				"call System.Boolean Verse.Pawn::get_IsPrisonerOfColony()",
				"ldloc.s 4 (System.Boolean)",
				"brfalse.s Label47",
				"ldarg.0 NULL",
				"ldfld RimWorld.Pawn_GuestTracker Verse.Pawn::guest",
				"ldnull NULL",
				"ldc.i4.0 NULL",
				"callvirt System.Void RimWorld.Pawn_GuestTracker::SetGuestStatus(RimWorld.Faction newHost, RimWorld.GuestStatus guestStatus)",
				"brfalse.s Label48 [Label47]",
				"ldarg.0 NULL",
				"ldfld RimWorld.Pawn_GuestTracker Verse.Pawn::guest",
				"ldsfld RimWorld.PrisonerInteractionModeDef RimWorld.PrisonerInteractionModeDefOf::NoInteraction",
				"stfld RimWorld.PrisonerInteractionModeDef RimWorld.Pawn_GuestTracker::interactionMode",
				"ldarg.0 NULL",
				"ldfld RimWorld.Pawn_GuestTracker Verse.Pawn::guest",
				"callvirt System.Boolean RimWorld.Pawn_GuestTracker::get_Released()",
				"ldc.i4.0 NULL",
				"ceq NULL",
				"ldloc.3 NULL",
				"and NULL",
				"brfalse.s Label49",
				"ldarg.0 NULL",
				"call static System.Void RimWorld.GuestUtility::Notify_PrisonerEscaped(Verse.Pawn prisoner)",
				"ldarg.0 NULL [Label48, Label49]",
				"ldfld RimWorld.Pawn_GuestTracker Verse.Pawn::guest",
				"ldc.i4.0 NULL",
				"callvirt System.Void RimWorld.Pawn_GuestTracker::set_Released(System.Boolean value)",
				"ldarg.0 NULL [Label46]",
				"ldc.i4.0 NULL",
				"call System.Boolean Verse.Thing::DeSpawnOrDeselect(Verse.DestroyMode mode)",
				"pop NULL",
				"ldarg.0 NULL",
				"ldfld Verse.Pawn_InventoryTracker Verse.Pawn::inventory",
				"ldc.i4.0 NULL",
				"callvirt System.Void Verse.Pawn_InventoryTracker::set_UnloadEverything(System.Boolean value)",
				"ldloc.3 NULL",
				"brfalse.s Label50",
				"ldarg.0 NULL",
				"ldc.i4.0 NULL",
				"ldc.i4.0 NULL",
				"ldc.i4.1 NULL",
				"call System.Void Verse.Pawn::ClearMind(System.Boolean ifLayingKeepLaying, System.Boolean clearInspiration, System.Boolean clearMentalState)",
				"ldarg.0 NULL [Label50]",
				"ldfld RimWorld.Pawn_RelationsTracker Verse.Pawn::relations",
				"brfalse.s Label51",
				"ldarg.0 NULL",
				"ldfld RimWorld.Pawn_RelationsTracker Verse.Pawn::relations",
				"callvirt System.Void RimWorld.Pawn_RelationsTracker::Notify_ExitedMap()",
				"call static RimWorld.Planet.WorldPawns Verse.Find::get_WorldPawns() [Label51]",
				"ldarg.0 NULL",
				"ldc.i4.0 NULL",
				"callvirt System.Void RimWorld.Planet.WorldPawns::PassToWorld(Verse.Pawn pawn, RimWorld.Planet.PawnDiscardDecideMode discardMode)",
				"ldarg.0 NULL",
				"ldfld System.Collections.Generic.List`1<System.String> Verse.Thing::questTags",
				"ldstr \"LeftMap\"",
				"ldarg.0 NULL",
				"ldstr \"SUBJECT\"",
				"call static Verse.NamedArgument Verse.NamedArgumentUtility::Named(System.Object arg, System.String label)",
				"call static System.Void RimWorld.QuestUtility::SendQuestTargetSignals(System.Collections.Generic.List`1<System.String> questTags, System.String signalPart, Verse.NamedArgument arg1)",
				"call static RimWorld.FactionManager Verse.Find::get_FactionManager()",
				"ldarg.0 NULL",
				"callvirt System.Void RimWorld.FactionManager::Notify_PawnLeftMap(Verse.Pawn pawn)",
				"call static RimWorld.IdeoManager Verse.Find::get_IdeoManager()",
				"ldarg.0 NULL",
				"callvirt System.Void RimWorld.IdeoManager::Notify_PawnLeftMap(Verse.Pawn pawn)",
				"ret NULL"
            }; 

			if (list.Count != expectedInstructions.Count)
			{
				Log.Error("RescueGoodwillFix: ExitMap function doesn't have expected number of instructions. Maybe the devs fixed the issue and this mod is no longer necessary? $basics$");
				Log.Error("RescueGoodwillFix: Mod disabled so that nothing breaks.");
				return list;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].ToString() != expectedInstructions[i])
				{
					Log.Error("RescueGoodwillFix: ExitMap function instructions vs expected instructions has a missmatch.");
					Log.Error("Expected");
					Log.Error(expectedInstructions[i]);
					Log.Error("Got");
					Log.Error(list[i].ToString());
					Log.Error("Maybe the devs fixed the issue and this mod is no longer necessary? $basics$");
                    Log.Error("RescueGoodwillFix: Mod disabled so that nothing breaks.");
                    return list;
				}
			}

			/*
			Log.Message("$basics$ start");
			foreach ( var instruction in list )
			{
				Log.Message(instruction.ToString());
			}
            Log.Message("$basics$ end");
			*/

            var f_Notify_MemberExitedMap = AccessTools.Method(typeof(Faction), nameof(Faction.Notify_MemberExitedMap));
			idx = list.FirstIndexOf(instr => instr.Calls(f_Notify_MemberExitedMap));
			if (idx < 0 || idx >= list.Count)
				Log.Error("RescueGoodwillFix: Cannot find call Faction::Notify_MemberExitedMap() in Pawn::ExitMap. $basics$");
            else
			{
				list[idx - 1] = new CodeInstruction(OpCodes.Ldloc_3);
				Log.Message("RescueGoodwillFix: Changed ldloc_s to ldloc_3. $basics$");
				Log.Message("RescueGoodwillFix: Mod enabled.");
			}
            return list;
        }
    }
}
