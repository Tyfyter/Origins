using Origins.Tiles;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Core; 
[ReinitializeDuringResizeArrays]
public class PaintedTileLights : ILoadable {
	public static bool[] BuiltInPaintedLights = TileID.Sets.Factory.CreateNamedSet(nameof(BuiltInPaintedLights))
	.Description("Tiles which change light color on their own when painted")
	.RegisterBoolSet(
		TileID.MushroomGrass,
		TileID.MushroomPlants,
		TileID.MushroomTrees,
		TileID.MushroomBlock,
		TileID.ShroomitePlating,
		TileID.MushroomStatue,
		TileID.MushroomVines,
		TileID.MushroomBeam,
		TileID.Cattail
	);
	void ILoadable.Load(Mod mod) {
		try {
			MonoModHooks.Add(typeof(TileLoader).GetMethod(nameof(TileLoader.ModifyLight)), ModifyLight);
		} catch (Exception e) {
			if (Origins.LogLoadingILError(nameof(PaintedTileLights), e)) throw;
		}
	}
	static void ModifyLight(orig_ModifyLight orig, int i, int j, int type, ref float r, ref float g, ref float b) {
		orig(i, j, type, ref r, ref g, ref b);
		if (OriginClientConfig.Instance.DyeLightTiles && !BuiltInPaintedLights[type]) GlowingTileExtensions.PaintLight(ref r, ref g, ref b, Main.tile[i, j].TileColor);
	}
	delegate void orig_ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b);
	void ILoadable.Unload() { }
}
