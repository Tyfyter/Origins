using AltLibrary;
using AltLibrary.Common.AltBiomes;
using AltLibrary.Common.Systems;
using AltLibrary.Core.Generation;
using CalamityMod.Prefixes;
using ModLiquidLib.ModLoader;
using Origins.Backgrounds;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Mounts.Star_Soldier;
using Origins.Items.Other.Fish;
using Origins.Items.Pets;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Liquids;
using Origins.NPCs.Ashen;
using Origins.Reflection;
using Origins.Tiles;
using Origins.Tiles.Ashen;
using Origins.Tiles.Ashen.Hanging_Scrap;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.Walls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Personalities;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.WorldBuilding;
using static Origins.OriginExtensions;
using static Terraria.ModLoader.ModContent;
using static Terraria.WorldGen;

namespace Origins.World.BiomeData {
	public class Ashen_Biome : ModBiome, IItemObtainabilityProvider {
		public static IItemDropRule FirstOrbDropRule;
		public static IItemDropRule OrbDropRule;
		public override int Music => Origins.Music.AshenScrapyard;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconEvilAshen";
		public override string BackgroundPath => "Origins/UI/MapBGs/Ashen_Scrapyard";
		public override string MapBackground => BackgroundPath;
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => GetInstance<Ashen_Surface_Background>();
		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => BiomeUGBackground<Riven_Underground_Background>();
		public override int BiomeTorchItemType => ItemType<Ashen_Torch>();
		public override int BiomeCampfireItemType => ItemType<Ashen_Campfire_Item>();
		public static bool forcedBiomeActive;
		public override bool IsBiomeActive(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.ZoneAshenProgress = Math.Min(OriginSystem.ashenTiles - (NeededTiles - ShaderTileCount), ShaderTileCount) / ShaderTileCount;
			LinearSmoothing(ref originPlayer.ZoneAshenProgressSmoothed, originPlayer.ZoneAshenProgress, OriginSystem.biomeShaderSmoothing * 0.1f);

			return IsActive;
		}
		public static bool IsActive => OriginSystem.ashenTiles >= NeededTiles;
		public override void SpecialVisuals(Player player, bool isActive) {
			//OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			//Filters.Scene["Origins:ZoneAshen"].GetShader().UseProgress(originPlayer.ZoneAshenProgressSmoothed);
			//player.ManageSpecialBiomeVisuals("Origins:ZoneAshen", originPlayer.ZoneAshenProgressSmoothed > 0, player.Center);

			if (SkyManager.Instance["Origins:ZoneAshen"] != null && isActive != SkyManager.Instance["Origins:ZoneAshen"].IsActive()) {
				if (isActive)
					SkyManager.Instance.Activate("Origins:ZoneAshen", player.Center);
				else
					SkyManager.Instance["Origins:ZoneAshen"].Deactivate();
			}
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneAshenProgress * 0.98f;
		}
		public override void Load() {
			FirstOrbDropRule = ItemDropRule.Common(ItemType<Neural_Network>());
			FirstOrbDropRule.OnSuccess(ItemDropRule.ByCondition(DropConditions.NotFromItems, ItemID.MusketBall, 1, 100, 100));

			IItemDropRule AceShrapnelRule = ItemDropRule.NotScalingWithLuck(ItemType<Ace_Shrapnel>());
			AceShrapnelRule.OnSuccess(ItemDropRule.ByCondition(DropConditions.NotFromItems, ItemType<Scrap>(), 1, 200, 200));

			OrbDropRule = new OneFromRulesRule(1,
				FirstOrbDropRule,
				AceShrapnelRule,
				ItemDropRule.NotScalingWithLuck(ItemType<Area_Denial>()),
				ItemDropRule.NotScalingWithLuck(ItemType<Smiths_Hammer>()),
				ItemDropRule.NotScalingWithLuck(ItemType<Seal_Of_Cinders>())
			);
		}
		public override void Unload() {
			FirstOrbDropRule = null;
			OrbDropRule = null;
		}
		public IEnumerable<int> ProvideItemObtainability() {
			List<DropRateInfo> drops = [];
			OrbDropRule.ReportDroprates(drops, new DropRateInfoChainFeed(1));
			return drops.Select(d => d.itemId);
		}

		public const int NeededTiles = 200;
		public const int ShaderTileCount = 25;
		public const short DefaultTileDust = DustID.Lihzahrd;
		public class SpawnRates : SpawnPool {
			public const float PowerZombie = 0.8f;
			public const float Mummy = 1;
			public const float Ghoul = 2;
			public const float Mimic = 0.01f;
			public const float CursedWeapon = 0.01f;
			public const float ScrapyardStryder = 0.01f;
			public const float Quakemaker = 0.03f;
			public const float Watcher = 0.03f;
			public override string Name => $"{nameof(Ashen_Biome)}_{base.Name}";
			public override void SetStaticDefaults() {
				Priority = SpawnPoolPriority.BiomeHigh;
				static float DesertCave(NPCSpawnInfo spawnInfo) => spawnInfo.DesertCave && Main.hardMode ? 1 : 0;
				AddSpawn(NPCID.DesertDjinn, DesertCave);
				AddSpawn(NPCID.DesertLamiaDark, DesertCave);
				AddSpawn(NPCID.DesertBeast, DesertCave);
				static Func<NPCSpawnInfo, float> MimicRate(float rate) => (spawnInfo) => {
					if (Main.hardMode && !spawnInfo.PlayerSafe && spawnInfo.SpawnTileY > Main.rockLayer && !spawnInfo.DesertCave) return rate;
					return 0;
				};
				AddSpawn(NPCType<Trash_Compactor_Mimic>(), MimicRate(Mimic));
				AddSpawn(NPCType<Malfunctioning_Missile>(), MimicRate(CursedWeapon));
			}
			public static float LandEnemyRate(NPCSpawnInfo spawnInfo, bool hardmode = false) {
				if (hardmode && !Main.hardMode) return 0f;
				return 1f;
			}
			public static float FlyingEnemyRate(NPCSpawnInfo spawnInfo, bool hardmode = false) {
				return LandEnemyRate(spawnInfo, hardmode);
			}
			public override bool IsActive(NPCSpawnInfo spawnInfo) {
				if (Main.WindyEnoughForKiteDrops) return false;
				return (TileLoader.GetTile(spawnInfo.SpawnTileType) is IAshenTile ashenTile && ashenTile.CountsForSpawns(spawnInfo)) || forcedBiomeActive;
			}
		}
		public static class Gen {
		}
	}
	#region variations
	public class Underground_Ashen_Biome : ModBiome {
		public override int Music => Origins.Music.AshenMines;
		public override string BackgroundPath => "Origins/UI/MapBGs/Ashen_Mines";
		public override string BestiaryIcon => "Origins/UI/IconStonerAshen";
		public override string MapBackground => BackgroundPath;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && Ashen_Biome.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneAshenProgress * 0.99f;
		}
	}
	public class Ashen_Desert : ModBiome {
		public override int Music => Origins.Music.AshenScrapyard;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => GetInstance<Ashen_Desert_Background>();
		public override string BackgroundPath => "Origins/UI/MapBGs/Ashen_Desert";
		public override string BestiaryIcon => "Origins/UI/IconDesertAshen";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneDesert && Ashen_Biome.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneAshenProgress * 0.99f;
		}
	}
	public class Ashen_Underground_Desert : ModBiome {
		public override int Music => Origins.Music.AshenMines;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Ashen_Desert";
		public override string BestiaryIcon => "Origins/UI/IconCatacombsAshen";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && player.ZoneDesert && Ashen_Biome.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneAshenProgress;
		}
	}
	public class Ashen_Ice_Biome : ModBiome {
		public override int Music => Origins.Music.AshenMines;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Ashen_Snow";
		public override string BestiaryIcon => "Origins/UI/IconSnowAshen";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneRockLayerHeight && player.ZoneSnow && Ashen_Biome.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.GetModPlayer<OriginPlayer>().ZoneAshenProgress;
		}
	}
	public class Ashen_Ocean : ModBiome {
		public override int Music => Origins.Music.AshenScrapyard;
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override string BackgroundPath => "Origins/UI/MapBGs/Ashen_Scrapyard";
		public override string BestiaryIcon => "Origins/UI/WorldGen/IconEvilAshen";
		public override string MapBackground => BackgroundPath;
		public override bool IsBiomeActive(Player player) {
			return player.ZoneBeach && Ashen_Biome.IsActive;
		}
		public override float GetWeight(Player player) {
			return player.OriginPlayer().ZoneAshenProgress * 1f;
		}
	}
	#endregion variations
	public class Ashen_Alt_Biome : AltBiome {
		public override string WorldIcon => "Origins/UI/WorldGen/IconAshen";
		public override string OuterTexture => "Origins/UI/WorldGen/Outer_Ashen";
		public override string IconSmall => "Origins/UI/WorldGen/IconEvilAshen";
		public override Color OuterColor => new(255, 170, 170);
		public override Color NameColor => new(255, 100, 100);
		public override Color? BiomeSightColor => Color.OrangeRed;
		public override IShoppingBiome Biome => GetInstance<Ashen_Biome>();
		public override void SetStaticDefaults() {
			BiomeType = BiomeType.Evil;

			AddTileConversion(TileType<Ashen_Grass>(), TileID.Grass);
			AddTileConversion(TileType<Ashen_Jungle_Grass>(), TileID.JungleGrass);
			AddTileConversion(TileType<Tainted_Stone>(), TileID.Stone);
			AddTileConversion(TileType<Sootsand>(), TileID.Sand);
			AddTileConversion(TileType<Soot_Sandstone>(), TileID.Sandstone);
			AddTileConversion(TileType<Hardened_Sootsand>(), TileID.HardenedSand);
			AddTileConversion(TileType<Brown_Ice>(), TileID.IceBlock);

			CreateGrassType(new(true, true),
				(TileID.Dirt, TileType<Ashen_Grass>()),
				(TileID.Mud, TileType<Ashen_Jungle_Grass>()),
				(TileType<Murky_Sludge>(), TileType<Ashen_Murky_Sludge_Grass>())
			);

			BiomeFlesh = TileID.AncientGoldBrick;
			BiomeFleshWall = WallID.AncientGoldBrickWall;

			SeedType = ItemType<Ashen_Grass_Seeds>();
			BiomeOre = TileType<Sanguinite_Ore>();
			BiomeOreItem = ItemType<Sanguinite_Ore_Item>();
			BiomeOreBrick = TileType<Sanguinite_Brick>();
			AltarTile = TileType<Ashen_Altar>();

			BiomeChestItem = ItemType<Star_Soldier_Summon_Item>();
			BiomeChestTile = TileType<Ashen_Dungeon_Chest>();
			BiomeChestTileStyle = 1;
			BiomeKeyItem = ItemType<Ashen_Key>();

			MimicType = NPCType<Trash_Compactor_Mimic>();

			BloodBunny = NPCType<Springjumper>();
			BloodPenguin = NPCType<Robot_Penguin>();
			BloodGoldfish = NPCType<Scraptooth>();

			this.AddOriginsWallConversions<Tainted_Stone_Wall>(WallVersion.Natural,
				WallID.Cave7Unsafe,
				WallID.CaveUnsafe,
				WallID.Cave2Unsafe,
				WallID.Cave3Unsafe,
				WallID.Cave4Unsafe,
				WallID.Cave5Unsafe,
				WallID.Cave6Unsafe,
				WallID.Cave8Unsafe,
				WallID.EbonstoneUnsafe,
				WallID.CorruptionUnsafe1,
				WallID.CorruptionUnsafe2,
				WallID.CorruptionUnsafe3,
				WallID.CorruptionUnsafe4,
				WallID.CrimstoneUnsafe,
				WallID.CrimsonUnsafe1,
				WallID.CrimsonUnsafe2,
				WallID.CrimsonUnsafe3,
				WallID.CrimsonUnsafe4,
				WallID.Stone
			);
			AddWallConversions<Soot_Sandstone_Wall>(
				WallID.Sandstone,
				WallID.CorruptSandstone,
				WallID.CrimsonSandstone,
				WallID.HallowSandstone
			);
			AddWallConversions<Hardened_Sootsand_Wall>(
				WallID.HardenedSand,
				WallID.CorruptHardenedSand,
				WallID.CrimsonHardenedSand,
				WallID.HallowHardenedSand
			);
			AddWallConversions(OriginsWall.GetWallID<Ashen_Grass_Wall>(WallVersion.Natural),
				WallID.GrassUnsafe,
				WallID.Grass
			);
			this.AddChambersiteTileConversions(Chambersite_Ore.GetOreID(TileType<Tainted_Stone>()));

			EvilBiomeGenerationPass = new Ashen_Generation_Pass();
		}
		public override AltMaterialContext MaterialContext {
			get {
				AltMaterialContext context = new();
				context.SetEvilSword(ItemType<Switchblade_Broadsword>());
				context.SetEvilOre(ItemType<Sanguinite_Ore_Item>());
				context.SetEvilBar(ItemType<Sanguinite_Bar>());
				context.SetEvilHerb(ItemType<Surveysprout_Item>());
				context.SetVileComponent(ItemType<Phoenum>());
				context.SetVileInnard(ItemType<Biocomponent10>());
				context.SetEvilBossDrop(ItemType<NE8>());
				return context;
			}
		}
		public class Ashen_Generation_Pass : EvilBiomeGenerationPass {
			public static List<Rectangle> scrapyards = [];
			public override string ProgressMessage => Language.GetTextValue("Mods.Origins.AltBiomes.Ashen_Alt_Biome.GenPassName");
			public override void GenerateEvil(int evilBiomePosition, int evilBiomePositionWestBound, int evilBiomePositionEastBound) {
				WorldBiomeGeneration.ChangeRange.ResetRange();

				ushort stoneType = (ushort)TileType<Tainted_Stone>();
				ushort stoneWallType = (ushort)OriginsWall.GetWallID<Tainted_Stone_Wall>(WallVersion.Natural);

				ushort sludgeType = (ushort)TileType<Murky_Sludge>();
				for (int i = evilBiomePositionWestBound; i < evilBiomePositionEastBound; i++) {
					int top = 0;
					bool setTop = false;
					int sludgeDepth = genRand.Next(7, 16);
					for (int j = (int)GenVars.worldSurfaceLow; j < GenVars.worldSurfaceHigh + 32; j++) {
						Tile tile = Main.tile[i, j];
						if (tile.HasTile && !(TileID.Sets.BreakableWhenPlacing[tile.TileType] || Main.tileCut[tile.TileType])) {
							if (tile.TileType == TileID.Trees) {
								OriginSystem.RemoveTree(i, j);
								continue;
							}
							AreaAnalysis analysis = AreaAnalysis.March(i, j, AreaAnalysis.Orthogonals, AreaAnalysis.HasFullSolidTile, analysis => analysis.Counted.Count > 200);
							if (!analysis.Broke) {
								for (int k = 0; k < analysis.Counted.Count; k++) {
									tile = Main.tile[analysis.Counted[k]];
									tile.HasTile = false;
								}
								continue;
							}
							if (setTop.TrySet(true)) top = j;
							else if (j >= top + sludgeDepth) break;
							tile.TileType = sludgeType;
							WorldBiomeGeneration.ChangeRange.AddChangeToRange(i, j);
						} else setTop = false;
					}
					for (int j = (int)GenVars.worldSurfaceLow; j < GenVars.worldSurfaceHigh + 32; j++) {
						Tile tile = Main.tile[i, j];
						tile.WallType = WallID.None;
						if (tile.HasTile && !(TileID.Sets.BreakableWhenPlacing[tile.TileType] || Main.tileCut[tile.TileType])) break;
					}
				}

				Rectangle range = WorldBiomeGeneration.ChangeRange.GetRange();
				bool anyTiles;
				int extendedMinY = range.Top;
				do {
					anyTiles = false;
					extendedMinY--;
					for (int i = range.Left; i < range.Right && !anyTiles; i++) {
						anyTiles = Framing.GetTileSafely(i, extendedMinY).HasTile;
					}
				} while (anyTiles);
				extendedMinY++;
				WorldBiomeGeneration.ChangeRange.AddChangeToRange(evilBiomePosition, extendedMinY);
				range = WorldBiomeGeneration.ChangeRange.GetRange();

				WorldBiomeGeneration.EvilBiomeGenRanges.Add(range);
				AltBiome biome = GetInstance<Ashen_Alt_Biome>();
				ushort grass = (ushort)TileType<Ashen_Grass>();
				for (int i = range.Left; i < range.Right; i++) {
					int slopeFactor = Math.Min(Math.Min(i - range.Left, range.Right - i), 99);
					for (int j = range.Top - 10; j < range.Bottom; j++) {
						if (genRand.NextBool(5) && genRand.Next(slopeFactor, 100) < 20) continue;
						if (range.Bottom - j < 5 && genRand.NextBool(5)) break;
						Tile tile = Framing.GetTileSafely(i, j);
						AltLibrary.Core.ALConvert.Convert(biome, i, j, 0);
						if (tile.TileType == TileID.Dirt && (!Framing.GetTileSafely(i - 1, j).HasTile || !Framing.GetTileSafely(i + 1, j).HasTile || !Framing.GetTileSafely(i, j - 1).HasTile || !Framing.GetTileSafely(i, j + 1).HasTile)) {
							tile.TileType = grass;
						}
					}
				}
				OriginSystem.Instance.hasAshen = true;
				scrapyards.Add(range);
			}

			public override void PostGenerateEvil() { }
		}
		public override void ModifyGenPass(List<GenPass> passes, GenPass originalPass) {
			if (originalPass.Name == "Lakes") {
				Ashen_Generation_Pass.scrapyards.Clear();
				passes.Add(new PassLegacy("Scrap and Oil", (_, _) => {
					for (int i = 0; i < Ashen_Generation_Pass.scrapyards.Count; i++) ScrapyardPass2(Ashen_Generation_Pass.scrapyards[i]);
				}));
			}
			static void ScrapyardPass2(Rectangle scrapyard) {
				ushort sludgeType = (ushort)TileType<Murky_Sludge>();
				ushort scrapType = (ushort)TileType<Scrap_Heap>();
				for (int j = 0; j < scrapyard.Height; j++) {
					for (int i = 0; i < scrapyard.Width; i++) {
						Tile tile = Main.tile[i, j];
						if (tile.LiquidAmount > 0) {
							tile.LiquidType = LiquidLoader.LiquidType<Oil>();
							tile = Main.tile[i, j + 1];
							if (tile.HasFullSolidTile()) {
								int sludgeDepth = genRand.Next(7, 16);
								for (int k = 1; k < sludgeDepth; k++) {
									tile = Main.tile[i, j + k];
									if (!tile.HasFullSolidTile()) break;
									tile.TileType = sludgeType;
								}
							}
						}
					}
				}
				int tries = 1000;
				for (int i = 0; i < scrapyard.Width / 50; i++) {
					int x = scrapyard.X + genRand.Next(scrapyard.Width);
					int y = scrapyard.Y + genRand.Next(scrapyard.Height);
					Tile tile = Main.tile[x, y];
					if (!tile.HasTile) {
						if (tries > 0) {
							i--;
							tries--;
						}
						continue;
					}
					while (tile.HasTile) tile = Main.tile[x, --y];
					Vector2 posMin = new(float.PositiveInfinity);
					Vector2 posMax = new(float.NegativeInfinity);
					Carver.Filter[] lemons = new Carver.Filter[genRand.Next(2, 5)];
					for (int k = 0; k < lemons.Length; k++) {
						lemons[k] = Carver.PointyLemon(
							new(x, y),
							scale: genRand.NextFloat(4, 8),
							rotation: genRand.NextFloat(0, MathHelper.Pi),
							aspectRatio: genRand.NextFloat(2, 3),
							roundness: genRand.NextFloat(1, 2),
							ref posMin,
							ref posMax
						);
					}
					Carver.DoCarve(
						Carver.EmptyTile + Carver.Or(lemons),
						pos => {
							Tile tile = Framing.GetTileSafely(pos.ToPoint());
							tile.HasTile = true;
							tile.TileType = scrapType;
							return 0;
						},
						posMin, posMax
					);
				}
				tries = 1000;
				for (int i = 0; i < scrapyard.Width / 10; i++) {
					int x = scrapyard.X + genRand.Next(scrapyard.Width);
					int y = scrapyard.Y + genRand.Next(scrapyard.Height);
					Tile tile = Main.tile[x, y];
					if (!tile.HasTile) {
						if (tries > 0) {
							i--;
							tries--;
						}
						continue;
					}
					while (tile.HasTile) tile = Main.tile[x, --y];
					y++;
					new Hanging_Scrap_Action(new(x, y), new(HangingScrap.GetRandom(genRand), (Half)Main.rand.NextFloat(float.Tau))).Perform();
				}
			}
		}
		public class Ashen_Fishing_Pool : FishingLootPool<Ashen_Alt_Biome> {
			public override bool IsActive(Player player, FishingAttempt attempt) => base.IsActive(player, attempt) && (attempt.BobberInLiquid(LiquidID.Water) || attempt.BobberInLiquid<Oil>());
			public override void SetStaticDefaults() {
				GetInstance<Ashen_Crates>().Add(this);
				Legendary.Add(new SequentialCatches(
					FishingCatch.Item(ItemID.ScalyTruffle, (player, attempt) => Main.hardMode && player.ZoneSnow && attempt.heightLevel == 3 && !Main.rand.NextBool(3))
				));
				Rare.Add(FishingCatch.Item(ItemType<Internal_Combustionfish>()));
				AddQuestFish(ItemType<Scrapfish>());
				Uncommon.Add(FishingCatch.Item(ItemType<Polyeel>()));
			}
		}
	}
}
