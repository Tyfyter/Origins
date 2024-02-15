using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Items.Accessories {
	public class Terrarian_Voodoo_Doll : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Misc"
		};
		Guid owner;
		Player player;
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.rare = ItemRarityID.Green;
		}
		public override bool OnPickup(Player player) {
			if (!Main.dedServ && owner == Guid.Empty) {
				owner = player.GetModPlayer<OriginPlayer>().guid;
			}
			return true;
		}
		public OriginPlayer RefreshPlayer() {
			if (player is null && OriginPlayer.playersByGuid.TryGetValue(owner, out int id)) {
				player = Main.player[id];
			}
			if (player is not null) {
				OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
				if (originPlayer.guid != owner) {
					player = null;
					return null;
				}
				return originPlayer;
			}
			return null;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (RefreshPlayer() is not null) {
				tooltips.Add(new TooltipLine(Mod, "Tooltip0", Language.GetTextValue("Mods.Origins.Items.Terrarian_Voodoo_Doll.TooltipPlayer", player.name)));
			} else {
				tooltips.Add(new TooltipLine(Mod, "Tooltip0", Language.GetTextValue("Mods.Origins.Items.Terrarian_Voodoo_Doll.TooltipYouDoNotRecognizeTheBodiesInTheWater")));
			}
		}
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			if (RefreshPlayer() is OriginPlayer originPlayer) {
				originPlayer.voodooDollIndex = Item.whoAmI;
				if (player.whoAmI == Main.myPlayer && !player.shimmerImmune && !player.shimmerUnstuckHelper.ShouldUnstuck) {
					int headX = (int)(Item.Center.X / 16f);
					int headY = (int)((Item.position.Y + 1f) / 16f);
					if (Item.position.Y / 16f < Main.UnderworldLayer && Main.tile[headX, headY] != null && Main.tile[headX, headY].LiquidType == LiquidID.Shimmer && Main.tile[headX, headY].LiquidAmount >= 0) {
						player.AddBuff(353, 60);
					}
				}
			}
		}
		public override void NetSend(BinaryWriter writer) {
			writer.Write(owner.ToByteArray());
		}
		public override void NetReceive(BinaryReader reader) {
			owner = new Guid(reader.ReadBytes(16));
		}
		public override void SaveData(TagCompound tag) {
			tag.Add("owner", owner.ToByteArray());
		}
		public override void LoadData(TagCompound tag) {
			if (tag.TryGet("owner", out byte[] guidBytes)) {
				owner = new Guid(guidBytes);
			} else {
				owner = Guid.Empty;
			}
		}
	}
}
