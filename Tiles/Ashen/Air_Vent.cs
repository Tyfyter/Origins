using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Air_Vent : OriginTile {
		public override void Load() {
			Mod.AddContent(new TileItem(this).WithExtraStaticDefaults(i => RegisterItemDrop(i.type, -1)));
		}
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			HitSound = SoundID.Tink;

			// Names
			AddMapEntry(new Color(21, 28, 25));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.RandomStyleRange = 4;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.AnchorBottom = new AnchorData();
			TileObjectData.newTile.AnchorWall = true;
			TileObjectData.addTile(Type);
			DustType = Ashen_Biome.DefaultTileDust;
			AnimationFrameHeight = 36;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 0) {
				frameCounter = 0;
				if (++frame >= 4) frame = 0;
			}
		}
		public override void NearbyEffects(int i, int j, bool closer) {
			if (closer) return;
			Vector2 targetPoint = Main.LocalPlayer.Center;
			Vector2 fanPoint = new(i * 16, j * 16);
			OriginSystem instance = OriginSystem.Instance;
			if (!instance.nearestFanSound.HasValue || targetPoint.DistanceSQ(fanPoint) < targetPoint.DistanceSQ(instance.nearestFanSound.Value)) {
				instance.nearestFanSound = fanPoint;
			}
		}
	}
}
