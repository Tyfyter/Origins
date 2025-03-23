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
using ThoriumMod;
using System.Reflection.Emit;
using Terraria.DataStructures;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Origins.Items;
using Origins.Items.Other.Consumables;
using Origins.NPCs.Defiled.Boss;
using Origins.NPCs.Riven.World_Cracker;
using AltLibrary.Common.Systems;
using static Terraria.GameContent.Bestiary.IL_BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions;
using AltLibrary.Common.AltBiomes;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Tiles.BossDrops;
using Origins.Items.Pets;
using Origins.NPCs.Fiberglass;
using Origins.NPCs;
using PegasusLib.Graphics;
using ThoriumMod.Projectiles.Bard;
using ThoriumMod.Items;
using ThoriumMod.Items.Darksteel;
using Origins.Items.Accessories;
using Origins.Buffs;
using Origins.NPCs.Brine.Boss;
using Origins.NPCs.TownNPCs;

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
		Mod fargosMutant;
		public static Mod FargosMutant { get => instance.fargosMutant; set => instance.fargosMutant = value; }
		Func<bool> checkAprilFools;
		public static Func<bool> CheckAprilFools {
			get => OriginClientConfig.Instance.DebugMenuButton.ForceAprilFools ?
				() => true :
				instance.checkAprilFools ??= ModLoader.TryGetMod("HolidayLib", out Mod HolidayLib) ? HolidayLibCheckAprilFools(HolidayLib) : DefaultCheckAprilFools;
			set => instance.checkAprilFools = value;
		}
		Func<object[], object> holidayForceChanged;
		public static void HolidayForceChanged() => instance.holidayForceChanged([]);
		public static Condition AprilFools => new("Mods.Origins.Conditions.AprilFools", () => CheckAprilFools());
		ModKeybind goToKeybindKeybind;
		public static bool GoToKeybindKeybindPressed => instance.goToKeybindKeybind?.JustPressed ?? false;
		Action<ModKeybind> goToKeybind;
		public static void GoToKeybind(ModKeybind keybind) {
			instance.goToKeybind?.Invoke(keybind);
		}
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
				HolidayLib.Call("ADDHOLIDAY", "April fools", () => OriginSystem.Instance.ForceAF.ToInt());
				holidayForceChanged = (Func<object[], object>)HolidayLib.Call("GETFUNC", "HOLIDAYFORCECHANGED");
			} else {
				checkAprilFools = DefaultCheckAprilFools;
				holidayForceChanged = _ => -1;
			}
			if (ModLoader.TryGetMod("ItemSourceHelper", out Mod itemSourceHelper)) {
				itemSourceHelper.Call("AddIconicWeapon", DamageClasses.Explosive.Type, (int)ItemID.Bomb);
				itemSourceHelper.Call("AddShimmerFakeCondition", RecipeConditions.ShimmerTransmutation);
			}
			if (ModLoader.TryGetMod("ColoredDamageTypes", out Mod coloredDamageTypes)) {
				static bool PushesDamageClass(ILContext il, Instruction instruction) {
					if (instruction.MatchLdarg(out int arg)) return il.Method.Parameters[arg].ParameterType.FullName == il.Import(typeof(DamageClass)).FullName;
					if (instruction.MatchLdloc(out int loc)) return il.Body.Variables[loc].VariableType.FullName == il.Import(typeof(DamageClass)).FullName;
					if (instruction.MatchCallOrCallvirt(out MethodReference method)) return method.ReturnType.FullName == il.Import(typeof(DamageClass)).FullName;
					if (instruction.MatchLdfld(out FieldReference field) || instruction.MatchLdsfld(out field)) return field.FieldType.FullName == il.Import(typeof(DamageClass)).FullName;
					return false;
				}
				static void FixMethods(ILContext il) {
					ILCursor c = new(il);
					while (c.TryGotoNext(MoveType.Before,
						i => i.MatchCallOrCallvirt<object>(nameof(ToString)) && PushesDamageClass(il, i.Previous)
					)) {
						c.Remove();
						c.EmitCallvirt(typeof(ModType).GetMethod("get_" + nameof(ModType.FullName)));
					}
				}
				foreach (MethodInfo method in coloredDamageTypes.GetType().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
					MonoModHooks.Modify(method, FixMethods);
				}
				foreach (MethodInfo method in coloredDamageTypes.Code.GetType("ColoredDamageTypes.DamageTypes")?.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) ?? []) {
					MonoModHooks.Modify(method, FixMethods);
				}
				//MonoModHooks.Modify(coloredDamageTypes.GetType().GetMethod("LoadModdedDamageTypes", BindingFlags.Public | BindingFlags.Static), FixMethods);
			}
		}
		static Func<bool> HolidayLibCheckAprilFools(Mod HolidayLib) => (Func<bool>)HolidayLib.Call("GETACTIVELOOKUP", "April fools");
		static bool DefaultCheckAprilFools() => (DateTime.Today.Month == 4 && DateTime.Today.Day == 1) || OriginSystem.Instance.ForceAF;
		public static void PostSetupContent(Mod mod) {
			if (ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist)) {
				static Func<bool> IfEvil<T>() where T : AltBiome {
					AltBiome biome = ModContent.GetInstance<T>();
					return () => Main.drunkWorld || WorldBiomeManager.GetWorldEvil(true) == biome || ModLoader.HasMod("BothEvils");
				}
				bossChecklist.Call("LogBoss",
					mod,
					nameof(Defiled_Amalgamation).Replace("_", ""),
					3f,
					() => NPC.downedBoss2,
					ModContent.NPCType<Defiled_Amalgamation>(),
					new Dictionary<string, object> {
						["availability"] = IfEvil<Defiled_Wastelands_Alt_Biome>(),
						["spawnInfo"] = Language.GetOrRegister("Mods.Origins.NPCs.Defiled_Amalgamation.BossChecklistIntegration.SpawnCondition"),
						["spawnItems"] = ModContent.ItemType<Nerve_Impulse_Manipulator>(),
						["collectibles"] = new List<int> {
							RelicTileBase.ItemType<Defiled_Amalgamation_Relic>(),
							TrophyTileBase.ItemType<Defiled_Amalgamation_Trophy>(),
							ModContent.ItemType<Defiled_Amalgamation_Mask>(),
							ModContent.ItemType<Blockus_Tube>(),
						}
					}
				);
				Asset<Texture2D> wcHeadTexture = ModContent.Request<Texture2D>(typeof(World_Cracker_Head).GetDefaultTMLName());
				Asset<Texture2D> wcBodyTexture = ModContent.Request<Texture2D>(typeof(World_Cracker_Body).GetDefaultTMLName());
				Asset<Texture2D> wcTailTexture = ModContent.Request<Texture2D>(typeof(World_Cracker_Tail).GetDefaultTMLName());
				Asset<Texture2D> wcArmorTexture = ModContent.Request<Texture2D>("Origins/NPCs/Riven/World_Cracker/World_Cracker_Armor");
				bossChecklist.Call("LogBoss",
					mod,
					"WorldCracker",
					3f,
					() => NPC.downedBoss2,
					new List<int> { ModContent.NPCType<World_Cracker_Head>(), ModContent.NPCType<World_Cracker_Body>(), ModContent.NPCType<World_Cracker_Tail>() },
					new Dictionary<string, object> {
						["availability"] = IfEvil<Riven_Hive_Alt_Biome>(),
						["spawnItems"] = ModContent.ItemType<Sus_Ice_Cream>(),
						["spawnInfo"] = Language.GetOrRegister("Mods.Origins.NPCs.World_Cracker_Head.BossChecklistIntegration.SpawnCondition"),
						["collectibles"] = new List<int> {
							RelicTileBase.ItemType<World_Cracker_Relic>(),
							TrophyTileBase.ItemType<World_Cracker_Trophy>(),
							ModContent.ItemType<World_Cracker_Mask>(),
							ModContent.ItemType<Fleshy_Globe>(),
						},
						["customPortrait"] = (SpriteBatch spriteBatch, Rectangle area, Color color) => {
							void DrawSegment(Rectangle frame, Vector2 position, Texture2D baseTexture, int @switch) {
								switch (@switch) {
									case 0:
									spriteBatch.Draw(
										baseTexture,
										position,
										null,
										color,
										MathHelper.PiOver2,
										baseTexture.Size() * 0.5f,
										1,
										0,
									0);
									break;
									case 1:
									Vector2 halfSize = frame.Size() / 2;
									spriteBatch.Draw(
										wcArmorTexture.Value,
										position,
										frame,
										color,
										MathHelper.PiOver2,
										halfSize,
										1,
										0,
									0);
									break;
								}
							}
							Vector2 center = area.Center();
							Vector2 diff = new(0, 48);
							for (int j = 0; j < 2; j++) {
								DrawSegment(new Rectangle(168, 0, 52, 56), center + diff * 3, wcTailTexture.Value, j);
								for (int i = 3; i-- > -2;) {
									DrawSegment(new Rectangle(104, 60 * Math.Abs(i % 2), 62, 58), center + diff * i, wcBodyTexture.Value, j);
								}
								DrawSegment(new Rectangle(0, 0, 102, 58), center + diff * -3, wcHeadTexture.Value, j);
							}
						}
					}
				);
				Asset<Texture2D> fwTexture = ModContent.Request<Texture2D>("Origins/UI/Fiberglass_Weaver_Preview");
				bossChecklist.Call("LogBoss",
					mod,
					nameof(Fiberglass_Weaver).Replace("_", ""),
					2.1f,
					() => Boss_Tracker.Instance.downedFiberglassWeaver,
					ModContent.NPCType<Fiberglass_Weaver>(),
					new Dictionary<string, object> {
						["spawnInfo"] = Language.GetOrRegister("Mods.Origins.NPCs.Fiberglass_Weaver.BossChecklistIntegration.SpawnCondition"),
						["spawnItems"] = ModContent.ItemType<Shaped_Glass>(),
						["collectibles"] = new List<int> {
							RelicTileBase.ItemType<Fiberglass_Weaver_Relic>(),
							TrophyTileBase.ItemType<Fiberglass_Weaver_Trophy>(),
							ModContent.ItemType<Fiberglass_Weaver_Head>()
						},
						["customPortrait"] = (SpriteBatch spriteBatch, Rectangle area, Color color) => {
							SpriteBatchState state = spriteBatch.GetState();
							spriteBatch.Restart(state, samplerState: SamplerState.PointClamp);
							try {
								spriteBatch.Draw(fwTexture.Value, area.Center(), null, color, 0, fwTexture.Size() * 0.5f, 2, SpriteEffects.None, 0);
							} finally {
								spriteBatch.Restart(state);
							}
						}
					}
				);
				Asset<Texture2D> ldTexture = ModContent.Request<Texture2D>("Origins/NPCs/Brine/Boss/Rock_Bottom");
				bossChecklist.Call("LogBoss",
					mod,
					nameof(Lost_Diver).Replace("_", ""),
					7.3f,
					() => Boss_Tracker.Instance.downedLostDiver,
					new List<int> {
						ModContent.NPCType<Lost_Diver>(),
						ModContent.NPCType<Lost_Diver_Transformation>(),
						ModContent.NPCType<Mildew_Carrion>()
					},
					new Dictionary<string, object> {
						["spawnInfo"] = Language.GetOrRegister("Mods.Origins.NPCs.Lost_Diver.BossChecklistIntegration.SpawnCondition"),
						["spawnItems"] = ModContent.ItemType<Lost_Picture_Frame>(),
						["collectibles"] = new List<int> {
							RelicTileBase.ItemType<Lost_Diver_Relic>(),
							TrophyTileBase.ItemType<Lost_Diver_Trophy>(),
							ModContent.ItemType<Lost_Diver_Helmet>(),
							ModContent.ItemType<Lost_Diver_Chest>(),
							ModContent.ItemType<Lost_Diver_Greaves>()
						},
						["overrideHeadTextures"] = ModContent.GetInstance<Lost_Diver>().BossHeadTexture,
						["customPortrait"] = (SpriteBatch spriteBatch, Rectangle area, Color color) => {
							SpriteBatchState state = spriteBatch.GetState();
							spriteBatch.Restart(state, samplerState: SamplerState.PointClamp);
							try {
								spriteBatch.Draw(ldTexture.Value, area.Center(), ldTexture.Value.Frame(1, 15, 0, 4), color, 0, ldTexture.Value.Frame(1, 15, 0, 4).Size() * 0.5f, 2, SpriteEffects.None, 0);
							} finally {
								spriteBatch.Restart(state);
							}
						}
					}
				);
			}
			if (ModLoader.TryGetMod("Fargowiltas", out instance.fargosMutant)) {
				FargosMutant.Call("AddSummon", 3, ModContent.ItemType<Nerve_Impulse_Manipulator>(), () => NPC.downedBoss2, Item.buyPrice(gold: 10));
				FargosMutant.Call("AddSummon", 3, ModContent.ItemType<Sus_Ice_Cream>(), () => NPC.downedBoss2, Item.buyPrice(gold: 10));
				FargosMutant.Call("AddSummon", 2.1, ModContent.ItemType<Shaped_Glass>(), () => Boss_Tracker.Instance.downedFiberglassWeaver, Item.buyPrice(gold: 8));
				FargosMutant.Call("AddSummon", 7.3, ModContent.ItemType<Lost_Picture_Frame>(), () => Boss_Tracker.Instance.downedLostDiver, Item.buyPrice(gold: 22));
			}

			void AddModdedNPCAssimilation<TDebuff>(string name, AssimilationAmount assimilationAmount, HashSet<int> set = null) where TDebuff : AssimilationDebuff {
				if (NPCID.Search.TryGetId(name, out int id)) {
					set?.Add(id);
					if (assimilationAmount != default) AssimilationLoader.AddNPCAssimilation<TDebuff>(id, assimilationAmount);
				}
			}
			void AddModdedProjectileAssimilation<TDebuff>(string name, AssimilationAmount assimilationAmount, string creditNPC) where TDebuff : AssimilationDebuff {
				if (ProjectileID.Search.TryGetId(name, out int id)) {
					AssimilationLoader.AddProjectileAssimilation<TDebuff>(id, assimilationAmount);
					if (NPCID.Search.TryGetId(creditNPC, out id)) {
						BiomeNPCGlobals.assimilationDisplayOverrides.TryAdd(id, []);
						BiomeNPCGlobals.assimilationDisplayOverrides[id][ModContent.GetInstance<TDebuff>().AssimilationType] = assimilationAmount;
					}
				}
			}
			void AddModdedDebuffAssimilation<TDebuff>(string name, AssimilationAmount assimilationAmount) where TDebuff : AssimilationDebuff {
				if (BuffID.Search.TryGetId(name, out int id)) {
					AssimilationLoader.AddProjectileAssimilation<TDebuff>(id, assimilationAmount);
				}
			}
			AddModdedProjectileAssimilation<Corrupt_Assimilation>("ThoriumMod/VileSpit", 0.06f, "ThoriumMod/VileFloater");
			AddModdedProjectileAssimilation<Corrupt_Assimilation>("ThoriumMod/VileSpitCold", 0.05f, "ThoriumMod/ChilledSpitter");
			AddModdedNPCAssimilation<Corrupt_Assimilation>("SpiritMod/Masticator", 0.06f, CorruptGlobalNPC.NPCTypes);
			AddModdedProjectileAssimilation<Corrupt_Assimilation>("SpiritMod/CorruptVomitProj", 0.08f, "SpiritMod/Masticator");
			AddModdedNPCAssimilation<Corrupt_Assimilation>("SpiritMod/Vilemoth", 0.04f, CorruptGlobalNPC.NPCTypes);
			AddModdedNPCAssimilation<Corrupt_Assimilation>("SpiritMod/VileWasp", 0.04f, CorruptGlobalNPC.NPCTypes);
			AddModdedNPCAssimilation<Corrupt_Assimilation>("SpiritMod/Teratoma", 0.05f, CorruptGlobalNPC.NPCTypes);
			AddModdedProjectileAssimilation<Corrupt_Assimilation>("SpiritMod/TeratomaProj", 0.07f, "SpiritMod/Teratoma");
			AddModdedNPCAssimilation<Corrupt_Assimilation>("SpiritMod/PurpleClubberfish", 0.05f, CorruptGlobalNPC.NPCTypes);
			AddModdedNPCAssimilation<Corrupt_Assimilation>("SpiritMod/Toxikarp", 0.03f, CorruptGlobalNPC.NPCTypes);
			AddModdedDebuffAssimilation<Corrupt_Assimilation>("SpiritMod/Crystallization", 0.01f / 60f);

			AddModdedNPCAssimilation<Crimson_Assimilation>("SpiritMod/Spewer", default, CrimsonGlobalNPC.NPCTypes);
			AddModdedProjectileAssimilation<Crimson_Assimilation>("SpiritMod/VomitProj", 0.08f, "SpiritMod/Spewer");
			AddModdedNPCAssimilation<Crimson_Assimilation>("SpiritMod/CrimsonTrapper", default, CrimsonGlobalNPC.NPCTypes);
			AddModdedProjectileAssimilation<Crimson_Assimilation>("SpiritMod/ArterialBloodClump", 0.08f, "SpiritMod/CrimsonTrapper");
			AddModdedNPCAssimilation<Crimson_Assimilation>("SpiritMod/Bladetongue", default, CrimsonGlobalNPC.NPCTypes);
		}
		public static void LateLoad() {
			if (ModLoader.TryGetMod("ControllerConfigurator", out Mod controllerConfigurator) && controllerConfigurator.Call("GETGOTOKEYBINDKEYBIND") is ModKeybind keybind) {
				instance.goToKeybindKeybind = keybind;
				instance.goToKeybind = (keybind) => controllerConfigurator.Call("OPENKEYBINDSTOSEARCH", keybind);
			}
			if (ModLoader.TryGetMod("GlobalLootViewer", out Mod globalLootViewer)) {
				globalLootViewer.Call("IgnoreConditionWhenHighLighting", DropConditions.PlayerInteraction);
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

			if (ModLoader.TryGetMod("CalamityOverhaul", out Mod calamityOverhaul)) {
				MethodInfo HasPwoerEffect = calamityOverhaul.Code.GetType("CalamityOverhaul.Common.Effects.EffectLoader")?.GetMethod("HasPwoerEffect", BindingFlags.NonPublic | BindingFlags.Instance);
				if (HasPwoerEffect is not null) {
					MonoModHooks.Modify(HasPwoerEffect, il => {
						ILCursor c = new(il);
						ILLabel label = c.MarkLabel();
						c.MoveBeforeLabels();
						c.EmitDelegate(() => Main.gameMenu);
						c.EmitBrfalse(label);
						c.EmitLdcI4(0);
						c.EmitRet();
					});
				}
			}
			if (ModLoader.TryGetMod("Aequus", out Mod aequus)) {
				MethodInfo GetTotalStats = aequus.Code.GetType("Aequus.Common.DamageClasses.DamageClassStatFloat")?.GetMethod("GetTotalStats", BindingFlags.Public | BindingFlags.Instance, [typeof(DamageClass)]);
				if (GetTotalStats is not null) {
					MonoModHooks.Modify(GetTotalStats, il => {
						ILCursor c = new(il);
						if(c.TryGotoNext(MoveType.Before, 
							i => i.MatchLdsfld(aequus.Code.GetType("Aequus.Common.DamageClasses.AequusDamageClasses"), "DamageClasses")
						)) {
							c.Remove();
							c.Index++;
							c.Remove();
							c.EmitCall(typeof(DamageClassLoader).GetMethod(nameof(DamageClassLoader.GetDamageClass)));
						}
					});
				}
			}
			if (ModLoader.TryGetMod("SpiritMod", out Mod spiritMod)) {
				if (spiritMod.Code.GetType("SpiritMod.GlobalClasses.Items.GlyphGlobalItem")?.GetMethod("CanBeAppliedTo", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) is MethodInfo CanBeAppliedTo) {
					MonoModHooks.Add(CanBeAppliedTo, (Func<Player, Item, bool> orig, Player player, Item item) => {
						if (PrefixLoader.GetPrefix(item.prefix) is ICanReforgePrefix canReforgePrefix && !canReforgePrefix.CanReforge(item)) return false;
						return orig(player, item);
					});
				}
			}
			if (ModLoader.HasMod("ferventarms")) compatRecommendations.Add(Language.GetText("Mods.Origins.ModCompatNotes.FerventArms"));
			if (ModLoader.TryGetMod("Munchies", out Mod munchies)) {
				munchies.Call("AddSingleConsumable",
					Origins.instance,
					"1.4.2",
					ModContent.GetInstance<Mojo_Injection>(),
					"player",
					() => Main.LocalPlayer.OriginPlayer().mojoInjection
				);
				munchies.Call("AddSingleConsumable",
					Origins.instance,
					"1.4.2",
					ModContent.GetInstance<Crown_Jewel>(),
					"player",
					() => Main.LocalPlayer.OriginPlayer().crownJewel
				);
			}
		}
		public static void AddRecipes() {
			if (instance.thorium is not null) AddThoriumRecipes();
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
			MonoModHooks.Add(
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
			);
			if (typeof(ThoriumPlayer).GetField(nameof(ThoriumPlayer.breathOverMax)) is not null) OriginExtensions.OnIncreaseMaxBreath += [JITWhenModsEnabled("ThoriumMod")] (player, _) => player.GetModPlayer<ThoriumMod.ThoriumPlayer>().breathOverMax = true;
			(string name, float? assimilation)[] thoriumNPCs = [
				("TheInnocent", 0.02f),
				("FrostWormHead", null),
				("FrostWormBody", null),
				("FrostWormTail", null),
				("SnowEater", 0.03f),
				("TheStarved", 0.10f),
				("HorrificCharger", 0.02f),
				("VileFloater", null),
				("ChilledSpitter", null),
				("Freezer", 0.09f),
				("SoulCorrupter", null),
			];
			for (int i = 0; i < thoriumNPCs.Length; i++) {
				(string name, float? assimilation) = thoriumNPCs[i];
				if (Thorium.TryFind(name, out ModNPC npc)) {
					CorruptGlobalNPC.NPCTypes.Add(npc.Type);
					if (assimilation.HasValue) AssimilationLoader.AddNPCAssimilation<Corrupt_Assimilation>(npc.Type, assimilation.Value);
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
				("Blister", 0.04f),
				("Coldling", null),
				("FrozenGross", null),
				("EpiDermon", null)
			];
			for (int i = 0; i < thoriumNPCs.Length; i++) {
				(string name, float? assimilation) = thoriumNPCs[i];
				if (Thorium.TryFind(name, out ModNPC npc)) {
					CrimsonGlobalNPC.NPCTypes.Add(npc.Type);
					if (assimilation.HasValue) AssimilationLoader.AddNPCAssimilation<Crimson_Assimilation>(npc.Type, assimilation.Value);
				} else {
					Origins.LogError($"Could not find npc \"{name}\" in Thorium");
				}
			}
		}
		[JITWhenModsEnabled("ThoriumMod")]
		static void AddThoriumRecipes() {
			Recipe.Create(ModContent.ItemType<Asylum_Whistle>())
			.AddIngredient<aDarksteelAlloy>(15)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ModContent.ItemType<Bomb_Handling_Device>())
			.AddIngredient<aDarksteelAlloy>(15)
			.AddTile(TileID.Anvils)
			.Register();
		}
		[JITWhenModsEnabled("Fargowiltas")]
		static void SetupFargos() {
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
	public interface IBardDamageClassOverride {
		DamageClass DamageType { get; }
	}
}
