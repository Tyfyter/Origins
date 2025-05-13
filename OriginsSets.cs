using Origins.Items.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		}
		[ReinitializeDuringResizeArrays]
		public static class NPCs {
			public static string[] JournalEntries { get; } = NPCID.Sets.Factory.CreateNamedSet($"{nameof(NPCs)}_{nameof(JournalEntries)}")
			.Description("Controls which npcs are associated with which journal entries")
			.RegisterCustomSet<string>(null);
		}
		[ReinitializeDuringResizeArrays]
		public static class Prefixes {
			public static bool[] SpecialPrefix { get; } = PrefixID.Sets.Factory.CreateNamedSet(nameof(SpecialPrefix))
			.Description("Denotes prefixes which have effects other than stat changes")
			.RegisterBoolSet(false);
		}
	}
}
