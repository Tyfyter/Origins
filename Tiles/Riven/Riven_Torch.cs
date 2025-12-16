using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.World.BiomeData;
using ReLogic.Content;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
namespace Origins.Tiles.Riven {
	public class Riven_Torch_Tile : OriginTile {
		private Asset<Texture2D> flameTexture;

		public override void SetStaticDefaults() {
			// Properties
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileWaterDeath[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.DisableSmartInteract[Type] = true;
			TileID.Sets.Torch[Type] = true;

			DustType = DustID.CoralTorch;
			AdjTiles = [TileID.Torches];

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Torches, 0));

			// This code adds style-specific properties to style 1. Style 1 is used by ExampleWaterTorch. This code allows the tile to be placed in liquids. More info can be found in the guide: https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#newsubtile-and-newalternate
			TileObjectData.newSubTile.CopyFrom(TileObjectData.newTile);
			TileObjectData.newSubTile.LinkedAlternates = true;
			TileObjectData.newSubTile.WaterDeath = true;
			TileObjectData.newSubTile.LavaDeath = true;
			TileObjectData.newSubTile.WaterPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newSubTile.LavaPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.addSubTile(1);

			TileObjectData.addTile(Type);

			// Etc
			AddMapEntry(new Color(217, 95, 54), Language.GetText("ItemName.Torch"));

			// Assets
			if (!Main.dedServ) {
				RequestIfExists("Origins/Tiles/Riven/Riven_Torch_Tile_Flame", out flameTexture);
			}
			DustType = Riven_Hive.DefaultTileDust;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;

			// We can determine the item to show on the cursor by getting the tile style and looking up the corresponding item drop.
			int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
			player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type, style);
		}

		public override float GetTorchLuck(Player player) => player.InModBiome<Riven_Hive>().ToDirectionInt();

		public override void NumDust(int i, int j, bool fail, ref int num) => num = Main.rand.Next(1, 3);

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			Tile tile = Main.tile[i, j];

			// If the torch is on
			if (tile.TileFrameX < 66) {
				Vector3 light = Riven_Torch.Light;
				r = light.X;
				g = light.Y;
				b = light.Z;
			}
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			// This code slightly lowers the draw position if there is a solid tile above, so the flame doesn't overlap that tile. Terraria torches do this same logic.
			offsetY = 0;

			if (WorldGen.SolidTile(i, j - 1)) {
				offsetY = 4;
			}
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			if (flameTexture is null) return;
			if (!TileDrawing.IsVisible(Main.tile[i, j])) return;
			// The following code draws multiple flames on top our placed torch.

			int offsetY = 0;

			if (WorldGen.SolidTile(i, j - 1)) {
				offsetY = 4;
			}

			Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);

			if (Main.drawToScreen) {
				zero = Vector2.Zero;
			}

			ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i); // Don't remove any casts.
			Color color = new(100, 100, 100, 0);
			int width = 20;
			int height = 20;
			Tile tile = Main.tile[i, j];
			int frameX = tile.TileFrameX;
			int frameY = tile.TileFrameY;
			int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
			if (style == 1) {
				// ExampleWaterTorch should be a bit greener.
				color.G = 255;
			}

			for (int k = 0; k < 7; k++) {
				float xx = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
				float yy = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;

				spriteBatch.Draw(flameTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + xx, j * 16 - (int)Main.screenPosition.Y + offsetY + yy) + zero, new Rectangle(frameX, frameY, width, height), color, 0f, default, 1f, SpriteEffects.None, 0f);
			}
		}
	}
	public class Riven_Torch : ModItem, ICustomWikiStat {
		public static Vector3 Light => new Vector3(0.15f, 1.05f, 0.95f) * Riven_Hive.NormalGlowValue.GetValue();
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;

			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.ShimmerTorch;
			ItemID.Sets.SingleUseInGamepad[Type] = true;
			ItemID.Sets.Torches[Type] = true;
		}

		public override void SetDefaults() {
			// DefaultToTorch sets various properties common to torch placing items. Hover over DefaultToTorch in Visual Studio to see the specific properties set.
			// Of particular note to torches are Item.holdStyle, Item.flame, and Item.noWet. 
			Item.DefaultToTorch(TileType<Riven_Torch_Tile>(), 0, false);
			Item.value = 50;
		}

		public override void HoldItem(Player player) {
			if (player.wet) {
				return;
			}

			Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

			Lighting.AddLight(position, Light);
		}

		public override void PostUpdate() {
			if (!Item.wet) {
				Lighting.AddLight(Item.Center, Light);
			}
		}

		public override void AddRecipes() {
			CreateRecipe(3)
			.AddIngredient(ItemID.Torch, 3)
			.AddIngredient(OriginTile.TileItem<Spug_Flesh>())
			.SortAfterFirstRecipesOf(ItemID.Torch)
			.Register();

			CreateRecipe(3)
			.AddIngredient(ItemID.Torch, 3)
			.AddIngredient(ItemType<Brittle_Quartz_Item>())
			.SortAfterFirstRecipesOf(ItemID.Torch)
			.Register();

			CreateRecipe(3)
			.AddIngredient(ItemID.Torch, 3)
			.AddIngredient(ItemType<Primordial_Permafrost_Item>())
			.SortAfterFirstRecipesOf(ItemID.Torch)
			.Register();
		}
		public void ModifyWikiStats(JObject data) {
			float time = Main.GlobalTimeWrappedHourly;
			Main.GlobalTimeWrappedHourly = -MathHelper.PiOver2;

			JObject minLight = new();
			Origins.gameFrameCount++;
			WikiPageExporter.AddTorchLightStats(minLight, Light);

			Main.GlobalTimeWrappedHourly = MathHelper.PiOver2;
			Origins.gameFrameCount++;
			WikiPageExporter.AddTorchLightStats(data, Light);

			Main.GlobalTimeWrappedHourly = time;
			data["LightIntensity"] = $"{minLight["LightIntensity"]}-{data["LightIntensity"]}";
		}
	}
}
