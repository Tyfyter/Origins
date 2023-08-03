using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class Defiled_Pot : ModTile {
		public override void SetStaticDefaults() {
			Main.tileSpelunker[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileFrameImportant[Type] = true;
			//Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Pots, 0));
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.CoordinateHeights = new int[]{ 16, 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.AnchorInvalidTiles = new int[] { TileID.MagicalIceBlock };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);
			/*TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.FindEmptyChest), -1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.AfterPlacement_Hook), -1, 0, false);
			TileObjectData.newTile.AnchorInvalidTiles = new int[] { TileID.MagicalIceBlock };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);*/
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			WorldGen.CheckPot(i, j);
		}
	}
	public class Defiled_Pot_Item : ModItem {
		public override string Texture => "Origins/Tiles/Defiled/Defiled_Pot";
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Defiled_Pot>());
		}
		public override bool? UseItem(Player player) {
			WorldGen.Place2x2(Player.tileTargetX, Player.tileTargetY, (ushort)Item.createTile, 0);
			return base.UseItem(player);
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Add(new(Mod, "createTile", Item.createTile+""));
		}
	}
}
