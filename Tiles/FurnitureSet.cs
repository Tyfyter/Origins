using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles {
	public abstract class FurnitureSet<TItem> : FurnitureSet where TItem : ModItem {
		public sealed override int IngredientItem => ModContent.ItemType<TItem>();
	}
	public abstract class FurnitureSet : ILoadable {
		public virtual string Name => GetType().Name.Replace("_Furniture", "").Replace("_Furniture_Set", "");
		public virtual string TextureBase => (GetType().Namespace + ".").Replace('.', '/');
		public abstract Color MapColor { get; }
		public abstract int DustType { get; }
		public virtual int FlameDust => -1;
		public virtual bool FurnitureFancyGlows => true;
		public virtual bool LightFancyGlows => true;
		public virtual bool CandleFancyGlow => LightFancyGlows;
		public virtual bool CandelabraFancyGlow => LightFancyGlows;
		public virtual bool LampFancyGlow => LightFancyGlows;
		public virtual bool ChandelierFancyGlow => LightFancyGlows;
		public virtual bool LanternFancyGlow => LightFancyGlows;
		public virtual Vector3 LightColor { get => OriginExtensions.TorchColor(TorchID.Torch); }
		public virtual bool LanternSway => true;
		public virtual bool ChandelierSway => true;
		public virtual int CraftingStation => TileID.WorkBenches;
		public abstract int IngredientItem { get; }
		public void Load(Mod mod) {
			void AddTile(ModTile instance) {
				if (!ExcludeTile(instance)) {
					mod.AddContent(instance);
					tilesByType.TryAdd(instance.GetType(), instance);
				}
			}
			AddTile(new FurnitureSet_Platform(this));
			AddTile(new FurnitureSet_Door(this));
			AddTile(new FurnitureSet_Grandfather_Clock(this));
			AddTile(new FurnitureSet_Chair(this));
			AddTile(new FurnitureSet_Toilet(this));
			AddTile(new FurnitureSet_Sofa(this));
			AddTile(new FurnitureSet_Bathtub(this));
			AddTile(new FurnitureSet_Sink(this));
			AddTile(new FurnitureSet_Candle(this));
			AddTile(new FurnitureSet_Candelabra(this));
			AddTile(new FurnitureSet_Chandelier(this));
			AddTile(new FurnitureSet_Lamp(this));
			AddTile(new FurnitureSet_Lantern(this));
			AddTile(new FurnitureSet_Bookcase(this));
			AddTile(new FurnitureSet_Piano(this));
			AddTile(new FurnitureSet_Table(this));
			AddTile(new FurnitureSet_Work_Bench(this));
			AddTile(new FurnitureSet_Dresser(this));
			AddTile(new FurnitureSet_Bed(this));
			AddTile(new FurnitureSet_Chest(this));
		}
		protected virtual bool ExcludeTile(ModTile tile) => false;
		public virtual void SetupTile(ModTile tile) { }
		/// <inheritdoc	cref="ModTile.AdjustMultiTileVineParameters"/>
		public virtual void ChandelierSwayParams(LightFurnitureBase tile, int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor) { }
		/// <inheritdoc	cref="ModTile.GetTileFlameData"/>
		public virtual void ChandelierFlameData(LightFurnitureBase tile, int i, int j, ref TileDrawing.TileFlameData tileFlameData) { }

		/// <inheritdoc	cref="ModTile.AdjustMultiTileVineParameters"/>
		public virtual void LanternSwayParams(LightFurnitureBase tile, int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor) { }
		/// <inheritdoc	cref="ModTile.GetTileFlameData"/>
		public virtual void LanternFlameData(LightFurnitureBase tile, int i, int j, ref TileDrawing.TileFlameData tileFlameData) { }

		public void Unload() { }
		readonly Dictionary<Type, ModTile> tilesByType = [];
		public IReadOnlyDictionary<Type, ModTile> TilesByType => tilesByType;
		public static TTile Get<TSet, TTile>() where TSet : FurnitureSet where TTile : ModTile {
			ModTile tile = null;
			ModContent.GetInstance<TSet>()?.tilesByType?.TryGetValue(typeof(TTile), out tile);
			return tile as TTile;
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Platform(FurnitureSet furnitureSet) : Platform_Tile {
		public override string Name => furnitureSet.Name + "_Platform";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type, 2)
				.AddIngredient(furnitureSet.IngredientItem, 1)
				.Register();
				Recipe.Create(furnitureSet.IngredientItem)
				.AddIngredient(item.type, 2)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Door(FurnitureSet furnitureSet) : DoorBase {
		public override string Name => furnitureSet.Name + "_Door";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 6)
				.AddTile(furnitureSet.CraftingStation)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Grandfather_Clock(FurnitureSet furnitureSet) : ClockBase {
		public override string Name => furnitureSet.Name + "_Grandfather_Clock";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddRecipeGroup(RecipeGroupID.IronBar, 3)
				.AddIngredient(ItemID.Glass, 6)
				.AddIngredient(furnitureSet.IngredientItem, 10)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Chair(FurnitureSet furnitureSet) : ChairBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Chair";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 4)
				.AddTile(furnitureSet.CraftingStation)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.FurnitureFancyGlows && glowTexture.Exists) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Toilet(FurnitureSet furnitureSet) : ChairBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Toilet";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.Toilets;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 6)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public override void HitWire(int i, int j) {
			ToiletHitWire(i, j);
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.FurnitureFancyGlows && glowTexture.Exists) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Sofa(FurnitureSet furnitureSet) : ChairBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Sofa";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.Benches;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 5)
				.AddIngredient(ItemID.Silk, 2)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.FurnitureFancyGlows && glowTexture.Exists) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Bathtub(FurnitureSet furnitureSet) : FurnitureBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Bathtub";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.Bathtubs;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 14)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.FurnitureFancyGlows && glowTexture.Exists) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Sink(FurnitureSet furnitureSet) : FurnitureBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Sink";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.Sinks;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 6)
				.AddIngredient(ItemID.WaterBucket)
				.AddTile(furnitureSet.CraftingStation)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
			ModCompatSets.AnySinks[Item.Type] = true;
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.FurnitureFancyGlows && glowTexture.Exists) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Candle(FurnitureSet furnitureSet) : LightFurnitureBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Candle";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.Candles;
		public override Color MapColor => furnitureSet.MapColor;
		public override int FlameDust => furnitureSet.FlameDust;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 4)
				.AddIngredient(ItemID.Torch)
				.AddTile(furnitureSet.CraftingStation)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (IsOn(Main.tile[i, j])) {
				(r, g, b) = furnitureSet.LightColor;
			}
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.CandleFancyGlow && glowTexture.Exists && IsOn(tile)) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Candelabra(FurnitureSet furnitureSet) : LightFurnitureBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Candelabra";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.Candelabras;
		public override Color MapColor => furnitureSet.MapColor;
		public override int FlameDust => furnitureSet.FlameDust;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 5)
				.AddIngredient(ItemID.Torch, 3)
				.AddTile(furnitureSet.CraftingStation)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (IsOn(Main.tile[i, j])) {
				(r, g, b) = furnitureSet.LightColor;
			}
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.CandelabraFancyGlow && glowTexture.Exists && IsOn(tile)) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Lamp(FurnitureSet furnitureSet) : LightFurnitureBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Lamp";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.Lamps;
		public override Color MapColor => furnitureSet.MapColor;
		public override int FlameDust => furnitureSet.FlameDust;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Torch)
				.AddIngredient(furnitureSet.IngredientItem, 3)
				.AddTile(furnitureSet.CraftingStation)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (IsOn(Main.tile[i, j])) {
				(r, g, b) = furnitureSet.LightColor;
			}
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.LampFancyGlow && glowTexture.Exists && IsOn(tile)) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Chandelier(FurnitureSet furnitureSet) : LightFurnitureBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Chandelier";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.Chandeliers;
		public override Color MapColor => furnitureSet.MapColor;
		public override int FlameDust => furnitureSet.FlameDust;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 4)
				.AddIngredient(ItemID.Torch, 4)
				.AddIngredient(ItemID.Chain)
				.AddTile(TileID.Anvils)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			TileID.Sets.MultiTileSway[Type] = furnitureSet.ChandelierSway;
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			if (!furnitureSet.ChandelierSway) return true;
			Tile tile = Main.tile[i, j];
			if (TileObjectData.IsTopLeft(tile)) {
				// Makes this tile sway in the wind and with player interaction when used with TileID.Sets.MultiTileSway
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);
			}
			// We must return false here to prevent the normal tile drawing code from drawing the default static tile. Without this a duplicate tile will be drawn.
			return false;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			if (!furnitureSet.ChandelierSway) base.PostDraw(i, j, spriteBatch);
		}
		public override void AdjustMultiTileVineParameters(int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor) {
			if (furnitureSet.ChandelierSway) {
				glowTexture = GlowTexture;
				glowColor = GlowColor;
				furnitureSet.ChandelierSwayParams(this, i, j, ref overrideWindCycle, ref windPushPowerX, ref windPushPowerY, ref dontRotateTopTiles, ref totalWindMultiplier, ref glowTexture, ref glowColor);
			}
		}
		public override void GetTileFlameData(int i, int j, ref TileDrawing.TileFlameData tileFlameData) {
			furnitureSet.ChandelierFlameData(this, i, j, ref tileFlameData);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (IsOn(Main.tile[i, j])) {
				(r, g, b) = furnitureSet.LightColor;
			}
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.ChandelierFancyGlow && glowTexture.Exists && IsOn(tile)) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Lantern(FurnitureSet furnitureSet) : LightFurnitureBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Lantern";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.HangingLanterns;
		public override Color MapColor => furnitureSet.MapColor;
		public override int FlameDust => furnitureSet.FlameDust;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 6)
				.AddIngredient(ItemID.Torch)
				.AddTile(furnitureSet.CraftingStation)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			TileID.Sets.MultiTileSway[Type] = furnitureSet.LanternSway;
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			if (furnitureSet.LanternSway) return;
			Tile tile = Main.tile[i, j];
			TileObjectData data = TileObjectData.GetTileData(tile);
			int x = i - tile.TileFrameX / 18 % data.Width;
			int topLeftY = j - tile.TileFrameY / 18 % data.Height;
			if (WorldGen.IsBelowANonHammeredPlatform(x, topLeftY)) {
				offsetY -= 8;
			}
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			if (!furnitureSet.LanternSway) return true;
			Tile tile = Main.tile[i, j];
			if (TileObjectData.IsTopLeft(tile)) {
				// Makes this tile sway in the wind and with player interaction when used with TileID.Sets.MultiTileSway
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);
			}
			// We must return false here to prevent the normal tile drawing code from drawing the default static tile. Without this a duplicate tile will be drawn.
			return false;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			if (!furnitureSet.LanternSway) base.PostDraw(i, j, spriteBatch);
		}
		public override void AdjustMultiTileVineParameters(int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor) {
			if (furnitureSet.LanternSway) {
				glowTexture = GlowTexture;
				glowColor = GlowColor;
				furnitureSet.LanternSwayParams(this, i, j, ref overrideWindCycle, ref windPushPowerX, ref windPushPowerY, ref dontRotateTopTiles, ref totalWindMultiplier, ref glowTexture, ref glowColor);
			}
		}
		public override void GetTileFlameData(int i, int j, ref TileDrawing.TileFlameData tileFlameData) {
			furnitureSet.LanternFlameData(this, i, j, ref tileFlameData);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (IsOn(Main.tile[i, j])) {
				(r, g, b) = furnitureSet.LightColor;
			}
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.LanternFancyGlow && glowTexture.Exists && IsOn(tile)) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Bookcase(FurnitureSet furnitureSet) : FurnitureBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Bookcase";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.Bookcases;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
			ModCompatSets.AnyBookcases[Item.Type] = true;
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.FurnitureFancyGlows && glowTexture.Exists) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Piano(FurnitureSet furnitureSet) : FurnitureBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Piano";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.Pianos;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.FurnitureFancyGlows && glowTexture.Exists) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Table(FurnitureSet furnitureSet) : FurnitureBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Table";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.Tables;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 8)
				.AddTile(furnitureSet.CraftingStation)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
			ModCompatSets.AnyTables[Item.Type] = true;
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.FurnitureFancyGlows && glowTexture.Exists) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Work_Bench(FurnitureSet furnitureSet) : FurnitureBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Work_Bench";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override int BaseTileID => TileID.WorkBenches;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 10)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
			ModCompatSets.AnyWorkBenches[Item.Type] = true;
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.FurnitureFancyGlows && glowTexture.Exists) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Dresser(FurnitureSet furnitureSet) : DresserBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Dresser";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 16)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.FurnitureFancyGlows && glowTexture.Exists) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Bed(FurnitureSet furnitureSet) : BedBase, IGlowingModTile {
		public override string Name => furnitureSet.Name + "_Bed";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override Color MapColor => furnitureSet.MapColor;
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient(furnitureSet.IngredientItem, 15)
				.AddIngredient(ItemID.Silk, 5)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public AutoCastingAsset<Texture2D> GlowTexture => glowTexture;
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public Color GlowColor => GlowmaskColor;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (furnitureSet.FurnitureFancyGlows && glowTexture.Exists) {
				color = Vector3.Max(color, furnitureSet.LightColor);
			}
		}
	}
	[Autoload(false)]
	public class FurnitureSet_Chest(FurnitureSet furnitureSet) : ModChest {
		public readonly FurnitureSet furnitureSet = furnitureSet;
		public override string Name => furnitureSet.Name + "_Chest";
		public override string Texture => furnitureSet.TextureBase + Name;
		public override void Load() {
			Mod.AddContent(new FurnitureSet_Chest_Item(this));
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			AddMapEntry(new Color(200, 200, 200), CreateMapEntryName(), MapChestName);
			AdjTiles = [TileID.Containers];
			DustType = furnitureSet.DustType;
			furnitureSet.SetupTile(this);
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
	}
	[Autoload(false)]
	public class FurnitureSet_Chest_Item(FurnitureSet_Chest chest) : ModItem {
		protected override bool CloneNewInstances => true;
		public override string Name => chest.Name + "_Item";
		public override string Texture => chest.Texture + "_Item";
		public override void SetStaticDefaults() => ModCompatSets.AnyChests[Type] = true;
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 9999;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = chest.Type;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(chest.furnitureSet.IngredientItem, 8)
			.AddRecipeGroup(RecipeGroupID.IronBar, 2)
			.AddTile(chest.furnitureSet.CraftingStation)
			.Register();
		}
	}
}
