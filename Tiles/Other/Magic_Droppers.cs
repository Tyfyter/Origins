using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils;
using Origins.Gores;
using Origins.Liquids;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
	#region Base Classes
	//The tile that spawns the liquid's droplet gore
	//Adapted from vanilla's droplet tiles
	public abstract class BaseMagicDropper<TLiquid, TDrop> : BaseMagicDropper where TLiquid : ModLiquid where TDrop : ModGore {
		public override int LiquidType => LiquidLoader.LiquidType<TLiquid>();
		public override int DroppletType => ModContent.GoreType<TDrop>();
	}
	public abstract class BaseMagicDropper<TLiquid> : BaseMagicDropper where TLiquid : ModLiquid {
		public override int LiquidType => LiquidLoader.LiquidType<TLiquid>();
	}
	[ReinitializeDuringResizeArrays]
	public abstract class BaseMagicDropper : ModTile {
		public static bool[] AnyLiquidSensorIngredient = ItemID.Sets.Factory.CreateBoolSet();
		public override string Texture => $"Terraria/Images/Tiles_{TileID.WaterDrip}";
		public abstract Color MapColor { get; }
		public abstract int LiquidType { get; }
		public abstract int DroppletType { get; }
		public virtual int HitDust => DustID.Dirt;
		public virtual bool UsedForLiquidSensorAny => true;
		public virtual string[] ItemLegacyNames => null;
		public virtual string ItemTexture => base.Texture;
		public sealed override void Load() {
			new DropperItem(this)
			.WithExtraStaticDefaults(item => AnyLiquidSensorIngredient[item.type] = UsedForLiquidSensorAny)
			.WithExtraDefaults(item => item.value = Item.sellPrice(copper: 40))
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.EmptyDropper)
				.AddLiquid(LiquidType)
				.AddTile(TileID.CrystalBall)
				.SortAfterFirstRecipesOf(ItemID.MagicHoneyDropper)
				.Register();
			}).RegisterItem();
			if (ItemLegacyNames is not null) ModTypeLookup<ModItem>.RegisterLegacyNames(this.GetTileItem(), ItemLegacyNames);
			OnLoad();
		}
		public virtual void OnLoad() { }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.BreakableWhenPlacing[Type] = true;
			AddMapEntry(MapColor, CreateMapEntryName());
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			EmitLiquidDrops(i, j, Main.tile[i, j], 180);
			return true;
		}
		private void EmitLiquidDrops(int i, int j, Tile tileCache, int dripChance) {
			if (tileCache.LiquidAmount != 0 || !Main.rand.NextBool(dripChance * 2)) {
				return;
			}
			Rectangle positionRect = new(i * 16, j * 16, 16, 16);
			positionRect.X -= 34;
			positionRect.Width += 68;
			positionRect.Y -= 100;
			positionRect.Height = 400;
			for (int k = 0; k < Main.maxGore; k++) {
				if (Main.gore[k].active && GoreID.Sets.LiquidDroplet[Main.gore[k].type]) {
					Rectangle gorePosRect = new((int)Main.gore[k].position.X, (int)Main.gore[k].position.Y, 16, 16);
					if (positionRect.Intersects(gorePosRect)) {
						return;
					}
				}
			}
			Vector2 position = new(i * 16, j * 16);
			int goreInd = Gore.NewGore(new EntitySource_TileUpdate(i, j), position, default, DroppletType);
			Gore gore = Main.gore[goreInd];
			gore.velocity *= 0f;
		}

		public override bool CanPlace(int i, int j) {
			if (!Main.tile[i, j - 1].BottomSlope) {
				int x = Player.tileTargetX;
				int y = Player.tileTargetY - 1;
				if (Main.tile[x, y].HasUnactuatedTile && Main.tileSolid[Main.tile[x, y].TileType] && !Main.tileSolidTop[Main.tile[x, y].TileType]) {
					return true;
				}
			}
			return false;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Main.tile[i, j - 1];
			if (!tile.HasTile || tile.BottomSlope || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]) {
				WorldGen.KillTile(i, j);
			}
			return false;
		}
		public override bool CanDrop(int i, int j) {
			return false;
		}
		public override bool CreateDust(int i, int j, ref int type) {
			return false;
		}
	}
	[Autoload(false)]
	public class DropperItem(BaseMagicDropper dropper, bool debug = false) : TileItem(dropper, debug, dropper.ItemTexture) {}
	#endregion
	public class Magic_Dropper_Oil : BaseMagicDropper<Oil, Oil_Drip> {
		public override Color MapColor => FromHexRGB(0x0A0A0A);
	}
	public class Magic_Dropper_Brine : BaseMagicDropper<Liquids.Brine, Brine_Drip> {
		public override string[] ItemLegacyNames => ["Magic_Brine_Dropper"];
		public override Color MapColor => FromHexRGB(0x00583F);
	}
}
