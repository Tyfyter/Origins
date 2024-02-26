using Origins.Dev;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.Back)]
	public class Snipers_Mark : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat"
		};
		public static int BackSlot { get; private set; }
		public override void SetStaticDefaults() {
			BackSlot = Item.backSlot;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(gold: 6);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().rubyReticle = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Ruby_Reticle>())
			.AddIngredient(ItemID.RangerEmblem)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
