using Microsoft.Xna.Framework.Graphics;
using Origins.Reflection;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Modular_Bunk : BedBase {
		AutoLoadingAsset<Texture2D> innerTexture = typeof(Modular_Bunk).GetDefaultTMLName() + "_inner";
		public override Color MapColor { get; }
		public override void ModifyTileData() {
			TileObjectData.newTile.Height = 5;
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
			TileObjectData.newTile.Origin = new Point16(TileObjectData.newTile.Width / 2, TileObjectData.newTile.Height - 1);
			foreach (TileObjectData data in TileMethods.GetAlternates(TileObjectData.newTile)) {
				data.Height = TileObjectData.newTile.Height;
				data.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
				data.Origin = new Point16(TileObjectData.newTile.Width / 2, TileObjectData.newTile.Height - 1);
			}
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			OriginsSets.Tiles.MultitileCollisionOffset[Type] = OffsetBookcaseCollision;
		}
		static void OffsetBookcaseCollision(Tile tile, ref float y, ref int height) {
			if(tile.TileFrameY / 18 is not 0 and not 3)
				height = -1600;
		}
		public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY) {
			// Because beds have special smart interaction, this splits up the left and right side into the necessary 2x2 sections
			width = 2; // Default to the Width defined for TileObjectData.newTile
			height = 3; // Default to the Height defined for TileObjectData.newTile
						//extraY = 0; // Depends on how you set up frameHeight and CoordinateHeights and CoordinatePaddingFix.Y
		}
		public override void ModifySleepingTargetInfo(int i, int j, ref TileRestingInfo info) {
			info.VisualOffset.Y += 0;
		}
		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			int spawnX = (i - (tile.TileFrameX / 18)) + (tile.TileFrameX >= 72 ? 5 : 2);
			int spawnY = j + 2;
			switch (tile.TileFrameY / 18) {
				case 1:
				case 4:
				spawnY--;
				break;
				case 2:
				spawnY -= 2;
				break;
			}
			if (!Player.IsHoveringOverABottomSideOfABed(i, j)) { // This assumes your bed is 4x2 with 2x2 sections. You have to write your own code here otherwise
				if (player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance)) {
					player.GamepadEnableGrappleCooldown();
					player.sleeping.StartSleeping(player, i, spawnY - 2);
				}
			} else {
				player.FindSpawn();
				if (player.SpawnX == spawnX && player.SpawnY == spawnY) {
					player.RemoveSpawn();
					Main.NewText(Language.GetTextValue("Game.SpawnPointRemoved"), 255, 240, 20);
				} else if (Player.CheckSpawn(spawnX, spawnY)) {
					player.ChangeSpawn(spawnX, spawnY);
					Main.NewText(Language.GetTextValue("Game.SpawnPointSet"), 255, 240, 20);
				}
			}
			return true;
		}
		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			if (!Player.IsHoveringOverABottomSideOfABed(i, j)) {
				if (player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance)) { // Match condition in RightClick. Interaction should only show if clicking it does something
					player.noThrow = 2;
					player.cursorItemIconEnabled = true;
					player.cursorItemIconID = ItemID.SleepingIcon;
				}
			} else {
				player.noThrow = 2;
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = Item.Type;
			}
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];
			Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition;
			if (!Main.drawToScreen) {
				pos.X += Main.offScreenRange;
				pos.Y += Main.offScreenRange;
			}
			Lighting.GetCornerColors(i, j, out VertexColors vertices);
			Vector4 destination = new(pos, 16, 16);
			Rectangle source = new(tile.TileFrameX, tile.TileFrameY, 16, 16);
			Main.tileBatch.Draw(
				innerTexture,
				destination,
				source,
				vertices
			);
			return base.PreDraw(i, j, spriteBatch);
		}
	}
	public class Players_Behind_Tiles_Overlay() : Overlay(EffectPriority.High, RenderLayers.Walls), ILoadable {
		readonly List<Player> playersBehindTiles = [];
		public override void Draw(SpriteBatch spriteBatch) {
			playersBehindTiles.Clear();
			List<Player> playersBehindNPCs = MainReflection.PlayersThatDrawBehindNPCs;
			for (int i = 0; i < playersBehindNPCs.Count; i++) {
				Point coords = (playersBehindNPCs[i].Bottom + new Vector2(0f, -2f)).ToTileCoordinates();
				if (Framing.GetTileSafely(coords).TileIsType(ModContent.TileType<Modular_Bunk>())) {
					playersBehindTiles.Add(playersBehindNPCs[i]);
				}
			}
			playersBehindNPCs.RemoveAll(playersBehindTiles.Contains);
			SpriteBatchState state = spriteBatch.GetState();
			try {
				spriteBatch.End();
				Main.PotionOfReturnRenderer.DrawPlayers(Main.Camera, playersBehindTiles.Where(p => p.PotionOfReturnOriginalUsePosition.HasValue));
				Main.PlayerRenderer.DrawPlayers(Main.Camera, playersBehindTiles);
			} finally {
				spriteBatch.Begin(state);
			}
		}
		public override void Update(GameTime gameTime) { }
		public override void Activate(Vector2 position, params object[] args) {
			Opacity = 1;
			Mode = OverlayMode.Active;
		}
		public override void Deactivate(params object[] args) { }
		public override bool IsVisible() => true;
		public static void ForceActive() {
			if (Overlays.Scene[typeof(Players_Behind_Tiles_Overlay).FullName].Mode != OverlayMode.Active) {
				Overlays.Scene.Activate(typeof(Players_Behind_Tiles_Overlay).FullName, default);
			}
		}
		public void Load(Mod mod) {
			Overlays.Scene[GetType().FullName] = this;
		}
		public void Unload() { }
	}
}
