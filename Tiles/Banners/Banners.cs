using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Banners {
	[Autoload(false)]
	public class Banner(ModNPC npc, int killsRequired = 50) : ModTile {
		public ModNPC NPC => npc;
		internal int killsRequired = killsRequired;
		public override string Name => npc.Name + "_Banner";
		public override void Load() {
			Mod.AddContent(new Banner_Item(this));
		}
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.MultiTileSway[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(91, 0));
			TileObjectData.addTile(Type);
			DustType = -1;
			AddMapEntry(new Color(13, 88, 130), Language.GetText("MapObject.Banner"));
		}
		public override void NearbyEffects(int i, int j, bool closer) {
			if (!closer) {
				Main.SceneMetrics.NPCBannerBuff[npc.Type] = true;
				Main.SceneMetrics.hasBanner = true;
			}
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY -= 2;
			/*Tile tile = Main.tile[i, j];
			TileObjectData data = TileObjectData.GetTileData(tile);
			int x = i - tile.TileFrameX / 18 % data.Width;
			int topLeftY = j - tile.TileFrameY / 18 % data.Height;
			if (WorldGen.IsBelowANonHammeredPlatform(x, topLeftY)) {
				offsetY -= 8;
			}*/
		}
		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 != 0) spriteEffects = SpriteEffects.FlipHorizontally;
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];
			if (TileObjectData.IsTopLeft(tile)) {
				// Makes this tile sway in the wind and with player interaction when used with TileID.Sets.MultiTileSway
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);
			}
			// We must return false here to prevent the normal tile drawing code from drawing the default static tile. Without this a duplicate tile will be drawn.
			return false;
		}
	}
	public class Banner_Item(Banner tile) : ModItem {
		[CloneByReference] readonly Banner tile = tile;
		protected override bool CloneNewInstances => true;
		public override string Name => tile.Name + "_Item";
		public override LocalizedText DisplayName {
			get {
				if (OriginExtensions.TryGetText(this.GetLocalizationKey("DisplayName"), out LocalizedText text)) return text;
				return Language.GetOrRegister("Mods.Origins.Items.Banner_Item.DisplayName").WithFormatArgs(tile.NPC.DisplayName);
			}
		}
		public override LocalizedText Tooltip {
			get {
				if (OriginExtensions.TryGetText(this.GetLocalizationKey("Tooltip"), out LocalizedText text)) return text;
				return Language.GetOrRegister("Mods.Origins.Items.Banner_Item.Tooltip").WithFormatArgs(tile.NPC.DisplayName);
			}
		}
		public override void Load() => BannerGlobalNPC.BannerItems.Add(this);
		public override void SetStaticDefaults() {
			_ = tile.NPC.Name;
			ItemID.Sets.KillsToBanner[Type] = tile.killsRequired;
		}
		internal void CacheItemType() {
			if (BannerGlobalNPC.NPCTypesWithBanners.Contains(tile.NPC.GetType())) BannerGlobalNPC.NPCToBannerItem.Add(tile.NPC.Type, Type);
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(tile.Type);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 2);
		}
	}
	public class BannerGlobalNPC : GlobalNPC {
		public static HashSet<Type> NPCTypesWithBanners { get; private set; } = [];
		public static Dictionary<int, int> NPCToBannerItem { get; private set; } = [];
		public static List<Banner_Item> BannerItems { get; private set; } = [];
		public override void Unload() {
			NPCTypesWithBanners = null;
			NPCToBannerItem = null;
			BannerItems = null;
		}
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => NPCToBannerItem.ContainsKey(entity.type);
		public override void SetDefaults(NPC entity) {
			entity.ModNPC.Banner = entity.type;
			entity.ModNPC.BannerItem = NPCToBannerItem[entity.type];
		}
		public static void BuildBannerCache() {
			for (int i = 0; i < BannerItems.Count; i++) {
				BannerItems[i].CacheItemType();
			}
			BannerItems.Clear();
			BannerItems.TrimExcess();
		}
	}
}
