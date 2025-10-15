using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Melee {
	public class Nematoclaw : ModItem, IElementalItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.ToxicSource
		];
		public ushort Element => Elements.Acid;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FetidBaghnakhs);
			Item.damage = 48;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, 180);
		}
	}
}
