using CalamityMod.Items.Placeables;
using CalamityMod.Items.Potions.Alcohol;
using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
namespace Origins.Items.Accessories {
	public class Terrarian_Voodoo_Doll : ModItem, ICustomWikiStat {
		public string[] Categories => [
		];
		Guid owner;
		Player player;
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemSpawnDecaySpeed[Type] = 0;
			ItemID.Sets.OverflowProtectionTimeOffset[Type] = -36000;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.rare = ItemRarityID.Green;
			Item.master = true;
		}
		public override bool OnPickup(Player player) {
			if (!Main.dedServ && owner == Guid.Empty) {
				owner = player.GetModPlayer<OriginPlayer>().guid;
			}
			return true;
		}
		public OriginPlayer RefreshPlayer() {
			if (player is null && owner != Guid.Empty && OriginPlayer.playersByGuid.TryGetValue(owner, out int id)) {
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
			} else if (OriginPlayer.LocalOriginPlayer?.guid == Guid.Empty) {
				tooltips.Add(new TooltipLine(Mod, "Tooltip0", Language.GetTextValue("Mods.Origins.Items.Terrarian_Voodoo_Doll.WhyDontYouExist")));
			} else {
				tooltips.Add(new TooltipLine(Mod, "Tooltip0", Language.GetTextValue("Mods.Origins.Items.Terrarian_Voodoo_Doll.TooltipYouDoNotRecognizeTheBodiesInTheWater")));
			}
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (!Main.dedServ && owner == Guid.Empty) {
				owner = player.OriginPlayer().guid;
			}
			player.OriginPlayer().pickupRangeBoost += 75;
		}
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			if (RefreshPlayer() is OriginPlayer originPlayer) {
				originPlayer.voodooDoll = Item;
				if (player.whoAmI == Main.myPlayer && !player.shimmerImmune && !player.shimmerUnstuckHelper.ShouldUnstuck) {
					int headX = (int)(Item.Center.X / 16f);
					int headY = (int)((Item.position.Y + 1f) / 16f);
					if (Item.position.Y / 16f < Main.UnderworldLayer && Main.tile[headX, headY] != null && Main.tile[headX, headY].LiquidType == LiquidID.Shimmer && Main.tile[headX, headY].LiquidAmount >= 0) {
						player.AddBuff(BuffID.Shimmer, 60);
					}
				}
				TryPickupItems();
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
		public static bool PreventItemPickup(Item item, Player player) {
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
			writer.Write(owner.ToByteArray(false));
		}
		public override void NetReceive(BinaryReader reader) {
			owner = new Guid(reader.ReadBytes(16), false);
		}
		public override void SaveData(TagCompound tag) {
			tag.Add("owner", owner.ToByteArray(false));
		}
		public override void LoadData(TagCompound tag) {
			if (tag.TryGet("owner", out byte[] guidBytes)) {
				owner = new Guid(guidBytes, false);
			} else {
				owner = Guid.Empty;
			}
		}
		public class Voodoo_Doll_Persistence_System : ModSystem {
			public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
				if (OriginPlayer.LocalOriginPlayer?.voodooDoll is null) return;
				Point center = OriginPlayer.LocalOriginPlayer.voodooDoll.Center.ToTileCoordinates();
				Rectangle tileRectangle = new(center.X - Main.buffScanAreaWidth / 2, center.Y - Main.buffScanAreaHeight / 2, Main.buffScanAreaWidth, Main.buffScanAreaHeight);
				tileRectangle = WorldUtils.ClampToWorld(tileRectangle);
				for (int i = tileRectangle.Left; i < tileRectangle.Right; i++) {
					for (int j = tileRectangle.Top; j < tileRectangle.Bottom; j++) {
						if (!tileRectangle.Contains(i, j)) continue;
						Tile tile = Main.tile[i, j];
						if (tile == null) continue;
						if (!tile.HasTile) continue;
						tileRectangle.Contains(i, j);
						if (tile.TileType == TileID.Campfire && tile.TileFrameY < 36) {
							Main.SceneMetrics.HasCampfire = true;
						}
						if (tile.TileType == TileID.WaterCandle && tile.TileFrameX < 18) {
							Main.SceneMetrics.WaterCandleCount++;
						}
						if (tile.TileType == TileID.PeaceCandle && tile.TileFrameX < 18) {
							Main.SceneMetrics.PeaceCandleCount++;
						}
						if (tile.TileType == TileID.ShadowCandle && tile.TileFrameX < 18) {
							Main.SceneMetrics.ShadowCandleCount++;
						}
						if (tile.TileType == TileID.Chimney && tile.TileFrameX < 54) {
							Main.SceneMetrics.HasCampfire = true;
						}
						if (tile.TileType == TileID.CatBast && tile.TileFrameX < 72) {
							Main.SceneMetrics.HasCatBast = true;
						}
						if (tile.TileType == TileID.HangingLanterns && tile.TileFrameY >= 324 && tile.TileFrameY <= 358) {
							Main.SceneMetrics.HasHeartLantern = true;
						}
						if (tile.TileType == TileID.HangingLanterns && tile.TileFrameY >= 252 && tile.TileFrameY <= 286) {
							Main.SceneMetrics.HasStarInBottle = true;
						}
						if (tile.TileType == TileID.Banners && (tile.TileFrameX >= 396 || tile.TileFrameY >= 54)) {
							int bannerType = tile.TileFrameX / 18 - 21;
							for (int k = tile.TileFrameY; k >= 54; k -= 54) {
								bannerType += 90 + 21;
							}
							int bannerItemType = Item.BannerToItem(bannerType);
							if (ItemID.Sets.BannerStrength.IndexInRange(bannerItemType) && ItemID.Sets.BannerStrength[bannerItemType].Enabled) {
								Main.SceneMetrics.NPCBannerBuff[bannerType] = true;
								Main.SceneMetrics.hasBanner = true;
							}
						}
						TileLoader.NearbyEffects(i, j, tile.TileType, closer: false);
					}
				}
			}
			public override void SaveWorldData(TagCompound tag) {
				List<TagCompound> dolls = [];
				foreach (Item item in Main.ActiveItems) {
					if (item.ModItem is Terrarian_Voodoo_Doll doll) {
						dolls.Add(new() {
							["Owner"] = doll.owner.ToByteArray(false),
							["Position"] = item.position
						});
					}
				}
				tag.Add("Dolls", dolls);
			}
			public override void LoadWorldData(TagCompound tag) {
				int type = ModContent.ItemType<Terrarian_Voodoo_Doll>();
				if (tag.TryGet("Dolls", out List<TagCompound> dolls)) {
					for (int i = 0; i < dolls.Count; i++) {
						if (dolls[i].TryGet("Owner", out byte[] guidBytes) && dolls[i].TryGet("Position", out Vector2 position)) {
							Item item = Main.item[Item.NewItem(
								new EntitySource_Misc("persistence"),
								position,
								type
							)];
							if (item.ModItem is Terrarian_Voodoo_Doll doll) doll.owner = new Guid(guidBytes, false);
							item.velocity = Vector2.Zero;
						}
					}
				}
			}
		}
	}
}
