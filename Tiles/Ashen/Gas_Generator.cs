using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Tools.Liquids;
using Origins.Items.Tools.Wiring;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen; 
public class Gas_Generator : ModTile, IGlowingModTile {
	public static AutoLoadingAsset<Texture2D> GlowTexture = typeof(Gas_Generator).GetDefaultTMLName() + "_Glow";
	public Color GlowColor => Color.White;
	AutoCastingAsset<Texture2D> IGlowingModTile.GlowTexture => GlowTexture;
	public static int FuelFrameCount => 6;
	public static int MaxFuel => 60 * 60 * 10;
	public static int FuelPerBucket => 60 * 60 * 5;
	public override void Load() {
		new TileItem(this)
		.WithExtraDefaults(item => {
			item.CloneDefaults(ItemID.Sawmill);
			item.createTile = Type;
			//item.rare++;
			item.value += Item.buyPrice(gold: 1);
		}).WithOnAddRecipes(item => {
			Recipe.Create(item.type)
			.AddIngredient(ItemID.Cog, 5)
			.AddRecipeGroup(ALRecipeGroups.SilverBars)
			.AddIngredient<Scrap>(18)
			.AddTile<Metal_Presser>()
			.Register();
		}).RegisterItem();
		this.SetupGlowKeys();
	}
	public void FancyLightingGlowColor(Tile tile, ref Vector3 color) { }
	public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
		drawData.glowTexture = GlowTexture;
		drawData.glowSourceRect = new Rectangle(drawData.tileFrameX, drawData.tileFrameY + drawData.addFrY, 16, 16);
		drawData.glowColor = GlowColor;
	}
	public override void SetStaticDefaults() {
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.Width = 3;
		TileObjectData.newTile.SetHeight(2);
		TileObjectData.newTile.SetOriginBottomCenter();
		this.SetAnimationHeight();
		TileObjectData.addTile(Type);
		AddMapEntry(new Color(40, 30, 18), this.GetTileItem().DisplayName);
		DustType = Ashen_Biome.DefaultTileDust;
	}
	public override void NumDust(int i, int j, bool fail, ref int num) {
		num = fail ? 1 : 3;
	}
	public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
		TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out int left, out int top);
		frameYOffset = Gas_Generator_TE.GetData(new(left, top)).Frame * AnimationFrameHeight;
	}
	public override void PlaceInWorld(int i, int j, Item item) {
		TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out int left, out int top);
		ModContent.GetInstance<Gas_Generator_TE>().AddTileEntity(new(left, top), new());
	}
	public override bool RightClick(int i, int j) {
		Item heldItem = Main.LocalPlayer.HeldItem;
		if (heldItem.type == ModContent.ItemType<Oil_Bucket>()) {
			TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out i, out j);
			ModContent.GetInstance<Gas_Generator_TE>().tileEntities[new(i, j)].Fuel += FuelPerBucket;
			if (heldItem.stack > 1) {
				Main.LocalPlayer.GetItem(Main.myPlayer, new(ItemID.EmptyBucket), GetItemSettings.ItemCreatedFromItemUsage);
				heldItem.stack--;
			} else {
				heldItem.ChangeItemType(ItemID.EmptyBucket);
			}
			return true;
		}
		return false;
	}
	public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects) {
		position.Y += 2;
		return base.PreDrawPlacementPreview(i, j, spriteBatch, ref frame, ref position, ref color, validPlacement, ref spriteEffects);
	}
	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
		offsetY = 2;
		TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out int left, out int top);
		if (ModContent.GetInstance<Gas_Generator_TE>().tileEntities[new(left, top)].Fuel > 0) {
			offsetY -= Main.rand.Next(3); // very bad temp 
		}
	}
	class Gas_Generator_TE : TESystem<Gas_Generator_TE.Data> {
		public static Data GetData(Point16 position) {
			ModContent.GetInstance<Gas_Generator_TE>().tileEntities.TryGetValue(position, out Data data);
			return data;
		}
		protected override bool IsValidTile(Tile tile) => tile.TileIsType(ModContent.TileType<Gas_Generator>());
		public class Data() : ITileEntityData {
			int fuel;
			public int Fuel {
				get => fuel;
				set => IsDirty |= fuel.TrySet(Math.Min(value, MaxFuel));
			}
			public int Frame => fuel > 0 ? (int)(1 + (fuel / (float)MaxFuel) * FuelFrameCount) : 0;
			public void Update(Point16 position) {
				bool shouldGenerate = fuel > 0;
				if (Main.tile[position].Get<Ashen_Wire_Data>().IsTilePowered == shouldGenerate) goto consume;
				TileObjectData tileData = TileObjectData.GetTileData(Main.tile[position]);
				TileUtils.GetMultiTileTopLeft(position.X, position.Y, tileData, out int left, out int top);
				for (int j = 0; j < tileData.Height; j++) {
					for (int i = 0; i < tileData.Width; i++) {
						Ashen_Wire_Data.SetTilePowered(left + i, top + j, shouldGenerate);
					}
				}
				consume:
				if (fuel > 0) fuel--;
			}

			void ITileEntityData.SaveTE(TagCompound tag) {
				tag[nameof(fuel)] = fuel;
			}
			static Data ITileEntityData.LoadTE(TagCompound tag) {
				Data data = new();
				tag.TryGet(nameof(fuel), out data.fuel);
				return data;
			}
			void ITileEntityData.NetSend(BinaryWriter writer) {
				writer.Write(fuel);
			}
			static Data ITileEntityData.NetReceive(BinaryReader reader) => new() {
				fuel = reader.ReadInt32()
			};
			public bool IsDirty { get; set; }
		}
	}
	public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
}