using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Graphics;
using Origins.Tiles.Ashen;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Origins.Walls;
using Origins.World.BiomeData;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
	#region base classes
	public class Chambersite_Ore : OriginTile {
		public static List<Chambersite_Ore> chambersiteTiles = [];
		public virtual int StoneTile => TileID.Stone;
		public virtual int StoneItem => ItemID.StoneBlock;
		public virtual string ItemTexture => $"Terraria/Images/Item_{StoneItem}";
		public virtual LocalizedText ItemDisplayName => Lang.GetItemName(StoneItem);
		public override string Texture => $"Terraria/Images/Tiles_{StoneTile}";
		public virtual Color MapColor => FromHexRGB(0x116166);
		public new virtual SoundStyle HitSound => SoundID.Tink;
		public new virtual int DustType => DustID.Stone;
		public virtual string OverlayPath => "Origins/Tiles/Overlays/Chambersite/Chambersite_Ore";
		public virtual string ItemOverlayPath => "Origins/Tiles/Overlays/Chambersite/Chambersite_Ore_Item";
		protected Asset<Texture2D> Overlay { get; private set; }
		public TileItem Item { get; protected set; }
		public virtual Recipe ItemRecipe(Item item) => Recipe.Create(item.type)
			.AddIngredient<Chambersite_Item>()
			.AddIngredient(StoneItem)
			.AddTile(TileID.HeavyWorkBench)
			.AddCondition(Condition.InGraveyard)
			.Register();

		public static Chambersite_Ore GetOre(int stone) => chambersiteTiles.FirstOrDefault(ore => ore.StoneTile == stone);
		public static int GetOreID(int stone) => GetOre(stone).Type;

		public override void Load() {
			Mod.AddContent(Item = new Chambersite_Ore_Item(this).WithOnAddRecipes(item => ItemRecipe(item)));
			chambersiteTiles.Add(this);
			PaintKey = CustomTilePaintLoader.CreateKey();
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) Overlay = Request<Texture2D>(OverlayPath);
			Main.tileSolid[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[StoneTile];
			Main.tileMerge[Type][StoneTile] = true;
			Main.tileMerge[StoneTile][Type] = true;
			for (int i = 0; i < chambersiteTiles.Count; i++) {
				if (chambersiteTiles[i].Type != Type) {
					Main.tileMerge[Type][chambersiteTiles[i].Type] = true;
					Main.tileMerge[chambersiteTiles[i].Type][Type] = true;
					Main.tileMerge[Type][chambersiteTiles[i].StoneTile] = true;
					Main.tileMerge[chambersiteTiles[i].StoneTile][Type] = true;
				}
			}
			TileID.Sets.Ore[Type] = true;
			AddMapEntry(MapColor, Language.GetText("Mods.Origins.Items.Chambersite_Item.DisplayName"));
			MinPick = 65;
			MineResist = 2f;
			RegisterItemDrop(ItemType<Chambersite_Item>());
			base.HitSound = HitSound;
			base.DustType = DustType;
			chambersiteTiles.Add(this);
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (!TileDrawing.IsVisible(tile)) return;
			Texture2D texture = CustomTilePaintLoader.TryGetTileAndRequestIfNotReady(PaintKey, tile.TileColor, Overlay);
			OriginExtensions.DrawTileGlow(texture, Lighting.GetColor(i, j), i, j, spriteBatch);
		}
		protected CustomTilePaintLoader.CustomTileVariationKey PaintKey { get; private set; }
		internal record struct ChambersiteParmeters(ModTile Tile, ModItem Item, string TileOverlay, string ItemOverlay, Func<int> DustType, Func<SoundStyle> HitSound, params string[] LegacyNames) {
			public Func<SoundStyle> HitSound { get; } = HitSound ?? (() => SoundID.Tink);
		}
		public static string overlay_path_base = "Origins/Tiles/Overlays/Chambersite/Chambersite_Ore_";
		public static ModTile Create(ModTile baseTile, ModItem baseItem, Func<int> dustType, string tileOverlay = null, string itemOverlay = null, Func<SoundStyle> hitSound = null, params string[] legacyNames) {
			if (baseItem.Mod != baseTile.Mod) throw new ArgumentException($"{nameof(baseTile)} and {nameof(baseItem)} must be from the same mod, I don't know what you're doing, but I do know that it's a bad idea", nameof(baseItem));
			Chambersite_Ore_Modular tile = new(new(baseTile, baseItem, tileOverlay, itemOverlay, dustType, hitSound, legacyNames));
			baseTile.Mod.AddContent(tile);
			return tile;
		}
	}
	[Autoload(false)]
	public class Chambersite_Ore_Item(Chambersite_Ore tile) : TileItem(tile) {
		[field: CloneByReference]
		Chambersite_Ore Tile { get; } = tile;
		public override string Texture => Tile.ItemTexture;
		public override LocalizedText DisplayName => Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.Chambersite_Ore.DisplayName")).WithFormatArgs(Tile.ItemDisplayName);
		public override LocalizedText Tooltip => LocalizedText.Empty;
		protected Asset<Texture2D> Overlay { get; private set; }
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
			Overlay = Request<Texture2D>(Tile.ItemOverlayPath);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.value = Item.sellPrice(silver: 1);
			Item.height = 14;
			Item.width = 14;
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Texture2D texture = TextureAssets.Item[Type].Value;
			if (itemColor != Color.Transparent) drawColor = drawColor.MultiplyRGBA(itemColor);
			spriteBatch.Draw(texture, position, null, drawColor, 0, origin, scale, SpriteEffects.None, 0);
			spriteBatch.Draw(Overlay.Value, position, null, drawColor, 0, origin + Vector2.UnitY * 2, scale, SpriteEffects.None, 0);
			return false;
		}
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			Texture2D texture = TextureAssets.Item[Type].Value;
			Vector2 position = Item.Center - Main.screenPosition;
			spriteBatch.Draw(texture, position, null, lightColor, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0);
			spriteBatch.Draw(Overlay.Value, position, null, lightColor, rotation, (Overlay.Size() * 0.5f) + Vector2.UnitY, scale, SpriteEffects.None, 0);
			return false;
		}
	}
	[Autoload(false)]
	internal class Chambersite_Ore_Modular(Chambersite_Ore.ChambersiteParmeters parameters) : Chambersite_Ore {
		public override string Name => "Chambersite_Ore_" + parameters.Tile.Name;
		public override int StoneItem => parameters.Item.Type;
		public override string ItemTexture => parameters.Item.Texture;
		public override LocalizedText ItemDisplayName => parameters.Item.DisplayName;
		public override int StoneTile => parameters.Tile.Type;
		public override string Texture => parameters.Tile.Texture;
		public override string OverlayPath => parameters.TileOverlay ?? base.OverlayPath;
		public override string ItemOverlayPath => parameters.ItemOverlay ?? base.ItemOverlayPath;
		public override SoundStyle HitSound => parameters.HitSound();
		public override int DustType => parameters.DustType();
		public override Recipe ItemRecipe(Item item) => base.ItemRecipe(item).SortAfterFirstRecipesOf(GetOre(TileID.Stone).Item.Type);
	}
	#endregion
	public class Chambersite_Ore_Ebonstone : Chambersite_Ore {
		public override int StoneTile => TileID.Ebonstone;
		public override int StoneItem => ItemID.EbonstoneBlock;
		public override int DustType => DustID.Corruption;
		public override Color MapColor => new(109, 90, 128);
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			GetInstance<CorruptionAltBiome>().AddChambersiteTileConversions(Type);
		}
	}
	public class Chambersite_Ore_Crimstone : Chambersite_Ore {
		public override int StoneTile => TileID.Crimstone;
		public override int StoneItem => ItemID.CrimstoneBlock;
		public override int DustType => DustID.Crimstone;
		public override Color MapColor => new(128, 44, 45);
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			GetInstance<CrimsonAltBiome>().AddChambersiteTileConversions(Type);
		}
	}
	/*public class Chambersite_Ore_Defiled_Stone : Chambersite_Ore {
		public override int StoneType => TileType<Defiled_Stone>();
		//public override Color MapColor => new(200, 200, 200);
	}
	public class Chambersite_Ore_Riven_Flesh : Chambersite_Ore {
		public override int StoneType => TileType<Spug_Flesh>();
		//public override Color MapColor => new(0, 125, 200);
		public override string OverlayPath => base.OverlayPath + "_Flesh";
	}
	public class Chambersite_Ore_Tainted_Stone : Chambersite_Ore {
		public override int StoneType => TileType<Tainted_Stone>();
		//public override Color MapColor => new Color(133, 89, 62);
	}*/
}
