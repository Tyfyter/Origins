using Microsoft.Xna.Framework.Graphics;
using Origins.Dusts;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Industrial_Work_Bench : ModTile, IGlowingModTile {
		public const int BaseTileID = TileID.HeavyWorkBench;
		public static int ID { get; private set; }
		public TileItem Item { get; protected set; }
		public override void Load() {
			Mod.AddContent(Item = new TileItem(this).WithExtraDefaults(item => {
				item.CloneDefaults(ItemID.HeavyWorkBench);
				item.createTile = Type;
				item.rare++;
				item.value += Terraria.Item.buyPrice(gold: 1);
			}));
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
			TileObjectData.newTile.SetHeight(3);
			AnimationFrameHeight = TileObjectData.newTile.CoordinateHeights.Sum() + 2 * TileObjectData.newTile.Height;
			TileObjectData.addTile(Type);

			AddMapEntry(FromHexRGB(0x0A3623), Item.DisplayName);
			AdjTiles = [BaseTileID, Type];
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;

			RegisterItemDrop(Item.Type);
			ID = Type;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (frameCounter.CycleUp(7)) frame.CycleUp(2);
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			if (Main.tileFrame[Type] != 0) this.DrawTileGlow(i, j, spriteBatch);
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			const float brightness = 0.8f;
			if (Main.tileFrame[Type] == 0 && tile.TileFrameX == 18 && tile.TileFrameY == 0) color = Vector3.Max(color, new(brightness, brightness * 0.45f, brightness * 0.2f));
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
	}
}
