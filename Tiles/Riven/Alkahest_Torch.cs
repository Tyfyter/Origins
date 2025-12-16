using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Items.Materials;
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
	public class Alkahest_Torch_Tile : OriginTile {
		private Asset<Texture2D> flameTexture;

		public override void SetStaticDefaults() {
			// Properties
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileWaterDeath[Type] = false;
			TileID.Sets.FramesOnKillWall[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.DisableSmartInteract[Type] = true;
			TileID.Sets.AllowLightInWater[Type] = true;
			TileID.Sets.Torch[Type] = true;

			DustType = DustID.BubbleBurst_Green;
			AdjTiles = [TileID.Torches];

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Torches, 0));

			TileObjectData.newTile.LinkedAlternates = true;
			TileObjectData.newTile.WaterDeath = false;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.Allowed;
			TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;

			TileObjectData.addTile(Type);

			// Etc
			AddMapEntry(new Color(200, 200, 200), Language.GetText("ItemName.Torch"));

			// Assets
			if (!Main.dedServ) {
				RequestIfExists("Origins/Tiles/Riven/Alkahest_Torch_Tile_Flame", out flameTexture);
			}
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;

			// We can determine the item to show on the cursor by getting the tile style and looking up the corresponding item drop.
			int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
			player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type, style);
		}

		public override float GetTorchLuck(Player player) => player.InModBiome<Riven_Hive>().ToInt();

		public override void NumDust(int i, int j, bool fail, ref int num) => num = Main.rand.Next(1, 3);

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			Tile tile = Main.tile[i, j];

			// If the torch is on
			if (tile.TileFrameX < 66) {
				Vector3 light = Alkahest_Torch.Light;
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

			Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);

			if (Main.drawToScreen) {
				zero = Vector2.Zero;
			}

			ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i); // Don't remove any casts.
			Color color = new Color(100, 100, 100, 0);
			int width = 20;
			int height = 20;
			var tile = Main.tile[i, j];
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
	public class Alkahest_Torch : ModItem, ICustomWikiStat {
		public static Vector3 Light => new(1.2f, 1.3f, 0.9f);
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;

			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.ShimmerTorch;
			ItemID.Sets.SingleUseInGamepad[Type] = true;
			ItemID.Sets.Torches[Type] = true;
			ItemID.Sets.WaterTorches[Type] = true;
		}

		public override void SetDefaults() {
			// DefaultToTorch sets various properties common to torch placing items. Hover over DefaultToTorch in Visual Studio to see the specific properties set.
			// Of particular note to torches are Item.holdStyle, Item.flame, and Item.noWet. 
			Item.DefaultToTorch(TileType<Alkahest_Torch_Tile>(), 0, false);
			Item.rare = ItemRarityID.Blue;
			Item.value = 50;
		}

		public override void HoldItem(Player player) {
			Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

			Lighting.AddLight(position, Light);
		}

		public override void PostUpdate() {
			Lighting.AddLight(Item.Center, Light);
		}

		public override void AddRecipes() {
			CreateRecipe(33)
			.AddIngredient(ItemID.Torch, 33)
			.AddIngredient(ItemType<Alkahest>())
			.SortAfterFirstRecipesOf(ItemID.IchorTorch)
			.Register();
		}
		public void ModifyWikiStats(JObject data) {
			WikiPageExporter.AddTorchLightStats(data, Light);
		}
	}
}
