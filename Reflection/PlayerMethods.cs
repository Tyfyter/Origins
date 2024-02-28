using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class PlayerMethods : ReflectionLoader {
		public override Type ParentType => GetType();
		private delegate void ApplyNPCOnHitEffects_Del(Item sItem, Rectangle itemRectangle, int damage, float knockBack, int npcIndex, int dmgRandomized, int dmgDone);
		[ReflectionParentType(typeof(Player)), ReflectionMemberName("ApplyNPCOnHitEffects")]
		private static ApplyNPCOnHitEffects_Del _ApplyNPCOnHitEffects;
		//private delegate void GrabItems_Del(int playerIndex);
		//private static GrabItems_Del _GrabItems;
		public static void ApplyNPCOnHitEffects(Player player, Item sItem, Rectangle itemRectangle, int damage, float knockBack, int npcIndex, int dmgRandomized, int dmgDone) {
			Basic._target.SetValue(_ApplyNPCOnHitEffects, player);
			_ApplyNPCOnHitEffects(sItem, itemRectangle, damage, knockBack, npcIndex, dmgRandomized, dmgDone);
		}
		/*public static void GrabItems(Player player) {
			Basic._target.SetValue(_ApplyNPCOnHitEffects, player);
			_GrabItems(player.whoAmI);
		}*/
	}
}