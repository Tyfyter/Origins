using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Oil_Derrick : ModTile, IComplexMineDamageTile {
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
			AddMapEntry(new Color(40, 30, 18), CreateMapEntryName());
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
		public override bool CanExplode(int i, int j) => false;
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (frameCounter.CycleUp(5)) frame.CycleUp(7);
		}
	}
}
