using Microsoft.Xna.Framework.Graphics;
using Origins.Dusts;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Industrial_Work_Bench : ModTile, IGlowingModTile, IAshenWireTile {
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
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			const float brightness = 0.8f;
			if (Main.tileFrame[Type] != 0 && tile.TileFrameX == 18 && tile.TileFrameY == 0) color.DoFancyGlow(new(brightness, brightness * 0.45f, brightness * 0.2f), tile.TileColor);
		}
		public override void HitWire(int i, int j) {
			UpdatePowerState(i, j, AshenWireTile.DefaultIsPowered(i, j));
		}
		public void UpdatePowerState(int i, int j, bool powered) {
			bool wasPowered = Main.tile[i, j].TileFrameY >= 18 * 3;
			if (powered == wasPowered) return;
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int x = 0; x < data.Width; x++) {
				for (int y = 0; y < data.Height; y++) {
					Tile tile = Main.tile[left + x, top + y];
					tile.TileFrameY = (short)(tile.TileFrameY % (18 * 3) + (powered ? 3 * 18 : 0));
				}
			}
			if (!NetmodeActive.SinglePlayer) NetMessage.SendTileSquare(-1, left, top, data.Width, data.Height);
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
	}
}
