using Origins.Items.Other;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod.Items.BasicAccessories;

namespace Origins.Items.Accessories {
	[ExtendsFromMod("ThoriumMod")]
	public class Chambersite_Ring : GemRingBase/*, ICustomWikiStat*/ {
		public override string Texture => typeof(Large_Chambersite).GetDefaultTMLName();
		public override int PrimaryIngredientType => ModContent.ItemType<Chambersite_Item>();
		public override int StatIncrease => 6;
        public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetDamage(DamageClasses.Explosive).Flat += StatIncrease;
		}
	}
}
