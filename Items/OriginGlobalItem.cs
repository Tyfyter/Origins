using Origins.Items.Accessories;
using Origins.Items.Weapons;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.LootConditions;
using Origins.NPCs;
using Origins.Questing;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

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
			if (item.damage is 0 or -1 && (Origins.ExplosiveBaseDamage?.TryGetValue(item.shoot, out int damage) ?? false)) {
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
			if (Origins.itemGlowmasks?[item.type] is not 0 and not -1 and not null) item.glowMask = Origins.itemGlowmasks[item.type];
		}
		public override void ModifyItemScale(Item item, Player player, ref float scale) {
			OriginPlayer originPlayer = player.OriginPlayer();
			if (item.CountsAsClass(DamageClass.Melee) || item.CountsAsClass(DamageClass.SummonMeleeSpeed)) {
				scale *= originPlayer.meleeScaleMultiplier;
				if (originPlayer.resizingGlove) scale *= originPlayer.resizingGloveScale;
			}
		}
		public override bool CanUseItem(Item item, Player player) {
			if ((item.mountType != -1 || Main.projHook.GetIfInRange(item.shoot)) && player.OriginPlayer().weakShimmer) return false;
			return true;
		}
		public override bool? CanAutoReuseItem(Item item, Player player) {
			if (player.nonTorch != -1 && player.inventory[player.nonTorch].type == Boomphracken.ID && player.HeldItem.CountsAsClass<Thrown_Explosive>()) {
				player.selectItemOnNextUse = true;
				return false;
			}
			return null;
		}
		public override bool NeedsAmmo(Item item, Player player) {
			if (player.OriginPlayer().wishingGlassActive) return false;
			return true;
		}
		public override bool ConsumeItem(Item item, Player player) {
			if (player.OriginPlayer().wishingGlassActive && item.CountsAsClass(DamageClass.Throwing)) {
				if (item.CountsAsClass(DamageClasses.Explosive)) return Main.rand.NextFloat() >= item.useAnimation * 0.0175f;
				return false;
			}
			return base.ConsumeItem(item, player);
		}
		public override void UpdateEquip(Item item, Player player) {
			switch (item.type) {
				case ItemID.MiningHelmet:
				player.GetCritChance(DamageClasses.Explosive) += 3;
				break;
				case ItemID.MiningShirt:
				player.GetDamage(DamageClasses.Explosive) += 0.05f;
				break;
				case ItemID.RainHat:
				player.GetDamage(DamageClass.Generic) += Main.raining ? 0.07f : 0.05f;
				break;
				case ItemID.RainCoat:
				player.GetDamage(DamageClass.Generic) += Main.raining ? 0.07f : 0.05f;
				break;
			}
		}
		public override bool OnPickup(Item item, Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.oldCryostenSet) {
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
			if (head.type == ItemID.RainHat && body.type == ItemID.RainCoat) return "rain";
			return "";
		}
		public override void UpdateArmorSet(Player player, string set) {
			switch (set) {
				case "miner":
				player.setBonus += Language.GetTextValue("Mods.Origins.SetBonuses.Miner");
				player.GetModPlayer<OriginPlayer>().minerSet = true;
				return;
				case "pearlwood":
				if (OriginConfig.Instance.WoodBuffs) {
					player.setBonus += Language.GetTextValue("Mods.Origins.SetBonuses.Pearlwood");
					player.GetDamage(DamageClass.Generic) += 0.15f;
					player.endurance += (1 - player.endurance) * 0.05f;
					return;
				}
				break;
				case "rain":
				if (OriginConfig.Instance.RainSetBuff) {
					player.setBonus += Language.GetTextValue("Mods.Origins.SetBonuses.RainBuff") + "\n";
					if (Main.raining) player.moveSpeed += 0.35f;
				}
				player.setBonus += Language.GetTextValue("Mods.Origins.SetBonuses.Rain");
				player.OriginPlayer().rainSet = true;
				return;
			}
		}
		public static void AddVanillaTooltips(int itemType, List<TooltipLine> tooltips, bool forceAll = false) {
			void Insert(string target, string name, string textKey, bool after = true) {
				int index = tooltips.FindIndex(line => line.Name == target);
				TooltipLine line = new(Origins.instance, "name", Language.GetOrRegister(textKey).Value);
				if (index == -1) {
					tooltips.Add(line);
					return;
				}
				tooltips.Insert(index + after.ToInt(), line);
			}
			void Add(string name, string textKey) {
				int index = tooltips.FindLastIndex(line => line.Name.StartsWith("Tooltip"));
				TooltipLine line = new(Origins.instance, "name", Language.GetOrRegister(textKey).Value);
				if (index == -1) {
					tooltips.Add(line);
					return;
				}
				tooltips.Insert(index + 1, line);
			}
			switch (itemType) {
				case ItemID.MiningHelmet:
				Add("Tooltip1", "Mods.Origins.Items.MiningHelmet.BuffTooltip");
				break;
				case ItemID.MiningShirt:
				Add("Tooltip1", "Mods.Origins.Items.MiningShirt.BuffTooltip");
				break;
				case ItemID.RainHat:
				if (forceAll || OriginConfig.Instance.RainSetBuff) {
					Add("Tooltip0", "Mods.Origins.Items.RainHat.BuffTooltip");
				}
				break;
				case ItemID.RainCoat:
				if (forceAll || OriginConfig.Instance.RainSetBuff) {
					Add("Tooltip0", "Mods.Origins.Items.RainCoat.BuffTooltip");
				}
				break;
				case ItemID.Harpoon:
				Add("Tooltip0", "Mods.Origins.Items.Harpoon_Gun.VanillaTooltip");
				break;
			}
		}
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			AddVanillaTooltips(item.type, tooltips);
			if (item.shoot > ProjectileID.None && Origins.ArtifactMinion[item.shoot]) tooltips.Insert(1, new(Mod, "ArtifactTag", Language.GetOrRegister("Mods.Origins.Items.GenericTooltip.ArtifactTag").Value));
			if (PrefixLoader.GetPrefix(item.prefix) is IModifyTooltipsPrefix modifyTooltipsPrefix) {
				modifyTooltipsPrefix.ModifyTooltips(item, tooltips);
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
				foreach (Quest quest in Quest_Registry.Quests) {
					if (quest.UpdateInventoryEvent is not null) {
						quest.UpdateInventoryEvent(item);
					}
				}
				if (!string.IsNullOrWhiteSpace(OriginsSets.Items.JournalEntries[item.type])) player.OriginPlayer().UnlockJournalEntry(OriginsSets.Items.JournalEntries[item.type]);
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
				/*case ItemID.BrainOfCthulhuBossBag: {
					itemLoot.Add(OriginGlobalNPC.BrainOfCthulhuWeaponDrops);
					break;
				}*/
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
						rule.Add(
							ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Tones_Of_Agony>()),
							ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Asylum_Whistle>()),
							ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Bomb_Launcher>()),
							ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Bomb_Handling_Device>())
						);
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
						rule.Add(
							ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Boiler>()),
							ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Firespit>()),
							ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Dragons_Breath>()),
							ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Hand_Grenade_Launcher>())
						);
						foundMain = true;
					}
					if (!foundMain) Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Main"), Lang.GetItemName(item.type)));
					break;
				}
				case ItemID.FrozenCrate: {// shares drop rules with hardmode version by reference, adding to either adds to both
					bool foundMain = false;
					OneFromRulesRule rule = dropRules.FindDropRule<OneFromRulesRule>(dropRule => {
						List<DropRateInfo> drops = [];
						dropRule.ReportDroprates(drops, default);
						return drops.Any(i => i.itemId == ItemID.IceBoomerang);
					});
					if (rule is not null) {
						rule.Add(
							ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Cryostrike>())
						);
						foundMain = true;
					}
					if (!foundMain) Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Main"), Lang.GetItemName(item.type)));
					break;
				}
				case ItemID.OasisCrate: {// shares drop rules with hardmode version by reference, adding to either adds to both
					if (!OriginGlobalNPC.AddToOneFromOptionsRule(dropRules, ItemID.AncientChisel, ModContent.ItemType<Desert_Crown>())) {
						Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Main"), Lang.GetItemName(item.type)));
					}
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
