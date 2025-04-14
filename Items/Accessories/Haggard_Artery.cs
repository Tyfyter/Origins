using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Haggard_Artery : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string[] Categories => [
			"Combat"
		];
		public string EntryName => "Origins/" + typeof(Haggard_Artery_Entry).Name;
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.damage = 45;
			Item.DamageType = DamageClasses.Explosive;
			Item.knockBack = 4;
			Item.useTime = 6;
			Item.useAnimation = 1;//used as the numerator for the chance
			Item.reuseDelay = 30;//used as the denominator for the chance
			Item.shoot = ModContent.ProjectileType<Explosive_Artery_P>();
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Explosive_Artery>())
			.AddIngredient(ModContent.ItemType<Messy_Leech>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveArtery = true;
			originPlayer.explosiveArteryItem = Item;
			originPlayer.messyLeech = true;
		}
	}
	public class Haggard_Artery_Entry : JournalEntry {
		public override string TextKey => nameof(Haggard_Artery);
	}
}
