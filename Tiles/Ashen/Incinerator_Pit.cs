using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Graphics;
using Origins.Graphics.Primitives;
using Origins.World.BiomeData;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Core.MultiTypeMultiTile;

namespace Origins.Tiles.Ashen {
	public class Incinerator_Pit : OriginTile, IComplexMineDamageTile, IGlowingModTile, IMultiTypeMultiTile {
		AutoLoadingTexture grinderTexture = typeof(Incinerator_Pit).GetDefaultTMLName("_Grinder");
		public static int ID { get; private set; }
		public static ShapeMap Shape => field = field || new ShapeMap(
			new() {
				['X'] = (ushort)ModContent.TileType<Incinerator_Pit>(),
				['O'] = (ushort)ModContent.TileType<Incinerator_Pit_Pit>()
			},
			"X         XXX",
			"XOOO   OOOXXX",
			"XOOOOOOOOOXXX",
			"XOOOOOOOOOXXX",
			"XOOOOOOOOOXXX",
			"XOOOOOOOOOXXX",
			"XOOOOOOOOOXXX",
			"XXXXXXXXXXXXX"
		);
		public override void Load() {
			new TileItem(this, true).RegisterItem();
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (ShouldGlow(tile)) color.DoFancyGlow(new(0.912f, 0.579f, 0f), tile.TileColor);
		}
		protected virtual Color MapColor => new Color(81, 44, 23);
		public override void SetStaticDefaults() {
			this.SetIDProp();
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;

			// Names
			AddMapEntry(MapColor, CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 13;
			TileObjectData.newTile.SetHeight(8);
			TileObjectData.newTile.SetOriginBottomCenter();
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.newTile.HookPlaceOverride = Shape.Place;
			TileObjectData.addTile(Type);
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}

		public void MinePower(int i, int j, int minePower, ref int damage) {
			if (minePower < 55) damage = 0;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (ShouldGlow(Main.tile[i, j])) {
				r = 0.0912f;
				g = 0.0579f;
				b = 0f;
			}
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			if (TileObjectData.IsTopLeft(i, j)) Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomSolid);
			return false;
		}
		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			const float speed = 0.3f;
			Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			Vector2 offset = new Vector2(i, j) * 16 - Main.screenPosition;
			for (int n = 0; n < vertices.Length; n++) {
				Vector2 pos = new(n % 14, n / 14);
				vertices[n].TextureCoordinate = pos / new Vector2(13, 8);
				vertices[n].Position = new(pos * 16 + offset, 0);
				vertices[n].Color = Color.White;
			}

			Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.Tile[Incinerator_Pit_Pit.ID].Value;
			Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, dices, 0, dices.Length / 3);

			Main.graphics.GraphicsDevice.Textures[0] = grinderTexture;
			DrawGrinder(offset + new Vector2(46, 64), (float)Main.timeForVisualEffects * speed);
			DrawGrinder(offset + new Vector2(134, 64), (float)Main.timeForVisualEffects * -speed + MathHelper.PiOver2);

			for (int n = 0; n < vertices.Length; n++) vertices[n].Color = Lighting.GetColor(i + n % 14, j + n / 14);
			Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.Tile[ID].Value;
			Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, dices, 0, dices.Length / 3);
			static void DrawGrinder(Vector2 position, float rotMult) {
				grinderRect.Draw(
				position,
				Color.White,
				new(88, 84),
				rotMult,
				position
			);
			}
		}
		public static PlayerDeathReason DeathReason(Player player) => PlayerDeathReason.ByCustomReason(TextUtils.LanguageTree.Find("Mods.Origins.DeathMessage.Incinerator_Pit").SelectFrom(player).ToNetworkText());
		public static void HurtEntity(Entity entity, Action<int> bounce, Func<bool> hurt) {
			foreach (Point pos in entity.Hitbox.IterateTilesIn()) {
				Tile tile = Main.tile[pos];
				if (tile.TileType != Incinerator_Pit_Pit.ID) continue;
				if (tile.TileFrameY <= 18) bounce((tile.TileFrameX < 18 * 4).ToDirectionInt());
				else {
					if (entity.velocity.Y < 0 || tile.TileFrameY <= 18 * 3) entity.velocity.Y += 4;
					if (hurt()) {

					}
				}
				break;
			}
		}
		public static bool ShouldGlow(Tile tile) => Shape[tile.TileFrameX / 18, tile.TileFrameY / 18, 0] == 'O';
		bool IMultiTypeMultiTile.IsValidTile(Tile tile, int left, int top, int style) => Shape.Matches(tile, left, top, style);
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		private readonly VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[14 * 9];
		static readonly short[] dices = Enumerable.Range(0, 13).SelectMany(x => Enumerable.Range(0, 8).SelectMany<int, short>(y => [
			(short)(x + y * 14), (short)(x + 1 + y * 14), (short)(x + 14 + y * 14),
			(short)(x + 1 + y * 14), (short)(x + 1 + 14 + y * 14), (short)(x + 14 + y * 14),
		])).ToArray();
		private static readonly VertexRectangle grinderRect = new();
	}
	public class Incinerator_Pit_Pit : Incinerator_Pit {
		public new static int ID { get; private set; }
		protected override Color MapColor => new Color(255, 81, 0);
		public override void Load() { }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.tileSolid[Type] = false;
		}
	}
}
