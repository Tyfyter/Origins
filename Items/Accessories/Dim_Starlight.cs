using Origins.Dev;
using Origins.Journal;
using Origins.Tiles.Defiled;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Dim_Starlight : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string[] Categories => [
			"Vitality",
			"MagicBoostAcc"
		];
		static short glowmask;
		public string EntryName => "Origins/" + typeof(Dim_Starlight_Entry).Name;
		public class Dim_Starlight_Entry : JournalEntry {
			public override string TextKey => "Dim_Starlight";
			public override JournalSortIndex SortIndex => new("The_Defiled", 7);
		}
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.BandofStarpower] = ModContent.ItemType<Dim_Starlight>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Dim_Starlight>()] = ItemID.BandofStarpower;
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
		}
        public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient(ItemID.BandofStarpower)
            .AddIngredient(ModContent.ItemType<Wilting_Rose_Item>())
            .AddTile(TileID.TinkerersWorkbench)
			.DisableDecraft()
            .Register();

            Recipe.Create(ItemID.PanicNecklace)
            .AddIngredient(ItemID.SwiftnessPotion)
            .AddIngredient(Type)
            .AddTile(TileID.TinkerersWorkbench)
			.DisableDecraft()
			.Register();
        }
        public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.dimStarlight = true;
			float light = 0.2f + (originPlayer.dimStarlightCooldown / 1000f);
			Lighting.AddLight(player.Center, light, light, light);
		}
	}
}
