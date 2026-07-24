using AltLibrary.Common.AltBiomes;
using MonoMod.Cil;
using Origins.Core;
using Origins.Items.Tools.Liquids;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Tiles;
[ReinitializeDuringResizeArrays]
public class Tile_Lubrication : TESystem<Tile_Lubrication.Data> {
	public static int[] MaxOil = TileID.Sets.Factory.CreateIntSet();
	public static float[] OilCraftingQuality = TileID.Sets.Factory.CreateFloatSet(0.1f);
	public static (Point16 pos, float quality)[] AdjToOiled = TileID.Sets.Factory.CreateCustomSet<(Point16, float)>(default);
	public static int[] ReduceConsumptionThreshold = ItemID.Sets.Factory.CreateIntSet(1,
		ItemID.FallenStar, int.MaxValue,
		ItemID.DemoniteBar, int.MaxValue,
		ItemID.CrimtaneBar, int.MaxValue,
		ItemID.ShadowScale, int.MaxValue,
		ItemID.TissueSample, int.MaxValue
	);
	public static bool isExtractinatingWithOil = false;
	public override void Load() {
		Origins.DoILEdit(TileLoader.RightClick, IL_ApplyOil);
		On_Player.PlaceThing_ItemInExtractinator += On_Player_PlaceThing_ItemInExtractinator;
		try {
			IL_Player.AdjTiles += IL_Player_AdjTiles;
		} catch (Exception e) {
			if (Origins.LogLoadingILError($"Player.AdjTiles: CheckLubrication", e)) throw;
		}
	}
	public override void SetStaticDefaults() {
		MaxOil[TileID.Extractinator] = 300;
		MaxOil[TileID.ChlorophyteExtractinator] = 300;
		MaxOil[TileID.Autohammer] = 50;
		MaxOil[TileID.HeavyWorkBench] = 50;
		MaxOil[TileID.IceMachine] = 50;
		MaxOil[TileID.Loom] = 50;
		MaxOil[TileID.Sawmill] = 50;
		MaxOil[TileID.SkyMill] = 50;
		MaxOil[TileID.Solidifier] = 50;
		MaxOil[TileID.SteampunkBoiler] = 50;
	}
	static void IL_ApplyOil(ILContext il) {
		ILCursor c = new(il);
		c.EmitLdarg0();
		c.EmitLdarg1();
		c.EmitDelegate(TryApplyOil);
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
	#region extractinators
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
	#endregion
	static readonly HashSet<Point16> checkedPoints = [];
	static void IL_Player_AdjTiles(ILContext il) {
		ILCursor c = new(il);
		c.EmitDelegate(static () => {
			Array.Clear(AdjToOiled);
			checkedPoints.Clear();
		});
		int i = -1;
		int j = -1;
		int tile = -1;
		c.GotoNext(MoveType.After,
			il => il.MatchLdloc(out i),
			il => il.MatchLdloc(out j),
			il => il.MatchCall<Tilemap>("get_Item"),
			il => il.MatchStloc(out tile),
			il => il.MatchLdloca(tile),
			il => il.MatchCall<Tile>("get_type"),
			il => il.MatchLdindU2(),
			il => il.MatchCall(typeof(TileLoader), "AdjTiles")
		);
		c.EmitLdloc(i);
		c.EmitLdloc(j);
		c.EmitDelegate(static (int i, int j) => {
			ushort tileType = Main.tile[i, j].TileType;
			if (MaxOil[tileType] <= 0) return;
			MultiTypeMultiTile.GetMainTile(i, j, out i, out j);
			Point16 pos = new(i, j);
			if (checkedPoints.Add(pos) && GetData(pos).OilCount > 0) {
				MaximizeQuality(tileType, pos, OilCraftingQuality[tileType]);
				if (TileLoader.GetTile(tileType)?.AdjTiles is int[] adjTiles) {
					for (int k = 0; k < adjTiles.Length; k++) MaximizeQuality(adjTiles[k], pos, OilCraftingQuality[tileType]);
				}
			}
			static void MaximizeQuality(int tileType, Point16 pos, float quality) {
				if (AdjToOiled[tileType].quality < quality) AdjToOiled[tileType] = (pos, quality);
			}
		});
	}
	static Recipe.IngredientQuantityCallback OiledIngredientQuantity(int[] requiredTiles) => (Recipe recipe, int type, ref int amount, bool isDecrafting) => {
		if (isDecrafting) return;
		int reducedPortion = amount - ReduceConsumptionThreshold[type];
		if (reducedPortion <= 0) return;
		float quality = 0;
		for (int i = 0; i < requiredTiles.Length; i++) {
			float currentQuality = AdjToOiled[requiredTiles[i]].quality;
			if (currentQuality == 0) return;
			quality += currentQuality;
		}
		quality /= requiredTiles.Length;
		for (int i = 0; i < reducedPortion; i++) {
			if (Main.rand.NextFloat() < quality) amount--;
		}
		if (lastCraftingOilConsumedTime.TrySet(PegasusLib.PegasusLib.GameTickCount)) {
			for (int i = 0; i < requiredTiles.Length; i++) {
				GetData(AdjToOiled[requiredTiles[i]].pos).OilCount--;
			}
		}
	};
	static uint lastCraftingOilConsumedTime;
	static void RestoreOilConsume(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack) => lastCraftingOilConsumedTime = 0;
	public override void PostSetupContent() {
		foreach (AltBiome biome in AltLib.AllBiomes) {
			if (biome.MaterialContext is not AltMaterialContext materialContext) continue;
			NeverPreserveIngredient(materialContext.EvilBar);
			NeverPreserveIngredient(materialContext.EvilBossDrop);
			NeverPreserveIngredient(materialContext.EvilOre);
			NeverPreserveIngredient(materialContext.EvilSword);
			NeverPreserveIngredient(materialContext.VileComponent);
			NeverPreserveIngredient(materialContext.LightBar);
			NeverPreserveIngredient(materialContext.TrueCombinationSword);
			NeverPreserveIngredient(materialContext.TrueLightSword);
			NeverPreserveIngredient(materialContext.UnderworldSword);
			NeverPreserveIngredient(materialContext.TropicalBar);
			NeverPreserveIngredient(materialContext.TropicalComponent);
			NeverPreserveIngredient(materialContext.TropicalSword);
		}
	}
	public static void NeverPreserveIngredient(int item) {
		if (ReduceConsumptionThreshold.IndexInRange(item)) ReduceConsumptionThreshold[item] = int.MaxValue;
	}
	public override void PostAddRecipes() {
		bool[] oilable = TileID.Sets.Factory.CreateBoolSet();
		for (int i = 0; i < TileID.Count; i++) oilable[i] = MaxOil[i] > 0;
		for (int i = TileID.Count; i < TileLoader.TileCount; i++) {
			if (MaxOil[i] > 0) {
				oilable[i] = true;
				if (TileLoader.GetTile(i)?.AdjTiles is int[] adjTiles) {
					for (int k = 0; k < adjTiles.Length; k++) oilable[adjTiles[k]] = true;
				}
			}
		}
		foreach (Recipe recipe in Main.recipe) {
			if (!recipe.requiredItem.Any(i => i.stack > ReduceConsumptionThreshold[i.type])) continue;
			IEnumerable<int> tiles = recipe.requiredTile.Where(t => oilable[t]);
			if (!tiles.Any()) continue;
			recipe.AddConsumeIngredientCallback(OiledIngredientQuantity(tiles.ToArray()));
			recipe.AddOnCraftCallback(RestoreOilConsume);
		}
	}
	static Data GetData(Point16 position) {
		Dictionary<Point16, Data> tileEntities = ModContent.GetInstance<Tile_Lubrication>().tileEntities;
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