using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Metal_Presser : ModTile, IGlowingModTile {
		public const int BaseTileID = TileID.HeavyWorkBench;
		public static int ID { get; private set; }
		public TileItem Item { get; protected set; }
		public override void Load() {
			Mod.AddContent(Item = new TileItem(this).WithExtraDefaults(item => {
				item.CloneDefaults(ItemID.HeavyWorkBench);
				item.createTile = Type;
				item.rare++;
				item.value += Terraria.Item.buyPrice(gold: 1, silver: 50);
			}).WithOnAddRecipes(item => Recipe.Create(Item.Type)
				.AddIngredient(ItemID.HeavyWorkBench)
				.AddIngredient<NE8>(10)
				.AddIngredient<Silicon_Bar>(6)
				.AddTile(TileID.TinkerersWorkbench)
				.Register()));
			this.SetupGlowKeys();
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileNoAttach[Type] = true;

			TileID.Sets.DisableSmartCursor[Type] = TileID.Sets.DisableSmartCursor[BaseTileID];
			TileID.Sets.AvoidedByNPCs[Type] = TileID.Sets.AvoidedByNPCs[BaseTileID];

			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(BaseTileID, 0));
			TileObjectData.newTile.LavaDeath = Main.tileLavaDeath[Type];
			AnimationFrameHeight = TileObjectData.newTile.CoordinateHeights.Sum() + 2 * TileObjectData.newTile.Height;
			TileObjectData.addTile(Type);

			AddMapEntry(FromHexRGB(0x0A3623), Item.DisplayName);
			AdjTiles = [BaseTileID, Type];
			DustType = Ashen_Biome.DefaultTileDust;

			RegisterItemDrop(Item.Type);
			ID = Type;
		}
		public static bool ShouldGlow(Tile tile) {
			int frameY = tile.TileFrameY / 18;
			return frameY == 1 || frameY == 7;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 7) {
				frameCounter = 0;
				frame = ++frame % 8;
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			int frameY = frameYOffset / 18;
			Tile tile = Framing.GetTileSafely(i, j);

			if (frameY == 3 && tile.TileFrameY / 18 == 1) {
				Vector2 pos = new Vector2(i, j).ToWorldCoordinates() + new Vector2(0, 5);
				Vector2 vel = -(Vector2.UnitY * 0.7f);
				if (tile.TileFrameX / 18 == 0) {
					pos.X += Main.rand.Next(3, 14);
					vel.X = -0.8f;
				} else if (tile.TileFrameX / 18 == 2) {
					pos.X -= Main.rand.Next(3, 14);
					vel.X = 0.8f;
				} else pos.X += Main.rand.Next(-6, 7);
				Dust.NewDustPerfect(pos, DustID.Torch, vel);
			}
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
	}
}
