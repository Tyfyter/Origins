using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
	public class Chambersite : OriginTile {
		public override void SetStaticDefaults() { //TODO: gemstone properties
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 500;
			Main.tileObsidianKill[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileSpelunker[Type] = true;
			Main.tileMergeDirt[Type] = false;
			AddMapEntry(new Color(10, 60, 25));
			MinPick = 35;
			MineResist = 3;
			DustType = DustID.GemEmerald;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Main.tile[i, j];
			Tile up = Main.tile[i, j - 1];
			Tile down = Main.tile[i, j + 1];
			Tile left = Main.tile[i - 1, j];
			Tile right = Main.tile[i + 1, j];
			int upType = -1;
			int downType = -1;
			int leftType = -1;
			int rightType = -1;
			if (up != null && up.HasUnactuatedTile && !up.BottomSlope) upType = up.TileType;
			if (down != null && down.HasUnactuatedTile && !down.IsHalfBlock && !down.TopSlope) downType = down.TileType;
			if (left != null && left.HasUnactuatedTile && !left.IsHalfBlock && !left.RightSlope && TileID.Sets.OpenDoorID[left.TileType] == -1) leftType = left.TileType;
			if (right != null && right.HasUnactuatedTile && !right.IsHalfBlock && !right.LeftSlope && TileID.Sets.OpenDoorID[right.TileType] == -1) rightType = right.TileType;

			short rand = (short)(WorldGen.genRand.Next(3) * 18);
			if (downType >= 0 && Main.tileSolid[downType] && !Main.tileSolidTop[downType]) {
					tile.TileFrameY = rand;
			} else if (leftType >= 0 && Main.tileSolid[leftType] && !Main.tileSolidTop[leftType]) {
					tile.TileFrameY = (short)(108 + rand);
			} else if (rightType >= 0 && Main.tileSolid[rightType] && !Main.tileSolidTop[rightType]) {
					tile.TileFrameY = (short)(162 + rand);
			} else if (upType >= 0 && Main.tileSolid[upType] && !Main.tileSolidTop[upType]) {
					tile.TileFrameY = (short)(54 + rand);
			} else {
				WorldGen.KillTile(i, j);
			}
			return false;
		}
	}
	public class Chambersite_Item : MaterialItem {
		public override int ResearchUnlockCount => 15;
		public override bool Hardmode => true;
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Chambersite>());
			Item.value = Item.sellPrice(silver: 13);
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = Item.CommonMaxStack;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.CrystalShard, 16)
			.AddCondition(RecipeConditions.ShimmerTransmutation)
			.AddDecraftCondition(Condition.Hardmode)
			.Register();

			Recipe.Create(ItemID.CrystalShard, 16)
			.AddIngredient(Type)
			.AddCondition(RecipeConditions.ShimmerTransmutation)
			.Register();
		}
	}
}
