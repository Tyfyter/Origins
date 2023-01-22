using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
    public class Defiled_Altar : ModTile, IComplexMineDamageTile {
        public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
            Main.tileHammer[Type] = true;
            Main.tileLighted[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.CoordinateHeights = new[] { 18, 18 };
			TileObjectData.addTile(Type);
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("{$Defiled} Altar");
			AddMapEntry(new Color(200, 200, 200), name);
			//disableSmartCursor = true;
			AdjTiles = new int[] { TileID.DemonAltar };
            ID = Type;
		}

		public void MinePower(int i, int j, int minePower, ref int damage) {
			Player player = Main.LocalPlayer;
			if (Main.hardMode && player.HeldItem.hammer >= 80) {
				damage += (int)(1.2f * minePower);
			} else {
				player.Hurt(PlayerDeathReason.ByOther(4), player.statLife / 2, -player.direction);
			}
		}

        public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
            if (Main.netMode == NetmodeID.MultiplayerClient || !Main.hardMode || WorldGen.noTileActions || WorldGen.gen){
		        return;
	        }
            ushort defiledStoneID = (ushort)ModContent.TileType<Defiled_Stone>();
	        int type = WorldGen.altarCount % 3;
	        float veinCount = Main.maxTilesX / 4200;
	        int veinSize = 1 - type;
	        veinCount = veinCount * 310f - (85 * type);
	        veinCount *= 0.85f;
	        veinCount /= WorldGen.altarCount / 3 + 1;
            int messageID = 12;
	        switch (type) {
	            case 0:
		        if (WorldGen.SavedOreTiers.Cobalt == -1) {
			        WorldGen.SavedOreTiers.Cobalt = TileID.Cobalt;
			        if (WorldGen.genRand.NextBool(2)) {
				        WorldGen.SavedOreTiers.Cobalt = TileID.Palladium;
			        }
		        }
		        messageID = 12;
		        if (WorldGen.SavedOreTiers.Cobalt == TileID.Palladium) {
			        messageID += 9;
			        veinCount *= 0.9f;
		        }
		        type = WorldGen.SavedOreTiers.Cobalt;
		        veinCount *= 1.05f;
		        break;
	            case 1:
		        if (WorldGen.SavedOreTiers.Mythril == -1) {
			        WorldGen.SavedOreTiers.Mythril = TileID.Mythril;
			        if (WorldGen.genRand.NextBool(2)) {
				        WorldGen.SavedOreTiers.Mythril = TileID.Orichalcum;
			        }
		        }
		        messageID = 13;
		        if (WorldGen.SavedOreTiers.Mythril == TileID.Orichalcum) {
			        messageID += 9;
			        veinCount *= 0.9f;
		        }
		        type = WorldGen.SavedOreTiers.Mythril;
		        break;
	            default:
		        if (WorldGen.SavedOreTiers.Adamantite == -1) {
			        WorldGen.SavedOreTiers.Adamantite = TileID.Adamantite;
			        if (WorldGen.genRand.NextBool(2)) {
				        WorldGen.SavedOreTiers.Adamantite = TileID.Titanium;
			        }
		        }
		        messageID = 14;
		        if (WorldGen.SavedOreTiers.Adamantite == TileID.Titanium) {
			        messageID += 9;
			        veinCount *= 0.9f;
		        }
		        type = WorldGen.SavedOreTiers.Adamantite;
		        break;
	        }

		    if (Main.netMode == NetmodeID.SinglePlayer) {
			    Main.NewText(Lang.misc[messageID].Value, 50, byte.MaxValue, 130);
		    }else if (Main.netMode == NetmodeID.Server) {
			    ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.misc[messageID].Key), new Color(50, 255, 130));
		    }

	        for (int k = 0; k < veinCount; k++) {
		        int oreX = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
		        double minDepth = Main.worldSurface;
		        if (type == TileID.Mythril || type == TileID.Orichalcum) {
			        minDepth = Main.rockLayer;
		        }
		        if (type == TileID.Adamantite || type == TileID.Titanium) {
			        minDepth = (Main.rockLayer + Main.rockLayer + Main.maxTilesY) / 3.0;
		        }
		        int oreY = WorldGen.genRand.Next((int)minDepth, Main.maxTilesY - 150);
		        WorldGen.OreRunner(oreX, oreY, WorldGen.genRand.Next(5, 9 + veinSize), WorldGen.genRand.Next(5, 9 + veinSize), (ushort)type);
	        }

	        int stoneType = WorldGen.genRand.Next(3);
	        int stoneCount = 0;
	        while (stoneType != 2 && stoneCount++ < 1000) {
		        int stoneX = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
		        int stoneY = WorldGen.genRand.Next((int)Main.rockLayer + 50, Main.maxTilesY - 300);
		        if (!Main.tile[stoneX, stoneY].HasTile || Main.tile[stoneX, stoneY].TileType != 1) {
			        continue;
		        }
		        if (stoneType == 1) {
				    Main.tile[stoneX, stoneY].TileType = defiledStoneID;
		        } else {
			        Main.tile[stoneX, stoneY].TileType = TileID.Pearlstone;
		        }
		        if (Main.netMode == NetmodeID.Server) {
			        NetMessage.SendTileSquare(-1, stoneX, stoneY, 1);
		        }
		        break;
	        }

	        if (Main.netMode != NetmodeID.MultiplayerClient) {
		        int wraithCount = Main.rand.Next(2) + 1;
		        for (int l = 0; l < wraithCount; l++) {
			        NPC.SpawnOnPlayer(Player.FindClosest(new Vector2(i * 16, j * 16), 16, 16), NPCID.Wraith);
		        }
	        }

	        WorldGen.altarCount++;
	        AchievementsHelper.NotifyProgressionEvent(6);
		}
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
            r = g = b = 0.5f;
        }
    }
    public class Defiled_Altar_Item : ModItem {
        public override string Texture => "Origins/Tiles/Defiled/Defiled_Altar";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("{$Defiled} Altar (Debugging Item)");
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
            Item.createTile = ModContent.TileType<Defiled_Altar>();
        }
    }
}
