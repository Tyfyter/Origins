using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Other.Testing;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
namespace Origins.Tiles.Riven {
	public class Gel_Blister : ModTile, IGlowingModTile {
		public static AutoCastingAsset<Texture2D> LesionGlowTexture { get; private set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get => LesionGlowTexture; private set => LesionGlowTexture = value; }
		public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * GlowValue);
		}
		public override void SetStaticDefaults() {
			Main.tileSpelunker[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 1200;
			Main.tileFrameImportant[Type] = true;
			Main.tileHammer[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLighted[Type] = true;
			//Main.tileOreFinderPriority[Type] = 500;
			TileID.Sets.GeneralPlacementTiles[Type] = false;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.CoordinateHeights = [16, 16];
			TileObjectData.newTile.AnchorWall = true;
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.WaterDeath = false;
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(20, 136, 182), CreateMapEntryName());
			AdjTiles = [TileID.ShadowOrbs];
			HitSound = SoundID.NPCDeath1;
		}
		public override bool CreateDust(int i, int j, ref int type) {
			Origins.instance.SpawnGoreByName(
				new EntitySource_TileBreak(i, j),
				new Vector2(i * 16, j * 16),
				default,
				"Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)
			);
			return false;
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			WorldGen.SectionTileFrame(i, j + 2, i + 4, j + 4 + 2);
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			return true;
		}
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Riven_Hive.CheckLesion(i, j, Type);
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowTexture = drawData.drawTexture;
			drawData.glowSourceRect = new Rectangle(drawData.tileFrameX, drawData.tileFrameY, 16, 16);
			drawData.glowColor = GlowColor;
			drawData.finalColor = Color.Transparent;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.05f * GlowValue;
			g = 0.0375f * GlowValue;
			b = 0.015f * GlowValue;
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Gel_Blister_Item : TestingItem {
		public override string Texture => typeof(Gel_Blister).GetDefaultTMLName();
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Gel_Blister>();
		}
	}
}
