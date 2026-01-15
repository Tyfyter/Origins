using Terraria;
using Terraria.ModLoader;

namespace Origins.Core {
	internal class AnyBasicTreeSupportWhatsoever : ILoadable {
		static Point currentTreeTilePos;
		public static Point CurrentTreeTilePos => currentTreeTilePos;
		public void Load(Mod mod) {
			On_WorldGen.GetCommonTreeFoliageData += On_WorldGen_GetCommonTreeFoliageData;
		}
		static bool On_WorldGen_GetCommonTreeFoliageData(On_WorldGen.orig_GetCommonTreeFoliageData orig, int i, int j, int xoffset, ref int treeFrame, ref int treeStyle, out int floorY, out int topTextureFrameWidth, out int topTextureFrameHeight) {
			currentTreeTilePos.X = i;
			currentTreeTilePos.Y = j;
			return orig(i, j, xoffset, ref treeFrame, ref treeStyle, out floorY, out topTextureFrameWidth, out topTextureFrameHeight);
		}
		public void Unload() {}
	}
}
