using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Table_Saw : ModTile {
		public const int BaseTileID = TileID.Sawmill;
		public static int ID { get; private set; }
		public override void Load() {
			new TileItem(this)
			.WithExtraDefaults(item => {
				item.CloneDefaults(ItemID.Sawmill);
				item.createTile = Type;
				//item.rare++;
				item.value += Item.buyPrice(gold: 1);
			}).WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Cog, 5)
				.AddRecipeGroup(ALRecipeGroups.SilverBars)
				.AddIngredient<Scrap>(18)
				.AddTile<Metal_Presser>()
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
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.SetHeight(2);
			TileObjectData.newTile.Origin = new(1, 1);
			AnimationFrameHeight = TileObjectData.newTile.CoordinateHeights.Sum() + 2 * TileObjectData.newTile.Height;
			TileObjectData.addTile(Type);

			AddMapEntry(FromHexRGB(0x0A3623), this.GetTileItem().DisplayName);
			AdjTiles = [BaseTileID, Type];
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
			ID = Type;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (frameCounter.CycleUp(5)) frame.CycleUp(3);
		}
	}
}
