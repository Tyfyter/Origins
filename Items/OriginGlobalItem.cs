using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items {
    public class OriginGlobalItem : GlobalItem {
        public override void SetDefaults(Item item) {
            switch(item.type) {
                case ItemID.Grenade:
                item.damage = (int)(item.damage*0.8);
                break;
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
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            switch(item.type) {
                case ItemID.MiningShirt:
                originPlayer.Explosive_Damage+=0.05f;
                break;
            }
        }
        public override bool OnPickup(Item item, Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            if(originPlayer.Cryosten_Set) {
                if(item.type == ItemID.Heart||item.type == ItemID.CandyApple||item.type == ItemID.SugarPlum) {
                    originPlayer.cryostenLifeRegenCount+=20;
                }
            }
            return true;
        }
        public override string IsArmorSet(Item head, Item body, Item leg) {
            if(head.type==ItemID.MiningHelmet&&body.type==ItemID.MiningShirt&&leg.type==ItemID.MiningPants) return "miner";
            if(head.type==ItemID.PearlwoodHelmet&&body.type==ItemID.PearlwoodBreastplate&&leg.type==ItemID.PearlwoodGreaves) return "pearlwood";
            return "";
        }
        public override void UpdateArmorSet(Player player, string set) {
            switch(set) {
                case "miner":
                player.setBonus+="\n20% reduced self-damage";
                player.GetModPlayer<OriginPlayer>().Miner_Set = true;
                break;
                case "pearlwood":
                player.setBonus+="\n15% increased damage\nReduces damage taken by 5%";
                player.allDamage+=0.15f;
                player.endurance+=0.05f;
                break;
            }
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            try {
                if(IsExplosive(item)) {
                    if(NeedsDamageLine(item)&&Origins.ExplosiveBaseDamage.ContainsKey(item.type)) {
                        Main.HoverItem.damage = Origins.ExplosiveBaseDamage[item.type];
                        tooltips.Insert(1, new TooltipLine(mod, "Damage", $"{Main.player[item.owner].GetWeaponDamage(Main.HoverItem)} {Language.GetText("explosive")} {Language.GetText("damage")}"));
                        return;
                    } else {
                        for(int i = 1; i < tooltips.Count; i++) {
                            TooltipLine tooltip = tooltips[i];
                            if(tooltip.Name.Equals("Damage")) {
                                tooltip.text = tooltip.text.Insert(tooltip.text.IndexOf(' '), " "+Language.GetText("explosive"));
                            }else if(tooltip.Name.Equals("CritChance")) {
                                tooltips.RemoveAt(i);
                                return;
                            }
                        }
                    }
                }else switch(item.type) {
                    case ItemID.MiningShirt:
                    tooltips.Insert(3, new TooltipLine(mod, "Tooltip0", "5% increased explosive damage"));
                    break;
                }
            } catch(Exception e) {
                mod.Logger.Error(e);
            }
        }
        public static bool IsExplosive(Item item) {
            return Origins.ExplosiveItems[item.type]||Origins.ExplosiveAmmo[item.ammo]||Origins.ExplosiveAmmo[item.useAmmo]||Origins.ExplosiveProjectiles[item.shoot];
        }
        public static bool NeedsDamageLine(Item item) {
            return !(item.melee||item.ranged||item.magic||item.summon||item.thrown);
        }
    }
}
