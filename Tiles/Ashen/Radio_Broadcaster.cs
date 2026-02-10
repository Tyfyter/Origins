using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Tools.Liquids;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Radio_Broadcaster : OriginTile, IGlowingModTile, IAshenWireTile {
		public override void Load() {
			new TileItem(this)
			.WithExtraStaticDefaults(this.DropTileItem)
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Wire, 8)
				.AddIngredient<Oil_Bucket>()
				.AddIngredient(ModContent.ItemType<Scrap>(), 12)
				.AddTile(ModContent.TileType<Metal_Presser>())
				.AddOnCraftCallback((recipe, item, consumedItems, destinationStack) => {
					for (int i = 0; i < consumedItems.Count; i++) {
						if (consumedItems[i].type == ModContent.ItemType<Oil_Bucket>()) {
							Main.LocalPlayer.GetItem(Main.myPlayer, new Item(ItemID.EmptyBucket, consumedItems[i].stack), new GetItemSettings(NoText: true, CanGoIntoVoidVault: true));
						}
					}
				})
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
			TileObjectData.newTile.Origin = new Point16(TileObjectData.newTile.Width / 2 - 1, TileObjectData.newTile.Height - 1);
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
			}
		}

		public void UpdatePowerState(int i, int j, bool powered) {
			AshenWireTile.DefaultUpdatePowerState(i, j, powered, tile => ref tile.TileFrameX, 18 * 2, true);
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			FrameSurrounding(i, j, out _, out _);
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
	}
}
