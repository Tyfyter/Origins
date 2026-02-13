using Origins.Items.Materials;
using Origins.World.BiomeData;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Medicine_Fabricator : ModTile {
		public const int BaseTileID = TileID.AlchemyTable;
		public static int ID { get; private set; }
		public override void Load() {
			new TileItem(this)
			.WithExtraDefaults(item => {
				item.CloneDefaults(ItemID.AlchemyTable);
				item.createTile = Type;
				item.rare++;
				item.value += Item.buyPrice(gold: 1);
			})
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.AlchemyTable)
				.AddIngredient<NE8>(10)
				.AddIngredient<Silicon_Bar>(6)
				.AddTile(TileID.TinkerersWorkbench)
				.Register();
			}).RegisterItem();
		}
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileNoAttach[Type] = true;

			TileID.Sets.DisableSmartCursor[Type] = TileID.Sets.DisableSmartCursor[BaseTileID];
			TileID.Sets.AvoidedByNPCs[Type] = TileID.Sets.AvoidedByNPCs[BaseTileID];

			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(BaseTileID, 0));
			TileObjectData.newTile.LavaDeath = Main.tileLavaDeath[Type];
			AnimationFrameHeight = TileObjectData.newTile.CoordinateHeights.Sum() + 2 * TileObjectData.newTile.Height;
			TileObjectData.addTile(Type);

			AddMapEntry(FromHexRGB(0x0A3623), this.GetTileItem().DisplayName);
			AdjTiles = [ BaseTileID, Type, TileID.Bottles ];
			DustType = Ashen_Biome.DefaultTileDust;
			ID = Type;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 5) {
				frameCounter = 0;
				frame = ++frame % 9;
			}
		}
	}
	public class Medicine_Fapricator_Alchemy : GlobalTile {
		public override int[] AdjTiles(int type) {
			if (type == Medicine_Fabricator.ID) Main.LocalPlayer.alchemyTable = true;
			return base.AdjTiles(type);
		}
	}
}
