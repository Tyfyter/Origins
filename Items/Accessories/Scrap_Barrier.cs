using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shield)]
	public class Scrap_Barrier : ModItem, ICustomWikiStat {
		public override void SetDefaults() {
			Item.DefaultToAccessory(48, 36);
			Item.rare = CursedRarity.ID;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().scrapBarrierCursed = true;
		}
		public override void AddRecipes() {
			Recipe.Create(ModContent.ItemType<Scrap_Barrier_Uncursed>())
			.AddIngredient(Type)
			.AddTile(TileID.BewitchingTable)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Shield)]
	public class Scrap_Barrier_Uncursed : ModItem, ICustomWikiStat {
		public override string Texture => "Origins/Items/Accessories/Scrap_Barrier";
		public override void SetDefaults() {
			Item.DefaultToAccessory(48, 36);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			player.endurance += (1 - player.endurance) * 0.05f;
			player.lifeRegen += 4;
		}
	}
	public class Scrap_Barrier_Debuff : ModBuff {
		public override string Texture => "Origins/Items/Accessories/Scrap_Barrier";
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
		}
	}
}
