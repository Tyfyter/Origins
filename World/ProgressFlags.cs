using AltLibrary.Common.AltBiomes;
using AltLibrary.Common.Systems;
using Origins.World.BiomeData;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.World; 
public class ProgressFlags : ProgressFlagSystem, IBroken {
	static string IBroken.BrokenReason => "Use event directly";
	public static ProgressFlag SolarEclipseOccurred { get; private set; }

	public static ProgressFlag DownedFiberglassWeaver { get; private set; }
	public static ProgressFlag DownedEaterOfWorlds { get; private set; }
	public static ProgressFlag DownedBrainOfCthulhu { get; private set; }
	public static ProgressFlag DownedDefiledAmalgamation { get; private set; }
	public static ProgressFlag DownedWorldCracker { get; private set; }
	public static ProgressFlag DownedTrenchmaker { get; private set; }
	public static ProgressFlag DownedLostDiver { get; private set; }
	public static ProgressFlag DownedShimmerConstruct { get; private set; }

	public static ProgressFlag DownedChambersiteSentinel { get; private set; }

	public static ProgressFlag DownedDefiledMimic { get; private set; }
	public static ProgressFlag DownedRivenMimic { get; private set; }
	public static ProgressFlag DownedTrashCompactorMimic { get; private set; }
	const int current_version = 1;
	public override void PostWorldLoad() {
		base.PostWorldLoad();
		MethodInfo set = typeof(ProgressFlag).GetProperty(nameof(ProgressFlag.IsSet)).SetMethod;// to avoid triggering events
		if (lastSaveVersion < 1 && NPC.downedBoss2) {
			switch (WorldBiomeManager.GetWorldEvil()) {
				case CorruptionAltBiome:
				set.Invoke(DownedEaterOfWorlds, [true]);
				break;
				case CrimsonAltBiome:
				set.Invoke(DownedBrainOfCthulhu, [true]);
				break;
				case Defiled_Wastelands_Alt_Biome:
				set.Invoke(DownedDefiledAmalgamation, [true]);
				break;
				case Riven_Hive_Alt_Biome:
				set.Invoke(DownedWorldCracker, [true]);
				break;
			}
		}
	}
	public override void Load() {
		base.Load();
		On_Main.UpdateTime_StartNight += (On_Main.orig_UpdateTime_StartNight orig, ref bool stopEvents) => {
			if (Main.eclipse && !Main.IsFastForwardingTime()) SolarEclipseOccurred.Set(true);
			orig(ref stopEvents);
		};
		if (typeof(ProgressFlag).GetEvent("OnSet") is EventInfo @event) {
			@event.AddEventHandler(DownedDefiledAmalgamation, DownedEvilBoss);
			@event.AddEventHandler(DownedWorldCracker, DownedEvilBoss);
			@event.AddEventHandler(DownedTrenchmaker, DownedEvilBoss);
		}
		static void DownedEvilBoss() {
			if (!NPC.downedBoss2 || Main.rand.NextBool(2)) WorldGen.spawnMeteor = true;
			NPC.SetEventFlagCleared(ref NPC.downedBoss2, GameEventClearedID.DefeatedEaterOfWorldsOrBrainOfChtulu);
		}
	}
	public override void ClearWorld() {
		base.ClearWorld();
		lastSaveVersion = default;
	}
	public override void SaveWorldData(TagCompound tag) {
		base.SaveWorldData(tag);
		tag[nameof(lastSaveVersion)] = current_version;
	}
	public override void LoadWorldData(TagCompound tag) {
		base.LoadWorldData(tag);
		tag.TryGet(nameof(lastSaveVersion), out lastSaveVersion);
	}
	static int lastSaveVersion;
}
file class TrackVanillaEvil : GlobalNPC {
	public override void OnKill(NPC npc) {
		switch (npc.type) {
			case NPCID.EaterofWorldsHead:
			case NPCID.EaterofWorldsBody:
			case NPCID.EaterofWorldsTail:
			if (npc.boss) ProgressFlags.DownedEaterOfWorlds.Set();
			break;
			case NPCID.BrainofCthulhu:
			if (npc.boss) ProgressFlags.DownedBrainOfCthulhu.Set();
			break;
		}
	}
}
