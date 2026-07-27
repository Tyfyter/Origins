using MonoMod.Cil;
using Origins.World;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Core;
[ReinitializeDuringResizeArrays]
public class BlockTileInteractions : ILoadable {
	public static bool canSkipCheck = true;
	public static bool[] TilesBlockInteraction = TileID.Sets.Factory.CreateBoolSet();
	static uint lastUpdatedOn = OriginSystem.gameTickCount;
	public void Load(Mod mod) {
		On_Player.IsInTileInteractionRange += On_Player_IsInTileInteractionRange;
		try {
			IL_TileSmartInteractCandidateProvider.FillPotentialTargetTiles += FixSmartCursor;
		} catch (Exception e) {
			if (Origins.LogLoadingILError($"{nameof(BlockTileInteractions)}.{nameof(FixSmartCursor)}", e)) throw;
		}
	}
	static void FixSmartCursor(ILContext il) {
		ILCursor c = new(il);
		int x = -1;
		int y = -1;
		ILLabel skip = default;
		c.GotoNext(MoveType.AfterLabel,
			i => i.MatchLdsflda<Main>(nameof(Main.tile)),
			i => i.MatchLdloc(out x),
			i => i.MatchLdloc(out y)
		);
		int index = c.Index;
		c.GotoNext(
			i => i.MatchCallOrCallvirt<Tile>("active"),
			i => i.MatchBrfalse(out skip)
		);
		c.Index = index;
		c.MoveAfterLabels();
		c.EmitLdloc(x);
		c.EmitLdloc(y);
		c.EmitLdarg1();
		c.EmitCall(((Delegate)ILIsReachable).Method);
		c.EmitBrfalse(skip);
		static bool ILIsReachable(int x, int y, SmartInteractScanSettings settings) => canSkipCheck || IsReachable(x, y, settings);
	}
	static bool On_Player_IsInTileInteractionRange(On_Player.orig_IsInTileInteractionRange orig, Player self, int targetX, int targetY, Terraria.DataStructures.TileReachCheckSettings settings) {
		if (self.whoAmI == Main.myPlayer && !canSkipCheck && !IsReachable(targetX, targetY)) return false;
		if (!orig(self, targetX, targetY, settings)) return false;
		return true;
	}
	public void Unload() { }
	static readonly HashSet<Point> reachablePoints = [];
	//Remove allocations for list & list items
	static readonly List<Point> discardCounted = [];
	static int TileRangeX => Math.Min(Player.tileRangeX, 50);
	static int TileRangeY => Math.Min(Player.tileRangeY, 50);
	static bool IsReachable(int x, int y, SmartInteractScanSettings? settings = null) {
		if (lastUpdatedOn.TrySet(OriginSystem.gameTickCount)) {
			reachablePoints.Clear();
			discardCounted.Clear();
			Player player = Main.LocalPlayer;
			if (settings is SmartInteractScanSettings _settings) {
				range = (_settings.LX, _settings.HX, _settings.LY, _settings.HY);
				if (_settings.HX - _settings.LX > 100) {
					range.LX = (int)(player.position.X / 16f) - TileRangeX + 1;
					range.HX = (int)((player.position.X + player.width) / 16f) + TileRangeX - 1;
					range.LX = Utils.Clamp(range.LX, 10, Main.maxTilesX - 10);
					range.HX = Utils.Clamp(range.HX, 10, Main.maxTilesX - 10);
				}
				if (_settings.HY - _settings.LY > 100) {
					range.LY = (int)(player.position.Y / 16f) - TileRangeY + 1;
					range.HY = (int)((player.position.Y + player.height) / 16f) + TileRangeY - 2;
					range.LY = Utils.Clamp(range.LY, 10, Main.maxTilesY - 10);
					range.HY = Utils.Clamp(range.HY, 10, Main.maxTilesY - 10);
				}
			} else {
				range.LX = (int)(player.position.X / 16f) - TileRangeX + 1;
				range.HX = (int)((player.position.X + player.width) / 16f) + TileRangeX - 1;
				range.LY = (int)(player.position.Y / 16f) - TileRangeY + 1;
				range.HY = (int)((player.position.Y + player.height) / 16f) + TileRangeY - 2;
				range.LX = Utils.Clamp(range.LX, 10, Main.maxTilesX - 10);
				range.HX = Utils.Clamp(range.HX, 10, Main.maxTilesX - 10);
				range.LY = Utils.Clamp(range.LY, 10, Main.maxTilesY - 10);
				range.HY = Utils.Clamp(range.HY, 10, Main.maxTilesY - 10);
			}
			AreaAnalysis.March((int)player.position.X / 16, (int)player.position.Y / 16, AreaAnalysis.Orthogonals, Count, Break, reachablePoints, discardCounted);
		}
		pos.X = x;
		pos.Y = y;
		Tile tile = Main.tile[x, y];
		if (Main.tileFrameImportant[tile.TileType] && TileObjectData.GetTileData(tile) is TileObjectData data) {
			TileUtils.GetMultiTileTopLeft(x, y, data, out pos.X, out pos.Y);
			if (TileLoader.GetTile(tile.TileType) is IMultiTypeMultiTile multitile) pos += multitile.MainTileOffset;
		}
		return reachablePoints.Contains(pos);

		static bool Break(AreaAnalysis analysis) => false;
		static bool Count(Point position) {
			if (position.X < range.LX || position.X > range.HX) return false;
			if (position.Y < range.LY || position.Y > range.HY) return false;
			Tile tile = Main.tile[position];
			if (!tile.HasTile) return true;
			if (TilesBlockInteraction[tile.TileType]) {
				if (Main.tileFrameImportant[tile.TileType] && TileObjectData.GetTileData(tile) is TileObjectData data) {
					TileUtils.GetMultiTileTopLeft(position.X, position.Y, data, out pos.X, out pos.Y);
					if (TileLoader.GetTile(tile.TileType) is IMultiTypeMultiTile multitile) pos += multitile.MainTileOffset;
					reachablePoints.Add(pos);
				}
				return false;
			}
			return true;
		}
	}
	static Point pos = new();
	static (int LX, int HX, int LY, int HY) range;
}
