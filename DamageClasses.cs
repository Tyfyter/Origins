﻿using PegasusLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;

namespace Origins {
	public class DamageClasses : ILoadable {
		private static DamageClass explosive;
		private static DamageClass thrownExplosive;
		private static DamageClass rangedMagic;
		private static DamageClass incantation;
		private static DamageClass meleeMagic;
		public static DamageClass Explosive => explosive ??= ModContent.GetInstance<Explosive>();
		public static DamageClass ThrownExplosive => thrownExplosive ??= ModContent.GetInstance<Thrown_Explosive>();
		public static Dictionary<DamageClass, DamageClass> ExplosiveVersion { get; private set; }
		public static DamageClass RangedMagic => rangedMagic ??= ModContent.GetInstance<Ranged_Magic>();
		public static DamageClass Incantation => incantation ??= ModContent.GetInstance<Incantation>();
		public static DamageClass MeleeMagic => meleeMagic ??= ModContent.GetInstance<Melee_Magic>();
		public static DamageClassList All => new();
		public static HashSet<DamageClass> HideInConfig { get; private set; } = [];
		public void Load(Mod mod) {
			IList<DamageClass> damageClasses = All;
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

		public void Unload() {
			explosive = null;
			ExplosiveVersion = null;
			rangedMagic = null;
			HideInConfig = null;
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
		public override bool GetPrefixInheritance(DamageClass damageClass) => DamageClasses.Explosive.GetsPrefixesFor(damageClass) || Throwing.GetsPrefixesFor(damageClass);
		public override void SetDefaultStats(Player player) {
			//player.GetCritChance(this) += 4;
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
			return damageClass == DamageClasses.Explosive || damageClass == other || other.GetEffectInheritance(damageClass);
		}
		public override bool GetPrefixInheritance(DamageClass damageClass) => DamageClasses.Explosive.GetsPrefixesFor(damageClass) || other.GetsPrefixesFor(damageClass);
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
}
