using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using OnTerraria = On.Terraria;
using Origins.Tiles.Defiled;
using Origins.World;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;

namespace Origins.Items {
    public class OriginGlobalItem : GlobalItem {
        public override void SetDefaults(Item item) {
            switch(item.type) {
                case ItemID.Grenade:
                item.damage = (int)(item.damage*0.8);
                item.ammo = ItemID.Grenade;
                item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
                break;
                case ItemID.BouncyGrenade:
                case ItemID.StickyGrenade:
                case ItemID.PartyGirlGrenade:
                case ItemID.Beenade:
                item.ammo = ItemID.Grenade;
                item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
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
                item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
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
            if(OriginConfig.Instance.WoodBuffs)switch(item.type) {
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
		}
        public override void UpdateEquip(Item item, Player player) {
            switch(item.type) {
                case ItemID.MiningHelmet:
                player.GetCritChance(DamageClasses.Explosive)+=3;
                break;
                case ItemID.MiningShirt:
                player.GetDamage(DamageClasses.Explosive) += 0.05f;
                break;
            }
        }
        public override bool OnPickup(Item item, Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            if(originPlayer.cryostenSet) {
                if(item.type == ItemID.Heart||item.type == ItemID.CandyApple||item.type == ItemID.SugarPlum) {
                    originPlayer.cryostenLifeRegenCount+=20;
                }
            }
            return true;
        }
        public override string IsArmorSet(Item head, Item body, Item leg) {
            if(head.type==ItemID.MiningHelmet&&body.type==ItemID.MiningShirt&&leg.type==ItemID.MiningPants) return "miner";
            if(OriginConfig.Instance.WoodBuffs&&head.type==ItemID.PearlwoodHelmet&&body.type==ItemID.PearlwoodBreastplate&&leg.type==ItemID.PearlwoodGreaves) return "pearlwood";
            return "";
        }
        public override void UpdateArmorSet(Player player, string set) {
            switch(set) {
                case "miner":
                player.setBonus+="\n20% reduced self-damage";
                player.GetModPlayer<OriginPlayer>().minerSet = true;
                return;
            }
            if(OriginConfig.Instance.WoodBuffs&&set=="pearlwood") {
                player.setBonus+="\n15% increased damage\nReduces damage taken by 5%";
                player.GetDamage(DamageClass.Generic)+=0.15f;
                player.endurance+=0.05f;
                return;
            }
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            try {
                if(item.CountsAsClass(DamageClasses.Explosive)) {
                    if(NeedsDamageLine(item)&&Origins.ExplosiveBaseDamage.ContainsKey(item.type)) {
                        Main.HoverItem.damage = Origins.ExplosiveBaseDamage[item.type];
                        Player player = Main.player[item.playerIndexTheItemIsReservedFor];
                        tooltips.Insert(1, new TooltipLine(Mod, "Damage", $"{player.GetWeaponDamage(Main.HoverItem)} {Language.GetText("explosive")}{Language.GetText("LegacyTooltip.55")}"));
                        int crit = player.GetWeaponCrit(item);
                        tooltips.Insert(2, new TooltipLine(Mod, "CritChance", $"{crit}{Language.GetText("LegacyTooltip.41")}"));
                        return;
                    }
                }else switch(item.type) {
                        case ItemID.MiningHelmet:
                    tooltips.Insert(3, new TooltipLine(Mod, "Tooltip0", "3% increased explosive critical strike chance"));
                    break;
                    case ItemID.MiningShirt:
                    tooltips.Insert(3, new TooltipLine(Mod, "Tooltip0", "5% increased explosive damage"));
                    break;
                }
            } catch(Exception e) {
                Mod.Logger.Error(e);
            }
        }
        [Obsolete]
        public static bool NeedsDamageLine(Item item) {
            return false;//!(item.melee||item.ranged||item.magic||item.summon||item.thrown);
        }
        public static ushort GetItemElement(Item item) {
            if(item.ModItem is null) {
                return Origins.VanillaElements[item.type];
            }else if(item.ModItem is IElementalItem elementalItem) {
                return elementalItem.Element;
            }
            return 0;
        }
    }
}
