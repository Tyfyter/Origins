using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Graphics;
using Origins.Tiles.Ashen;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using ReLogic.Content;
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
		public override string Texture => StoneTile < TileID.Count ? $"Terraria/Images/Tiles_{StoneTile}" : GetModTile(StoneTile).Texture;
		public virtual Color MapColor => FromHexRGB(0x116166);
		public new virtual SoundStyle HitSound => SoundID.Tink;
		public new virtual int DustType => DustID.Stone;
		public virtual string OverlayPath => "Origins/Tiles/MergerOverlays/Chambersite_Ore";
		public virtual string ItemOverlayPath => "Origins/Tiles/MergerOverlays/Chambersite_Ore_Item";
		protected Asset<Texture2D> Overlay { get; private set; }
		public TileItem Itm { get; protected set; }
		public virtual Recipe ItemRecipe(Item item) => Recipe.Create(item.type)
			.AddIngredient<Chambersite_Item>()
			.AddIngredient(StoneItem)
			.AddTile(TileID.HeavyWorkBench)
			.AddCondition(Condition.InGraveyard)
			.Register();

		public static Chambersite_Ore GetOre(int stone) => chambersiteTiles.FirstOrDefault(ore => ore.StoneTile == stone);
		public static int GetOreID(int stone) => GetOre(stone).Type;

		public override void Load() {
			Mod.AddContent(Itm = new Chambersite_Ore_Item(this).WithOnAddRecipes(item => {
				ItemRecipe(item);
			}));
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
	}
	[Autoload(false)]
	public class Chambersite_Ore_Item(Chambersite_Ore tile) : TileItem(tile) {
		public string GetTexture() {
			if (tile.StoneItem < ItemID.Count) return $"Terraria/Images/Item_{tile.StoneItem}";
			else return GetModItem(tile.StoneItem).Texture;
		}
		public LocalizedText GetName() {
			if (tile.StoneItem < ItemID.Count) return Language.GetText($"ItemName.{ItemID.Search.GetName(tile.StoneItem)}");
			else return GetModItem(tile.StoneItem).DisplayName;
		}
		public override string Texture => GetTexture();
		public override LocalizedText DisplayName => Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.Chambersite_Ore.DisplayName")).WithFormatArgs(GetName());
		public override LocalizedText Tooltip => LocalizedText.Empty;
		protected Asset<Texture2D> Overlay { get; private set; }
		public override void SetDefaults() {
			Overlay = Request<Texture2D>(tile.ItemOverlayPath);
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
	public class Chambersite_Ore_Base((int tile, int item, string tileOverlay, string itemOverlay, SoundStyle hitSound, int dustType) stone) : Chambersite_Ore {
		public string GetName() {
			if (stone.tile < TileID.Count) return TileID.Search.GetName(stone.tile);
			else return GetModTile(stone.tile).Name;
		}
		public override string Name => "Chambersite_Ore_" + GetName();
		public override int StoneItem => stone.item;
		public override int StoneTile => stone.tile;
		public override string OverlayPath => string.IsNullOrEmpty(stone.tileOverlay) ? base.OverlayPath : stone.tileOverlay;
		public override string ItemOverlayPath => string.IsNullOrEmpty(stone.itemOverlay) ? base.ItemOverlayPath : stone.itemOverlay;
		public override SoundStyle HitSound => stone.hitSound;
		public override int DustType => stone.dustType;
		public override Recipe ItemRecipe(Item item) => base.ItemRecipe(item).SortAfterFirstRecipesOf(GetOre(TileID.Stone).Itm.Type);
	}
	#endregion
	internal class Chambersite_Ore_Loadable : LateLoadable {
		public static string mergePath = "Origins/Tiles/MergerOverlays/Chambersite_Ore";
		public static List<(int tile, int item, string tileOverlay, string itemOverlay, SoundStyle hitSound, int dustType)> Stones = [
			(TileID.Ebonstone, ItemID.EbonstoneBlock, null, null, SoundID.Tink, DustID.Corruption),
			(TileID.Crimstone, ItemID.CrimstoneBlock, null, null, SoundID.Tink, DustID.Crimstone),
			(TileType<Defiled_Stone>(), ItemType<Defiled_Stone_Item>(), null, null, Origins.Sounds.DefiledIdle, Defiled_Wastelands.DefaultTileDust),
			(TileType<Spug_Flesh>(), ItemType<Riven_Flesh_Item>(), mergePath + "_Flesh", null, SoundID.Tink, Riven_Hive.DefaultTileDust),
			(TileType<Tainted_Stone>(), ItemType<Tainted_Stone_Item>(), null, null, SoundID.Tink, Ashen_Biome.DefaultTileDust)
		];
		public override void Load() {
			foreach ((int, int, string, string, SoundStyle, int) stone in Stones) {
				Mod.AddContent(new Chambersite_Ore_Base(stone));
			}
		}
	}
	/*	public class Chambersite_Ore_Ebonstone : Chambersite_Ore {
			public override int StoneType => TileID.Ebonstone;
			//public override Color MapColor => new(109, 90, 128);
		}
		public class Chambersite_Ore_Crimstone : Chambersite_Ore {
			public override int StoneType => TileID.Crimstone;
			//public override Color MapColor => new(128, 44, 45);
		}
		public class Chambersite_Ore_Defiled_Stone : Chambersite_Ore {
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
