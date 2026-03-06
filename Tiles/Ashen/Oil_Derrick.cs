using Origins.Core;
using Origins.World.BiomeData;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Oil_Derrick : ModTile, IComplexMineDamageTile {
		Sound ambientSound = EnvironmentSounds.Register<Sound>();
		public override void Load() => new TileItem(this).RegisterItem();
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Width = 8;
			TileObjectData.newTile.SetHeight(7);
			TileObjectData.newTile.SetOriginBottomCenter();
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
				if (Main.tileFrame[type] == 1) SoundEngine.PlaySound(SoundID.Item108.WithPitch(-1.4f).WithVolume(0.32f * mult), position);
				if (Main.rand.NextBool(150)) SoundEngine.PlaySound(SoundID.Item148.WithPitch(-0.5f).WithVolume(0.48f * mult), position);
			}
		}
	}
}
