using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria;
using Microsoft.Xna.Framework;

namespace Origins.Tiles.Other {
	public class Swarm_Statue : ModTile {
		public override string Texture => "Origins/Tiles/Other/Defiled_Fountain";
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.IsAMechanism[Type] = true; // Ensures that this tile and connected pressure plate won't be removed during the "Remove Broken Traps" worldgen step

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.addTile(Type);

			DustType = DustID.Stone;

			AddMapEntry(new Color(144, 148, 144), Language.GetText("MapObject.Statue"));
		}

		// This hook allows you to make anything happen when this statue is powered by wiring.
		// In this example, powering the statue either spawns a random coin with a 95% chance, or, with a 5% chance - a goldfish.
		public override void HitWire(int i, int j) {
			const float range = 16 * 50;
			// Find the coordinates of top left tile square through math
			int y = j - Main.tile[i, j].TileFrameY / 18;
			int x = i - Main.tile[i, j].TileFrameX / 18;

			const int TileWidth = 2;
			const int TileHeight = 3;

			// Here we call SkipWire on all tile coordinates covered by this tile. This ensures a wire signal won't run multiple times.
			for (int yy = y; yy < y + TileHeight; yy++) {
				for (int xx = x; xx < x + TileWidth; xx++) {
					Wiring.SkipWire(xx, yy);
				}
			}

			// Calculate the center of this tile.
			Vector2 center = new((x + TileWidth * 0.5f) * 16, (y + TileHeight * 0.5f) * 16);
			foreach (Player player in Main.ActivePlayers) {
				if (!player.dead && player.DistanceSQ(center) < range * range) {
					player.AddBuff(ModContent.BuffType<Swarm_Statue_Buff>(), 7);
				}
			}
		}
	}
	public class Swarm_Statue_Item : ModItem, IItemObtainabilityProvider {
		public IEnumerable<int> ProvideItemObtainability() => [Type];
		public override string Texture => "Origins/Tiles/Other/Defiled_Fountain_Item";
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Swarm_Statue>());
		}
	}
	public class Swarm_Statue_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Battle;
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().swarmStatue = true;
		}
	}
}
