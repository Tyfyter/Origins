using Origins.Tiles;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.CrossMod;
public class FancyLighting : IModIntegration {
	static IEnumerable<string> IModIntegration.RequiredMods => [
		"FancyLighting"
	];
	public static Emisarry<bool> SmoothLightingEnabled { get; private set; } = Const(false);
	public static Emisarry<bool> FancyLightingEngineEnabled { get; private set; } = Const(false);
	public static Emisarry<SmoothLightingGlowRenderMode> GlowRenderMode { get; private set; } = Const(SmoothLightingGlowRenderMode.Disabled);
	public static Emisarry<SmoothLightingLightMapOverride> SmoothLightingActiveLightMapOverride { get; private set; } = Const(SmoothLightingLightMapOverride.None);
	public static Emisarry<AmbientOcclusionRenderState> AmbientOcclusionActiveRenderState { get; private set; } = Const(AmbientOcclusionRenderState.Inactive);
	public static bool FancyLightingDrawingRealLight => SmoothLightingActiveLightMapOverride == SmoothLightingLightMapOverride.None;
	public static bool EnhancedGlowMaskSupport => GlowRenderMode == SmoothLightingGlowRenderMode.Accurate;
	public static bool DrawingAOMap => AmbientOcclusionActiveRenderState != AmbientOcclusionRenderState.Inactive;
	static void IModIntegration.LateLoad() {
		Mod fancyLighting = ModLoader.GetMod("FancyLighting");
		int[] data = (int[])fancyLighting.Call("GetConfigStateInfo");
		string[] metadataNames = (string[])fancyLighting.Call("GetConfigStateInfoMetadataNames");
		foreach (ModTile tile in Origins.instance.GetContent<ModTile>()) {
			if (tile is IGlowingModTile glowingTile) fancyLighting.Call("AddCustomTileLighting", tile.Type, (TileLightModifier)glowingTile.FancyLightingGlowColor);
		}
		fancyLighting.Call("AddCustomTileLighting", TileID.DyePlants, (TileLightModifier)DyePlantGlow);
		fancyLighting.Call("AddCustomTileLighting", TileID.Cactus, (TileLightModifier)CactusGlow);
		fancyLighting.Call("AddCustomTileLighting", TileID.Trees, (TileLightModifier)TreeGlow);

		SmoothLightingEnabled = CreateBool("SmoothLightingEnabled");
		FancyLightingEngineEnabled = CreateBool("FancyLightingEngineEnabled");
		GlowRenderMode = Create<SmoothLightingGlowRenderMode>("SmoothLightingGlowRenderMode");
		SmoothLightingActiveLightMapOverride = Create<SmoothLightingLightMapOverride>("SmoothLightingActiveLightMapOverride");
		AmbientOcclusionActiveRenderState = Create<AmbientOcclusionRenderState>("AmbientOcclusionActiveRenderState");

		EnumEmissary<TEnum> Create<TEnum>(string name) where TEnum : struct, Enum => new(data, metadataNames.IndexOf(name));
		BoolEmissary CreateBool(string name) => new(data, metadataNames.IndexOf(name));
	}
	static void DyePlantGlow(Tile tile, int x, int y, ref Vector3 color) {
		if (tile.TileFrameX == 204 || tile.TileFrameX == 202) CactusGlow(tile, x, y, ref color);
	}
	static void CactusGlow(Tile tile, int x, int y, ref Vector3 color) {
		WorldGen.GetCactusType(x, y, tile.TileFrameX, tile.TileFrameY, out int sandType);
		if (PlantLoader.Get<ModCactus>(80, sandType) is IGlowingModPlant glowingPlant) 
			glowingPlant.FancyLightingGlowColor(tile, ref color);
	}
	static void TreeGlow(Tile tile, int x, int y, ref Vector3 color) {
		if (OriginExtensions.GetTreeType(x, y) is IGlowingModTile glowingTree)
			glowingTree.FancyLightingGlowColor(tile, x, y, ref color);
	}
	static void IModIntegration.Recommend() => OriginsModIntegrations.compatRecommendations.Add(Language.GetText("Mods.Origins.ModCompatNotes.AddFancyLighting"));
	public enum SmoothLightingGlowRenderMode {
		Disabled = 0,
		Fast = 1,
		Accurate = 2,
	}
	public enum SmoothLightingLightMapOverride {
		None = 0,
		FullBright = 1,
		NearBlack = 2,
	}
	public enum AmbientOcclusionRenderState {
		Inactive = 0,
		SolidTiles = 1,
		NonSolidTiles = 2,
		TileEntities = 3,
	}
	delegate void TileLightModifier(Tile tile, int x, int y, ref Vector3 color);
	public abstract class Emisarry<T> {
		public abstract T Value { get; }
		public static implicit operator T(Emisarry<T> value) => value.Value;
	}
	class EnumEmissary<TEnum>(int[] array, int index) : Emisarry<TEnum> where TEnum : struct, Enum {
		readonly int[] array = array;
		readonly int index = array.IndexInRange(index) ? index : throw new IndexOutOfRangeException($"Index {index} was out of the range of array (length {array.Length})");
		static readonly Func<int, TEnum> convert;
		public override TEnum Value => convert(array[index]);
		static EnumEmissary() {
			ParameterExpression input = Expression.Parameter(typeof(int));
			convert = Expression.Lambda<Func<int, TEnum>>(Expression.Convert(input, typeof(TEnum)), input).Compile();
		}
	}
	class BoolEmissary(int[] array, int index) : Emisarry<bool> {
		readonly int[] array = array;
		readonly int index = array.IndexInRange(index) ? index : throw new IndexOutOfRangeException($"Index {index} was out of the range of array (length {array.Length})");
		public override bool Value => array[index] != 0;
	}
	class ConstEmissary<T>(T value) : Emisarry<T> {
		public override T Value => value;
	}
	static ConstEmissary<T> Const<T>(T value) => new(value);
}
