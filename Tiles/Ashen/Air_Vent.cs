using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Tools.Wiring;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using PegasusLib.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Air_Vent : OriginTile, IAshenWireTile {
		public override void Load() {
			new TileItem(this)
			.WithExtraStaticDefaults(this.DropTileItem)
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddRecipeGroup(ALRecipeGroups.CopperBars)
				.AddIngredient<Scrap>(6)
				.AddTile<Metal_Presser>()
				.Register();
			}).RegisterItem();
		}
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Air_Vent).GetDefaultTMLName("_Glow");
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			HitSound = SoundID.Tink;

			// Names
			AddMapEntry(new Color(21, 28, 25));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.RandomStyleRange = 4;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.AnchorBottom = new AnchorData();
			TileObjectData.newTile.AnchorWall = true;
			TileObjectData.addTile(Type);
			DustType = Ashen_Biome.DefaultTileDust;
			AnimationFrameHeight = 36;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (frameCounter.CycleUp(2)) frame.CycleUp(4);
		}
		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];
			using Graphics.GraphicsExt.TilebatchOverride @override = Main.tileBatch.OverrideState(samplerState: SamplerState.PointClamp, transformMatrix: Main.Transform);
			Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition;
			short tileFrameX = tile.TileFrameX;
			short tileFrameY = tile.TileFrameY;
			Main.instance.TilesRenderer.GetTileDrawData(i, j, tile, Type, ref tileFrameX, ref tileFrameY, out _, out _, out int tileTop, out _, out int addFrX, out int addFrY, out _, out _, out _, out _);
			tileFrameX += (short)addFrX;
			tileFrameY += (short)addFrY;
			pos = pos.Floor();
			pos.Y += tileTop;
			VertexColors glow = new(Color.White);
			Vector4 destination = new(0, 0, 16, 16);
			Rectangle frame = new(0, 0, 16, 16);
			for (int x = 0; x < 2; x++) {
				destination.X = (int)pos.X + x * 16;
				frame.X = tileFrameX + x * 18;
				for (int y = 0; y < 2; y++) {
					destination.Y = (int)pos.Y + y * 16;
					frame.Y = tileFrameY + y * 18;
					Lighting.GetCornerColors(i + x, j + y, out VertexColors vertices);
					Main.tileBatch.Draw(
						TextureAssets.Tile[Type].Value,
						destination,
						frame,
						vertices
					);
					Main.tileBatch.Draw(
						glowTexture,
						destination,
						frame,
						glow
					);
				}
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameY < 18 * 2) frameYOffset = 0;
		}
		public override void HitWire(int i, int j) {
			if (Ashen_Wire_Data.HittingAshenWires) UpdatePowerState(i, j, AshenWireTile.DefaultIsPowered(i, j));
		}
		public void UpdatePowerState(int i, int j, bool powered) {
			AshenWireTile.DefaultUpdatePowerState(i, j, powered, tile => ref tile.TileFrameY, 18 * 2);
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			if (TileObjectData.IsTopLeft(i, j)) Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
			return false;
		}
		public override void NearbyEffects(int i, int j, bool closer) {
			if (closer) return;
			if (Main.tile[i, j].TileFrameY < 18 * 2) return;
			Vector2 targetPoint = Main.LocalPlayer.Center;
			Vector2 fanPoint = new(i * 16, j * 16);
			OriginSystem instance = OriginSystem.Instance;
			if (!instance.nearestFanSound.HasValue || targetPoint.DistanceSQ(fanPoint) < targetPoint.DistanceSQ(instance.nearestFanSound.Value)) {
				instance.nearestFanSound = fanPoint;
			}
		}
	}
}
