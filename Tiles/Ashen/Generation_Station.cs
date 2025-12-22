using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using PegasusLib.Networking;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Generation_Station : OriginTile, IComplexMineDamageTile, IGlowingModTile {
		public static int ID { get; private set; }
		public override void Load() {
			Mod.AddContent(new TileItem(this, true));
			this.SetupGlowKeys();
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (ShouldGlow(tile)) color = Vector3.Max(color, new Vector3(1.0f, 0.61f, 0.1f));
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;

			// Names
			AddMapEntry(FromHexRGB(0x8B4422), CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 5;
			TileObjectData.newTile.Height = 7;
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
			TileObjectData.newTile.Origin = new Point16(TileObjectData.newTile.Width / 2, TileObjectData.newTile.Height - 1);
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.FlattenAnchors = true;
			AnimationFrameHeight = TileObjectData.newTile.CoordinateHeights.Sum() + TileObjectData.newTile.Height * 2;
			TileObjectData.addTile(Type);
			ID = Type;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (ShouldGlow(Main.tile[i, j])) {
				r = 0.5f;
				g = 0.31f;
				b = 0.05f;
			}
		}
		public static bool ShouldGlow(Tile tile) {
			if (tile.TileFrameX == 2 * 18 && tile.TileFrameY == 4 * 18) return Main.tileFrame[ID] == 1;
			if (tile.TileFrameX >= 2 * 18) return false;
			return tile.TileFrameY == 5 * 18;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 20) {
				frameCounter = 0;
				frame = ++frame % 2;
			}
		}
		public override bool RightClick(int i, int j) {
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			short diff = (short)((Framing.GetTileSafely(left, top).TileFrameX > 0 ? -18 : 18) * data.Width);
			for (int x = 0; x < data.Width; x++) {
				for (int y = 0; y < data.Height; y++) {
					Framing.GetTileSafely(left + x, top + y).TileFrameX += diff;
				}
			}
			new Power_Multitile_Action(i, j, diff < 0).Perform();
			return base.RightClick(i, j);
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public override void PlaceInWorld(int i, int j, Item item) => new Power_Multitile_Action(i, j, true).Perform();
	}
	public record class Power_Multitile_Action(int X, int Y, bool Powered) : SyncedAction {
		public override bool ServerOnly => true;
		public Power_Multitile_Action() : this(default, default, default) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			X = reader.ReadInt32(),
			Y = reader.ReadInt32(),
			Powered = reader.ReadBoolean()
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write(X);
			writer.Write(Y);
			writer.Write(Powered);
		}
		protected override void Perform() {
			Ashen_Wire_Data.SetMultiTilePowered(X, Y, Powered);
		}
	}
}
