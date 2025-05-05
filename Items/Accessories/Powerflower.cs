using Origins.Dev;
using Origins.Layers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Powerflower : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Vitality",
			"MagicBoostAcc"
		];
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Accessory_Tangela_Layer.AddTangelaMask<Face_Tangela_Layer>(Item.faceSlot, Texture + "_Face_Tangela");
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 28);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 3);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.dimStarlight = true;
			player.manaFlower = true;
			player.manaCost *= 0.92f;
			Lighting.AddLight(player.Center, 0.3f, 0.3f, 0f);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.ManaFlower)
			.AddIngredient(ModContent.ItemType<Dim_Starlight>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
