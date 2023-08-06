using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Origins.Items {
	public interface IOnSpawnProjectilePrefix {
		void OnProjectileSpawn(Projectile projectile, IEntitySource source);
	}
	public interface IModifyTooltipsPrefix {
		void ModifyTooltips(Item item, List<TooltipLine> tooltips);
	}
	public interface ICanReforgePrefix {
		bool CanReforge(Item item);
	}
	public class Heavy_Cal_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Ranged;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Heavy Caliber");
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult += 0.4581f;
		}
		public override float RollChance(Item item) {
			return 1f;
		}
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.20f;// damage
			critBonus += 10;// crit
			shootSpeedMult -= 0.15f;// velocity
			knockbackMult += 0.20f;// knockback
		}
	}
	public class Assuring_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Accessory;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Assuring");
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult += 0.67f;
		}
		public override float RollChance(Item item) {
#if false
			return Origins.NPCs.TownNPCs.Cubekon_Tinkerer.rolling ? 0.3f : 0;
#else
			return 0f;
#endif
		}
		public override void Apply(Item item) {
			item.defense += 7;
		}
	}
	public class Consistent_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Accessory;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Consistent");
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult += 0.67f;
		}
		public override float RollChance(Item item) {
#if false
			return Origins.NPCs.TownNPCs.Cubekon_Tinkerer.rolling ? 0.3f : 0;
#else
			return 0f;
#endif
		}
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			critBonus += 8;
		}
	}
	public class Malicious_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Accessory;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Malicious");
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult += 0.67f;
		}
		public override float RollChance(Item item) {
#if false
			return Origins.NPCs.TownNPCs.Cubekon_Tinkerer.rolling ? 0.3f : 0;
#else
			return 0f;
#endif
		}
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.06f;
		}
	}
	public class Sudden_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Accessory;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Sudden");
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult += 0.67f;
		}
		public override float RollChance(Item item) {
#if false
			return Origins.NPCs.TownNPCs.Cubekon_Tinkerer.rolling ? 0.3f : 0;
#else
			return 0f;
#endif
		}
		//movement speed boost here
	}
	public class Vicious_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Accessory;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Vicious");
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult += 0.67f;
		}
		public override float RollChance(Item item) {
#if false
			return Origins.NPCs.TownNPCs.Cubekon_Tinkerer.rolling ? 0.3f : 0;
#else
			return 0f;
#endif
		}
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			useTimeMult -= 0.06f;
		}
	}
	public class Esoteric_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Accessory;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Esoteric");
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult += 0.67f;
		}
		public override float RollChance(Item item) {
#if false
			return Origins.NPCs.TownNPCs.Cubekon_Tinkerer.rolling ? 0.3f : 0;
#else
			return 0f;
#endif
		}
		public override void Apply(Item item) {
			item.manaIncrease += 40;
		}
	}
	public class Unbounded_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Unbounded");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.1f;
			critBonus += 2;
			//blastRadiusMult += 0.1f;
		}
	}
	public class Eased_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Eased");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			useTimeMult -= 0.1f;
			shootSpeedMult += 0.1f;
		}
	}
	public class Loaded_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Loaded");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.05f;
			knockbackMult += 0.1f;
			//blastRadiusMult += 0.25f;
		}
	}
	public class Penetrative_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Penetrative");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.05f;
			critBonus += 5;
		}
	}
	public class Catastrophic_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Catastrophic");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.1f;
			useTimeMult -= 0.05f;
			critBonus += 2;
			shootSpeedMult += 0.1f;
			//blastRadiusMult += 0.1f;
		}
	}
	public class Persuasive_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Persuasive");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			knockbackMult += 0.1f;
			//blastRadiusMult += 0.15f;
		}
	}
	public class Dud_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Dud");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult -= 0.1f;
			shootSpeedMult -= 0.1f;
			knockbackMult -= 0.2f;
			//selfDamageMult -= 0.25f;
		}
	}
	public class Wimpy_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Wimpy");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			shootSpeedMult -= 0.1f;
			knockbackMult -= 0.1f;
			//blastRadiusMult -= 0.25f;
		}
	}
	public class Betraying_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Betraying");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			//selfDamageMult += 0.5f;
		}
	}
	public class Lightweight_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Lightweight");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult -= 0.15f;
			useTimeMult -= 0.1f;
			shootSpeedMult += 0.2f;
			//blastRadiusMult += 0.1f;
		}
	}
	public class Heavy_Duty_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Heavy-Duty");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.15f;
			useTimeMult += 0.2f;
			shootSpeedMult -= 0.1f;
			knockbackMult += 0.15f;
			//blastRadiusMult += 0.25f;
		}
	}
	public class Safe_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Safe");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			//selfDamageMult -= 0.2f;
		}
	}
	public class Guileless_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Guileless");
		}
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.1f;
			critBonus += 5;
			useTimeMult -= 0.1f;
			shootSpeedMult += 0.15f;
			knockbackMult += 0.15f;
			//blastRadiusMult += 0.25f;
		}
	}
	public class Imperfect_Prefix : ModPrefix, IOnSpawnProjectilePrefix, IModifyTooltipsPrefix, ICanReforgePrefix {
		public override PrefixCategory Category => PrefixCategory.Ranged;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Imperfect");
		}
		public override bool CanRoll(Item item) => true;
		public override float RollChance(Item item) => 0f;
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult -= 0.1f;
			critBonus -= 4;
			useTimeMult += 0.1f;
			knockbackMult -= 0.15f;
		}
		public void OnProjectileSpawn(Projectile projectile, IEntitySource source) {
			projectile.velocity = projectile.velocity.RotatedByRandom(MathHelper.ToRadians(10f));
		}

		public void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Name == "Price") {
					tooltips[i].Text = "Cannot sell";
					tooltips[i].OverrideColor = Color.Gray;
					break;
				}
			}
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) {
			yield return new TooltipLine(
				Mod,
				"PrefixSpread",
				"+10° Spread"
				) {
				IsModifier = true,
				IsModifierBad = true
			};
		}
		public bool CanReforge(Item item) => false;
	}
}