using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
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
	public interface IBlastRadiusPrefix {
		StatModifier BlastRadius();
	}
	public interface ISelfDamagePrefix {
		StatModifier SelfDamage();
	}
	public static class Prefixes {
		enum BonusStackType {
			Base,
			Additive,
			Multiplicative,
			Flat
		}
		public static IEnumerable<TooltipLine> GetStatLines(this ModPrefix prefix) {
			static TooltipLine GetLine(float value, string key, BonusStackType stack, bool inverted = false) {
				bool positive = value > 0;
				string formattedValue = positive ? "+" : "";
				switch (stack) {
					case BonusStackType.Base:
					case BonusStackType.Flat:
					formattedValue += value;
					break;

					case BonusStackType.Additive:
					case BonusStackType.Multiplicative:
					formattedValue += $"{value - 1:P0}";
					break;

					default:
					throw new ArgumentException($"{stack} is not a valid BonusStackType", nameof(stack));
				}
				return new TooltipLine(Origins.instance, key + stack.ToString(), Language.GetTextValue("Mods.Origins.Tooltips.Modifiers." + key, formattedValue)) {
					IsModifier = true, IsModifierBad = positive ^ inverted
				};
			}
			if (prefix is IBlastRadiusPrefix blastRadiusPrefix) {
				StatModifier blastRadius = blastRadiusPrefix.BlastRadius();
				if (blastRadius.Base != 0) yield return GetLine(blastRadius.Base, "BlastRadius", BonusStackType.Base);
				if (blastRadius.Additive != 0) yield return GetLine(blastRadius.Base, "BlastRadius", BonusStackType.Additive);
				if (blastRadius.Multiplicative != 0) yield return GetLine(blastRadius.Base, "BlastRadius", BonusStackType.Multiplicative);
				if (blastRadius.Flat != 0) yield return GetLine(blastRadius.Base, "BlastRadius", BonusStackType.Flat);
			}
			if (prefix is ISelfDamagePrefix selfDamagePrefix) {
				StatModifier selfDamage = selfDamagePrefix.SelfDamage();
				if (selfDamage.Base != 0) yield return GetLine(selfDamage.Base, "SelfDamage", BonusStackType.Base, true);
				if (selfDamage.Additive != 0) yield return GetLine(selfDamage.Base, "SelfDamage", BonusStackType.Additive, true);
				if (selfDamage.Multiplicative != 0) yield return GetLine(selfDamage.Base, "SelfDamage", BonusStackType.Multiplicative, true);
				if (selfDamage.Flat != 0) yield return GetLine(selfDamage.Base, "SelfDamage", BonusStackType.Flat, true);
			}
		}
	}
	public class Heavy_Cal_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Ranged;
		
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
	public class Unbounded_Prefix : ModPrefix, IBlastRadiusPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public StatModifier BlastRadius() => new(1, 1.15f);
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.1f;
			critBonus += 2;
			//blastRadiusMult += 0.1f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Eased_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			useTimeMult -= 0.1f;
			shootSpeedMult += 0.1f;
		}
	}
	public class Loaded_Prefix : ModPrefix, IBlastRadiusPrefix, ISelfDamagePrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public StatModifier BlastRadius() => new(1, 1.25f);
		public StatModifier SelfDamage() => new(1, 0.9f);
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.05f;
			knockbackMult += 0.1f;
			//blastRadiusMult += 0.25f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Penetrative_Prefix : ModPrefix, IBlastRadiusPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public StatModifier BlastRadius() => new(1, 1.10f);
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.05f;
			critBonus += 5;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Catastrophic_Prefix : ModPrefix, IBlastRadiusPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public StatModifier BlastRadius() => new(1, 1.10f);
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.1f;
			useTimeMult -= 0.05f;
			critBonus += 2;
			shootSpeedMult += 0.1f;
			//blastRadiusMult += 0.1f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Persuasive_Prefix : ModPrefix, IBlastRadiusPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public StatModifier BlastRadius() => new(1, 1.15f);
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			knockbackMult += 0.1f;
			//blastRadiusMult += 0.15f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Dud_Prefix : ModPrefix, ISelfDamagePrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public StatModifier SelfDamage() => new(1, 0.9f);
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult -= 0.1f;
			shootSpeedMult -= 0.1f;
			knockbackMult -= 0.2f;
			//selfDamageMult -= 0.25f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Wimpy_Prefix : ModPrefix, IBlastRadiusPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public StatModifier BlastRadius() => new(1, 0.75f);
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			shootSpeedMult -= 0.1f;
			knockbackMult -= 0.1f;
			//blastRadiusMult -= 0.25f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Betraying_Prefix : ModPrefix, ISelfDamagePrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public StatModifier SelfDamage() => new(1, 1.5f);
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			//selfDamageMult += 0.5f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Lightweight_Prefix : ModPrefix, IBlastRadiusPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public StatModifier BlastRadius() => new(1, 0.90f);
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult -= 0.15f;
			useTimeMult -= 0.1f;
			shootSpeedMult += 0.2f;
			//blastRadiusMult += 0.1f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Heavy_Duty_Prefix : ModPrefix, IBlastRadiusPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public StatModifier BlastRadius() => new(1, 1.25f);
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.15f;
			useTimeMult += 0.2f;
			shootSpeedMult -= 0.1f;
			knockbackMult += 0.15f;
			//blastRadiusMult += 0.25f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Safe_Prefix : ModPrefix, ISelfDamagePrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public StatModifier SelfDamage() => new(1, 0.6f);
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			//selfDamageMult -= 0.2f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Guileless_Prefix : ModPrefix, IBlastRadiusPrefix {
		public override PrefixCategory Category => PrefixCategory.Custom;
		public StatModifier BlastRadius() => new(1, 1.25f);
		public override bool CanRoll(Item item) => item.CountsAsClass<Explosive>();
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.1f;
			critBonus += 5;
			useTimeMult -= 0.1f;
			shootSpeedMult += 0.15f;
			knockbackMult += 0.15f;
			//blastRadiusMult += 0.25f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Imperfect_Prefix : ModPrefix, IOnSpawnProjectilePrefix, IModifyTooltipsPrefix, ICanReforgePrefix {
		public override PrefixCategory Category => PrefixCategory.Ranged;
		
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