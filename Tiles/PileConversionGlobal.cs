using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles {
	public class PileConversionGlobal : GlobalTile {
		static Dictionary<PileConversionParentData, List<PileConversionData>> conversions = new(new ConversionEqualityComparer());
		static Dictionary<int, PileConversionParentData> inverse = [];
		public override void Unload() {
			conversions = null;
			inverse = null;
		}
		public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak) {
			if (!Main.tileFrameImportant[type]) return true;
			int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
			if (style < 0) {
				if (type == TileID.SmallPiles) DoSmallPileConversion(i, j);
				return true;
			}
			int baseType = type;
			TileObjectData sharedData = TileObjectData.GetTileData(baseType, style);
			if (inverse.TryGetValue(type, out PileConversionParentData baseTile)) {
				baseType = baseTile.Type;
				style = baseTile.Styles.First();
				sharedData = TileObjectData.GetTileData(baseType, style);
			}
			if (baseType == TileID.SmallPiles) {
				DoSmallPileConversion(i, j);
				return true;
			}
			if (TryGetConversion(baseType, style, out List<PileConversionData> conversionDatas, ref baseTile)) {
				if (baseTile is null) return true;
				OriginExtensions.GetMultiTileTopLeft(i, j, sharedData, out int baseX, out int baseY);
				ushort destinationType = (ushort)baseType;
				for (int k = 0; k < conversionDatas.Count; k++) {
					PileConversionData data = conversionDatas[k];
					int count = 0;
					for (int x = 0; x < sharedData.Width; x++) {
						if (data.FloorTiles.Contains(Main.tile[baseX + x, baseY + sharedData.Height].TileType) && ++count >= sharedData.Width * data.RequiredPortion) {
							destinationType = data.Result;
							goto foundBestMatch;
						}
					}
				}
				foundBestMatch:
				if (destinationType != type) {
					bool isBaseTile = destinationType == baseTile.Type;
					int destinationStyle = isBaseTile ? WorldGen.genRand.Next(baseTile.Styles.ToArray()) : WorldGen.genRand.Next(TileObjectData.GetTileData(destinationType, 0).RandomStyleRange);
					int wrappingIsBad = 0;
					if (isBaseTile && sharedData.StyleWrapLimit > 0) {
						destinationStyle = sharedData.CalculatePlacementStyle(destinationStyle, 0, 0) % sharedData.StyleWrapLimit;
						wrappingIsBad = destinationStyle / sharedData.StyleWrapLimit * sharedData.StyleLineSkip;
					}
					int baseFrameX, baseFrameY;
					if (sharedData.StyleHorizontal) {
						baseFrameX = sharedData.CoordinateFullWidth * destinationStyle;
						baseFrameY = sharedData.CoordinateFullHeight * wrappingIsBad;
					} else {
						baseFrameX = sharedData.CoordinateFullWidth * wrappingIsBad;
						baseFrameY = sharedData.CoordinateFullHeight * destinationStyle;
					}
					for (int x = 0; x < sharedData.Width; x++) {
						int frameY = baseFrameY;
						for (int y = 0; y < sharedData.Height; y++) {
							Tile tile = Main.tile[baseX + x, baseY + y];
							tile.TileType = destinationType;
							tile.TileFrameX = (short)(baseFrameX + x * (sharedData.CoordinateWidth + sharedData.CoordinatePadding));
							tile.TileFrameY = (short)frameY;
							frameY += sharedData.CoordinateHeights[y] + sharedData.CoordinatePadding;
						}
					}
				}
			}
			return true;
		}
		public static void DoSmallPileConversion(int i, int j) {
			Tile startTile = Main.tile[i, j];
			int type = startTile.TileType;
			int baseType = type;
			bool isSmall = startTile.TileFrameY < 18;
			int tileWidth = isSmall ? 1 : 2;
			int styleWidth = tileWidth * 18;
			int style = startTile.TileFrameX / styleWidth;
			if (!isSmall) style += 100;
			if (inverse.TryGetValue(type, out PileConversionParentData baseTile)) {
				baseType = baseTile.Type;
				style = baseTile.Styles.First();
			}
			if (TryGetConversion(baseType, style, out List<PileConversionData> conversionDatas, ref baseTile)) {
				if (baseTile is null) return;
				int baseX = i;
				if (!isSmall && startTile.TileFrameX - (startTile.TileFrameX / styleWidth) * styleWidth >= 18) baseX -= 1;
				ushort destinationType = (ushort)baseType;
				for (int k = 0; k < conversionDatas.Count; k++) {
					PileConversionData data = conversionDatas[k];
					int count = 0;
					for (int x = 0; x < tileWidth; x++) {
						if (data.FloorTiles.Contains(Main.tile[baseX + x, j + 1].TileType) && ++count >= tileWidth * data.RequiredPortion) {
							destinationType = data.Result;
							goto foundBestMatch;
						}
					}
				}
				foundBestMatch:
				if (destinationType != type) {
					bool isBaseTile = destinationType == baseTile.Type;
					int destinationStyle = isBaseTile ? WorldGen.genRand.Next(baseTile.Styles.ToArray()) : WorldGen.genRand.Next(TileObjectData.GetTileData(destinationType, 0).RandomStyleRange);
					int frameY = 0;
					if (destinationStyle >= 100) {
						frameY = 18;
						destinationStyle -= 100;
					}
					int baseFrameX = destinationStyle * styleWidth;
					for (int x = 0; x < tileWidth; x++) {
						Tile tile = Main.tile[baseX + x, j];
						tile.TileType = destinationType;
						tile.TileFrameX = (short)(baseFrameX + x * 18);
						tile.TileFrameY = (short)frameY;
					}
				}
			}
		}
		public static void AddConversion(int type, HashSet<int> styles, int result, HashSet<int> floorTiles, float requiredPortion = 0.5f) {
			AddConversion(new(type, styles), new((ushort)result, floorTiles, requiredPortion));
		}
		private static void AddConversion(PileConversionParentData parent, PileConversionData data) {
			if (conversions.TryGetValue(parent, out List<PileConversionData> values)) values.Add(data);
			else conversions.Add(parent, [data]);
			inverse.Add(data.Result, parent);
		}
		static bool TryGetConversion(int type, int style, out List<PileConversionData> conversionDatas, ref PileConversionParentData baseTile) {
			foreach (KeyValuePair<PileConversionParentData, List<PileConversionData>> data in conversions) {
				if (data.Key.Type == type && data.Key.Styles.Contains(style)) {
					conversionDatas = data.Value;
					baseTile = data.Key;
					return true;
				}
			}
			conversionDatas = null;
			return false;
		}
		class ConversionEqualityComparer : IEqualityComparer<PileConversionParentData> {
			public bool Equals(PileConversionParentData x, PileConversionParentData y) => x.Type == y.Type && x.Styles.SetEquals(y.Styles);
			public int GetHashCode([DisallowNull] PileConversionParentData obj) => HashCode.Combine(obj.Type, obj.Styles.Count.GetHashCode());
		}
		public record PileConversionParentData(int Type, HashSet<int> Styles);
		public record PileConversionData(ushort Result, HashSet<int> FloorTiles, float RequiredPortion = 0.5f);
	}
}
