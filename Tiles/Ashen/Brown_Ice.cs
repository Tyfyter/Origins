using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Brown_Ice : OriginTile, IAshenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.Ices[Type] = true;
			TileID.Sets.IcesSlush[Type] = true;
			TileID.Sets.IcesSnow[Type] = true;
			TileID.Sets.IceSkateSlippery[Type] = true;
			TileID.Sets.Conversion.Ice[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[TileID.IceBlock];
			Main.tileMerge[Type][TileID.IceBlock] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(146, 101, 70));
			mergeID = TileID.IceBlock;
			DustType = Ashen_Biome.DefaultTileDust;
			HitSound = SoundID.Item50;
		}
		public override void FloorVisuals(Player player) {
			base.FloorVisuals(player);
		}
	}
	public class Brown_Ice_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.IceBlock, Type);
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Brown_Ice>());
			ModCompatSets.AnySnowBiomeTiles[Type] = true;
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
