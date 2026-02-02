using Origins.Items.Accessories;
using Origins.Items.Weapons.Ranged;
using Origins.Projectiles;
using Origins.Questing;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
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
	public interface IOnHitNPCPrefix {
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone);
	}
	public interface IProjectileAIPrefix {
		public void ProjectileAI(Projectile projectile);
	}
	public interface IModifyHitNPCPrefix {
		public void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers);
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
				bool negative;
				string text;
				switch (stack) {
					case BonusStackType.Base:
					case BonusStackType.Flat:
					text = $"{(int)value}";
					negative = value < 0;
					break;

					case BonusStackType.Additive:
					case BonusStackType.Multiplicative:
					text = $"{value - 1:P0}";
					negative = value < 1;
					break;

					default:
					throw new ArgumentException($"{stack} is not a valid BonusStackType", nameof(stack));
				}
				string textPrefix = negative ? "" : "+";
				return new TooltipLine(Origins.instance, key + stack.ToString(), Language.GetTextValue("Mods.Origins.Tooltips.Modifiers." + key, textPrefix + text)) {
					IsModifier = true, IsModifierBad = negative ^ inverted
				};
			}
			if (prefix is IBlastRadiusPrefix blastRadiusPrefix) {
				StatModifier blastRadius = blastRadiusPrefix.BlastRadius();
				if (blastRadius.Multiplicative != 0) {
					if (blastRadius.Base != 0) yield return GetLine(blastRadius.Base, "BlastRadius", BonusStackType.Base);
					if (blastRadius.Additive != 1) yield return GetLine(blastRadius.Additive, "BlastRadius", BonusStackType.Additive);
				}
				if (blastRadius.Multiplicative != 1) yield return GetLine(blastRadius.Multiplicative, "BlastRadius", BonusStackType.Multiplicative);
				if (blastRadius.Flat != 0) yield return GetLine(blastRadius.Flat, "BlastRadius", BonusStackType.Flat);
			}
			if (prefix is ISelfDamagePrefix selfDamagePrefix) {
				StatModifier selfDamage = selfDamagePrefix.SelfDamage();
				if (selfDamage.Multiplicative != 0) {
					if (selfDamage.Base != 0) yield return GetLine(selfDamage.Base, "SelfDamage", BonusStackType.Base, true);
					if (selfDamage.Additive != 1) yield return GetLine(selfDamage.Additive, "SelfDamage", BonusStackType.Additive, true);
				}
				if (selfDamage.Multiplicative != 1) yield return GetLine(selfDamage.Multiplicative, "SelfDamage", BonusStackType.Multiplicative, true);
				if (selfDamage.Flat != 0) yield return GetLine(selfDamage.Flat, "SelfDamage", BonusStackType.Flat, true);
			}
			if (prefix is ArtifactMinionPrefix artifactPrefix) {
				StatModifier maxLife = artifactPrefix.MaxLifeModifier;
				if (maxLife.Multiplicative != 0) {
					if (maxLife.Base != 0) yield return GetLine(maxLife.Base, "ArtifactMaxLife", BonusStackType.Base, false);
					if (maxLife.Additive != 1) yield return GetLine(maxLife.Additive, "ArtifactMaxLife", BonusStackType.Additive, false);
				}
				if (maxLife.Multiplicative != 1) yield return GetLine(maxLife.Multiplicative, "ArtifactMaxLife", BonusStackType.Multiplicative, false);
				if (maxLife.Flat != 0) yield return GetLine(maxLife.Flat, "ArtifactMaxLife", BonusStackType.Flat, false);
			}
		}
	}
	public abstract class OriginsPrefix : ModPrefix {
		public virtual bool HasDescription => false;
		public virtual bool HasDescriptionBad => false;
		public sealed override void Load() {
			_ = GetDescriptionLines().Count();
			OnLoad();
		}
		public virtual void OnLoad() { }
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => [
			..this.GetStatLines(),
			..GetDescriptionLines()
		];
		IEnumerable<TooltipLine> GetDescriptionLines() {
			if (HasDescription) yield return new TooltipLine(Origins.instance, Name, this.GetLocalization("Description").Value) { IsModifier = true };
			if (HasDescriptionBad) yield return new TooltipLine(Origins.instance, Name, this.GetLocalization("BadDescription").Value) { IsModifier = true, IsModifierBad = true };
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
	#region accessory prefixes
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
	#endregion accessory prefixes
	#region explosive prefixes
	public abstract class Explosive_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		public override bool CanRoll(Item item) => item.DamageType.GetsPrefixesFor<Explosive>();
		public override void ModifyValue(ref float valueMult) {
			if (this is IBlastRadiusPrefix blastRad) {
				StatModifier statModifier = blastRad.BlastRadius();
				valueMult *= 1 + (statModifier.Additive * statModifier.Multiplicative * (1 + statModifier.Base / 100) * (1 + statModifier.Flat / 100) - 1) * 0.5f;
			}
			if (this is ISelfDamagePrefix selfDamagePrefix) {
				StatModifier statModifier = selfDamagePrefix.SelfDamage();
				valueMult *= 1 + (statModifier.Additive * statModifier.Multiplicative * (1 + statModifier.Base / 100) * (1 + statModifier.Flat / 100) - 1) * -0.25f;
			}
		}
	}
	public class Unbounded_Prefix : Explosive_Prefix, IBlastRadiusPrefix {
		public StatModifier BlastRadius() => new(1, 1.15f);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.1f;
			critBonus += 2;
			//blastRadiusMult += 0.1f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Eased_Prefix : Explosive_Prefix {
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			useTimeMult -= 0.1f;
			shootSpeedMult += 0.1f;
		}
	}
	public class Loaded_Prefix : Explosive_Prefix, IBlastRadiusPrefix, ISelfDamagePrefix {
		public StatModifier BlastRadius() => new(1, 1.25f);
		public StatModifier SelfDamage() => new(1, 0.9f);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.05f;
			knockbackMult += 0.1f;
			//blastRadiusMult += 0.25f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Penetrative_Prefix : Explosive_Prefix, IBlastRadiusPrefix {
		public StatModifier BlastRadius() => new(1, 1.10f);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.05f;
			critBonus += 5;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Catastrophic_Prefix : Explosive_Prefix, IBlastRadiusPrefix {
		public StatModifier BlastRadius() => new(1, 1.10f);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.1f;
			useTimeMult -= 0.05f;
			critBonus += 2;
			shootSpeedMult += 0.1f;
			//blastRadiusMult += 0.1f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Persuasive_Prefix : Explosive_Prefix, IBlastRadiusPrefix {
		public StatModifier BlastRadius() => new(1, 1.15f);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			knockbackMult += 0.1f;
			//blastRadiusMult += 0.15f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Dud_Prefix : Explosive_Prefix, ISelfDamagePrefix {
		public StatModifier SelfDamage() => new(1, 0.9f);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult -= 0.1f;
			shootSpeedMult -= 0.1f;
			knockbackMult -= 0.2f;
			//selfDamageMult -= 0.25f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Wimpy_Prefix : Explosive_Prefix, IBlastRadiusPrefix {
		public StatModifier BlastRadius() => new(1, 0.75f);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			shootSpeedMult -= 0.1f;
			knockbackMult -= 0.1f;
			//blastRadiusMult -= 0.25f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Betraying_Prefix : Explosive_Prefix, ISelfDamagePrefix {
		public StatModifier SelfDamage() => new(1, 1.5f);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			//selfDamageMult += 0.5f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Lightweight_Prefix : Explosive_Prefix, IBlastRadiusPrefix {
		public StatModifier BlastRadius() => new(1, 0.90f);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult -= 0.15f;
			useTimeMult -= 0.1f;
			shootSpeedMult += 0.2f;
			//blastRadiusMult += 0.1f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Heavy_Duty_Prefix : Explosive_Prefix, IBlastRadiusPrefix {
		public StatModifier BlastRadius() => new(1, 1.25f);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.15f;
			useTimeMult += 0.2f;
			shootSpeedMult -= 0.1f;
			knockbackMult += 0.15f;
			//blastRadiusMult += 0.25f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Safe_Prefix : Explosive_Prefix, ISelfDamagePrefix {
		public StatModifier SelfDamage() => new(1, 0.6f);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			//selfDamageMult -= 0.2f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => this.GetStatLines();
	}
	public class Guileless_Prefix : Explosive_Prefix, IBlastRadiusPrefix {
		public StatModifier BlastRadius() => new(1, 1.25f);
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
	#endregion explosive prefixes
	public class Imperfect_Prefix : ModPrefix, IOnSpawnProjectilePrefix, IModifyTooltipsPrefix, ICanReforgePrefix, IOnHitNPCPrefix {
		public override PrefixCategory Category => PrefixCategory.Ranged;

		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
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
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.life <= 0) {
				int fromItemType = projectile.GetGlobalProjectile<OriginGlobalProj>().fromItemType;
				if (fromItemType == ModContent.ItemType<Shardcannon>()) {
					ModContent.GetInstance<Shardcannon_Quest>().UpdateKillCount();
				} else if (fromItemType == ModContent.ItemType<Harpoon_Burst_Rifle>()) {
					ModContent.GetInstance<Harpoon_Burst_Rifle_Quest>().UpdateKillCount();
				}
			}
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
				Language.GetTextValue("Mods.Origins.Tooltips.Modifiers.Spread", "+10")
				) {
				IsModifier = true,
				IsModifierBad = true
			};
		}
		public bool CanReforge(Item item) => false;
	}
	#region minion prefixes
	public abstract class MinionPrefix : OriginsPrefix {
		public override PrefixCategory Category => PrefixCategory.Magic;
		public override bool CanRoll(Item item) => ContentSamples.ProjectilesByType[item.shoot] is { minion: true } or { sentry: true };
		public virtual void UpdateProjectile(Projectile projectile, int time) { }
		public virtual void OnSpawn(Projectile projectile, IEntitySource source) { }
	}
	public class Speedy_Prefix : MinionPrefix {
		public override bool CanRoll(Item item) => base.CanRoll(item) && !Origins.ArtifactMinion[item.shoot];
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult -= 0.15f;
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (projectile.minion || projectile.sentry) {
				projectile.GetGlobalProjectile<MinionGlobalProjectile>().bonusUpdates += 0.12f;
			}
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => [
			..base.GetTooltipLines(item),
			new(Mod, "PrefixMinionSpeed", Language.GetOrRegister("Mods.Origins.Prefixes.PrefixMinionSpeed").Format($"+{0.12:P0}")) {
				IsModifier = true
			}
		];
		public override void ModifyValue(ref float valueMult) {
			base.ModifyValue(ref valueMult);
			valueMult *= 1.289f;
		}
	}
	public class Bloated_Prefix : MinionPrefix {
		public override bool CanRoll(Item item) => base.CanRoll(item) && !Origins.ArtifactMinion[item.shoot];
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.15f;
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (projectile.minion || projectile.sentry) {
				projectile.GetGlobalProjectile<MinionGlobalProjectile>().bonusUpdates -= 0.12f;
			}
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => [
			..base.GetTooltipLines(item),
			new(Mod, "PrefixMinionSpeed", Language.GetOrRegister("Mods.Origins.Prefixes.PrefixMinionSpeed").Format($"-{0.12:P0}")) {
				IsModifier = true,
				IsModifierBad = true
			}
		];
		public override void ModifyValue(ref float valueMult) {
			base.ModifyValue(ref valueMult);
			valueMult /= 1.289f;
		}
	}
	#region artifact prefixes
	public class Speedy_Artifact_Prefix : ArtifactPrefixVariant<Speedy_Prefix> {
		public override StatModifier MaxLifeModifier => new(0.9f, 1);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult -= 0.12f;
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (projectile.minion || projectile.sentry) {
				projectile.GetGlobalProjectile<MinionGlobalProjectile>().bonusUpdates += 0.15f;
			}
		}
		public override void ModifyValue(ref float valueMult) {
			base.ModifyValue(ref valueMult);
			valueMult *= 1.277f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => [
			..base.GetTooltipLines(item),
			new(Mod, "PrefixMinionSpeed", Language.GetOrRegister("Mods.Origins.Prefixes.PrefixMinionSpeed").Format($"+{0.15:P0}")) {
				IsModifier = true
			}
		];
	}
	public class Bloated_Artifact_Prefix : ArtifactPrefixVariant<Bloated_Prefix> {
		public override StatModifier MaxLifeModifier => new(1.1f, 1);
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult += 0.12f;
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (projectile.minion || projectile.sentry) {
				projectile.GetGlobalProjectile<MinionGlobalProjectile>().bonusUpdates -= 0.15f;
			}
		}
		public override void ModifyValue(ref float valueMult) {
			base.ModifyValue(ref valueMult);
			valueMult /= 1.277f;
		}
		public override IEnumerable<TooltipLine> GetTooltipLines(Item item) => [
			..base.GetTooltipLines(item),
			new(Mod, "PrefixMinionSpeed", Language.GetOrRegister("Mods.Origins.Prefixes.PrefixMinionSpeed").Format($"-{0.15:P0}")) {
				IsModifier = true,
				IsModifierBad = true
			}
		];
	}
	public abstract class ArtifactMinionPrefix : MinionPrefix {
		public virtual StatModifier MaxLifeModifier => StatModifier.Default;
		public virtual void OnKill(Projectile projectile) { }
		public override bool AllStatChangesHaveEffectOn(Item item) {
			if (MaxLifeModifier != StatModifier.Default) {
				if (!ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out Projectile proj) || proj.ModProjectile is not IArtifactMinion artifactProj || artifactProj.MaxLife == (int)MaxLifeModifier.ApplyTo(artifactProj.MaxLife)) {
					return false;
				}
			}
			return base.AllStatChangesHaveEffectOn(item);
		}
		public override bool CanRoll(Item item) {
			if (!Origins.ArtifactMinion[item.shoot]) return false;
			return base.CanRoll(item);
		}
		public override void ModifyValue(ref float valueMult) {
			base.ModifyValue(ref valueMult);
			valueMult *= 1 + (MaxLifeModifier.Additive * MaxLifeModifier.Multiplicative * (1 + MaxLifeModifier.Base / 100) * (1 + MaxLifeModifier.Flat / 100) - 1) * 0.25f;
		}
	}
	public abstract class ArtifactPrefixVariant<T> : ArtifactMinionPrefix where T : MinionPrefix {
		public override LocalizedText DisplayName => ModContent.GetInstance<T>().DisplayName;
		public override float RollChance(Item item) => ModContent.GetInstance<T>().RollChance(item);
	}
	public class Brittle_Prefix : ArtifactMinionPrefix {
		public override StatModifier MaxLifeModifier => new(0.75f, 1);
	}
	public class Wholesome_Prefix : ArtifactMinionPrefix {
		public override StatModifier MaxLifeModifier => new(1.05f, 1);
		public override bool HasDescription => true;
		public override void SetStaticDefaults() {
			Origins.SpecialPrefix[Type] = true;
		}
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult *= 1.05f;
			knockbackMult *= 1.05f;
			useTimeMult /= 1.05f;
			manaMult *= 1.05f;
		}
		public override void OnKill(Projectile projectile) {
			if (projectile.owner == Main.myPlayer) {
				if ((projectile.ModProjectile is not IArtifactMinion artifact || artifact.Life > 0) && projectile.GetEffectTimer<Wholesome_Sacrifice_Timer>() < 150) return;
				int item = Item.NewItem(
					projectile.GetSource_Death(),
					projectile.Center,
					ItemID.Heart
				);
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
				}
			}
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult *= 1.25f;
		}
		public override void UpdateProjectile(Projectile projectile, int time) {
			if (!projectile.minion && !projectile.sentry) return;
			int team = Main.player[projectile.owner].team;
			foreach (Player healee in Main.ActivePlayers) {
				if (healee.team == team && healee.statLife < healee.statLifeMax2 && projectile.Center.Clamp(healee.Hitbox).WithinRange(projectile.Center, 16 * 15)) {
					healee.lifeRegenTime += 4;
					Dust dust = Dust.NewDustDirect(
						healee.position,
						healee.width,
						healee.height,
						DustID.HealingPlus
					);
					dust.noGravity = true;
				}
			}
		}
		public class Wholesome_Sacrifice_Timer : PrefixProjectileEffectTimer<Wholesome_Prefix> {
			public override bool StartAtZero => true;
			public override bool AppliesToEntity(Projectile projectile) => projectile.friendly && base.AppliesToEntity(projectile);
		}
	}
	public class Vampiric_Prefix : ArtifactMinionPrefix, IOnHitNPCPrefix {
		public override StatModifier MaxLifeModifier => new(0.75f, 1);
		public override bool HasDescription => true;
		public override void SetStaticDefaults() {
			Origins.SpecialPrefix[Type] = true;
		}
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult *= 1.05f;
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult *= 1.25f;
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			OriginExtensions.MinionLifeSteal(projectile, target, damageDone);
		}
	}
	public class Firestarter_Prefix : ArtifactMinionPrefix, IOnHitNPCPrefix {
		public override bool HasDescription => true;
		public override bool HasDescriptionBad => true;
		public override void SetStaticDefaults() {
			Origins.SpecialPrefix[Type] = true;
		}
		public override void UpdateProjectile(Projectile projectile, int time) {
			if (projectile.numUpdates == -1 && time > 0 && time % 30 == 0) {
				projectile.DamageArtifactMinion(2, true);
			}
			if (Main.rand.NextBool(3, 4)) {
				Dust dust = Dust.NewDustDirect(projectile.position - Vector2.One * 2, projectile.width + 4, projectile.height + 4, DustID.Torch, projectile.velocity.X * 0.4f, projectile.velocity.Y * 0.4f, 100);
				if (Main.rand.NextBool(4)) {
					dust.noGravity = false;
					dust.scale = 0.85f;
				} else {
					dust.noGravity = true;
					dust.scale = 1.5f;
				}
				dust.velocity *= 1.8f;
				dust.velocity.Y -= 0.5f;
			}

			Lighting.AddLight(projectile.Center, 1f, 0.3f, 0.1f);
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, Main.rand.Next(240, 361));
		}
		public override void OnKill(Projectile projectile) {
			SoundEngine.PlaySound(SoundID.Item38.WithVolumeScale(0.5f), projectile.Center);
			SoundEngine.PlaySound(SoundID.Item45, projectile.Center);
			if (projectile.owner == Main.myPlayer) {
				Projectile.NewProjectile(
					projectile.GetSource_Death(),
					projectile.Center,
					Vector2.Zero,
					ModContent.ProjectileType<Firestarter_Explosion>(),
					projectile.damage / 2,
					projectile.knockBack
				);
			}
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult *= 1.45f;
		}
		public override bool AllStatChangesHaveEffectOn(Item item) {
			if (!ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out Projectile proj) || proj.ModProjectile is not IArtifactMinion) return false;
			return base.AllStatChangesHaveEffectOn(item);
		}
	}
	public class Firestarter_Explosion : ModProjectile {
		public override string Texture => typeof(Seal_Of_Cinders_Explosion).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 19;
			ProjectileID.Sets.MinionShot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.tileCollide = false;
			Projectile.width = Projectile.height = 110;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
		}
		public override void AI() {
			if (++Projectile.frameCounter > 1) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.timeLeft = 0;
			}
			Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.1f, 0f) * MathHelper.Min(Projectile.frame / 8f, 1));
		}
		public override bool PreDraw(ref Color lightColor) {
			lightColor = new(1f, 1f, 1f, 0f);
			return true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, Main.rand.Next(120, 181));
		}
	}
	#endregion artifact prefixes
	#endregion minion prefixes
	public class Prefix_Timer_Global : GlobalProjectile {
		internal static List<ProjectileEffectTimer> timers = [];
		public override bool InstancePerEntity => true;
		TimerInstance[] timerInstances = [];
		bool initialized = false;
		public override bool PreAI(Projectile projectile) {
			if (!initialized) {
				initialized = true;
				List<TimerInstance> relevantTimers = [];
				for (int i = 0; i < timers.Count; i++) {
					if (timers[i].AppliesToEntity(projectile)) {
						relevantTimers.Add(new(timers[i]));
					}
				}
				timerInstances = relevantTimers.ToArray();
			}
			for (int i = 0; i < timerInstances.Length; i++) {
				ref int time = ref timerInstances[i].time;
				if (time < int.MaxValue) time++;
			}
			return true;
		}
		public ref int Get<TTimer>() where TTimer : ProjectileEffectTimer {
			TTimer timer = ModContent.GetInstance<TTimer>();
			int timerIndex = timer.Index;
			for (int i = 0; i < timerInstances.Length; i++) {
				if (timerInstances[i].Type == timerIndex) return ref timerInstances[i].time;
			}
			Array.Resize(ref timerInstances, timerInstances.Length + 1);
			timerInstances[^1] = new(timer);
			return ref timerInstances[^1].time;
		}
		struct TimerInstance(int type) {
			public TimerInstance(ProjectileEffectTimer effectTimer) : this(effectTimer.Index) {
				if (effectTimer.StartAtZero) time = 0;
			}
			internal readonly int Type = type;
			internal int time = int.MaxValue;
		}
	}
	public static class ProjectileTimerExtensions {
		public static ref int GetEffectTimer<TTimer>(this Projectile projectile) where TTimer : ProjectileEffectTimer {
			return ref projectile.GetGlobalProjectile<Prefix_Timer_Global>().Get<TTimer>();
		}
	}
	public abstract class ProjectileEffectTimer : ILoadable {
		public int Index { get; private set; }
		public virtual bool StartAtZero => false;
		public abstract bool AppliesToEntity(Projectile projectile);
		public void Load(Mod mod) {
			Index = Prefix_Timer_Global.timers.Count;
			Prefix_Timer_Global.timers.Add(this);
		}
		public void Unload() {}
	}
	public abstract class PrefixProjectileEffectTimer<TPrefix> : ProjectileEffectTimer where TPrefix : ModPrefix {
		public override bool AppliesToEntity(Projectile projectile) => projectile.GetGlobalProjectile<OriginGlobalProj>().prefix is TPrefix;
	}
}