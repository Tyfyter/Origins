using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Graphics;
using Origins.Items.Tools.Liquids;
using Origins.Items.Tools.Wiring;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using ReLogic.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.UI;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace Origins.Tiles.Ashen; 
public class Gas_Generator : ModTile {
	readonly Sound activeSound = EnvironmentSounds.Register<Sound>();
	public static AutoLoadingAsset<Texture2D> GlowTexture = typeof(Gas_Generator).GetDefaultTMLName() + "_Glow";
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
		GlowPaintKey = CustomTilePaintLoader.CreateKey();
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
	}
	public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
		Tile tile = Main.tile[i, j];
		Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition;
		if (!Main.drawToScreen) pos += new Vector2(Main.offScreenRange);
		if (ModContent.GetInstance<Gas_Generator_TE>().tileEntities[new(i, j)].Fuel > 0) {
			FastRandom rand = new FastRandom(Origins.gameFrameCount).WithModifier(i, j);
			rand.Next(1);
			pos += rand.NextVector2Circular(1, 2); 
		}
		pos = pos.Floor();
		short tileFrameX = tile.TileFrameX;
		short tileFrameY = tile.TileFrameY;
		Main.instance.TilesRenderer.GetTileDrawData(i, j, tile, Type, ref tileFrameX, ref tileFrameY, out _, out _, out int tileTop, out _, out int addFrX, out int addFrY, out _, out _, out _, out _);
		tileFrameX += (short)addFrX;
		tileFrameY += (short)addFrY;
		pos.Y += tileTop;
		VertexColors glow = new(Color.White);
		Vector4 destination = new(0, 0, 16, 16);
		Rectangle frame = new(0, 0, 16, 16);
		bool isPowered = WiresUI.Settings.DrawWires && Main.tile[i, j].Get<Ashen_Wire_Data>().IsTilePowered;
		Color poweredColor = new(255, 113, 0);
		float pulse = Ashen_Wire_Data.pulse.Value * 0.8f;
		ApplyPowered(ref glow);
		for (int x = 0; x < 3; x++) {
			destination.X = pos.X + x * 16;
			frame.X = tileFrameX + x * 18;
			for (int y = 0; y < 2; y++) {
				destination.Y = pos.Y + y * 16;
				frame.Y = tileFrameY + y * 18;
				Lighting.GetCornerColors(i + x, j + y, out VertexColors vertices);
				ApplyPowered(ref vertices);
				Main.tileBatch.Draw(
					TextureAssets.Tile[Type].Value,
					destination,
					frame,
					vertices
				);
				Main.tileBatch.Draw(
					GlowTexture,
					destination,
					frame,
					glow
				);
			}
		}
		void ApplyPowered(ref VertexColors vertices) {
			if (!isPowered) return;
			vertices.TopLeftColor = Color.Lerp(vertices.TopLeftColor, poweredColor, pulse);
			vertices.TopRightColor = Color.Lerp(vertices.TopRightColor, poweredColor, pulse);
			vertices.BottomLeftColor = Color.Lerp(vertices.BottomLeftColor, poweredColor, pulse);
			vertices.BottomRightColor = Color.Lerp(vertices.BottomRightColor, poweredColor, pulse);
		}
	}
	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
		if (TileObjectData.IsTopLeft(i, j)) Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
		return false;
	}
	public override void NearbyEffects(int i, int j, bool closer) {
		if (closer) return;
		TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out i, out j);
		if (Gas_Generator_TE.GetData(new(i, j)).Fuel <= 0) return;
		activeSound.TrySetNearest(new(i * 16 + 8, j * 16 + 8));
	}
	class Sound : AEnvironmentSound {
		public int NoisyGenerator = 0;
		public override void UpdateSound(Vector2 position) {
			if (NoisyGenerator.CycleUp(8)) {
				SoundEngine.PlaySound(SoundID.Item22.WithVolume(0.7f), position);
				SoundEngine.PlaySound(SoundID.Item69.WithVolume(0.2f).WithPitch(1.5f), position);
			}
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