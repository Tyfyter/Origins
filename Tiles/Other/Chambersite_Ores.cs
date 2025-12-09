using AltLibrary;
using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Graphics;
using Origins.Walls;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
	#region base classes
	public class Chambersite_Ore : OriginTile {
		public static List<int> chambersiteTiles = [];
		public virtual int StoneType => TileID.Stone;
		public override string Texture => StoneType >= TileID.Count ? GetModTile(StoneType).Texture : $"Terraria/Images/Tiles_{StoneType}";
		public virtual Color MapColor => FromHexRGB(0x116166);
		public virtual string OverlayPath => "Origins/Tiles/MergerOverlays/Chambersite_Ore";
		public virtual string ItemOverlayPath => "Origins/Tiles/MergerOverlays/Chambersite_Ore_Item";
		protected Asset<Texture2D> Overlay { get; private set; }
		public TileItem Itm { get; protected set; }

		public override void Load() {
			Mod.AddContent(Itm = new Chambersite_Ore_Item(this).WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient<Chambersite_Item>()
				.AddIngredient(StoneType)
				.AddTile(TileID.HeavyWorkBench)
				.AddCondition(Condition.InGraveyard)
				.Register();
			}));
		}
		public override void SetStaticDefaults() {
			Overlay = Request<Texture2D>(OverlayPath);
			PaintKey = CustomTilePaintLoader.CreateKey();
			Main.tileSolid[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[StoneType];
			Main.tileMerge[Type][StoneType] = true;
			Main.tileMerge[StoneType][Type] = true;
			for (int i = 0; i < chambersiteTiles.Count; i++) {
				if (chambersiteTiles[i] != Type) {
					Main.tileMerge[Type][chambersiteTiles[i]] = true;
					Main.tileMerge[chambersiteTiles[i]][Type] = true;
				}
			}
			TileID.Sets.Ore[Type] = true;
			AddMapEntry(MapColor, Language.GetText("Mods.Origins.Items.Chambersite_Item.DisplayName"));
			MinPick = 65;
			MineResist = 2f;
			RegisterItemDrop(ItemType<Chambersite_Item>());
			chambersiteTiles.Add(Type);
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
			string path = "Terraria/Images/Item_";
			switch (tile.StoneType) {
				case TileID.Stone: {
					path += ItemID.StoneBlock;
					break;
				}
				case TileID.Ebonstone: {
					path += ItemID.EbonstoneBlock;
					break;
				}
				case TileID.Crimstone: {
					path += ItemID.CrimstoneBlock;
					break;
				}
				default: {
					path += ItemID.StoneBlock;
					break;
				}
			}
			return path;
		}
		public override string Texture => GetTexture();
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
	#endregion base classes
	[Autoload(false)]
	public class Chambersite_Ore_Base(int tile) : Chambersite_Ore {
		public string GetName() {
			if (tile >= TileID.Count) return GetModTile(tile).Name;
			else return TileID.Search.GetName(tile);
		}
		public override string Name => "Chambersite_Ore_" + GetName();
		public override int StoneType => tile;
	}
	internal class Chambersite_Ore_Loadable : LateLoadable {
		public override void Load() {
			// Could never have worked because TileConversions is empty until Mod.AddContent can't be run, but especially won't be able to work in 2 months when TileConversions doesn't exist
			/*foreach (AltBiome biome in AltLibrary.AltLibrary.AllBiomes) {
				if (biome.BiomeType == BiomeType.Evil && biome.TileConversions.TryGetValue(TileID.Stone, out int tileID)) {
					Chambersite_Ore ore;
					Chambersite_Stone_Wall wall;
					Mod.AddContent(ore = new Chambersite_Ore_Base(tileID));
					biome.AddChambersiteTileConversions(ore.Type);
				}
			}*/
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
