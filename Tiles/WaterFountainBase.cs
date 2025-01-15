using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles {
	public abstract class WaterFountainBase<TWaterStyle> : ModTile where TWaterStyle : ModWaterStyle {
		public virtual int Height => 4;
		public virtual int Frames => 6;
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.WaterFountain, 0));
			TileObjectData.newTile.Height = Height;
			TileObjectData.newTile.Origin = new(1, Height - 1);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(6, 157, 44), Language.GetText("MapObject.WaterFountain"));
		}
		public static bool IsEnabled(int i, int j) => Main.tile[i, j].TileFrameY >= 72;
		public static bool IsEnabled(Tile tile) => tile.TileFrameY >= 72;
		public override void NearbyEffects(int i, int j, bool closer) {
			if (closer && IsEnabled(i, j)) Main.SceneMetrics.ActiveFountainColor = ModContent.GetInstance<TWaterStyle>().Slot;
		}
		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			
			player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type, TileObjectData.GetTileStyle(Main.tile[i, j]));
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override bool RightClick(int i, int j) {
			SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
			ToggleTile(i, j);
			return true;
		}

		public override void HitWire(int i, int j) {
			ToggleTile(i, j);
		}

		// ToggleTile is a method that contains code shared by HitWire and RightClick, since they both toggle the state of the tile.
		// Note that TileFrameY doesn't necessarily match up with the image that is drawn, AnimateTile and AnimateIndividualTile contribute to the drawing decisions.
		public void ToggleTile(int i, int j) {
			Tile tile = Main.tile[i, j];
			int height = Height * 18;
			int topX = i - tile.TileFrameX % 36 / 18;
			int topY = j - tile.TileFrameY % height / 18;

			short frameAdjustment = (short)(tile.TileFrameY >= height ? -height : height);

			for (int x = topX; x < topX + 2; x++) {
				for (int y = topY; y < topY + Height; y++) {
					Main.tile[x, y].TileFrameY += frameAdjustment;

					if (Wiring.running) {
						Wiring.SkipWire(x, y);
					}
				}
			}

			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendTileSquare(-1, topX, topY, 3, 2);
			}
		}

		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 4) {
				frameCounter = 0;
				frame = (frame + 1) % Frames;
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameY >= Height * 18) {
				frameYOffset = Main.tileFrame[type] * Height * 18;
			} else {
				frameYOffset = 0;
			}
		}
	}
}
