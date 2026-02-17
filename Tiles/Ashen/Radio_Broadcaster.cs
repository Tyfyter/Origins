using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Graphics;
using Origins.Items.Tools.Liquids;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Radio_Broadcaster : OriginTile, IGlowingModTile, IAshenWireTile {
		Sound ambientSound = EnvironmentSounds.Register<Sound>();
		public override void Load() {
			new TileItem(this)
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Wire, 8)
				.AddIngredient<Oil_Bucket>()
				.AddIngredient<Scrap>(12)
				.AddTile<Metal_Presser>()
				.AddOnCraftCallback(CraftingCallbacks.BucketCrafting<Oil_Bucket>)
				.Register();
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Wire, 8)
				.AddIngredient<Oil_Bottomless_Bucket>()
				.AddIngredient<Scrap>(12)
				.AddTile<Metal_Presser>()
				.AddOnCraftCallback(CraftingCallbacks.NoConsumeCrafting<Oil_Bottomless_Bucket>)
				.Register();
			}).RegisterItem();
			this.SetupGlowKeys();
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) { }
		public override void SetStaticDefaults() {
			if (!Main.dedServ) GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			HitSound = SoundID.Tink;

			// Names
			AddMapEntry(FromHexRGB(0x753F1A));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.SetHeight(4, 18);
			TileObjectData.newTile.SetOriginBottomCenter();
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.FlattenAnchors = true;
			TileObjectData.addTile(Type);
			DustType = Ashen_Biome.DefaultTileDust;
			AnimationFrameHeight = 74;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void HitWire(int i, int j) {
			UpdatePowerState(i, j, AshenWireTile.DefaultIsPowered(i, j));
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX >= 18 * 2) frameYOffset = 0;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 8) {
				frameCounter = 0;
				frame = ++frame % 4;
				//SoundEngine.PlaySound(Origins.Sounds.HawkenThruster.WithPitch(2.5f).WithVolume(0.05f));
			}
			/*if (frame == 1) {
				SoundEngine.PlaySound(SoundID.Camera.WithPitch(0.5f).WithVolume(0.08f));
			}
			if (Main.rand.NextBool(10)) {
				SoundEngine.PlaySound(SoundID.Item141.WithPitch(3f).WithVolume(0.12f));
			}*/
		}

		public void UpdatePowerState(int i, int j, bool powered) {
			AshenWireTile.DefaultUpdatePowerState(i, j, powered, tile => ref tile.TileFrameX, 18 * 2, true);
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			FrameSurrounding(i, j, out _, out _);
		}
		public override void NearbyEffects(int i, int j, bool closer) {
			if (closer) return;
			ambientSound.TrySetNearest(new(i * 16 + 8, j * 16 + 8));
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		class Sound : AEnvironmentSound {
			public override void UpdateSound(Vector2 position) {
				int type = ModContent.TileType<Radio_Broadcaster>();
				float mult = 1 / float.Max(position.DistanceSQ(Main.Camera.Center) / (16 * 20 * 16 * 20), 1);
				if (Main.tileFrameCounter[type] == 0) {
					SoundEngine.PlaySound(Origins.Sounds.HawkenThruster.WithPitch(2.5f).WithVolume(0.2f * mult), position);
				}
				if (Main.tileFrame[type] == 1) SoundEngine.PlaySound(SoundID.Camera.WithPitch(0.5f).WithVolume(0.32f * mult), position);
				if (Main.rand.NextBool(10)) SoundEngine.PlaySound(SoundID.Item141.WithPitch(3f).WithVolume(0.48f * mult), position);
			}
		}
	}
}
