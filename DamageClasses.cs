using System;
using System.Collections;
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
		private static DamageClass incantation;
		private static DamageClass meleeMagic;
		private static DamageClass rangedSummon;
		private static DamageClass noSummonInherit;
		private static DamageClass rangedExplosiveInherit;
		public static DamageClass Explosive => explosive ??= ModContent.GetInstance<Explosive>();
		public static DamageClass ThrownExplosive => thrownExplosive ??= ModContent.GetInstance<Thrown_Explosive>();
		public static Dictionary<DamageClass, DamageClass> ExplosiveVersion { get; private set; }
		public static DamageClass RangedMagic => rangedMagic ??= ModContent.GetInstance<Ranged_Magic>();
		public static DamageClass Incantation => incantation ??= ModContent.GetInstance<Incantation>();
		public static DamageClass MeleeMagic => meleeMagic ??= ModContent.GetInstance<Melee_Magic>();
		public static DamageClass RangedSummon => rangedSummon ??= ModContent.GetInstance<Ranged_Summon>();
		public static DamageClass NoSummonInherit => noSummonInherit ??= ModContent.GetInstance<No_Summon_Inherit>();
		public static DamageClass RangedExplosiveInherit => rangedExplosiveInherit ??= ModContent.GetInstance<Ranged_Explosive_Inherit>();
		public static DamageClassList All => new();
		public static HashSet<DamageClass> HideInConfig { get; private set; } = [];
		public void Load(Mod mod) {
			DamageClassList damageClasses = All;
			int len = damageClasses.Count;
			ExplosiveVersion = new Dictionary<DamageClass, DamageClass>(new DamageClass_Equality_Comparer());
			mod.AddContent(explosive = new Explosive());
			mod.AddContent(thrownExplosive = new Thrown_Explosive());
			ExplosiveVersion.Add(DamageClass.Generic, explosive);
			ExplosiveVersion.Add(DamageClass.Throwing, thrownExplosive);
			for (int i = 0; i < len; i++) {
				DamageClass other = damageClasses[i];
				if (!other.GetEffectInheritance(explosive) && !ExplosiveVersion.ContainsKey(other)) {
					if (other is global::Origins.Explosive or ExplosivePlus) {
						ExplosiveVersion.Add(other, other);
					} else {
						ExplosiveVersion.Add(other, ExplosivePlus.CreateAndRegister(other));
					}
					HideInConfig.Add(ExplosiveVersion[other]);
				}
			}
		}
		delegate void CombineStats<T>(Player player, DamageClass from, StatInheritanceData inheritance, ref T value);
		public static void Patch() {
			static StatInheritanceData GenericInherit(DamageClass damageClass) {
				return damageClass.GetModifierInheritance(DamageClass.Generic);
			}
			static void ApplyStatShare<T>(Player player, DamageClass forClass, ref T value, CombineStats<T> applyShare) {
				float statShareStr = player.OriginPlayer().statSharePercent;
				if (statShareStr > 0) {
					foreach (DamageClass other in new StatShareEnumerator(forClass)) {
						StatInheritanceData inheritance = StatShare(forClass, other, statShareStr);
						if (inheritance.damageInheritance  == 0 &&
							inheritance.critChanceInheritance == 0 &&
							inheritance.attackSpeedInheritance == 0 &&
							inheritance.armorPenInheritance == 0 &&
							inheritance.knockbackInheritance == 0
						) continue;
						applyShare(player, other, inheritance, ref value);
					}
				}
			}
			static StatInheritanceData StatShare(DamageClass forClass, DamageClass fromClass, float strength) {
				return Scale(Subtract(forClass.GetModifierInheritance(DamageClass.Generic), forClass.GetModifierInheritance(fromClass)), strength);
			}
			static StatInheritanceData Subtract(StatInheritanceData a, StatInheritanceData b) => new(
				a.damageInheritance - b.damageInheritance,
				a.critChanceInheritance - b.critChanceInheritance,
				a.attackSpeedInheritance - b.attackSpeedInheritance,
				a.armorPenInheritance - b.armorPenInheritance,
				a.knockbackInheritance - b.knockbackInheritance
			);
			static StatInheritanceData Scale(StatInheritanceData a, float b) => new(
				a.damageInheritance * b,
				a.critChanceInheritance * b,
				a.attackSpeedInheritance * b,
				a.armorPenInheritance * b,
				a.knockbackInheritance * b
			);
			On_Player.GetTotalDamage += (orig, self, damageClass) => {
				StatModifier stat = orig(self, damageClass);
				if (damageClass == DamageClass.Ranged) stat = stat.CombineWith(self.GetDamage(RangedExplosiveInherit));
				if (damageClass != DamageClass.Summon) stat = stat.CombineWith(self.GetDamage(NoSummonInherit).Scale(GenericInherit(damageClass).damageInheritance));
				ApplyStatShare(self, damageClass, ref stat, StatShare);
				return stat;
				static void StatShare(Player player, DamageClass from, StatInheritanceData inheritance, ref StatModifier stat) {
					stat = stat.CombineWith(player.GetDamage(from).Scale(inheritance.damageInheritance));
				}
			};
			On_Player.GetTotalKnockback += (orig, self, damageClass) => {
				StatModifier stat = orig(self, damageClass);
				if (damageClass == DamageClass.Ranged) stat = stat.CombineWith(self.GetKnockback(RangedExplosiveInherit));
				if (damageClass != DamageClass.Summon) stat = stat.CombineWith(self.GetKnockback(NoSummonInherit).Scale(GenericInherit(damageClass).knockbackInheritance));
				ApplyStatShare(self, damageClass, ref stat, StatShare);
				return stat;
				static void StatShare(Player player, DamageClass from, StatInheritanceData inheritance, ref StatModifier stat) {
					stat = stat.CombineWith(player.GetKnockback(from).Scale(inheritance.knockbackInheritance));
				}
			};
			On_Player.GetTotalCritChance += (orig, self, damageClass) => {
				float stat = orig(self, damageClass);
				if (damageClass == DamageClass.Ranged) stat += self.GetCritChance(RangedExplosiveInherit);
				if (damageClass != DamageClass.Summon) stat += self.GetCritChance(NoSummonInherit) * GenericInherit(damageClass).critChanceInheritance;
				ApplyStatShare(self, damageClass, ref stat, StatShare);
				return stat;
				static void StatShare(Player player, DamageClass from, StatInheritanceData inheritance, ref float stat) {
					stat += player.GetCritChance(from) * inheritance.critChanceInheritance;
				}
			};
			On_Player.GetTotalArmorPenetration += (orig, self, damageClass) => {
				float stat = orig(self, damageClass);
				if (damageClass == DamageClass.Ranged) stat += self.GetArmorPenetration(RangedExplosiveInherit);
				if (damageClass != DamageClass.Summon) stat += self.GetArmorPenetration(NoSummonInherit) * GenericInherit(damageClass).armorPenInheritance;
				ApplyStatShare(self, damageClass, ref stat, StatShare);
				return stat;
				static void StatShare(Player player, DamageClass from, StatInheritanceData inheritance, ref float stat) {
					stat += player.GetArmorPenetration(from) * inheritance.armorPenInheritance;
				}
			};
			On_Player.GetTotalAttackSpeed += (orig, self, damageClass) => {
				float stat = orig(self, damageClass);
				if (damageClass == DamageClass.Ranged) stat += self.GetAttackSpeed(RangedExplosiveInherit) - 1f;
				if (damageClass != DamageClass.Summon) stat += (self.GetAttackSpeed(NoSummonInherit) - 1f) * GenericInherit(damageClass).attackSpeedInheritance;
				ApplyStatShare(self, damageClass, ref stat, StatShare);
				return stat;
				static void StatShare(Player player, DamageClass from, StatInheritanceData inheritance, ref float stat) {
					stat += (player.GetAttackSpeed(from) - 1f) * inheritance.attackSpeedInheritance;
				}
			};

			/*
			HashSet<MethodInfo> patched = [];
			foreach (DamageClass damageClass in All) {
				MethodInfo method = damageClass.GetType().GetMethod(nameof(DamageClass.GetModifierInheritance));
				if (!patched.Add(method)) continue;
				MonoModHooks.Add(method, (Func<DamageClass, DamageClass, StatInheritanceData> orig, DamageClass self, DamageClass other) => {
					if (self != DamageClass.Summon && other == NoSummonInherit) return orig(self, DamageClass.Generic);
					if (self == DamageClass.Ranged && other == RangedExplosiveInherit) return StatInheritanceData.Full;
					return orig(self, other);
				});
			}
			*/
		}
		public void Unload() {
			foreach (FieldInfo field in GetType().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
				if (field.FieldType.IsClass) {
					field.SetValue(null, null);
				}
			}
		}

		ref struct StatShareEnumerator(DamageClass forClass) {
			public readonly DamageClass Current => DamageClassLoader.GetDamageClass(index);
			int index;
			public bool MoveNext() {
				loop:
				if (++index >= DamageClassLoader.DamageClassCount) return false;
				if (forClass == DamageClass.Ranged && Current == RangedExplosiveInherit) goto loop;
				if (Current == NoSummonInherit) goto loop;
				if (Current == DamageClass.Default) goto loop;
				if (Current == forClass) goto loop;
				return true;
			}
			public void Reset() => index = 0;
			public readonly StatShareEnumerator GetEnumerator() => this;
		}
	}
	public readonly struct DamageClassList : IList<DamageClass> {
		public readonly DamageClass this[int index] {
			get => DamageClassLoader.GetDamageClass(index);
			set => throw new InvalidOperationException();
		}
		public int Count => DamageClassLoader.DamageClassCount;
		public bool IsReadOnly => true;

		public bool Contains(DamageClass item) {
			throw new NotImplementedException();
		}
		public void CopyTo(DamageClass[] array, int arrayIndex) {
			for (int i = 0; i < Count; i++) {
				array[i + arrayIndex] = this[i];
			}
		}
		public int IndexOf(DamageClass item) {
			if (Equals(item, this[item.Type])) return item.Type;
			for (int i = 0; i < Count; i++) {
				if (Equals(item, this[i])) return i;
			}
			return -1;
		}
		public IEnumerator<DamageClass> GetEnumerator() => new Enumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		struct Enumerator : IEnumerator<DamageClass> {
			public readonly DamageClass Current => DamageClassLoader.GetDamageClass(index);
			readonly object IEnumerator.Current => DamageClassLoader.GetDamageClass(index);
			int index;
			public readonly void Dispose() { }
			public bool MoveNext() => ++index < DamageClassLoader.DamageClassCount;
			public void Reset() {
				index = 0;
			}
		}
		public void Add(DamageClass item) => throw new InvalidOperationException();
		public void Clear() => throw new InvalidOperationException();
		public void Insert(int index, DamageClass item) => throw new InvalidOperationException();
		public bool Remove(DamageClass item) => throw new InvalidOperationException();
		public void RemoveAt(int index) => throw new InvalidOperationException();
	}
	public class DamageClass_Equality_Comparer : IEqualityComparer<DamageClass> {
		public bool Equals(DamageClass x, DamageClass y) => x.Type == y.Type;
		public int GetHashCode([DisallowNull] DamageClass obj) => obj.Type;
	}
	[Autoload(false)]
	public class Explosive : DamageClass {
		public override void SetDefaultStats(Player player) {
			//player.GetCritChance(this) += 4;
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == DamageClasses.Explosive || damageClass == DamageClasses.RangedExplosiveInherit) {
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
			return damageClass == DamageClasses.Explosive || damageClass == DamageClasses.RangedExplosiveInherit || damageClass == Throwing;
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
		public override bool GetPrefixInheritance(DamageClass damageClass) => DamageClasses.Explosive.GetsPrefixesFor(damageClass) || Throwing.GetsPrefixesFor(damageClass);
		public override void SetDefaultStats(Player player) {
			player.GetCritChance(this) -= 4;
		}
	}
	[Autoload(false)]
	public class ExplosivePlus(DamageClass other, string name) : DamageClass {
		public override string Name => name;

		readonly DamageClass other = other;
		public override LocalizedText DisplayName => Language.GetOrRegister($"Mods.Origins.DamageClasses.ExplosivePlus{(other.Mod is null ? "" : "Modded")}.DisplayName").WithFormatArgs(other.DisplayName);

		internal static ExplosivePlus CreateAndRegister(DamageClass other) {
			ExplosivePlus newClass = new(other, "ExplosivePlus" + other.FullName);
			typeof(ILoadable).GetMethod("Load").Invoke(newClass, [Origins.instance]);
			return newClass;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == DamageClasses.Explosive || damageClass == DamageClasses.RangedExplosiveInherit || damageClass == other || other.GetEffectInheritance(damageClass);
		}
		public override bool GetPrefixInheritance(DamageClass damageClass) => DamageClasses.Explosive.GetsPrefixesFor(damageClass) || other.GetsPrefixesFor(damageClass);
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic || damageClass == DamageClasses.Explosive || damageClass == other) {
				return StatInheritanceData.Full;
			}
			return other.GetModifierInheritance(damageClass);
		}
		public override void SetDefaultStats(Player player) {
			player.GetCritChance(this) -= 4;
		}
		public override bool UseStandardCritCalcs => other.UseStandardCritCalcs;
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
	public class Incantation : DamageClass {
		
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
	public class Melee_Magic : DamageClass {
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic || damageClass == Melee || damageClass == Magic) return StatInheritanceData.Full;
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Generic || damageClass == Melee || damageClass == Magic;
		}
	}
	public class Ranged_Summon : DamageClass {
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic || damageClass == Ranged || damageClass == Summon) {
				return StatInheritanceData.Full;
			}
			return StatInheritanceData.None;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == Generic || damageClass == Ranged || damageClass == Summon;
		}
		public override bool UseStandardCritCalcs => false;
		public override void SetDefaultStats(Player player) {

		}
	}
	public class No_Summon_Inherit : DamageClass { }
	public class Ranged_Explosive_Inherit : DamageClass { }
	public class Chambersite_Mine_Launcher_Damage : DamageClass {
		public override LocalizedText DisplayName => Language.GetOrRegister($"Mods.Origins.DamageClasses.ExplosivePlus.DisplayName").WithFormatArgs(Ranged.DisplayName);

		internal static ExplosivePlus CreateAndRegister(DamageClass other) {
			ExplosivePlus newClass = new(other, "ExplosivePlus" + other.FullName);
			typeof(ILoadable).GetMethod("Load").Invoke(newClass, [Origins.instance]);
			return newClass;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == DamageClasses.ExplosiveVersion[Ranged] || DamageClasses.ExplosiveVersion[Ranged].GetEffectInheritance(damageClass);
		}
		public override bool GetPrefixInheritance(DamageClass damageClass) {
			return damageClass == DamageClasses.ExplosiveVersion[Ranged] || DamageClasses.ExplosiveVersion[Ranged].GetEffectInheritance(damageClass);
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if (damageClass == Generic) {
				return StatInheritanceData.Full;
			}
			return DamageClasses.ExplosiveVersion[Ranged].GetModifierInheritance(damageClass);
		}
		public override void SetDefaultStats(Player player) {
			player.GetCritChance(this) -= 4;
		}
	}
}
