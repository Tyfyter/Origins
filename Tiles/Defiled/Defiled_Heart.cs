using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.Tiles.Other;
using Origins.World.BiomeData;
using PegasusLib;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
namespace Origins.Tiles.Defiled {
	public class Defiled_Heart : ModTile, IComplexMineDamageTile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileHammer[Type] = true;
			Main.tileLighted[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16];
			TileObjectData.newTile.Origin = new Point16(1, 2);
			TileObjectData.newTile.AnchorWall = false;
			TileObjectData.newTile.AnchorTop = AnchorData.Empty;
			TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
			TileObjectData.newTile.AnchorRight = AnchorData.Empty;
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.addTile(Type);
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("{$Defiled} Heart");
			AddMapEntry(new Color(50, 50, 50), name);
			//disableSmartCursor = true;
			AdjTiles = [TileID.ShadowOrbs];
			ID = Type;
			HitSound = Origins.Sounds.DefiledIdle;
			DustType = Defiled_Wastelands.DefaultTileDust;
		}
		public static AutoLoadingAsset<Texture2D> tangelaTexture = typeof(Defiled_Heart).GetDefaultTMLName() + "_Tangela";
		public void MinePower(int i, int j, int minePower, ref int damage) {
			if (minePower < 80) {
				damage = 0;
				HitSound = Origins.Sounds.DefiledIdle.WithPitch(-1f);
			} else {
				HitSound = Origins.Sounds.DefiledIdle;
			}
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 9) {
				frameCounter = 0;
				frame = ++frame % 4;
			}
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
		}
		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (TangelaVisual.DrawOver || TileDrawing.IsVisible(tile)) {
				Vector2 position = new Vector2(i * 16f, j * 16f) - Main.screenPosition;
				TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(tile), out int x, out int y);
				TangelaVisual.DrawTangela(
					tangelaTexture,
					position,
					new Rectangle(tile.TileFrameX, tile.TileFrameY, 48, 48),
					0,
					Vector2.Zero,
					Vector2.One,
					SpriteEffects.None,
					x + y * 787,
					new(i * 16f, j * 16f)
				);
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			// Tweak the frame drawn by x position so tiles next to each other are off-sync and look much more interesting
			int uniqueAnimationFrame = Main.tileFrame[Type] + i;
			if (i % 2 == 0)
				uniqueAnimationFrame += 3;
			if (i % 3 == 0)
				uniqueAnimationFrame += 3;
			if (i % 4 == 0)
				uniqueAnimationFrame += 3;
			uniqueAnimationFrame %= 4;

			frameYOffset = uniqueAnimationFrame * AnimationFrameHeight;
		}
		public override bool CanExplode(int i, int j) {
			return false;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = g = b = 0.3f;
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			ModContent.GetInstance<Defiled_Heart_TE_System>().AddTileEntity(new(i, j));
		}
	}
	public class Defiled_Heart_TE_System : TESystem {
		public override void PreUpdateEntities() {
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			for (int i = 0; i < tileEntityLocations.Count; i++) {
				Point16 pos = tileEntityLocations[i];
				if (!Main.tile[pos.X, pos.Y].TileIsType(Defiled_Heart.ID)) {
					tileEntityLocations.RemoveAt(i);
					i--;
					continue;
				}
			}
		}
		public override void LoadWorldData(TagCompound tag) {
			base.LoadWorldData(tag);
			tileEntityLocations.AddRange(ModContent.GetInstance<OriginSystem>().LegacySave_DefiledHearts);
			tileEntityLocations = tileEntityLocations.Distinct().ToList();
		}
	}
	public class Defiled_Heart_Item : ModItem, ICustomWikiStat, IItemObtainabilityProvider {
		public IEnumerable<int> ProvideItemObtainability() => new int[] { Type };
		public override string Texture => "Origins/Tiles/Defiled/Defiled_Heart";
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
		}
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
			Item.createTile = ModContent.TileType<Defiled_Heart>();
		}
		public bool ShouldHavePage => false;
	}
}
