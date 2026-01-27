using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	//[AutoloadEquip(EquipType.Shoes)]
	public class Exo_Legs : ModItem, ICustomWikiStat {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Swiftness;
		public string[] Categories => [
			WikiCategories.Movement
		];
		public static int ShoeID { get; private set; }
		public override void SetStaticDefaults() {
			ShoeID = Item.shoeSlot;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 32);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.hasMagiluminescence = true;
			player.noFallDmg = true;
			player.maxFallSpeed *= 1.35f;
			player.OriginPlayer().exoLegs = true;
		}
		public static void UpdateSpeeds(Player player) {
			Max(ref player.runAcceleration, 0.08f * 2.5f);
			Max(ref player.maxRunSpeed, 3f * 2);
			Max(ref player.accRunSpeed, 3f * 3.5f);
			Max(ref player.accRunSpeed, player.maxRunSpeed);

			Max(ref Player.jumpSpeed, 5.01f * 1.8f);
			Max(ref Player.jumpHeight, 15 + 8);
		}
	}
}
