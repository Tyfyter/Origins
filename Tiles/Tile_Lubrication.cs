using Avalon.Tiles;
using CalamityMod.NPCs.TownNPCs;
using MonoMod.Cil;
using Origins.Core;
using Origins.Items.Tools.Liquids;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Tiles;
[ReinitializeDuringResizeArrays]
public class Tile_Lubrication : TESystem<Tile_Lubrication.Data> {
	public static int[] MaxOil = TileID.Sets.Factory.CreateIntSet();
	public static bool isExtractinatingWithOil = false;
	public override void Load() {
		Origins.DoILEdit(TileLoader.RightClick, IL_ApplyOil);
		On_Player.PlaceThing_ItemInExtractinator += On_Player_PlaceThing_ItemInExtractinator;
	}
	public override void SetStaticDefaults() {
		MaxOil[TileID.Extractinator] = 300;
		MaxOil[TileID.ChlorophyteExtractinator] = 300;
	}
	static void IL_ApplyOil(ILContext il) {
		ILCursor c = new(il);
		c.EmitLdarg0();
		c.EmitLdarg1();
		c.EmitCall(((Delegate)TryApplyOil).Method);
		ILLabel label = c.DefineLabel();
		c.EmitBrfalse(label);
		c.EmitLdcI4(1);
		c.EmitRet();
		c.MarkLabel(label);
	}
	static bool TryApplyOil(int i, int j) {
		MultiTypeMultiTile.GetMainTile(i, j, out int x, out int y);
		Tile tile = Main.tile[x, y];
		int maxOil = MaxOil[tile.TileType];
		if (tile.HasTile && maxOil > 0 && Main.LocalPlayer.HeldItem?.ModItem is Oil_Bucket bucket) {
			Data data = GetData(new(x, y));
			if (data.OilCount >= maxOil) return false;
			bucket.ConsumeOil(Main.LocalPlayer);
			data.OilCount = maxOil;
			return true;
		}
		return false;
	}
	static void On_Player_PlaceThing_ItemInExtractinator(On_Player.orig_PlaceThing_ItemInExtractinator orig, Player self, ref Player.ItemCheckContext context) {
		if (!Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile) return;
		if (!self.ItemTimeIsZero || self.itemAnimation <= 0 || !self.controlUseItem) return;
		Item item = self.HeldItem;
		Vector2 position = self.position;
		if (!(position.X / 16f - Player.tileRangeX - item.tileBoost - self.blockRange <= Player.tileTargetX) || !((position.X + self.width) / 16f + Player.tileRangeX + item.tileBoost - 1f + self.blockRange >= Player.tileTargetX) || !(position.Y / 16f - Player.tileRangeY - item.tileBoost - self.blockRange <= Player.tileTargetY) || !((position.Y + self.height) / 16f + Player.tileRangeY + item.tileBoost - 2f + self.blockRange >= Player.tileTargetY)) return;
		orig(self, ref context);
		if (self.itemTime == 0) return;
		if (MaxOil[Main.tile[Player.tileTargetX, Player.tileTargetY].TileType] <= 0) return;
		MultiTypeMultiTile.GetMainTile(Player.tileTargetX, Player.tileTargetY, out int x, out int y);
		Data data = GetData(new(x, y));
		if (data?.OilCount > 0) {
			self.itemTime = Math.Max(self.itemTime / 2, 1);
			self.itemTimeMax = Math.Max(self.itemTimeMax / 2, 1);
			data.OilCount--;
		}
	}
	public static Data GetData(Point16 position) {
		System.Collections.Generic.Dictionary<Point16, Data> tileEntities = ModContent.GetInstance<Tile_Lubrication>().tileEntities;
		if (!tileEntities.TryGetValue(position, out Data data)) tileEntities[position] = data = new();
		return data;
	}
	protected override bool IsValidTile(Tile tile) => tile.HasTile && MaxOil[tile.TileType] > 0;
	public class Data : ITileEntityData {
		int tileType;
		int oilCount;
		public int OilCount {
			get => oilCount;
			set => IsDirty |= oilCount.TrySet(Math.Min(value, MaxOil[tileType]));
		}
		public void Update(Point16 position) => tileType = Main.tile[position].TileType;
		void ITileEntityData.SaveTE(TagCompound tag) {
			tag[nameof(tileType)] = TileID.Search.GetName(tileType);
			tag[nameof(oilCount)] = oilCount;
		}
		static Data ITileEntityData.LoadTE(TagCompound tag) {
			Data data = new();
			tag.TryGet(nameof(tileType), out string tileName);
			TileID.Search.TryGetId(tileName, out data.tileType);
			tag.TryGet(nameof(oilCount), out data.oilCount);
			return data;
		}
		void ITileEntityData.NetSend(BinaryWriter writer) {
			writer.Write(tileType);
			writer.Write(oilCount);
		}
		static Data ITileEntityData.NetReceive(BinaryReader reader, Data existing) {
			existing ??= new Data();
			existing.tileType = reader.ReadInt32();
			existing.oilCount = reader.ReadInt32();
			return existing;
		}
		public bool IsDirty { get; set; }
	}
}