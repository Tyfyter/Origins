using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Tools.Wiring;
using Origins.Items.Weapons.Ammo;
using Origins.UI;
using Origins.World.BiomeData;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace Origins.Tiles.Ashen; 
public class Solar_Battery : ModTile {
	public static int MaxPower => 60 * 60 * 3;
	public override void Load() {
		new TileItem(this)
		.WithExtraStaticDefaults(this.DropTileItem)
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
	}
	public override void SetStaticDefaults() {
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.Width = 3;
		TileObjectData.newTile.SetHeight(2);
		TileObjectData.newTile.SetOriginBottomCenter();
		TileObjectData.newTile.RandomStyleRange = 2;
		this.SetAnimationHeight();
		TileObjectData.addTile(Type);
		AddMapEntry(new Color(40, 30, 18), this.GetTileItem().DisplayName);
		DustType = Ashen_Biome.DefaultTileDust;
		displayRadices = new(this.GetTileItem().GetLocalization("Time"));
	}
	public override void NumDust(int i, int j, bool fail, ref int num) {
		num = fail ? 1 : 3;
	}
	Time_Radices displayRadices;
	public override bool RightClick(int i, int j) {
		TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out i, out j);
		float power = (int)Solar_Battery_TE.GetData(new(i, j)).Power;
		Main.NewText(string.Format(displayRadices.FormatTime((int)power), power / MaxPower));
		return true;
	}
	public override void PlaceInWorld(int i, int j, Item item) {
		TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out int left, out int top);
		ModContent.GetInstance<Solar_Battery_TE>().AddTileEntity(new(left, top), new());
	}
	public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects) {
		position.Y += 2;
		return base.PreDrawPlacementPreview(i, j, spriteBatch, ref frame, ref position, ref color, validPlacement, ref spriteEffects);
	}
	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
		offsetY = 2;
	}
	class Solar_Battery_TE : TESystem<Solar_Battery_TE.Data> {
		public static Data GetData(Point16 position) {
			ModContent.GetInstance<Solar_Battery_TE>().tileEntities.TryGetValue(position, out Data data);
			return data;
		}
		protected override bool IsValidTile(Tile tile) => tile.TileIsType(ModContent.TileType<Solar_Battery>());
		public class Data() : ITileEntityData {
			float power;
			float currentPowerPerTick;
			int checkPowerTimer;
			public float Power => power;
			public void Update(Point16 position) {
				if (checkPowerTimer.CycleUp(60)) {
					currentPowerPerTick = 0;
					if (Main.dayTime) {
						bool isSmogStorm = false;
						if (Main.WindyEnoughForKiteDrops) {
							int tileCount = 0;
							Rectangle tileRectangle = new(position.X + 1 - Main.buffScanAreaWidth / 2, position.Y + 1 - Main.buffScanAreaHeight / 2, Main.buffScanAreaWidth, Main.buffScanAreaHeight);
							tileRectangle = WorldUtils.ClampToWorld(tileRectangle);
							for (int i = tileRectangle.Left; i < tileRectangle.Right && !isSmogStorm; i++) {
								for (int j = tileRectangle.Top; j < tileRectangle.Bottom && !isSmogStorm; j++) {
									Tile tile = Main.tile[i, j];
									if (!tile.HasTile && OriginsSets.Tiles.AshenBiomeTiles[tile.TileType]) continue;
									isSmogStorm = ++tileCount >= Ashen_Biome.NeededTiles;
								}
							}
						}
						if (!isSmogStorm) {
							currentPowerPerTick = Solar_Panel.GetSunlightFactor(WaterStyleID.Purity,
								(position.X, position.Y),
								(position.X + 1, position.Y),
								(position.X + 2, position.Y)
							) * 2;
						}
					}
				}
				power.Cooldown();
				power.Warmup(MaxPower, currentPowerPerTick);
				bool shouldGenerate = power > 0;
				if (Main.tile[position].Get<Ashen_Wire_Data>().IsTilePowered == shouldGenerate) return;
				TileObjectData tileData = TileObjectData.GetTileData(Main.tile[position]);
				TileUtils.GetMultiTileTopLeft(position.X, position.Y, tileData, out int left, out int top);
				for (int j = 0; j < tileData.Height; j++) {
					for (int i = 0; i < tileData.Width; i++) {
						Ashen_Wire_Data.SetTilePowered(left + i, top + j, shouldGenerate);
					}
				}
			}

			void ITileEntityData.SaveTE(TagCompound tag) {
				tag[nameof(power)] = power;
				tag[nameof(checkPowerTimer)] = checkPowerTimer;
			}
			static Data ITileEntityData.LoadTE(TagCompound tag) {
				Data data = new();
				tag.TryGet(nameof(power), out data.power);
				tag.TryGet(nameof(checkPowerTimer), out data.checkPowerTimer);
				return data;
			}
			void ITileEntityData.NetSend(BinaryWriter writer) {
				writer.Write(power);
			}
			static Data ITileEntityData.NetReceive(BinaryReader reader, Data existing) {
				existing ??= new Data();
				existing.power = reader.ReadInt32();
				return existing;
			}

			public bool IsDirty { get; set; }
		}
	}
}