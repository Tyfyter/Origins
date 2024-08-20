using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins {
	public class DamageClasses : ILoadable {
		private static DamageClass explosive;
		private static DamageClass thrownExplosive;
		private static DamageClass rangedMagic;
		private static DamageClass summonMagicSpeed;
		public static DamageClass Explosive => explosive ??= ModContent.GetInstance<Explosive>();
		public static DamageClass ThrownExplosive => thrownExplosive ??= ModContent.GetInstance<Thrown_Explosive>();
		public static Dictionary<DamageClass, DamageClass> ExplosiveVersion { get; private set; }
		public static DamageClass RangedMagic => rangedMagic ??= ModContent.GetInstance<Ranged_Magic>();
		public static DamageClass SummonMagicSpeed => summonMagicSpeed ??= ModContent.GetInstance<Summon_Magic_Speed>();
		static FieldInfo _damageClasses;
		static FieldInfo _DamageClasses => _damageClasses ??= typeof(DamageClassLoader).GetField("DamageClasses", BindingFlags.Static | BindingFlags.NonPublic);
		public static List<DamageClass> All => (List<DamageClass>)_DamageClasses.GetValue(null);
		public void Load(Mod mod) {
			List<DamageClass> damageClasses = All;
			int len = damageClasses.Count;
			ExplosiveVersion = new Dictionary<DamageClass, DamageClass>(new DamageClass_Equality_Comparer());
			mod.AddContent(explosive = new Explosive());
			mod.AddContent(thrownExplosive = new Thrown_Explosive());
			for (int i = 0; i < len; i++) {
				DamageClass other = damageClasses[i];
				if (!other.GetEffectInheritance(explosive) && other is not ThrowingDamageClass) {
					if (other is global::Origins.Explosive or ExplosivePlus) {
						ExplosiveVersion.Add(other, other);
					} else {
						ExplosiveVersion.Add(other, ExplosivePlus.CreateAndRegister(other));
					}
				}
			}
			ExplosiveVersion.Add(DamageClass.Throwing, thrownExplosive);
		}

		public void Unload() {
			explosive = null;
			ExplosiveVersion = null;
			rangedMagic = null;
			_damageClasses = null;
		}
	}
	public class DamageClass_Equality_Comparer : IEqualityComparer<DamageClass> {
		public bool Equals(DamageClass x, DamageClass y) => x.Type == y.Type;
		public int GetHashCode([DisallowNull] DamageClass obj) => obj.Type;
	}
	[Autoload(false)]
	public class Explosive : DamageClass {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("explosive damage");
			foreach (var entry in DamageClasses.ExplosiveVersion) {
				try {
					entry.Value.SetStaticDefaults();
				} catch (Exception) { }
			}
		}
		public override void SetDefaultStats(Player player) {
			//player.GetCritChance(this) += 4;
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == DamageClasses.Explosive) {
				return StatInheritanceData.Full;
			}
			if (damageClass == Generic) {
				return new StatInheritanceData(0.5f, 1, 1, 1, 1);
			}
			return StatInheritanceData.None;
		}
	}
	[Autoload(false)]
	public class Thrown_Explosive : DamageClass {
		
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == DamageClasses.Explosive || damageClass == Throwing;
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == DamageClasses.Explosive) {
				return StatInheritanceData.Full;
			}
			if (damageClass == Generic) {
				return new StatInheritanceData(0.5f, 1, 1, 1, 1);
			}
			if (damageClass == Throwing) {
				return new StatInheritanceData(attackSpeedInheritance: 1);
			}
			return StatInheritanceData.None;
		}
		public override void SetDefaultStats(Player player) {
			//player.GetCritChance(this) += 4;
		}
	}
	[Autoload(false)]
	public class ExplosivePlus : DamageClass {
		private readonly string name;
		public override string Name => name;

		readonly DamageClass other;
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.DamageClasses.ExplosivePlus.DisplayName").WithFormatArgs(other.DisplayName);
		public ExplosivePlus(DamageClass other, string name) {
			this.other = other;
			this.name = name;
		}
		internal static ExplosivePlus CreateAndRegister(DamageClass other) {
			ExplosivePlus newClass = new(other, "ExplosivePlus" + other.FullName);
			typeof(ILoadable).GetMethod("Load").Invoke(newClass, [Origins.instance]);
			return newClass;
		}
		public override void SetStaticDefaults() {
			if (other is ThrowingDamageClass) {
				// DisplayName.SetDefault("explosive damage (thrown)");
			} else {
				// DisplayName.SetDefault("explosive " + (other.DisplayName?.GetDefault() ?? other.DisplayName));
			}
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == DamageClasses.Explosive || damageClass == other || other.GetEffectInheritance(damageClass);
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic || damageClass == DamageClasses.Explosive || damageClass == other) {
				return StatInheritanceData.Full;
			}
			return other.GetModifierInheritance(damageClass);
		}
		public override void SetDefaultStats(Player player) {
			//player.GetCritChance(this) -= 4;
		}
	}
	public class Ranged_Magic : DamageClass {
		
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic || damageClass == Ranged || damageClass == Magic) {
				return StatInheritanceData.Full;
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Generic || damageClass == Ranged || damageClass == Magic;
		}
		public override void SetDefaultStats(Player player) {

		}
	}
	public class Summon_Magic_Speed : DamageClass {
		
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic || damageClass == Summon) {
				return StatInheritanceData.Full;
			}
			if (damageClass == Magic) {
				return new StatInheritanceData(attackSpeedInheritance: 1);
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Generic || damageClass == Summon || damageClass == Magic;
		}
		public override void SetDefaultStats(Player player) {

		}
	}
}
