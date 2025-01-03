﻿using Microsoft.Xna.Framework;
using Origins.Items.Accessories;
using Origins.Items.Weapons;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.Journal;
using Origins.LootConditions;
using Origins.NPCs;
using Origins.Questing;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ID.ContentSamples.CreativeHelper;

namespace Origins.Items {
	public class OriginGlobalItem : GlobalItem {
		internal static bool isOriginsItemCloningDefaults = false;
		public override void SetDefaults(Item item) {
			bool statsModified = false;
			switch (item.type) {
				case ItemID.Grenade:
				item.damage = (int)(item.damage * 0.8);
				item.ammo = ItemID.Grenade;
				item.DamageType = DamageClasses.ThrownExplosive;
				item.notAmmo = true;
				statsModified = true;
				break;

				case ItemID.BouncyGrenade:
				case ItemID.StickyGrenade:
				case ItemID.PartyGirlGrenade:
				case ItemID.Beenade:
				item.ammo = ItemID.Grenade;
				item.DamageType = DamageClasses.ThrownExplosive;
				item.notAmmo = true;
				statsModified = true;
				break;

				case ItemID.Sunflower:
				item.ammo = ItemID.Sunflower;
				item.consumable = true;
				item.notAmmo = true;
				break;

				case ItemID.Fireblossom:
				item.ammo = ItemID.Fireblossom;
				item.consumable = true;
				item.notAmmo = true;
				break;

				case ItemID.Bomb:
				case ItemID.BouncyBomb:
				case ItemID.StickyBomb:
				case ItemID.BombFish:
				case ItemID.DryBomb:
				case ItemID.WetBomb:
				case ItemID.LavaBomb:
				case ItemID.HoneyBomb:
				case ItemID.ScarabBomb:
				case ItemID.DirtBomb:
				item.ammo = ItemID.Bomb;
				item.DamageType = DamageClasses.ThrownExplosive;
				item.notAmmo = true;
				statsModified = true;
				break;

				case ItemID.Dynamite:
				case ItemID.BouncyDynamite:
				case ItemID.StickyDynamite:
				case ItemID.MolotovCocktail:
				item.DamageType = DamageClasses.ThrownExplosive;
				item.notAmmo = true;
				statsModified = true;
				break;

				case ItemID.RocketLauncher:
				case ItemID.SnowmanCannon:
				case ItemID.ProximityMineLauncher:
				case ItemID.GrenadeLauncher:
				case ItemID.HellfireArrow:
				case ItemID.Stynger:
				case ItemID.StyngerBolt:
				item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
				statsModified = true;
				break;

				case ItemID.DD2ExplosiveTrapT1Popper:
				case ItemID.DD2ExplosiveTrapT2Popper:
				case ItemID.DD2ExplosiveTrapT3Popper:
				item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
				statsModified = true;
				break;

				case ItemID.MiningHelmet:
				case ItemID.MiningShirt:
				statsModified = true;
				break;
			}
			if (item.PaintOrCoating) {
				item.consumable = true;
			}
			if (item.damage is 0 or -1 && Origins.ExplosiveBaseDamage.TryGetValue(item.shoot, out int damage)) {
				item.damage = damage;
				statsModified = true;
			}
			if (OriginConfig.Instance.WoodBuffs) switch (item.type) {
					case ItemID.ShadewoodHelmet:
					case ItemID.EbonwoodHelmet:
					item.defense = 2;
					statsModified = true;
					break;

					case ItemID.ShadewoodBreastplate:
					case ItemID.EbonwoodBreastplate:
					item.defense = 3;
					statsModified = true;
					break;

					case ItemID.ShadewoodGreaves:
					case ItemID.EbonwoodGreaves:
					item.defense = 3;
					statsModified = true;
					break;

					case ItemID.PearlwoodHelmet:
					case ItemID.PearlwoodGreaves:
					item.defense = 6;
					statsModified = true;
					break;

					case ItemID.PearlwoodBreastplate:
					item.defense = 7;
					statsModified = true;
					break;
				}
			if (item.width == 0 && item.height == 0) {
				item.width = 4;
				item.height = 4;
			}
			if (statsModified && !isOriginsItemCloningDefaults) {
				item.StatsModifiedBy.Add(Mod);
			}
			if (Origins.itemGlowmasks[item.type] != 0) item.glowMask = Origins.itemGlowmasks[item.type];
		}
		public override bool? CanAutoReuseItem(Item item, Player player) {
			if (player.nonTorch != -1 && player.inventory[player.nonTorch].type == Boomphracken.ID && player.HeldItem.CountsAsClass<Thrown_Explosive>()) {
				player.selectItemOnNextUse = true;
				return false;
			}
			return null;
		}
		public override void UpdateEquip(Item item, Player player) {
			switch (item.type) {
				case ItemID.MiningHelmet:
				player.GetCritChance(DamageClasses.Explosive) += 3;
				break;
				case ItemID.MiningShirt:
				player.GetDamage(DamageClasses.Explosive) += 0.05f;
				break;
			}
		}
		public override bool OnPickup(Item item, Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.cryostenSet) {
				if (item.type == ItemID.Heart || item.type == ItemID.CandyApple || item.type == ItemID.SugarPlum) {
					originPlayer.cryostenLifeRegenCount += 60;
				}
			}
			if (originPlayer.hasProtOS) {
				if (item.CountsAsClass(DamageClasses.Explosive)) {
					Protomind.PlayRandomMessage(Protomind.QuoteType.Item_Is_Explosive, originPlayer.protOSQuoteCooldown, player.Top);
				} else if (item.OriginalRarity == ItemRarityID.Gray) {
					Protomind.PlayRandomMessage(Protomind.QuoteType.Item_Is_Bad, originPlayer.protOSQuoteCooldown, player.Top);
				} else if (item.type == ModContent.ItemType<Potato_Launcher>()) {
					Protomind.PlayRandomMessage(Protomind.QuoteType.Potato_Launcher, originPlayer.protOSQuoteCooldown, player.Top);
				}
			}
			return true;
		}
		public override void GrabRange(Item item, Player player, ref int grabRange) {
			if (player.TryGetModPlayer(out OriginPlayer originPlayer)) {
				grabRange += originPlayer.pickupRangeBoost;
				//if (originPlayer.isVoodooPickup) grabRange -= Player.defaultItemGrabRange - 75; 
			}
		}
		public override string IsArmorSet(Item head, Item body, Item leg) {
			if (head.type == ItemID.MiningHelmet && body.type == ItemID.MiningShirt && leg.type == ItemID.MiningPants) return "miner";
			if (OriginConfig.Instance.WoodBuffs && head.type == ItemID.PearlwoodHelmet && body.type == ItemID.PearlwoodBreastplate && leg.type == ItemID.PearlwoodGreaves) return "pearlwood";
			return "";
		}
		public override void UpdateArmorSet(Player player, string set) {
			switch (set) {
				case "miner":
				player.setBonus += Language.GetTextValue("Mods.Origins.SetBonuses.Miner");
				player.GetModPlayer<OriginPlayer>().minerSet = true;
				return;
			}
			if (OriginConfig.Instance.WoodBuffs && set == "pearlwood") {
				player.setBonus += Language.GetTextValue("Mods.Origins.SetBonuses.Pearlwood");
				player.GetDamage(DamageClass.Generic) += 0.15f;
				player.endurance += (1 - player.endurance) * 0.05f;
				return;
			}
		}
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			try {
				switch (item.type) {
					case ItemID.MiningHelmet:
					tooltips.Insert(3, new TooltipLine(Mod, "Tooltip0", "3% increased explosive critical strike chance"));
					break;
					case ItemID.MiningShirt:
					tooltips.Insert(3, new TooltipLine(Mod, "Tooltip0", "5% increased explosive damage"));
					break;
					case ItemID.Harpoon:
					tooltips.Insert(5, new TooltipLine(Mod, "Tooltip0", Language.GetTextValue("Mods.Origins.Items.Harpoon_Gun.VanillaTooltip")));
					break;
				}
			} catch (Exception e) {
				Mod.Logger.Error(e);
			}
			if (PrefixLoader.GetPrefix(item.prefix) is IModifyTooltipsPrefix modifyTooltipsPrefix) {
				modifyTooltipsPrefix.ModifyTooltips(item, tooltips);
			}
			if (item.ModItem is IJournalEntryItem journalItem) {
				if (Main.LocalPlayer.GetModPlayer<OriginPlayer>().DisplayJournalTooltip(journalItem)) {
					tooltips.Add(new TooltipLine(Mod, "JournalIndicator", Language.GetTextValue(journalItem.IndicatorKey)));
				}
			}
		}
		public override bool CanReforge(Item item)/* tModPorter Note: Use CanReforge instead for logic determining if a reforge can happen. */ {
			if (PrefixLoader.GetPrefix(item.prefix) is ICanReforgePrefix canReforgePrefix) {
				return canReforgePrefix.CanReforge(item);
			}
			return true;
		}
		public override void UpdateInventory(Item item, Player player) {
			if (player.whoAmI == Main.myPlayer) {
				foreach (var quest in Quest_Registry.Quests) {
					if (quest.UpdateInventoryEvent is not null) {
						quest.UpdateInventoryEvent(item);
					}
				}
			}
		}
		static OneFromRulesRule originsDevSetRule;
		static VaryingRateLeadingRule devSetRealDropRule = new VaryingRateLeadingRule(16, 1, (new Conditions.TenthAnniversaryIsUp(), 8, 1)).WithOnSuccess(originsDevSetRule = new(1));
		public static OneFromRulesRule OriginsDevSetRule => originsDevSetRule;
		public override void ModifyItemLoot(Item item, ItemLoot itemLoot) {
			static LocalizedText GetWarningText(string key) => Language.GetText("Mods.Origins.Warnings." + key);
			List<IItemDropRule> dropRules = itemLoot.Get(false);
			switch (item.type) {
				case ItemID.WallOfFleshBossBag: {
					if (!OriginGlobalNPC.AddToOneFromOptionsRule(dropRules, ItemID.WarriorEmblem, ModContent.ItemType<Exploder_Emblem>())) {
						Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Emblem"), Lang.GetItemName(item.type)));
					}
					if (!OriginGlobalNPC.AddToOneFromOptionsRule(dropRules, ItemID.BreakerBlade, ModContent.ItemType<Thermite_Launcher>())) {
						Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Weapon"), Lang.GetItemName(item.type)));
					}
					break;
				}
				case ItemID.EaterOfWorldsBossBag: {
					itemLoot.Add(OriginGlobalNPC.EaterOfWorldsWeaponDrops);
					break;
				}
				case ItemID.BrainOfCthulhuBossBag: {
					itemLoot.Add(OriginGlobalNPC.BrainOfCthulhuWeaponDrops);
					break;
				}
				case ItemID.QueenBeeBossBag: {
					if (!OriginGlobalNPC.AddToOneFromOptionsRule(dropRules, ItemID.BeeGun, ModContent.ItemType<Bee_Afraid_Incantation>())) {
						Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Weapon"), Lang.GetItemName(item.type)));
					}
					break;
				}
				case ItemID.LockBox: {
					bool foundMain = false;
					OneFromRulesRule rule = dropRules.FindDropRule<OneFromRulesRule>(dropRule => {
						List<DropRateInfo> drops = [];
						dropRule.ReportDroprates(drops, default);
						return drops.Any(i => i.itemId == ItemID.Muramasa);
					});
					if (rule is not null) {
						Array.Resize(ref rule.options, rule.options.Length + 4);
						rule.options[^4] = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Tones_Of_Agony>());
						rule.options[^3] = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Asylum_Whistle>());
						rule.options[^2] = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Bomb_Launcher>());
						rule.options[^1] = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Bomb_Handling_Device>());
						foundMain = true;
					}
					if (!foundMain) Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Main"), Lang.GetItemName(item.type)));
					break;
				}
				case ItemID.ObsidianLockbox: {
					bool foundMain = false;
					OneFromRulesRule rule = dropRules.FindDropRule<OneFromRulesRule>(dropRule => {
						List<DropRateInfo> drops = [];
						dropRule.ReportDroprates(drops, default);
						return drops.Any(i => i.itemId == ItemID.DarkLance);
					});
					if (rule is not null) {
						Array.Resize(ref rule.options, rule.options.Length + 4);
						rule.options[^4] = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Boiler>());
						rule.options[^3] = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Firespit>());
						rule.options[^2] = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Dragons_Breath>());
						rule.options[^1] = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Hand_Grenade_Launcher>());
						foundMain = true;
					}
					if (!foundMain) Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Main"), Lang.GetItemName(item.type)));
					break;
				}
			}
			if (ItemID.Sets.BossBag[item.type] && item.ModItem?.Mod == Origins.instance) {
				itemLoot.Add(devSetRealDropRule);
			}
		}
		public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset) {
			if (item.rare == CursedRarity.ID && line.Name == "ItemName") {
				Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(
					Main.spriteBatch,
					line.Font,
					line.Text,
					new Vector2(line.X, line.Y),
					CursedRarity.Color,
					new Color(0.15f, 0f, 0f) * (Main.mouseTextColor / 255f),
					line.Rotation,
					line.Origin,
					line.BaseScale,
					line.MaxWidth,
					line.Spread
				);
				return false;
			}
			return true;
		}
		public override void OnSpawn(Item item, IEntitySource source) {
			if (ItemID.Sets.Torches[item.type] && source is EntitySource_TileBreak tileSource && Main.tileCut[Framing.GetTileSafely(tileSource.TileCoords).TileType]) {
				int stack = item.stack;
				if (Main.LocalPlayer.InModBiome<Defiled_Wastelands>()) {
					item.SetDefaults(ModContent.ItemType<Defiled_Torch>());
				} else if (Main.LocalPlayer.InModBiome<Riven_Hive>()) {
					item.SetDefaults(ModContent.ItemType<Riven_Torch>());
				}
				item.stack = stack;
			}
		}
		public override bool CanPickup(Item item, Player player) {
			if (item.type is >= ItemID.LargeAmethyst and <= ItemID.LargeDiamond && Laser_Tag_Console.LaserTagGameActive) {
				return player.OriginPlayer().laserTagVestActive;
			}
			return true;
		}

		public static ushort GetItemElement(Item item) {
			if (item.ModItem is null) {
				return Origins.VanillaElements[item.type];
			} else if (item.ModItem is IElementalItem elementalItem) {
				return elementalItem.Element;
			}
			return 0;
		}
	}
}
