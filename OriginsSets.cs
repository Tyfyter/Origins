using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumTiles = ThoriumMod.Tiles;
using OMI = Origins.OriginsModIntegrations;
using System.Linq;
using static Terraria.ModLoader.ModContent;
using Terraria.Audio;

namespace Origins {
	public static class OriginsSets {
		public static void Initialize() {
			Tiles.Initialize();
			Walls.Initialize();
		}
		[ReinitializeDuringResizeArrays]
		public static class Items {
			// not named because it controls a change to vanilla mechanics only present in TO, likely to be moved to PegasusLib
			public static bool[] ItemsThatAllowRemoteRightClick { get; } = ItemID.Sets.Factory.CreateBoolSet();
			// not named because it controls a change to vanilla mechanics only present in TO, likely to be moved to PegasusLib
			public static float[] DamageBonusScale { get; } = ItemID.Sets.Factory.CreateFloatSet(1f);
			public static bool[] FelnumItem { get; } = ItemID.Sets.Factory.CreateBoolSet();
			public static string[] JournalEntries { get; } = ItemID.Sets.Factory.CreateNamedSet($"{nameof(Items)}_{nameof(JournalEntries)}")
			.Description("Controls which items are associated with which journal entries, multiple entries can be assigned by separating them with semicolons")
			.RegisterCustomSet<string>(null);
			public static bool[] SwungNoMeleeMelees { get; } = ItemID.Sets.Factory.CreateNamedSet($"{nameof(Items)}_{nameof(SwungNoMeleeMelees)}")
			.Description("Allows weapons with Item.noMelee to trigger effects meant for swung melee weapons")
			.RegisterBoolSet(
				ItemID.NightsEdge,
				ItemID.TrueNightsEdge,
				ItemID.Excalibur,
				ItemID.TrueExcalibur,
				ItemID.TerraBlade,
				ItemID.TheHorsemansBlade
			);
			public static bool[] ItemsThatCanChannelWithRightClick { get; } = ItemID.Sets.Factory.CreateBoolSet();
			public static bool[] PaintingsNotFromVendor { get; } = ItemID.Sets.Factory.CreateBoolSet();
			public static bool[] InvalidForDefiledPrefix { get; } = ItemID.Sets.Factory.CreateNamedSet(nameof(InvalidForDefiledPrefix))
			.RegisterBoolSet();
			public static bool[] IsGun { get; } = ItemID.Sets.Factory.CreateNamedSet(nameof(IsGun))
			.Description("Allows weapons with non-bullet ammo to trigger effects meant for guns")
			.RegisterBoolSet();
			public static bool[] EvilMaterialAchievement { get; } = ItemID.Sets.Factory.CreateBoolSet(
				ItemID.Deathweed,
				ItemID.VileMushroom,
				ItemID.DemoniteOre,
				ItemID.RottenChunk,
				ItemID.ShadowScale,
				ItemID.CursedFlame,
				ItemID.ViciousMushroom,
				ItemID.CrimtaneOre,
				ItemID.Vertebrae,
				ItemID.TissueSample,
				ItemID.Ichor
			);
			internal static bool[] InfoAccessorySlots_IsAMechanicalAccessory { get; } = ItemID.Sets.Factory.CreateNamedSet("InfoAccessorySlots", "IsAMechanicalAccessory").RegisterBoolSet();
		}
		[ReinitializeDuringResizeArrays]
		public static class Projectiles {
			public static bool[] ForceFelnumShockOnShoot { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(ForceFelnumShockOnShoot))
			.Description("Controls whether Felnum Armor's set bonus will trigger when the projectile is created for projectiles which would otherwise only consume the bonus when they hit an enemy")
			.RegisterBoolSet(false);
			public static float[] HomingEffectivenessMultiplier { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(HomingEffectivenessMultiplier))
			.Description("Controls the effectiveness of compatible homing effects")
			.RegisterFloatSet(1f,
				ProjectileID.ScarabBomb, 0f,
				ProjectileID.StyngerShrapnel, 0f,
				ProjectileID.ClusterFragmentsI, 0f,
				ProjectileID.ClusterFragmentsII, 0f,
				ProjectileID.BloodCloudMoving, 0f,
				ProjectileID.BloodCloudRaining, 0f,
				ProjectileID.RainCloudMoving, 0f,
				ProjectileID.RainCloudRaining, 0f,
				ProjectileID.PrincessWeapon, 0f,
				ProjectileID.ClingerStaff, 0f,
				ProjectileID.VilethornBase, 0f,
				ProjectileID.VilethornTip, 0f,
				ProjectileID.CrystalVileShardShaft, 0f,
				ProjectileID.CrystalVileShardHead, 0f,
				ProjectileID.NettleBurstLeft, 0f,
				ProjectileID.NettleBurstRight, 0f,
				ProjectileID.NettleBurstEnd, 0f,
				ProjectileID.MedusaHead, 0f,
				ProjectileID.PrincessWeapon, 0f,
				ProjectileID.MagnetSphereBolt, 0f,
				ProjectileID.InfernoFriendlyBlast, 0f,
				ProjectileID.LastPrism, 2f,
				ProjectileID.LastPrismLaser, 2f
			);
			public static int[] HomingEffectivenessMode { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(HomingEffectivenessMode))
			.Description("Controls the effectiveness of compatible homing effects")
			.RegisterIntSet();
			public static int[] MagicTripwireRange { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(MagicTripwireRange))
			.Description("Controls the range of Magic Tripwire and similar effects")
			.RegisterIntSet(0,
				ProjectileID.Grenade, 32,
				ProjectileID.BouncyGrenade, 32,
				ProjectileID.StickyGrenade, 32,
				ProjectileID.PartyGirlGrenade, 32,
				ProjectileID.Beenade, 32,
				ProjectileID.Bomb, 64,
				ProjectileID.BouncyBomb, 64,
				ProjectileID.StickyBomb, 64,
				ProjectileID.Dynamite, 64,
				ProjectileID.BouncyDynamite, 64,
				ProjectileID.StickyDynamite, 64,
				ProjectileID.BombFish, 64,
				ProjectileID.DryBomb, 12,
				ProjectileID.WetBomb, 12,
				ProjectileID.LavaBomb, 12,
				ProjectileID.HoneyBomb, 12,
				ProjectileID.ScarabBomb, 0,
				ProjectileID.MolotovCocktail, 64,
				ProjectileID.RocketI, 32,
				ProjectileID.RocketII, 32,
				ProjectileID.RocketIII, 64,
				ProjectileID.RocketIV, 64,
				ProjectileID.MiniNukeRocketI, 64,
				ProjectileID.MiniNukeRocketII, 64,
				ProjectileID.ClusterRocketI, 32,
				ProjectileID.ClusterRocketII, 32,
				ProjectileID.DryRocket, 32,
				ProjectileID.WetRocket, 32,
				ProjectileID.LavaRocket, 32,
				ProjectileID.HoneyRocket, 32,
				ProjectileID.ProximityMineI, 32,
				ProjectileID.ProximityMineII, 32,
				ProjectileID.ProximityMineIII, 64,
				ProjectileID.ProximityMineIV, 64,
				ProjectileID.MiniNukeMineI, 64,
				ProjectileID.MiniNukeMineII, 64,
				ProjectileID.ClusterMineI, 32,
				ProjectileID.ClusterMineII, 32,
				ProjectileID.DryMine, 32,
				ProjectileID.WetMine, 32,
				ProjectileID.LavaMine, 32,
				ProjectileID.HoneyMine, 32,
				ProjectileID.GrenadeI, 32,
				ProjectileID.GrenadeII, 32,
				ProjectileID.GrenadeIII, 64,
				ProjectileID.GrenadeIV, 64,
				ProjectileID.MiniNukeGrenadeI, 64,
				ProjectileID.MiniNukeGrenadeII, 64,
				ProjectileID.ClusterGrenadeI, 32,
				ProjectileID.ClusterGrenadeII, 32,
				ProjectileID.DryGrenade, 32,
				ProjectileID.WetGrenade, 32,
				ProjectileID.LavaGrenade, 32,
				ProjectileID.HoneyGrenade, 32,
				ProjectileID.RocketSnowmanI, 32,
				ProjectileID.RocketSnowmanII, 32,
				ProjectileID.RocketSnowmanIII, 64,
				ProjectileID.RocketSnowmanIV, 64,
				ProjectileID.MiniNukeSnowmanRocketI, 64,
				ProjectileID.MiniNukeSnowmanRocketII, 64,
				ProjectileID.ClusterSnowmanRocketI, 32,
				ProjectileID.ClusterSnowmanRocketII, 32,
				ProjectileID.DrySnowmanRocket, 32,
				ProjectileID.WetSnowmanRocket, 32,
				ProjectileID.LavaSnowmanRocket, 32,
				ProjectileID.HoneySnowmanRocket, 32,
				ProjectileID.RocketFireworkBlue, 64,
				ProjectileID.RocketFireworkGreen, 64,
				ProjectileID.RocketFireworkRed, 64,
				ProjectileID.RocketFireworkYellow, 64,
				ProjectileID.Celeb2Rocket, 64,
				ProjectileID.Celeb2RocketExplosive, 64,
				ProjectileID.Celeb2RocketLarge, 64,
				ProjectileID.Celeb2RocketExplosiveLarge, 64,
				ProjectileID.ElectrosphereMissile, 32,
				ProjectileID.HellfireArrow, 8,
				ProjectileID.Stynger, 12,
				ProjectileID.JackOLantern, 32
			);
			public static int[] MagicTripwireDetonationStyle { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(MagicTripwireDetonationStyle))
			.Description("Controls how Magic Tripwire and similar effects detonate the projectile")
			.RegisterIntSet(0,
				ProjectileID.Grenade, 1,
				ProjectileID.BouncyGrenade, 1,
				ProjectileID.StickyGrenade, 1,
				ProjectileID.PartyGirlGrenade, 1,
				ProjectileID.Beenade, 1,
				ProjectileID.Bomb, 1,
				ProjectileID.BouncyBomb, 1,
				ProjectileID.StickyBomb, 1,
				ProjectileID.Dynamite, 1,
				ProjectileID.BouncyDynamite, 1,
				ProjectileID.StickyDynamite, 1,
				ProjectileID.BombFish, 1,
				ProjectileID.DryBomb, 1,
				ProjectileID.WetBomb, 1,
				ProjectileID.LavaBomb, 1,
				ProjectileID.HoneyBomb, 1,
				ProjectileID.ScarabBomb, 1,
				ProjectileID.MolotovCocktail, 1,
				ProjectileID.RocketI, 2,
				ProjectileID.RocketII, 2,
				ProjectileID.RocketIII, 2,
				ProjectileID.RocketIV, 2,
				ProjectileID.MiniNukeRocketI, 2,
				ProjectileID.MiniNukeRocketII, 2,
				ProjectileID.ClusterRocketI, 2,
				ProjectileID.ClusterRocketII, 2,
				ProjectileID.DryRocket, 2,
				ProjectileID.WetRocket, 2,
				ProjectileID.LavaRocket, 2,
				ProjectileID.HoneyRocket, 2,
				ProjectileID.ProximityMineI, 2,
				ProjectileID.ProximityMineII, 2,
				ProjectileID.ProximityMineIII, 2,
				ProjectileID.ProximityMineIV, 2,
				ProjectileID.MiniNukeMineI, 2,
				ProjectileID.MiniNukeMineII, 2,
				ProjectileID.ClusterMineI, 2,
				ProjectileID.ClusterMineII, 2,
				ProjectileID.DryMine, 2,
				ProjectileID.WetMine, 2,
				ProjectileID.LavaMine, 2,
				ProjectileID.HoneyMine, 2,
				ProjectileID.GrenadeI, 2,
				ProjectileID.GrenadeII, 2,
				ProjectileID.GrenadeIII, 2,
				ProjectileID.GrenadeIV, 2,
				ProjectileID.MiniNukeGrenadeI, 2,
				ProjectileID.MiniNukeGrenadeII, 2,
				ProjectileID.ClusterGrenadeI, 2,
				ProjectileID.ClusterGrenadeII, 2,
				ProjectileID.DryGrenade, 2,
				ProjectileID.WetGrenade, 2,
				ProjectileID.LavaGrenade, 2,
				ProjectileID.HoneyGrenade, 2
			);
			public static bool[] CanBeDeflected { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(CanBeDeflected))
			.Description("Controls whether compatible projectile deflection and reflection effects will affect the projectile")
			.RegisterBoolSet(true,
				ProjectileID.FairyQueenSunDance
			);
			public static bool[] NoMultishot { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(NoMultishot))
			.Description("Projectiles in this set will not be duplicated by multishot effects")
			.RegisterBoolSet(false);
			public static bool[] IsEnemyOwned { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(IsEnemyOwned))
			.Description("Controls whether compatible effects will treat the projectile as owned by an enemy NPC")
			.RegisterBoolSet(false);
			public static (bool first, bool second, bool third)[] DuplicationAIVariableResets { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(DuplicationAIVariableResets))
			.Description("Controls which ai variables will be carried over to duplicates from compatible projectile duplication effects, false to carry over, true to reset")
			.RegisterCustomSet((false, false, false));
			public static Func<Projectile, bool>[] WeakpointAnalyzerAIReplacement { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(WeakpointAnalyzerAIReplacement))
			.Description("If a projectile has an entry in this set, copies from Weakpoint Analyzer will use before their AI, if it returns false, it will prevent the normal AI running")
			.RegisterCustomSet<Func<Projectile, bool>>(null,
				ProjectileID.ChlorophyteBullet, (Projectile projectile) => {
					float distSQ = projectile.DistanceSQ(projectile.GetGlobalProjectile<OriginGlobalProj>().weakpointAnalyzerTarget.Value);
					const float range = 128;
					const float rangeSQ = range * range;
					projectile.Opacity = MathHelper.Min(1f / (((distSQ * distSQ) / (rangeSQ * rangeSQ)) + 1), 1);
					if (projectile.damage == 0) {
						if (projectile.alpha < 170) {
							for (int i = 0; i < 10; i++) {
								float x2 = projectile.position.X - projectile.velocity.X / 10f * i;
								float y2 = projectile.position.Y - projectile.velocity.Y / 10f * i;
								Dust dust = Dust.NewDustDirect(new Vector2(x2, y2), 1, 1, DustID.CursedTorch);
								dust.alpha = projectile.alpha;
								dust.position.X = x2;
								dust.position.Y = y2;
								dust.velocity *= 0f;
								dust.noGravity = true;
							}
						}
						projectile.alpha = 255;
						return false;
					}
					return true;
				}
			);
			public static Action<Projectile, int>[] WeakpointAnalyzerSpawnAction { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(WeakpointAnalyzerSpawnAction))
			.Description("If a projectile has an entry in this set, copies from Weakpoint Analyzer will run it when spawned")
			.RegisterCustomSet<Action<Projectile, int>>(null);
			public static bool[] ApplyLifetimeModifiers { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(ApplyLifetimeModifiers))
			.Description("Controls whether compatible projectile lifetime modification effects will apply to the projectile type")
			.RegisterBoolSet(true);
			public static bool[] UsesTypeSpecificMinionPos { get; } = ProjectileID.Sets.Factory.CreateBoolSet();
			/*public static AssetSource<Texture2D>[][] ExtraTextures { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(ExtraTextures))
			.Description("Additional textures used by the projectile, ")
			.RegisterCustomSet<AssetSource<Texture2D>[]>([]);*/
			public static bool[] NoMildewSetTrail { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(NoMildewSetTrail))
			.RegisterBoolSet();
			public static float[] MinionBuffReceiverPriority { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(MinionBuffReceiverPriority))
			.Description("Used by effects which buff minions one at a time to determine which minion to buff")
			.RegisterFloatSet(1);
			public static Action<Projectile, float>[] SupportsRealSpeedBuffs { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(SupportsRealSpeedBuffs))
			.Description("If a value is present in this set for a projectile type, it will be called with the Projectile and speed modifier instead of modifying the update count")
			.RegisterCustomSet<Action<Projectile, float>>(null);
			public static bool[] DontPushBulletForward { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(DontPushBulletForward))
			.RegisterBoolSet();
			public static bool[] FireProjectiles { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(FireProjectiles))
			.Description("Projectiles which should trigger interactions involving heat or fire")
			.RegisterBoolSet(
				ProjectileID.WandOfSparkingSpark,
				ProjectileID.FlamingMace,

				ProjectileID.Flare,
				ProjectileID.BlueFlare,
				ProjectileID.RainbowFlare,
				ProjectileID.ShimmerFlare,
				ProjectileID.SpelunkerFlare,

				ProjectileID.FireArrow,

				ProjectileID.Flames,
				ProjectileID.FlamesTrap,

				ProjectileID.MolotovCocktail,
				ProjectileID.MolotovFire,
				ProjectileID.MolotovFire2,
				ProjectileID.MolotovFire3,

				ProjectileID.Sunfury,
				ProjectileID.Flamelash,
				ProjectileID.BallofFire,
				ProjectileID.Cascade,
				ProjectileID.Volcano,
				ProjectileID.Flamarang,
				ProjectileID.ImpFireball,

				ProjectileID.HelFire,
				ProjectileID.DD2PhoenixBowShot,

				ProjectileID.InfernoFriendlyBolt,
				ProjectileID.InfernoFriendlyBlast,
				ProjectileID.InfernoHostileBolt,
				ProjectileID.InfernoHostileBlast,

				ProjectileID.Spark,

				ProjectileID.DD2FlameBurstTowerT1Shot,
				ProjectileID.DD2FlameBurstTowerT2Shot,
				ProjectileID.DD2FlameBurstTowerT3Shot,

				ProjectileID.Fireball,
				ProjectileID.FlamingArrow,
				ProjectileID.DD2BetsyFlameBreath,

				ProjectileID.CursedFlare,
				ProjectileID.CursedArrow,
				ProjectileID.CursedDart,
				ProjectileID.CursedDartFlame,
				ProjectileID.CursedBullet,
				ProjectileID.ClingerStaff,
				ProjectileID.CursedFlameFriendly,

				ProjectileID.CursedFlameHostile,
				ProjectileID.EyeFire,

				ProjectileID.DarkLance,
				ProjectileID.ShadowFlame,
				ProjectileID.ShadowFlameArrow,
				ProjectileID.ShadowFlameKnife
			);
			static Projectiles() {
				foreach (KeyValuePair<int, Projectile> proj in ContentSamples.ProjectilesByType) {
					if (!NoMultishot.IndexInRange(proj.Key)) continue;
					if (proj.Value.DamageType is Thrown_Explosive) {
						HomingEffectivenessMode[proj.Key] = 1;
					}
					switch (proj.Value.aiStyle) {
						case ProjAIStyleID.Flail:
						case ProjAIStyleID.HeldProjectile:
						case ProjAIStyleID.Whip:
						NoMultishot[proj.Key] = true;
						break;

						default:
						if (proj.Value.minion || proj.Value.sentry) NoMultishot[proj.Key] = true;
						break;
					}
				}
			}
		}
		[ReinitializeDuringResizeArrays]
		public static class NPCs {
			public static string[] JournalEntries { get; } = NPCID.Sets.Factory.CreateNamedSet($"{nameof(NPCs)}_{nameof(JournalEntries)}")
			.Description("Controls which npcs are associated with which journal entries, multiple entries can be assigned by separating them with semicolons")
			.RegisterCustomSet<string>(null);
			public static Func<bool>[] BossKillCounterOverrider { get; } = NPCID.Sets.Factory.CreateNamedSet(nameof(BossKillCounterOverrider))
			.Description("If an NPC type has an entry in this set, that will be used instead of ")
			.RegisterCustomSet<Func<bool>>(null,
				NPCID.Retinazer, () => NPC.downedMechBoss2,
				NPCID.Spazmatism, () => false
			);
			public static bool[] TargetDummies { get; } = NPCID.Sets.Factory.CreateNamedSet($"{nameof(TargetDummies)}")
			.Description("Used to prevent exploits from some on-hit effects")
			.RegisterBoolSet(NPCID.TargetDummy);
			public static Action<NPC>[] CustomExpertScaling { get; } = NPCID.Sets.Factory.CreateCustomSet<Action<NPC>>(null);
			public static Predicate<NPC>[] CustomGroundedCheck { get; } = NPCID.Sets.Factory.CreateNamedSet($"{nameof(PegasusLib)}/{nameof(CustomGroundedCheck)}")
			.RegisterCustomSet<Predicate<NPC>>(null);
		}
		[ReinitializeDuringResizeArrays]
		public static class Tiles {
			public static int[] PlacementItem { get; } = TileID.Sets.Factory.CreateIntSet(-1);
			public static int[] ShimmerTransformToTile { get; } = TileID.Sets.Factory.CreateIntSet(-1,
				TileID.LavaMoss, TileID.RainbowMoss,
				TileID.KryptonMoss, TileID.RainbowMoss,
				TileID.XenonMoss, TileID.RainbowMoss,
				TileID.ArgonMoss, TileID.RainbowMoss,
				TileID.VioletMoss, TileID.RainbowMoss,

				TileID.Topaz, TileID.Amethyst,
				TileID.Sapphire, TileID.Topaz,
				TileID.Emerald, TileID.Sapphire,
				TileID.Ruby, TileID.Emerald,
				TileID.Diamond, TileID.Ruby
			);
			public static MultitileCollisionOffsetter[] MultitileCollisionOffset { get; } = TileID.Sets.Factory.CreateCustomSet<MultitileCollisionOffsetter>(null);
			public static SlowdownPercent[] MinionSlowdown { get; } = TileID.Sets.Factory.CreateCustomSet<SlowdownPercent>(0);
			public static bool[] DisableHoiking { get; } = TileID.Sets.Factory.CreateBoolSet(false);
			public static bool[] StructureSerializer_PlaceAsObject { get; } = TileID.Sets.Factory.CreateBoolSet();
			public static (SoundStyle open, SoundStyle close)[] ChestSoundOverride { get; } = TileID.Sets.Factory.CreateCustomSet<(SoundStyle, SoundStyle)>(default);
			public static bool[] GemTilesToChambersite { get; } = TileID.Sets.Factory.CreateNamedSet($"{nameof(Tiles)}_{nameof(GemTilesToChambersite)}")
				.Description("Gem ores in this set can be corrupted into chambersite ores")
				.RegisterBoolSet(
					TileID.Amethyst,
					TileID.Topaz,
					TileID.Sapphire,
					TileID.Emerald,
					TileID.Ruby,
					TileID.Diamond
				)
				.AddModdedContent([SetWithMods("ThoriumMod")] (set) => set.SetValues(true, TileType<ThoriumTiles.Aquamarine>(), TileType<ThoriumTiles.Opal>()))
				.AddModdedContent([SetWithMods("Avalon")] (set) => set.SetValues(true, GetModContent(OMI.Avalon, "Peridot"), GetModContent(OMI.Avalon, "Tourmaline"), GetModContent(OMI.Avalon, "Zircon")));
			public static bool[] ExposedGemsToChambersite { get; } = TileID.Sets.Factory.CreateNamedSet($"{nameof(Tiles)}_{nameof(ExposedGemsToChambersite)}")
				.Description("Placed gems in this set can be corrupted into placed chambersite")
				.RegisterBoolSet(
					TileID.ExposedGems
				)
				.AddModdedContent([SetWithMods("ThoriumMod")] (set) => set.SetValues(true, TileType<ThoriumTiles.PlacedGem>()))
				.AddModdedContent([SetWithMods("Avalon")] (set) => set.SetValues(true, GetModContent(OMI.Avalon, "PlacedGems")));
			internal static void Initialize() {
				try {
					IL_Collision.SlopeCollision += IL_Collision_SlopeCollision;
				} catch (Exception e) {
					if (Origins.LogLoadingILError(nameof(IL_Collision_SlopeCollision), e)) throw;
				}
			}
			static void IL_Collision_SlopeCollision(ILContext il) {
				ILCursor c = new(il);
				int tile = -1;
				ILLabel cont = default;
				c.GotoNext(MoveType.After,
					i => i.MatchLdloca(out tile),//IL_00f8: ldloca.s 20
					i => i.MatchCall<Tile>("active"),//IL_00fa: call instance bool Terraria.Tile::active()
					i => i.MatchBrfalse(out cont)//IL_00ff: brfalse IL_069c
				);
				c.EmitLdloca(tile);
				c.EmitDelegate((in Tile tile) => DisableHoiking[tile.TileType] && tile.BottomSlope);
				c.EmitBrtrue(cont);
			}
			public static int GetModContent(Mod mod, string name) {
				return mod.GetContent<ModTile>().First(content => content.Name == name).Type;
			}
		}
		public delegate void MultitileCollisionOffsetter(Tile tile, ref float y, ref int height);
		[ReinitializeDuringResizeArrays]
		public static class Walls {
			public static bool[] RivenWalls { get; } = WallID.Sets.Factory.CreateNamedSet(nameof(RivenWalls))
			.RegisterBoolSet(false);
			public static int[] GeneratesLiquid { get; } = WallID.Sets.Factory.CreateIntSet(-1);
			internal static void Initialize() {
				try {
					On_Liquid.Update += (orig, self) => {
						orig(self);
						Tile tile = Framing.GetTileSafely(self.x, self.y);
						int generateType = GeneratesLiquid[tile.WallType];
						if (generateType != -1) {
							tile.LiquidType = generateType;
							tile.LiquidAmount = 255;
						}
					};
				} catch (Exception e) {
					if (Origins.LogLoadingILError(nameof(GeneratesLiquid), e)) throw;
				}
			}
		}
		[ReinitializeDuringResizeArrays]
		public static class Prefixes {
			public static bool[] SpecialPrefix { get; } = PrefixID.Sets.Factory.CreateNamedSet(nameof(SpecialPrefix))
			.Description("Denotes prefixes which have effects other than stat changes")
			.RegisterBoolSet(false);
		}
		public static class Armor {
			[ReinitializeDuringResizeArrays]
			public static class Front {
				public static bool[] DrawsInNeckLayer { get; } = ArmorIDs.Front.Sets.Factory.CreateNamedSet(nameof(DrawsInNeckLayer))
				.RegisterBoolSet(false);
			}
		}
		public static class Misc {
			public static HashSet<(Effect effect, string pass)> BasicColorDyeShaderPasses = [];
			public static bool[] BasicColorDyeShaders;
			public static int ArmorShaderDataCount { get; private set; }
			public static void SetupDyes() {
				BasicColorDyeShaderPasses.Add((Main.pixelShader, "ArmorColored"));
				BasicColorDyeShaderPasses.Add((Main.pixelShader, "ArmorColoredAndBlack"));
				BasicColorDyeShaderPasses.Add((Main.pixelShader, "ArmorColoredAndSilverTrim"));
				FastFieldInfo<ArmorShaderDataSet, List<ArmorShaderData>> _shaderData = "_shaderData";
				FastFieldInfo<ShaderData, string> _passName = "_passName";
				List<ArmorShaderData> shaders = _shaderData.GetValue(GameShaders.Armor);
				ArmorShaderDataCount = shaders.Count + 1;
				BasicColorDyeShaders = new bool[ArmorShaderDataCount];
				for (int i = 0; i < shaders.Count; i++) {
					ArmorShaderData shader = shaders[i];
					if (BasicColorDyeShaderPasses.Contains((shader.Shader, _passName.GetValue(shader)))) {
						BasicColorDyeShaders[i + 1] = true;
					}
				}
				BasicColorDyeShaderPasses = null;
				for (int i = 1; i < BasicColorDyeShaders.Length; i++) {
					if (!BasicColorDyeShaders[i]) continue;
					Type type = shaders[i - 1].GetType();
					if (type != typeof(ArmorShaderData) && type.GetMethod(nameof(ArmorShaderData.GetSecondaryShader), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) is not null) {
						BasicColorDyeShaders[i] = false;
					}
				}
			}
		}
		public readonly struct SlowdownPercent {
			readonly float value;
			SlowdownPercent(float value) {
				System.Diagnostics.Debug.Assert(value >= 0 && value <= 1, "Slowdown percentage cannot be less than 0% or greater than 100%");
				this.value = value;
			}
			public static implicit operator SlowdownPercent(float value) => new(value);
			public static implicit operator float(SlowdownPercent value) => value.value;
		}
		private static void SetValues<T>(this T[] set, T value, params int[] indices) {
			for (int i = 0; i < indices.Length; i++) {
				if (indices[i] != -1) set[indices[i]] = value;
			}
		}
		/// <summary>
		/// Performs <paramref name="content"/> on the set, then returns the set
		/// Automatically skips the provided action if it has a <see cref="SetWithModsAttribute"/> with an unloaded mod
		/// Should never attempt to access a set except through its <paramref name="set"/> parameter, as all sets may not be initialized, and the one being set will certainly not be initialized
		/// </summary>
		private static T[] AddModdedContent<T>(this T[] set, Action<T[]> content) {
			if (ItemID.Sets.AlsoABuildingItem.Length == ItemID.Count) return set;
			if (!content.Method.TryGetCustomAttribute(out SetWithModsAttribute attribute) || attribute.ShouldJIT(null)) content(set);
			return set;
		}
		private static bool TryGetCustomAttribute<TAttribute>(this MemberInfo member, out TAttribute attribute) where TAttribute : Attribute {
			attribute = member.GetCustomAttribute<TAttribute>();
			return attribute is not null;
		}
		private sealed class SetWithModsAttribute(params string[] names) : MemberJitAttribute {
			public readonly string[] Names = names ?? throw new ArgumentNullException(nameof(names));
			public override bool ShouldJIT(MemberInfo member) => Names.All(ModLoader.HasMod);
		}
	}
}
