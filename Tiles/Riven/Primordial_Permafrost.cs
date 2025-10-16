using Origins.Journal;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Primordial_Permafrost : OriginTile, IRivenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.Ices[Type] = true;
			TileID.Sets.IcesSlush[Type] = true;
			TileID.Sets.IcesSnow[Type] = true;
			TileID.Sets.IceSkateSlippery[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.Conversion.Ice[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileBrick[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(100, 200, 200));
			mergeID = TileID.IceBlock;
			DustType = DustID.Water_Snow;
			HitSound = SoundID.Item50;
		}
		public override void FloorVisuals(Player player) {
			base.FloorVisuals(player);
		}
	}
	public class Primordial_Permafrost_Item : ModItem, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Primordial_Permafrost_Entry).Name;
		public class Primordial_Permafrost_Entry : JournalEntry {
			public override string TextKey => "Primordial_Permafrost";
			public override JournalSortIndex SortIndex => new("Riven", 3);
		}
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.IceBlock, Type);
			ModCompatSets.AnySnowBiomeTiles[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Primordial_Permafrost>());
		}
		public override void AddRecipes() {
			Recipe.Create(ItemID.IceTorch, 3)
			.AddIngredient(ItemID.Torch, 3)
			.AddIngredient(Type)
			.SortAfterFirstRecipesOf(ItemID.IceTorch)
			.Register();
		}
	}
}
