using Microsoft.Xna.Framework.Graphics.PackedVector;
using Origins.Items.Tools;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginsSets;

namespace Origins {
	public static class OriginsSets {
		[ReinitializeDuringResizeArrays]
		public static class Items {
			// not named because it controls a change to vanilla mechanics only present in TO, likely to be moved to PegasusLib
			public static bool[] ItemsThatAllowRemoteRightClick { get; } = ItemID.Sets.Factory.CreateBoolSet();
			// not named because it controls a change to vanilla mechanics only present in TO, likely to be moved to PegasusLib
			public static float[] DamageBonusScale { get; } = ItemID.Sets.Factory.CreateFloatSet(1f);
			public static string[] JournalEntries { get; } = ItemID.Sets.Factory.CreateNamedSet($"{nameof(Items)}_{nameof(JournalEntries)}")
			.Description("Controls which items are associated with which journal entries")
			.RegisterCustomSet<string>(null);
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
			.Description("Controls whether compatible projectile deflection effects will affect the projectile")
			.RegisterBoolSet(true);
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
			public static int[] RangedControlLocusDuplicateCount { get; } = ProjectileID.Sets.Factory.CreateNamedSet(nameof(RangedControlLocusDuplicateCount))
			.Description("The number of duplicates that will be made when this projectile is shot with the effects of Control Locus")
			.RegisterIntSet(5,
				ProjectileID.ChlorophyteBullet, 3
			);
		}
		[ReinitializeDuringResizeArrays]
		public static class NPCs {
			public static string[] JournalEntries { get; } = NPCID.Sets.Factory.CreateNamedSet($"{nameof(NPCs)}_{nameof(JournalEntries)}")
			.Description("Controls which npcs are associated with which journal entries")
			.RegisterCustomSet<string>(null);
			public static Func<bool>[] BossKillCounterOverrider { get; } = NPCID.Sets.Factory.CreateNamedSet(nameof(BossKillCounterOverrider))
			.Description("If an NPC type has an entry in this set, that will be used instead of ")
			.RegisterCustomSet<Func<bool>>(null,
				NPCID.Retinazer, () => NPC.downedMechBoss2,
				NPCID.Spazmatism, () => false
			);
		}
		[ReinitializeDuringResizeArrays]
		public static class Prefixes {
			public static bool[] SpecialPrefix { get; } = PrefixID.Sets.Factory.CreateNamedSet(nameof(SpecialPrefix))
			.Description("Denotes prefixes which have effects other than stat changes")
			.RegisterBoolSet(false);
		}
	}
}
