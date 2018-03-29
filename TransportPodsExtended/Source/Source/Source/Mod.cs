using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

using RimWorld;
using Verse;
using Harmony;

namespace TransportPodsExtended
{
	// Loads Harmony
	[StaticConstructorOnStartup]
	public class TPE_HarmonyPatch
	{
		static TPE_HarmonyPatch()
		{
			HarmonyInstance harmony = HarmonyInstance.Create("TransportPod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			/*
			var methods = harmony.GetPatchedMethods();

			foreach (var method in methods)
			{
				Log.Warning(method.ToString()); // Lists each loaded patch in this mod
			}
			*/

		}
	}
	static class LaunchableMethods
	{
		public static float FuelNeededToLaunchAtDist(float dist, CompLaunchable launchable)
		{
			return FuelNeededToLaunchAtDist(dist, PercentFull(launchable));
		}
		public static float FuelNeededToLaunchAtDist(float dist, float percentFull)
		{
			float ret = (MassFactor(percentFull) * dist);
			return ret;
		}

		public static int MaxLaunchDistanceAtFuelLevel(float fuelLevel, CompLaunchable launchable)
		{
			return MaxLaunchDistanceAtFuelLevel(fuelLevel, PercentFull(launchable));
		}

		public static int MaxLaunchDistanceAtFuelLevel(float fuelLevel, float percentFull)
		{
			int ret = Mathf.FloorToInt(fuelLevel / MassFactor(percentFull));
			return ret;
		}

		public static float MassFactor(float percentFull)
		{
			return (0.6f + 2.0f * percentFull);
		}

		public static float PercentFull(CompLaunchable launchable)
		{
			float mass = 0;
			List<CompTransporter> transporters = launchable.TransportersInGroup;
			foreach (CompTransporter transporter in transporters)
			{
				foreach (Thing t in transporter.GetDirectlyHeldThings())
				{
					mass += t.GetStatValue(StatDefOf.Mass);
				}
			}
			float max = transporters.Count * launchable.parent.GetStatValue(StatDefOf.CarryingCapacity);
			return mass / (max * (150 / launchable.parent.GetStatValue(StatDefOf.CarryingCapacity)));
		}
	}


	[HarmonyPatch(typeof(CompLaunchable), "MaxLaunchDistanceAtFuelLevel")]
	public static class MaxLaunchDistanceAtFuelLevelPatch
	{
		public static void Postfix(float fuelLevel, ref int __result)
		{
			__result = LaunchableMethods.MaxLaunchDistanceAtFuelLevel(fuelLevel, 0f);
		}
	}

	[HarmonyPatch(typeof(CompLaunchable), "TryLaunch")]
	//private void TryLaunch(GlobalTargetInfo target, PawnsArriveMode arriveMode, bool attackOnArrival)
	public static class TryLaunchPatch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			MethodInfo FuelNeededToLaunchAtDistInfo = AccessTools.Method(typeof(CompLaunchable), "FuelNeededToLaunchAtDist");
			MethodInfo FuelNeededToLaunchAtDistInfoPatch = AccessTools.Method(typeof(LaunchableMethods), "FuelNeededToLaunchAtDist",
				new Type[] { typeof(float), typeof(CompLaunchable) });

			foreach (CodeInstruction i in codeInstructions)
			{
				if (i.opcode == OpCodes.Call && i.operand == FuelNeededToLaunchAtDistInfo)
				{
					i.operand = FuelNeededToLaunchAtDistInfoPatch;
					yield return new CodeInstruction(OpCodes.Ldarg_0); //this
				}
				yield return i;
			}
		}
	}

	[HarmonyPatch(typeof(CompLaunchable), "ChoseWorldTarget")]
	//private void TryLaunch(GlobalTargetInfo target, PawnsArriveMode arriveMode, bool attackOnArrival)
	static class ChoseWorldTargetPatch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			return TryLaunchPatch.Transpiler(codeInstructions);
		}
	}

	[HarmonyPatch(typeof(CompLaunchable), "get_MaxLaunchDistance")]
	//private void TryLaunch(GlobalTargetInfo target, PawnsArriveMode arriveMode, bool attackOnArrival)
	public static class MaxLaunchDistance_Patch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			MethodInfo MaxLaunchDistanceAtFuelLevel = AccessTools.Method(typeof(CompLaunchable), "MaxLaunchDistanceAtFuelLevel");
			MethodInfo MaxLaunchDistanceAtFuelLevelPatch = AccessTools.Method(typeof(LaunchableMethods), "MaxLaunchDistanceAtFuelLevel",
				new Type[] { typeof(float), typeof(CompLaunchable) });

			foreach (CodeInstruction i in codeInstructions)
			{
				if (i.opcode == OpCodes.Call && i.operand == MaxLaunchDistanceAtFuelLevel)
				{
					i.operand = MaxLaunchDistanceAtFuelLevelPatch;
					yield return new CodeInstruction(OpCodes.Ldarg_0); //this
				}
				yield return i;
			}
		}
	}

	[HarmonyPatch(typeof(CompLaunchable), "get_MaxLaunchDistanceEverPossible")]
	//private void TryLaunch(GlobalTargetInfo target, PawnsArriveMode arriveMode, bool attackOnArrival)
	static class MaxLaunchDistanceEverPossible_Patch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			return MaxLaunchDistance_Patch.Transpiler(codeInstructions);
		}
	}
}