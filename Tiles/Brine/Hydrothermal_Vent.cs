using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Items.Other.Testing;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Brine {
	public class Hydrothermal_Vent : ModTile, IComplexMineDamageTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileLighted[Type] = false;
			Main.tileHammer[Type] = true;
			Main.tileBlockLight[Type] = false;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			//TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.Origin = new(0, 3);
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, 4).ToArray();
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(18, 73, 56), CreateMapEntryName());
			DustType = DustID.GreenMoss;
		}

		public void MinePower(int i, int j, int minePower, ref int damage) {
			if (minePower < 90) damage = 0;
		}
		public override void RandomUpdate(int i, int j) {
			if (!NPC.downedGolemBoss) return;
			Tile self = Framing.GetTileSafely(i, j);
			if (self.TileFrameX % 36 == 0 && self.TileFrameY == 0) {
				Transform(i, j, (ushort)ModContent.TileType<Hydrothermal_Vent_Goopy>());
			}
		}
		protected static bool Transform(int i, int j, ushort toType) {
			Tile tile = Main.tile[i, j];
			int style = TileObjectData.GetTileStyle(tile);
			if (style < 0) return false;
			TileObjectData data = TileObjectData.GetTileData(tile.TileType, style);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int x, out int y);
			for (int k = 0; k < data.Width; k++) {
				for (int l = 0; l < data.Height; l++) {
					Framing.GetTileSafely(x + k, y + l).TileType = toType;
				}
			}
			NetMessage.SendTileSquare(
				-1,
				x,
				y,
				data.Width,
				data.Height
			);
			return true;
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			Tile tile = Main.tile[i, j];
			int style = TileObjectData.GetTileStyle(tile);
			if (style < 0) return;
			TileObjectData data = TileObjectData.GetTileData(tile.TileType, style);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int x, out _);
			if ((x / 3) % 2 == 0) {
				tileFrameX += (short)(18 * (tileFrameX % 36 == 0).ToDirectionInt());
			}
		}
		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			Tile tile = Main.tile[i, j];
			int style = TileObjectData.GetTileStyle(tile);
			if (style < 0) return;
			TileObjectData data = TileObjectData.GetTileData(tile.TileType, style);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int x, out _);
			if ((x / 3) % 2 == 0) {
				spriteEffects ^= SpriteEffects.FlipHorizontally;
			}
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			if (Framing.GetTileSafely(i, j).TileFrameY == 0 && Main.rand.NextFloat(1000) < Main.gfxQuality * 2f) {
				Gore.NewGore(Entity.GetSource_None(), new Vector2(i + Main.rand.Next(2), j + Main.rand.Next(2)) * 16, default, GoreID.ChimneySmoke1 + Main.rand.Next(3));
			}
		}
	}
	public class Hydrothermal_Vent_Goopy : Hydrothermal_Vent, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			(int i, int j) = tile.GetTilePosition();
			int style = TileObjectData.GetTileStyle(tile);
			if (style < 0) return;
			TileObjectData data = TileObjectData.GetTileData(tile.TileType, style);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int x, out _);
			int tileFrameX = tile.TileFrameX / 18;
			if ((x / 3) % 2 == 0) {
				tileFrameX ^= 1;
			}
			switch ((tileFrameX, tile.TileFrameY / 18)) {
				case (0, 0):
				case (1, 0):
				case (0, 1):
				case (1, 1):

				case (3, 0):
				case (2, 1):

				case (4, 0):
				case (4, 1):
				case (4, 2):
				case (5, 1):
				case (5, 2):
				color.DoFancyGlow(new(0.912f, 0.879f, 0.394f), tile.TileColor);
				break;
			}
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileBlockLight[Type] = false;
			//TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.Origin = new(0, 3);
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, 4).ToArray();
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(56, 73, 56), CreateMapEntryName());
			DustType = DustID.GreenMoss;

		}
		public override void RandomUpdate(int i, int j) { }
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => Main.LocalPlayer.HeldItem is not { pick: > 0 };
		public override bool RightClick(int i, int j) {
			if (Main.LocalPlayer.HeldItem is not { pick: > 0 }) return false; // null check & pick power check
			if (!Transform(i, j, (ushort)ModContent.TileType<Hydrothermal_Vent>())) return false;
			int item = Item.NewItem(WorldGen.GetNPCSource_ShakeTree(i, j), new Vector2(i, j) * 16, ModContent.ItemType<Geothermal_Sludge>(), Main.rand.Next(6, 11));
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
			}
			return true;
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowColor = GlowColor;
			drawData.glowSourceRect = new(drawData.tileFrameX, drawData.tileFrameY, 16, 16);
			drawData.glowTexture = this.GetGlowTexture(drawData.tileCache.TileColor);

			if (Main.rand.NextFloat(1000) >= Main.gfxQuality * 1000f) return;
			Tile tile = Main.tile[i, j];
			int style = TileObjectData.GetTileStyle(tile);
			if (style < 0) return;
			TileObjectData data = TileObjectData.GetTileData(tile.TileType, style);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int x, out _);
			int tileFrameX = tile.TileFrameX / 18;
			Vector2 flipMult = Vector2.One;
			Vector2 smokePos = new(i * 16 + 8, j * 16 + 8);
			if ((x / 3) % 2 == 0) {
				tileFrameX ^= 1;
				flipMult.X *= -1;
				smokePos -= Vector2.One * 4;
			}
			bool doSmoke = true;
			switch ((tileFrameX, tile.TileFrameY / 18)) {
				case (0, 0):
				smokePos += new Vector2(4, 4) * flipMult;
				break;
				case (1, 0):
				smokePos += new Vector2(-4, 4) * flipMult;
				break;

				case (3, 0):
				smokePos += new Vector2(-8, -6) * flipMult;
				break;
				case (2, 1):
				smokePos += new Vector2(-6, -10) * flipMult;
				break;

				case (4, 0):
				smokePos += new Vector2(0, 4) * flipMult;
				break;
				case (4, 2):
				smokePos += new Vector2(0, -8) * flipMult;
				break;
				case (5, 1):
				smokePos += new Vector2(0, 6) * flipMult;
				break;

				default:
				doSmoke = false;
				break;
			}
			if (doSmoke) {
				Dust.NewDustDirect(
					smokePos,
					4,
					12,
					DustID.Smoke,
					0,
					-4,
					newColor: Color.DimGray
				).velocity *= 0.25f;
			}
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (Framing.GetTileSafely(i, j).TileFrameY == 0) {
				r = 0.05f;
				g = 0.0375f;
				b = 0.015f;
			}
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Hydrothermal_Vent_Item : TestingItem {
		public override string Texture => "Origins/Tiles/Brine/Hydrothermal_Vent";

		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = Item.CommonMaxStack;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Hydrothermal_Vent>();
		}
	}
}