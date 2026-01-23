using MagicStorage.Stations;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Tiles;
using Origins.World.BiomeData;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.CrossMod.MagicStorage.Tiles {
	#region base classes
	[ExtendsFromMod(nameof(MagicStorage))]
	public abstract class Evil_Altar_Tile<TBar, TCarcass>(string texture) : Evil_Altar_Tile(texture) where TBar : ModItem where TCarcass : ModItem {
		public override int Bar => ModContent.ItemType<TBar>();
		public override int Carcass => ModContent.ItemType<TCarcass>();
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public abstract class Evil_Altar_Tile(string texture) : ModTile {
		protected internal Fake_Altar_Item item;
		public abstract int Bar { get; }
		public abstract int Carcass { get; }
		public override string Texture => $"Origins/Tiles/{texture}";
		public override void Load() => Mod.AddContent(item = new Fake_Altar_Item(this));
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = false;
			Main.tileLavaDeath[Type] = false;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleWrapLimit = 36;
			TileObjectData.newTile.Origin = new Point16(1, 1);
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			ModifyTileData();
			TileObjectData.addTile(Type);

			AdjTiles = [Type, TileID.DemonAltar];

			AddMapEntry(Color.MediumPurple, Language.GetOrRegister("Mods.MagicStorage.Tiles.EvilAltarTile.MapEntry", PrettyPrintName));
		}
		public virtual void ModifyTileData() { }
	}
	[ExtendsFromMod(nameof(MagicStorage)), Autoload(false)]
	public class Fake_Altar_Item(Evil_Altar_Tile tile) : ModItem {
		public override string Name => tile.Name + "_Item";
		public override string Texture => tile.Texture + "_Item";
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 5;
			ModCompatSets.AnyFakeDemonAltars[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ModContent.ItemType<DemonAltar>());
			Item.createTile = tile.Type;
		}
		public override void AddRecipes() => CreateRecipe().AddIngredient(tile.Bar, 10).AddIngredient(tile.Carcass, 15).AddTile(TileID.DemonAltar).Register();
	}
	#endregion
	public class Fake_Defiled_Altar() : Evil_Altar_Tile<Defiled_Bar, Undead_Chunk>("Defiled/Defiled_Altar") { }
	public class Fake_Riven_Altar() : Evil_Altar_Tile<Encrusted_Bar, Riven_Carapace>("Riven/Riven_Altar"), IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color.DoFancyGlow(new Vector3(0.394f, 0.879f, 0.912f) * GlowValue, tile.TileColor);
		}
		public override void Load() {
			base.Load();
			this.SetupGlowKeys();
		}
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				GlowTexture = Mod.Assets.Request<Texture2D>("Tiles/Riven/Riven_Altar_Glow");
			}
			base.SetStaticDefaults();
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Fake_Ashen_Altar() : Evil_Altar_Tile<Sanguinite_Bar, NE8>("Ashen/Ashen_Altar") {
		public override void ModifyTileData() => AnimationFrameHeight = TileObjectData.newTile.CoordinateHeights.Sum() + 2 * TileObjectData.newTile.Height;
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 5) {
				frameCounter = 0;
				frame = ++frame % 3;
			}
		}
	}
}
