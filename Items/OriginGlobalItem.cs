using Microsoft.Xna.Framework;
using Origins.Items.Accessories;
using Origins.Items.Weapons;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Ranged;
using Origins.Journal;
using Origins.LootConditions;
using Origins.Questing;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items {
	public class OriginGlobalItem : GlobalItem {
		public override void SetDefaults(Item item) {
			switch (item.type) {
				case ItemID.Grenade:
				item.damage = (int)(item.damage * 0.8);
				item.ammo = ItemID.Grenade;
				item.DamageType = DamageClasses.ThrownExplosive;
				break;
				case ItemID.BouncyGrenade:
				case ItemID.StickyGrenade:
				case ItemID.PartyGirlGrenade:
				case ItemID.Beenade:
				item.ammo = ItemID.Grenade;
				item.DamageType = DamageClasses.ThrownExplosive;
				break;

				case ItemID.Fireblossom:
				item.ammo = ItemID.Fireblossom;
				item.consumable = true;
				break;
				case ItemID.Bomb:
				case ItemID.BouncyBomb:
				case ItemID.StickyBomb:
				case ItemID.Dynamite:
				case ItemID.BouncyDynamite:
				case ItemID.StickyDynamite:
				case ItemID.BombFish:
				case ItemID.MolotovCocktail:
				case ItemID.DryBomb:
				case ItemID.WetBomb:
				case ItemID.LavaBomb:
				case ItemID.HoneyBomb:
				case ItemID.ScarabBomb:
				item.ammo = ItemID.Bomb;
				item.DamageType = DamageClasses.ThrownExplosive;
				break;
				case ItemID.RocketLauncher:
				case ItemID.ProximityMineLauncher:
				case ItemID.GrenadeLauncher:
				case ItemID.HellfireArrow:
				item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
				break;
			}
			if (Origins.ExplosiveBaseDamage.TryGetValue(item.shoot, out int damage)) {
				item.damage = damage;
			}
			if (OriginConfig.Instance.WoodBuffs) switch (item.type) {
					case ItemID.ShadewoodHelmet:
					case ItemID.EbonwoodHelmet:
					item.defense = 4;
					break;
					case ItemID.ShadewoodBreastplate:
					case ItemID.EbonwoodBreastplate:
					item.defense = 6;
					break;
					case ItemID.ShadewoodGreaves:
					case ItemID.EbonwoodGreaves:
					item.defense = 5;
					break;
					case ItemID.PearlwoodHelmet:
					case ItemID.PearlwoodGreaves:
					item.defense = 6;
					break;
					case ItemID.PearlwoodBreastplate:
					item.defense = 7;
					break;
				}
			if (item.width == 0 && item.height == 0) {
				item.width = 4;
				item.height = 4;
			}
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
					originPlayer.cryostenLifeRegenCount += 20;
				}
			}
			return true;
		}
		public override string IsArmorSet(Item head, Item body, Item leg) {
			if (head.type == ItemID.MiningHelmet && body.type == ItemID.MiningShirt && leg.type == ItemID.MiningPants) return "miner";
			if (OriginConfig.Instance.WoodBuffs && head.type == ItemID.PearlwoodHelmet && body.type == ItemID.PearlwoodBreastplate && leg.type == ItemID.PearlwoodGreaves) return "pearlwood";
			return "";
		}
		public override void UpdateArmorSet(Player player, string set) {
			switch (set) {
				case "miner":
				player.setBonus += "\n20% reduced self-damage";
				player.GetModPlayer<OriginPlayer>().minerSet = true;
				return;
			}
			if (OriginConfig.Instance.WoodBuffs && set == "pearlwood") {
				player.setBonus += "\n15% increased damage\nReduces damage taken by 5%";
				player.GetDamage(DamageClass.Generic) += 0.15f;
				player.endurance += 0.05f;
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
				}
			} catch (Exception e) {
				Mod.Logger.Error(e);
			}
			if (item.ModItem is IJournalEntryItem journalItem) {
				if (Main.LocalPlayer.GetModPlayer<OriginPlayer>().DisplayJournalTooltip(journalItem)) {
					tooltips.Add(new TooltipLine(Mod, "JournalIndicator", Language.GetTextValue(journalItem.IndicatorKey)));
				}
			}
		}
		public override void UpdateInventory(Item item, Player player) {
			if (player.whoAmI == Main.myPlayer) {
				foreach (var quest in Quest_Registry.Quests) {
					if (!quest.SaveToWorld && quest.UpdateInventoryEvent is not null) {
						quest.UpdateInventoryEvent(item);
					}
				}
			}
		}
		/*
        [Obsolete("replace with ModifyItemLoot when that exists")]
		public override void OnSpawn(Item item, IEntitySource source) {
            if (source is EntitySource_ItemOpen) {
				switch (item.type) {
                    case ItemID.WarriorEmblem:
                    case ItemID.RangerEmblem:
                    case ItemID.SorcererEmblem:
                    case ItemID.SummonerEmblem:
					if (Main.rand.NextBool(OriginGlobalNPC.woFEmblemsCount)) {
                        int prefix = item.prefix;
                        item.SetDefaults(ModContent.ItemType<Exploder_Emblem>());
                        item.Prefix(prefix);
                    }
                    break;

                    case ItemID.DemoniteOre:
                    case ItemID.CrimtaneOre: {
                        int stack = item.stack;
                        switch (OriginSystem.WorldEvil) {
                            case OriginSystem.evil_wastelands:
                            item.SetDefaults(ModContent.ItemType<Defiled_Ore_Item>());
                            break;

                            case OriginSystem.evil_riven:
                            item.SetDefaults(ModContent.ItemType<Encrusted_Ore_Item>());
                            break;
                        }
                        item.stack = stack;
                    }
                    break;

                    case ItemID.CorruptSeeds:
                    case ItemID.CrimsonSeeds: {
                        int stack = item.stack;
                        switch (OriginSystem.WorldEvil) {
                            case OriginSystem.evil_wastelands:
                            item.SetDefaults(ModContent.ItemType<Defiled_Grass_Seeds>());
                            break;

                            case OriginSystem.evil_riven:
                            //item.type = ModContent.ItemType<Defiled_Grass_Seeds>();
                            break;
                        }
                        item.stack = stack;
                    }
                break;
                }
            }
        }//*/
		public override void ModifyItemLoot(Item item, ItemLoot itemLoot) {
			List<IItemDropRule> dropRules = itemLoot.Get(false);
			switch (item.type) {
				case ItemID.WallOfFleshBossBag: {
					IEnumerable<IItemDropRule> rules = dropRules.Where((r) =>
					r is OneFromOptionsNotScaledWithLuckDropRule dropRule &&
					dropRule.dropIds.Contains(ItemID.WarriorEmblem));
					if (rules.Any()) {
						OneFromOptionsNotScaledWithLuckDropRule rule = rules.First() as OneFromOptionsNotScaledWithLuckDropRule;
						if (rule is not null) {
							Array.Resize(ref rule.dropIds, rule.dropIds.Length + 1);
							rule.dropIds[^1] = ModContent.ItemType<Exploder_Emblem>();
						} else {
							Origins.instance.Logger.Warn("Emblem drop rule not present on WoF bag");
						}
					} else {
						Origins.instance.Logger.Warn("Emblem drop rule not present on WoF bag");
					}
					rules = dropRules.Where((r) =>
					r is OneFromOptionsNotScaledWithLuckDropRule dropRule &&
					dropRule.dropIds.Contains(ItemID.BreakerBlade));
					if (rules.Any()) {
						OneFromOptionsNotScaledWithLuckDropRule rule = rules.First() as OneFromOptionsNotScaledWithLuckDropRule;
						if (rule is not null) {
							Array.Resize(ref rule.dropIds, rule.dropIds.Length + 1);
							rule.dropIds[^1] = ModContent.ItemType<Thermite_Launcher>();
						} else {
							Origins.instance.Logger.Warn("Weapon drop rule not present on WoF bag");
						}
					} else {
						Origins.instance.Logger.Warn("Weapon drop rule not present on WoF bag");
					}
					break;
				}
				case ItemID.LockBox: {
					IEnumerable<IItemDropRule> rules = dropRules.Where((r) =>
					r is OneFromOptionsNotScaledWithLuckDropRule dropRule &&
					dropRule.dropIds.Contains(ItemID.Muramasa));
					if (rules.Any()) {
						OneFromOptionsNotScaledWithLuckDropRule rule = rules.First() as OneFromOptionsNotScaledWithLuckDropRule;
						if (rule is not null) {
							Array.Resize(ref rule.dropIds, rule.dropIds.Length + 4);
							rule.dropIds[^4] = ModContent.ItemType<Tones_Of_Agony>();
							rule.dropIds[^3] = ModContent.ItemType<Asylum_Whistle>();
							rule.dropIds[^2] = ModContent.ItemType<Bomb_Launcher>();
							rule.dropIds[^1] = ModContent.ItemType<Bomb_Yeeter>();
						} else {
							Origins.instance.Logger.Warn("Main drop rule not present on Lock Box");
						}
					} else {
						Origins.instance.Logger.Warn("Main drop rule not present on Lock Box");
					}
					break;
				}
				case ItemID.ObsidianLockbox: {
					IEnumerable<IItemDropRule> rules = dropRules.Where((r) =>
					r is OneFromOptionsNotScaledWithLuckDropRule dropRule &&
					dropRule.dropIds.Contains(ItemID.DarkLance));
					if (rules.Any()) {
						OneFromOptionsNotScaledWithLuckDropRule rule = rules.First() as OneFromOptionsNotScaledWithLuckDropRule;
						if (rule is not null) {
							Array.Resize(ref rule.dropIds, rule.dropIds.Length + 4);
							rule.dropIds[^4] = ModContent.ItemType<Boiler>();
							rule.dropIds[^3] = ModContent.ItemType<Firespit>();
							rule.dropIds[^2] = ModContent.ItemType<Dragons_Breath>();
							rule.dropIds[^1] = ModContent.ItemType<Hand_Grenade_Launcher>();
						} else {
							Origins.instance.Logger.Warn("Main drop rule not present on Obsidian Lock Box");
						}
					} else {
						Origins.instance.Logger.Warn("Main drop rule not present on Obsidian Lock Box");
					}
					break;
				}
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
