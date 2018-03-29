/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;

using RimWorld;
using RimWorld.Planet;
using Verse;
using Harmony;

namespace TransportPodsExtended
{

	[StaticConstructorOnStartup]
	public class TPE_HarmonyPatch
	{
		static TPE_HarmonyPatch()
		{
			HarmonyInstance harmony = HarmonyInstance.Create("TransportPod");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			var methods = harmony.GetPatchedMethods();

			foreach (var method in methods)
			{
				Log.Warning(method.ToString());
			}

		}
	}

	[HarmonyPatch(typeof(CompLaunchable))]
	[HarmonyPatch("MaxLaunchDistanceAtFuelLevel")]
	static class MaxDistancePatch
	{
		static void Postfix(ref int __result, float fuelLevel)
		{
			__result = Mathf.FloorToInt(fuelLevel / 10f);
		}
	}
	
	[HarmonyPatch(typeof(CompLaunchable))]
	[HarmonyPatch("TryLaunch")]
	static class TryLaunchPatch
	{

		static HashSet<LaunchableData> podData;

		static TryLaunchPatch()
		{
			
		}

		static void Prefix(CompLaunchable __instance, GlobalTargetInfo target, PawnsArriveMode arriveMode, bool attackOnArrival)
		{

			podData = new HashSet<LaunchableData>(__instance.TransportersInGroup.Where(ct => ct.Launchable.FuelingPortSource != null).GroupBy(ct => ct.Launchable.FuelingPortSource.TryGetComp<CompRefuelable>()).Select(grp => new LaunchableData(grp.Key, grp.Sum(ct => ct.GetDirectlyHeldThings().Sum(t => t.GetStatValue(StatDefOf.Mass))), grp.Key.Fuel, grp.Count())));


			for (int i = 0; i < __instance.TransportersInGroup.Count; i++)
			{

				Map map = __instance.parent.Map;
				int TraversalDistance = Find.WorldGrid.TraversalDistanceBetween(map.Tile, target.Tile);

				CompTransporter compTransporter = __instance.TransportersInGroup[i];
				Building fuelingPortSource = compTransporter.Launchable.FuelingPortSource;
				if (fuelingPortSource != null)
				{
					fuelingPortSource.TryGetComp<CompRefuelable>().ConsumeFuel((float) (TraversalDistance * (2 + GetTotalMass() / (150 * __instance.TransportersInGroup.Count)) - (2.25 * TraversalDistance)));
				}

			}

		}

		static void Postfix(CompLaunchable __instance, GlobalTargetInfo target, PawnsArriveMode arriveMode, bool attackOnArrival)
		{
			
		}

		static float GetTotalMass()
		{
			float mass = 0;

			foreach (LaunchableData data in podData)
			{
				mass += data.weight;
			}

			return mass;
		}

		public struct LaunchableData
		{
			public CompRefuelable refuelable;
			public float weight;
			public float initialFuel;
			public int podCount;

			public LaunchableData(CompRefuelable refuelable, float weight, float initialFuel, int podCount)
			{
				this.refuelable = refuelable;
				this.weight = weight;
				this.initialFuel = initialFuel;
				this.podCount = podCount;
			}
		}

	}

}
*/