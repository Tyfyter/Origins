using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Small_Storage_Container : ModChest {
		public override void Load() {
			Mod.AddContent(new TileItem(this));
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = false;
			for (int i = TileObjectData.GetTileData(Type, 0).RandomStyleRange; i > 0; i--) {
				AddMapEntry(new(36, 33, 31), CreateMapEntryName(), MapChestName);
			}
			AdjTiles = [TileID.Containers];
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override void ModifyTileData() => TileObjectData.newTile.RandomStyleRange = 3;
		public override bool IsLockedChest(int i, int j) => false;
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) => drawData.addFrY = 0;
	}
}
