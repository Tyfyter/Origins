using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
	public class Defiled_Ice : OriginTile, IDefiledTile {
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
			AddMapEntry(new Color(225, 225, 225));
			mergeID = TileID.IceBlock;
			AddDefiledTile();
			DustType = Defiled_Wastelands.DefaultTileDust;
			HitSound = SoundID.Item50;
		}
		public override void FloorVisuals(Player player) {
			base.FloorVisuals(player);
		}
	}
	public class Defiled_Ice_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.IceBlock, Type);
			ModCompatSets.AnySnowBiomeTiles[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Defiled_Ice>());
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
