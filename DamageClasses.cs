using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins {
	public class DamageClasses : ILoadable {
		private static DamageClass explosive;
		public static DamageClass Explosive => explosive ??= ModContent.GetInstance<Explosive>();
		public static Dictionary<DamageClass, DamageClass> ExplosiveVersion { get; private set; }
		public void Load(Mod mod) {
			List<DamageClass> damageClasses = (List<DamageClass>)(typeof(DamageClassLoader).GetField("DamageClasses", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null));
			int len = damageClasses.Count;
			ExplosiveVersion = new Dictionary<DamageClass, DamageClass>(new DamageClass_Equality_Comparer());
			for (int i = 0; i < len; i++) {
				DamageClass other = damageClasses[i];
				ExplosiveVersion.Add(other, ExplosivePlus.CreateAndRegister(other));
			}
		}

		public void Unload() {
			explosive = null;
			ExplosiveVersion = null;
		}
	}
	public class DamageClass_Equality_Comparer : IEqualityComparer<DamageClass> {
		public bool Equals(DamageClass x, DamageClass y) => x.Type == y.Type;

		public int GetHashCode([DisallowNull] DamageClass obj) => obj.Type;
	}
	public class Explosive : DamageClass {
		public override void SetDefaultStats(Player player) {
			player.GetCritChance(this) += 4;
		}
	}
	[Autoload(false)]
	public class ExplosivePlus : DamageClass {
		DamageClass other;
		public ExplosivePlus(DamageClass other) {
			this.other = other;
		}
		internal static ExplosivePlus CreateAndRegister(DamageClass other) {
			ExplosivePlus newClass = new(other);
			newClass.Register();
			return newClass;
		}
		public override bool GetEffectInheritance(DamageClass damageClass) {
			return damageClass == DamageClasses.Explosive || damageClass == other;
		}
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
			if(damageClass == Generic || damageClass == DamageClasses.Explosive || damageClass == other) {
				return StatInheritanceData.Full;
			}
			return StatInheritanceData.None;
		}
		public override void SetDefaultStats(Player player) {
			//player.GetCritChance(this) += 4;
		}
	}
}
