using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
namespace Origins.Items.Accessories {
	public class Terrarian_Voodoo_Doll : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"MasterAcc"
		};
		Guid owner;
		Player player;
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.rare = ItemRarityID.Master;
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
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<OriginPlayer>().pickupRangeBoost += 75;
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
				TryPickupItems();
				/*Rectangle playerHitbox = player.Hitbox;
				try {
					player.Hitbox = Item.Hitbox;
					originPlayer.isVoodooPickup = true;
					PlayerMethods.GrabItems(player);
				} finally {
					player.Hitbox = playerHitbox;
					originPlayer.isVoodooPickup = false;
				}*/
			}
		}
		void TryPickupItems() {
			Vector2 center = player.Center;
			try {
				player.Center = Item.Center;
				Rectangle pullHitbox = new((int)Item.position.X, (int)Item.position.Y, Item.width, Item.height);
				for (int i = 0; i < Main.maxItems; i++) {
					Item item = Main.item[i];
					if (!item.active) continue;
					if (item.type == Type) continue;
					if (PreventItemPickup(item, player)) continue;
					if (item.Hitbox.Intersects(Item.Hitbox)) {
						if (player.whoAmI != Main.myPlayer || (player.HeldItem.type == ItemID.None && player.itemAnimation > 0)) {
							continue;
						}
						if (CombinedHooks.OnPickup(item, player)) {
							PlayerMethods.PickupItem(player, i, item);
						} else {
							Main.item[i] = new Item();
							if (Main.netMode == NetmodeID.MultiplayerClient) {
								NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i);
							}
						}
					} else {
						int itemGrabRange = player.GetItemGrabRange(item);
						pullHitbox.Inflate(itemGrabRange, itemGrabRange);
						if (pullHitbox.Intersects(item.Hitbox)) {
							Player.ItemSpaceStatus status = player.ItemSpace(item);
							if (player.CanPullItem(item, status)) {
								item.shimmered = false;
								item.beingGrabbed = true;
								if (!ItemLoader.GrabStyle(item, player)) {
									if (player.manaMagnet && (item.type is ItemID.Star or ItemID.SoulCake or ItemID.SugarPlum)) {
										PlayerMethods.PullItem_Pickup(player, item, 12f, 5);
									} else if (player.lifeMagnet && (item.type is ItemID.Heart or ItemID.CandyApple or ItemID.CandyCane)) {
										PlayerMethods.PullItem_Pickup(player, item, 15f, 5);
									} else if (ItemID.Sets.NebulaPickup[item.type]) {
										PlayerMethods.PullItem_Pickup(player, item, 12f, 5);
									} else if (status.ItemIsGoingToVoidVault) {
										PlayerMethods.PullItem_ToVoidVault(player, item);
									} else if (player.goldRing && item.IsACoin) {
										PlayerMethods.PullItem_Pickup(player, item, 12f, 5);
									} else {
										PlayerMethods.PullItem_Common(player, item, 0.75f);
									}
								}
							}
						}
						pullHitbox.Inflate(-itemGrabRange, -itemGrabRange);
					}
				}
			} finally {
				player.Center = center;
			}
		}
		static bool PreventItemPickup(Item item, Player player) {
			return item.shimmerTime != 0f
				|| item.noGrabDelay != 0
				|| item.playerIndexTheItemIsReservedFor != player.whoAmI
				|| !player.CanAcceptItemIntoInventory(item)
				|| (item.shimmered && !((double)item.velocity.Length() < 0.2))
				|| !ItemLoader.CanPickup(item, player);
		}
		/*public override bool CanPickup(Player player) {
			return !player.GetModPlayer<OriginPlayer>().isVoodooPickup;
		}*/
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
