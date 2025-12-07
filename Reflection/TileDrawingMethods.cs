#pragma warning disable CS0649
#pragma warning disable IDE0044
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using static Terraria.GameContent.TilePaintSystemV2;
using PegasusLib;
using PegasusLib.Reflection;
using DelegateMethods = PegasusLib.Reflection.DelegateMethods;

namespace Origins.Reflection {
	public class TileDrawingMethods : ReflectionLoader {
		private delegate void DrawBasicTile_Del(Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY, TileDrawInfo drawData, Rectangle normalTileRect, Vector2 normalTilePosition);
		[ReflectionParentType(typeof(TileDrawing)), ReflectionMemberName("DrawBasicTile"), ReflectionDefaultInstance(typeof(Main), "instance", "TilesRenderer")]
		private static DrawBasicTile_Del _DrawBasicTile;
		public static FastFieldInfo<TileDrawing, Dictionary<Point, int>> _chestPositions;
		public static void DrawBasicTile(TileDrawing self, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY, TileDrawInfo drawData, Rectangle normalTileRect, Vector2 normalTilePosition) {
			DelegateMethods._target.SetValue(_DrawBasicTile, self);
			_DrawBasicTile(screenPosition, screenOffset, tileX, tileY, drawData, normalTileRect, normalTilePosition);
		}
	}
}