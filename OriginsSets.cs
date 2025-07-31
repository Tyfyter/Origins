using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins {
	public static class OriginsSets {
		[ReinitializeDuringResizeArrays]
		public static class Items {
			// not named because it controls a change to vanilla mechanics only present in TO, likely to be moved to PegasusLib
			public static bool[] ItemsThatAllowRemoteRightClick { get; } = ItemID.Sets.Factory.CreateBoolSet();
			// not named because it controls a change to vanilla mechanics only present in TO, likely to be moved to PegasusLib
			public static float[] DamageBonusScale { get; } = ItemID.Sets.Factory.CreateFloatSet(1f);
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
		}
		[ReinitializeDuringResizeArrays]
		public static class Projectiles {
			public static bool[] ForceFelnumShockOnShoot { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(ForceFelnumShockOnShoot))
			.Description("Controls whether Felnum Armor's set bonus will trigger when the projectile is created for projectiles which would otherwise only consume the bonus when they hit an enemy")
			.RegisterBoolSet(false);
			public static float[] HomingEffectivenessMultiplier { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(HomingEffectivenessMultiplier))
			.Description("Controls the effectiveness of compatible homing effects")
			.RegisterFloatSet(1f);
			public static int[] MagicTripwireRange { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(MagicTripwireRange))
			.Description("Controls the range of Magic Tripwire and similar effects")
			.RegisterIntSet(0);
			public static int[] MagicTripwireDetonationStyle { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(MagicTripwireDetonationStyle))
			.Description("Controls how Magic Tripwire and similar effects detonate the projectile")
			.RegisterIntSet(0);
			public static bool[] CanBeDeflected { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(CanBeDeflected))
			.Description("Controls whether compatible projectile deflection and reflection effects will affect the projectile")
			.RegisterBoolSet(true,
				ProjectileID.FairyQueenSunDance
			);
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
			public static bool[] ApplyLifetimeModifiers { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(ApplyLifetimeModifiers))
			.Description("Controls whether compatible projectile lifetime modification effects will apply to the projectile type")
			.RegisterBoolSet(true);
			public static bool[] UsesTypeSpecificMinionPos { get; } = ProjectileID.Sets.Factory.CreateBoolSet();
			/*public static AssetSource<Texture2D>[][] ExtraTextures { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(ExtraTextures))
			.Description("Additional textures used by the projectile, ")
			.RegisterCustomSet<AssetSource<Texture2D>[]>([]);*/
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
		}
		public delegate void MultitileCollisionOffsetter(Tile tile, ref float y, ref int height);
		[ReinitializeDuringResizeArrays]
		public static class Prefixes {
			public static bool[] SpecialPrefix { get; } = PrefixID.Sets.Factory.CreateNamedSet(nameof(SpecialPrefix))
			.Description("Denotes prefixes which have effects other than stat changes")
			.RegisterBoolSet(false);
		}
		public static class Misc {
			public static HashSet<(Effect effect, string pass)> BasicColorDyeShaderPasses = [];
			public static bool[] BasicColorDyeShaders;
			public static void SetupDyes() {
				BasicColorDyeShaderPasses.Add((Main.pixelShader, "ArmorColored"));
				BasicColorDyeShaderPasses.Add((Main.pixelShader, "ArmorColoredAndBlack"));
				BasicColorDyeShaderPasses.Add((Main.pixelShader, "ArmorColoredAndSilverTrim"));
				FastFieldInfo<ArmorShaderDataSet, List<ArmorShaderData>> _shaderData = "_shaderData";
				FastFieldInfo<ShaderData, string> _passName = "_passName";
				List<ArmorShaderData> shaders = _shaderData.GetValue(GameShaders.Armor);
				BasicColorDyeShaders = new bool[shaders.Count + 1];
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
	}
}
