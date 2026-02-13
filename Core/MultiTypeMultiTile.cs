using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Core {
	public interface IMultiTypeMultiTile {
		public bool IsValidTile(Tile tile, int left, int top);
		public bool CanBlockPlacement(Tile tile, int left, int top) => tile.HasTile;
		/// <summary>
		/// Called for each tile of the multitile's type in the area of the multitile when it's broken
		/// Return false to prevent the tile being broken
		/// </summary>
		public bool ShouldBreak(int x, int y, int left, int top) => true;
	}
	internal class MultiTypeMultiTile : ILoadable {
		public delegate bool PlacePartCheck(int i, int j);
		public static PlacementHook PlaceWhereTrue(TileObjectData tileData, PlacePartCheck placeCheck) => new((x, y, type, style, _, alternate) => {
			int num2 = 0;
			int num3 = 0;
			int num4 = tileData.CalculatePlacementStyle(style, alternate, 0);
			int num5 = 0;
			if (tileData.StyleWrapLimit > 0) {
				num5 = num4 / tileData.StyleWrapLimit * tileData.StyleLineSkip;
				num4 %= tileData.StyleWrapLimit;
			}
			if (tileData.StyleHorizontal) {
				num2 = tileData.CoordinateFullWidth * num4;
				num3 = tileData.CoordinateFullHeight * num5;
			} else {
				num2 = tileData.CoordinateFullWidth * num5;
				num3 = tileData.CoordinateFullHeight * num4;
			}
			for (int i = 0; i < tileData.Width; i++) {
				for (int j = 0; j < tileData.Height; j++) {
					if (!placeCheck(i, j)) continue;
					Tile tileSafely = Framing.GetTileSafely(x + i, y + j);
					if (tileSafely.HasTile && tileSafely.TileType != TileID.RollingCactus && (Main.tileCut[tileSafely.TileType] || TileID.Sets.BreakableWhenPlacing[tileSafely.TileType])) {
						WorldGen.KillTile(x + i, y + j);
						if (!Main.tile[x + i, y + j].HasTile && !NetmodeActive.SinglePlayer) {
							NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, x + i, y + j);
						}
					}
				}
			}
			for (int i = 0; i < tileData.Width; i++) {
				int num8 = num2 + i * (tileData.CoordinateWidth + tileData.CoordinatePadding);
				int num9 = num3;
				for (int j = 0; j < tileData.Height; j++) {
					if (placeCheck(i, j)) {
						Tile tile = Framing.GetTileSafely(x + i, y + j);
						if (!tile.HasTile) {
							tile.SetActive(true);
							tile.TileFrameX = (short)num8;
							tile.TileFrameY = (short)num9;
							tile.TileType = (ushort)type;
						}
					}
					num9 += tileData.CoordinateHeights[j] + tileData.CoordinatePadding;
				}
			}
			return 0;
		}, -1, 0, true);
		void ILoadable.Load(Mod mod) {
			try {
				MonoModHooks.Modify(typeof(TileLoader).GetMethod(nameof(TileLoader.CheckModTile)), IL_MultiTypeMultiTile);
				IL_TileObject.CanPlace += IL_TileObject_CanPlace;
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(IL_MultiTypeMultiTile), e)) throw;
			}
		}

		static void IL_TileObject_CanPlace(ILContext il) {
			ILCursor c = new(il);
			List<(int x, int y)> coordinateSets = [];
			//IL_001a: ldarg.0
			//IL_001b: ldloc.0
			//IL_001c: callvirt instance valuetype Terraria.DataStructures.Point16 Terraria.ObjectData.TileObjectData::get_Origin()
			//IL_0021: ldfld int16 Terraria.DataStructures.Point16::X
			//IL_0026: sub
			//IL_0027: stloc.1
			//IL_0028: ldarg.1
			//IL_0029: ldloc.0
			//IL_002a: callvirt instance valuetype Terraria.DataStructures.Point16 Terraria.ObjectData.TileObjectData::get_Origin()
			//IL_002f: ldfld int16 Terraria.DataStructures.Point16::Y
			//IL_0034: sub
			//IL_0035: stloc.2
			int curX = -1;
			int curY = -1;
			while (c.TryGotoNext(
				i => i.MatchLdarg(0),
				i => i.MatchLdloc(out _),
				i => i.MatchCallvirt<TileObjectData>("get_" + nameof(TileObjectData.Origin)),
				i => i.MatchLdfld<Point16>(nameof(Point16.X)),
				i => i.MatchSub(),
				i => i.MatchStloc(out curX),
				i => i.MatchLdarg(1),
				i => i.MatchLdloc(out _),
				i => i.MatchCallvirt<TileObjectData>("get_" + nameof(TileObjectData.Origin)),
				i => i.MatchLdfld<Point16>(nameof(Point16.Y)),
				i => i.MatchSub(),
				i => i.MatchStloc(out curY)
			)) {
				coordinateSets.Add((curX, curY));
			}
			c = new(il);
			//IL_05c0: ldsfld bool[] Terraria.Main::tileCut
			//IL_05c5: ldloca.s 38
			//IL_05c7: call instance uint16& Terraria.Tile::get_type()
			//IL_05cc: ldind.u2
			//IL_05cd: ldelem.u1
			//IL_05ce: brfalse.s IL_05ee
			ILLabel label = default;
			int tile = -1;
			c.GotoNext(MoveType.AfterLabel,
				i => i.MatchLdloca(out tile),
				i => i.MatchCall<Tile>($"active"),
				i => i.MatchBrfalse(out label),
				i => i.MatchLdsfld<Main>(nameof(Main.tileCut)),
				i => i.MatchLdloca(tile),
				i => i.MatchCall<Tile>($"get_type"),
				i => i.MatchLdindU2(),
				i => i.MatchLdelemU1()
			);
			int correctIndex = -1;

			//IL_055d: ldloc.s 12
			//IL_055f: ldloc.s 36
			//IL_0561: add
			//IL_0562: ldloc.s 13
			//IL_0564: ldloc.s 37
			//IL_0566: add
			//IL_0567: call valuetype Terraria.Tile Terraria.Framing::GetTileSafely(int32, int32)
			//IL_056c: stloc.s 38
			c.FindPrev(out _,
				i => i.MatchLdloc(coordinateSets[correctIndex].x),
				i => i.MatchLdloc(out _),
				i => i.MatchAdd(),
				i => {
					if (i.MatchLdloc(out int rightY)) {
						for (int j = 0; j < coordinateSets.Count; j++) {
							correctIndex = j;
							if (coordinateSets[j].y == rightY) break;
						}
						return true;
					}
					return false;
				},
				i => i.MatchLdloc(out _),
				i => i.MatchAdd(),
				i => i.MatchCall<Framing>(nameof(Framing.GetTileSafely)),
				i => i.MatchStloc(tile)
			);
			c.EmitLdarg2();
			c.EmitLdloc(tile);
			c.EmitLdloc(coordinateSets[correctIndex].x);
			c.EmitLdloc(coordinateSets[correctIndex].y);
			c.EmitDelegate((int tileType, Tile tile, int left, int top) => {
				return TileLoader.GetTile(tileType) is IMultiTypeMultiTile multiTypeMultiTile && multiTypeMultiTile.IsValidTile(tile, left, top);
			});
			c.EmitBrtrue(label);
		}

		static void IL_MultiTypeMultiTile(ILContext il) {
			ILCursor c = new(il);
			ILLabel label = default;
			int x = -1;
			int y = -1;
			c.GotoNext(MoveType.AfterLabel,
				i => i.MatchLdloca(out _),
				i => i.MatchCall<Tile>($"get_type"),
				i => i.MatchLdindU2(),
				i => i.MatchLdarg2(),
				i => i.MatchBeq(out label)
			);
			c.GotoPrev(MoveType.AfterLabel,
				i => i.MatchLdsflda<Main>(nameof(Main.tile)),
				i => i.MatchLdloc(out x),
				i => i.MatchLdloc(out y),
				i => i.MatchCall<Tilemap>("get_Item"),
				i => i.MatchStloc(out _),
				i => i.MatchLdloca(out _),
				i => i.MatchCall<Tile>("active"),
				i => i.MatchBrfalse(out _)
			);
			c.EmitLdarg2();
			c.EmitLdloc(x);
			c.EmitLdloc(y);
			c.EmitLdarg(il.Method.Parameters.First(p => p.Name == "i"));
			c.EmitLdarg(il.Method.Parameters.First(p => p.Name == "j"));
			c.EmitDelegate((int tileType, int x, int y, int left, int top) => {
				return TileLoader.GetTile(tileType) is IMultiTypeMultiTile multiTypeMultiTile && multiTypeMultiTile.IsValidTile(Main.tile[x, y], left, top);
			});
			c.EmitBrtrue(label);

			c.GotoNext(MoveType.Before,
				i => i.MatchBrfalse(out label),

				i => i.MatchLdloc(out x),
				i => i.MatchLdloc(out y),
				i => i.MatchLdcI4(0),
				i => i.MatchLdcI4(0),
				i => i.MatchLdcI4(0),
				i => i.MatchCall<WorldGen>(nameof(WorldGen.KillTile))
			);
			c.EmitBrfalse(label);
			c.EmitLdarg2();
			c.EmitLdloc(x);
			c.EmitLdloc(y);
			c.EmitLdarg(il.Method.Parameters.First(p => p.Name == "i"));
			c.EmitLdarg(il.Method.Parameters.First(p => p.Name == "j"));
			c.EmitDelegate((int tileType, int x, int y, int left, int top) => {
				if (TileLoader.GetTile(tileType) is IMultiTypeMultiTile multiTypeMultiTile) return multiTypeMultiTile.ShouldBreak(x, y, left, top);
				return true;
			});
		}
		void ILoadable.Unload() { }
	}
}
