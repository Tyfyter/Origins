using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.World.BiomeData;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Oil_Derrick : ModTile, IComplexMineDamageTile, IMultiTypeMultiTile {
		readonly Sound ambientSound = EnvironmentSounds.Register<Sound>();
		public override void Load() => new TileItem(this).RegisterItem();
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Width = 8;
			TileObjectData.newTile.SetHeight(7);
			TileObjectData.newTile.SetOriginBottomCenter();
			TileObjectData.newTile.HookPlaceOverride = MultiTypeMultiTile.PlaceWhereTrue(TileObjectData.newTile, IsPart);
			TileObjectData.newTile.AnchorBottom = new(AnchorType.SolidTile, TileObjectData.newTile.Width - 1, 1);
			TileObjectData.newTile.FlattenAnchors = true;
			this.SetAnimationHeight();
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(40, 30, 18), this.GetTileItem().DisplayName);
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
		public override bool CanExplode(int i, int j) => false;
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (frameCounter.CycleUp(8)) frame.CycleUp(7);
		}
		public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects) {
			position.Y += 2;
			return base.PreDrawPlacementPreview(i, j, spriteBatch, ref frame, ref position, ref color, validPlacement, ref spriteEffects);
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = 2;
		}
		public override void NearbyEffects(int i, int j, bool closer) {
			if (closer) return;
			if (Main.tile[i, j].TileFrameX < 18 * 2) ambientSound.TrySetNearest(new(i * 16 + 8, j * 16 + 8));
		}
		class Sound : AEnvironmentSound {
			SlotId droning;
			public override void UpdateSound(Vector2 position) {
				int type = ModContent.TileType<Oil_Derrick>();
				float mult = 1 / float.Max(position.DistanceSQ(Main.Camera.Center) / (16 * 20 * 16 * 20), 1);
				droning.PlaySoundIfInactive(Origins.Sounds.RadioBroadcaster.WithPitch(0.5f), position, playingSound => {
					if (GetPosition() is not Vector2 pos) return false;
					playingSound.Volume = 0.2f / float.Max(pos.DistanceSQ(Main.Camera.Center) / (16 * 20 * 16 * 20), 1);
					return true;
				});
				if (Main.tileFrame[type] == 5) SoundEngine.PlaySound(SoundID.Item108.WithPitch(-1.4f).WithVolume(0.32f * mult), position);
				if (Main.rand.NextBool(150)) SoundEngine.PlaySound(SoundID.Item148.WithPitch(-0.5f).WithVolume(0.48f * mult), position);
			}
		}
		public static bool IsPart(int i, int j) {
			switch (i, j) {
				case (0, 0):
				case (0, 1):
				case (0, 2):
				case (0, 3):
				case (0, 6):

				case (1, 0):
				case (1, 1):
				case (1, 2):

				case (2, 0):
				case (2, 1):
				case (2, 2):

				case (3, 0):
				case (4, 0):
				case (5, 0):
				return false;
			}
			return true;
		}

		public bool IsValidTile(Tile tile, int left, int top) {
			(int i, int j) = tile.GetTilePosition();
			i -= left;
			j -= top;
			if (IsPart(i, j)) {
				if (tile.TileType != Type) return false;
				Tile topLeft = Main.tile[left, top];
				return tile.TileFrameX == topLeft.TileFrameX + i * 18 && tile.TileFrameY == topLeft.TileFrameY + j * 18;
			}
			return true;
		}
		public bool ShouldBreak(int x, int y, int left, int top) => IsPart(x - left, y - top);
	}
}
