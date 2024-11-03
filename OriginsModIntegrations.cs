using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
//using ThoriumMod.Items;
using System.Reflection;
//using ThoriumMod.Projectiles.Bard;
using Microsoft.Xna.Framework;
using Origins.Tiles;
using Origins.NPCs.MiscE;
using MonoMod.Cil;
using Microsoft.Xna.Framework.Input;
using Terraria.Localization;
using Origins.Dev;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Origins {
	public class OriginsModIntegrations : ILoadable {
		private static OriginsModIntegrations instance;
		Mod wikiThis;
		public static Mod WikiThis { get => instance.wikiThis; set => instance.wikiThis = value; }
		Mod epikV2;
		public static Mod EpikV2 { get => instance.epikV2; set => instance.epikV2 = value; }
		Asset<Texture2D> phaseIndicator;
		public static Asset<Texture2D> PhaseIndicator { get => instance.phaseIndicator; set => instance.phaseIndicator = value; }
		Mod herosMod;
		public static Mod HEROsMod { get => instance.herosMod; set => instance.herosMod = value; }
		Mod thorium;
		public static Mod Thorium { get => instance.thorium; set => instance.thorium = value; }
		Mod fancyLighting;
		public static Mod FancyLighting { get => instance.fancyLighting; set => instance.fancyLighting = value; }
		Func<bool> checkAprilFools;
		public static Func<bool> CheckAprilFools { 
			get => instance.checkAprilFools ??= ModLoader.TryGetMod("HolidayLib", out Mod HolidayLib) ? HolidayLibCheckAprilFools(HolidayLib) : DefaultCheckAprilFools;
			set => instance.checkAprilFools = value;
		}
		public static Condition AprilFools => new("Mods.Origins.Conditions.AprilFools", CheckAprilFools);
		public void Load(Mod mod) {
			instance = this;
			if (!Main.dedServ && ModLoader.TryGetMod("Wikithis", out wikiThis)) {
				WikiThis.Call("AddModURL", Origins.instance, "tyfyter.github.io/OriginsWiki/{}");
			}
			if (ModLoader.TryGetMod("ThoriumMod", out instance.thorium)) {
				LoadThorium();
			}
			if (ModLoader.TryGetMod("HolidayLib", out Mod HolidayLib)) {
				checkAprilFools = (Func<bool>)HolidayLib.Call("GETACTIVELOOKUP", "April fools");
			} else {
				checkAprilFools = DefaultCheckAprilFools;
			}
			if (ModLoader.TryGetMod("ItemSourceHelper", out Mod itemSourceHelper)) {
				itemSourceHelper.Call("AddIconicWeapon", DamageClasses.Explosive.Type, (int)ItemID.Bomb);
			}
		}
		static Func<bool> HolidayLibCheckAprilFools(Mod HolidayLib) => (Func<bool>)HolidayLib.Call("GETACTIVELOOKUP", "April fools");
		static bool DefaultCheckAprilFools() => DateTime.Today.Month == 4 && DateTime.Today.Day == 1;
		public static void LateLoad() {
			if (ModLoader.TryGetMod("PhaseIndicator", out Mod phaseIndicatorMod) && phaseIndicatorMod.RequestAssetIfExists("PhaseIndicator", out Asset<Texture2D> phaseIndicatorTexture)) {
				instance.phaseIndicator = phaseIndicatorTexture;
			}
			if (ModLoader.TryGetMod("EpikV2", out instance.epikV2)) {
				EpikV2.Call("AddModEvilBiome", ModContent.GetInstance<Defiled_Wastelands>());
				EpikV2.Call("AddModEvilBiome", ModContent.GetInstance<Riven_Hive>());
				/*EpikV2.Call("AddBiomeKey",
					ModContent.ItemType<Defiled_Biome_Keybrand>(),
					ModContent.ItemType<Defiled_Key>(),
					ModContent.TileType<Defiled_Dungeon_Chest>(),
					36,
					ItemID.CorruptionKey
				);*///just here so it can eventually be used
			}
			if (ModLoader.TryGetMod("HEROsMod", out instance.herosMod)) {
				HEROsMod.Call(
					"AddItemCategory",
					"Explosive",
					"Weapons",
					(Predicate<Item>)((Item i) => i.CountsAsClass<Explosive>())
				);
			}
			if (ModLoader.TryGetMod("FancyLighting", out instance.fancyLighting)) {
				instance.LoadFancyLighting();
			} else {
				compatRecommendations.Add(Language.GetText("Mods.Origins.ModCompatNotes.AddFancyLighting"));
			}

			if (ModLoader.TryGetMod("ModDemoUtils", out Mod modDemoUtils)) {
				ItemWikiProvider itemWikiProvider = new();
				modDemoUtils.Call("RegisterDemo", Origins.instance, "Tyfyter/Origins");
				modDemoUtils.Call("AddStatProvider", Origins.instance, (Item item) => {
					return itemWikiProvider.GetStats(item.ModItem).First().Item2;
				});
			}
		}
		public void Unload() {
			instance = null;
			compatRecommendations = null;
			compatErrors = null;
		}
		public static List<LocalizedText> compatRecommendations = [];
		public static List<LocalizedText> compatErrors = [];
		[JITWhenModsEnabled("FancyLighting")]
		void LoadFancyLighting() {
			try {
				Type smoothLightingType = fancyLighting.GetType().Assembly.GetType("FancyLighting.SmoothLighting");
				//MethodInfo[] methods = smoothLightingType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				MonoModHooks.Add(
					smoothLightingType.GetMethod("TileShine", BindingFlags.NonPublic | BindingFlags.Static),
					(hook_TileShine)((orig_TileShine orig, ref Vector3 color, Tile tile) => {
						orig(ref color, tile);
						if (tile.HasTile) {
							if (TileLoader.GetTile(tile.TileType) is IGlowingModTile glowingTile) glowingTile.FancyLightingGlowColor(tile, ref color);
							switch (tile.TileType) {
								case TileID.DyePlants:
								if (tile.TileFrameX == 204 || tile.TileFrameX == 202) goto case TileID.Cactus;
								break;
								case TileID.Cactus: {
									Point pos = tile.GetTilePosition();
									WorldGen.GetCactusType(pos.X, pos.Y, tile.TileFrameX, tile.TileFrameY, out int sandType);
									if (PlantLoader.Get<ModCactus>(80, sandType) is IGlowingModPlant glowingPlant) {
										glowingPlant.FancyLightingGlowColor(tile, ref color);
									}
									break;
								}
								case TileID.VanityTreeSakura:
								case TileID.VanityTreeYellowWillow:
								break;
								default: {
									if (OriginExtensions.GetTreeType(tile) is IGlowingModTile glowingTree) {
										glowingTree.FancyLightingGlowColor(tile, ref color);
									}
									break;
								}
							}
						}
					})
				);
				/*
				int wallShineCount = 0;
				foreach (var item in smoothLightingType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
					MethodInfo wallShineMethod = null;
					if (item.GetMethod("<CalculateSmoothLightingReach>b__0", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) is MethodInfo meth0) {
						wallShineMethod = meth0;
					} else if (item.GetMethod("<CalculateSmoothLightingHiDef>b__0", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) is MethodInfo meth1) {
						wallShineMethod = meth1;
					}
					if (wallShineMethod is not null && false) {
						MonoModHooks.Modify(wallShineMethod, (il) => {
							ILCursor c = new(il);
							c.GotoNext(MoveType.Before,
								i => i.MatchCall("FancyLighting.Util.VectorToColor", "Assign")
							);
							c.EmitLdloc(4);
							c.EmitDelegate<Func<Vector3, Tile, Vector3>>((color, tile) => {
								if (WallLoader.GetWall(tile.WallType) is IGlowingModWall glowingWall) glowingWall.FancyLightingGlowColor(tile, ref color);
								return color;
							});
						});
						wallShineCount++;
					}
				}
				if (wallShineCount == 0) {
					Origins.LogLoadingWarning(Language.GetText("Mods.Origins.Warnings.FancyLightingWallShineDelegateMissing"));
				}
				//*/
			} catch (Exception e) {
				Origins.LogError("Exception thrown while loading Fancy Lighting Integration:", e);
				FancyLighting = null;
#if DEBUG
				throw;
#endif
			}
			/*for (int i = 0; i < OriginTile.IDs.Count; i++) {
				if (OriginTile.IDs[i] is IGlowingModTile glowingTile) {
					glowingTiles[OriginTile.IDs[i].Type] = true;
					glowingTileColors[OriginTile.IDs[i].Type] = glowingTile.GlowColor;
				}
			}*/
		}
		delegate void orig_TileShine(ref Vector3 color, Tile tile);
		delegate void hook_TileShine(orig_TileShine orig, ref Vector3 color, Tile tile);
		[JITWhenModsEnabled("ThoriumMod")]
		static void LoadThorium() {
			///TODO: redo bardness
			/*MonoModHooks.Add(
				typeof(BardItem).GetMethod("SetDefaults", BindingFlags.Public | BindingFlags.Instance),
				(Action<Action<BardItem>, BardItem>)([JITWhenModsEnabled("ThoriumMod")](orig, self) => {
					orig(self);
					if (self is IBardDamageClassOverride classOverride) {
						self.Item.DamageType = classOverride.DamageType;
					}
				})
			);
			MonoModHooks.Add(
				typeof(BardProjectile).GetMethod("SetDefaults", BindingFlags.Public | BindingFlags.Instance),
				(Action<Action<BardProjectile>, BardProjectile>)([JITWhenModsEnabled("ThoriumMod")](orig, self) => {
					orig(self);
					if (self is IBardDamageClassOverride classOverride) {
						self.Projectile.DamageType = classOverride.DamageType;
					}
				})
			);*/
			//OriginExtensions.OnIncreaseMaxBreath += (player, _) => player.GetModPlayer<ThoriumMod.ThoriumPlayer>().breathOverMax = true;
			(string name, float? assimilation)[] thoriumNPCs = [
				("TheInnocent", 0.02f),
				("FrostWormHead", null),
				("FrostWormBody", null),
				("FrostWormTail", null),
				("SnowEater", 0.03f),
				("TheStarved", 0.10f),
				("HorrificCharger", null),
				("VileFloater", null),
				("ChilledSpitter", null),
				("Freezer", 0.09f),
				("SoulCorrupter", null),
			];
			for (int i = 0; i < thoriumNPCs.Length; i++) {
				(string name, float? assimilation) = thoriumNPCs[i];
				if (Thorium.TryFind(name, out ModNPC npc)) {
					CorruptGlobalNPC.NPCTypes.Add(npc.Type);
					if (assimilation.HasValue) CorruptGlobalNPC.AssimilationAmounts.Add(npc.Type, assimilation.Value);
				} else {
					Origins.LogError($"Could not find npc \"{name}\" in Thorium");
				}
			}

			thoriumNPCs = [
				("LivingHemorrhage", null),
				("Clot", null),
				("Coolmera", null),
				("FrozenFace", null),
				("BlisterPod", null),
				("Blister", 0.01f),
				("Coldling", null),
				("FrozenGross", null),
				("EpiDermon", null)
			];
			for (int i = 0; i < thoriumNPCs.Length; i++) {
				(string name, float? assimilation) = thoriumNPCs[i];
				if (Thorium.TryFind(name, out ModNPC npc)) {
					CrimsonGlobalNPC.NPCTypes.Add(npc.Type);
					if (assimilation.HasValue) CrimsonGlobalNPC.AssimilationAmounts.Add(npc.Type, assimilation.Value);
				} else {
					Origins.LogError($"Could not find npc \"{name}\" in Thorium");
				}
			}
		}
	}
	public interface ICustomWikiDestination {
		string WikiPageName { get; }
	}
	[ExtendsFromMod("ThoriumMod")]
	public class OriginsThoriumPlayer : ModPlayer {
		public bool altEmpowerment = false;
		public override void ResetEffects() {
			altEmpowerment = false;
		}
	}
	[ExtendsFromMod("ThoriumMod")]
	public class OriginsThoriumGlobalNPC : GlobalNPC {
		public override bool InstancePerEntity => true;
		int sonorousShredderHitCount = 0;
		int sonorousShredderHitTime = 0;
		public override void ResetEffects(NPC npc) {
			if(sonorousShredderHitTime > 0) {
				if (--sonorousShredderHitTime <= 0) {
					sonorousShredderHitCount = 0;
				}
			}
		}
		public bool SonorousShredderHit() {
			if (sonorousShredderHitCount < 4) {
				sonorousShredderHitCount++;
				sonorousShredderHitTime = 300;
				return false;
			} else {
				sonorousShredderHitCount = 0;
				sonorousShredderHitTime = 0;
				return true;
			}
		}
	}
	public interface IBardDamageClassOverride {
		DamageClass DamageType { get; }
	}
}
