using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class PlayerMethods : ILoadable {
		private delegate void ApplyNPCOnHitEffects_Del(Item sItem, Rectangle itemRectangle, int damage, float knockBack, int npcIndex, int dmgRandomized, int dmgDone);
		private static ApplyNPCOnHitEffects_Del _ApplyNPCOnHitEffects;
		public static FastFieldInfo<Delegate, object> _target;
		public void Load(Mod mod) {
			_target = new("_target", BindingFlags.NonPublic | BindingFlags.Instance);
			_ApplyNPCOnHitEffects = typeof(Player).GetMethod("ApplyNPCOnHitEffects", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate<ApplyNPCOnHitEffects_Del>(new Player());
		}
		public void Unload() {
			_target = null;
			_ApplyNPCOnHitEffects = null;
		}
		public static void ApplyNPCOnHitEffects(Player player, Item sItem, Rectangle itemRectangle, int damage, float knockBack, int npcIndex, int dmgRandomized, int dmgDone) {
			_target.SetValue(_ApplyNPCOnHitEffects, player);
			_ApplyNPCOnHitEffects(sItem, itemRectangle, damage, knockBack, npcIndex, dmgRandomized, dmgDone);
		}
	}
}