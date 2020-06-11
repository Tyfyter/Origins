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
        public override void UpdateEquip(Item item, Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            switch(item.type) {
                case ItemID.MiningShirt:
                originPlayer.Explosive_Damage+=0.05f;
                break;
            }
        }
        public override string IsArmorSet(Item head, Item body, Item leg) {
            if(head.type==ItemID.MiningHelmet&&body.type==ItemID.MiningShirt&&leg.type==ItemID.MiningPants) return "miner";
            return "";
        }
        public override void UpdateArmorSet(Player player, string set) {
            switch(set) {
                case "miner":
                player.setBonus+="\n20% reduced self-damage";
                player.GetModPlayer<OriginPlayer>().Miner_Set = true;
                break;
            }
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            try {
                if(IsExplosive(item)) {
                    if(NeedsDamageLine(item)){
                        Main.HoverItem.damage = Origins.ExplosiveBaseDamage[item.type];
                        tooltips.Insert(1, new TooltipLine(mod, "Damage", $"{Main.player[item.owner].GetWeaponDamage(Main.HoverItem)} {Language.GetText("explosive")} {Lang.tip[55]}"));
                        return;
                    }
                    for(int i = 1; i < tooltips.Count; i++) {
                        TooltipLine tooltip = tooltips[i];
                        if(tooltip.Name.Equals("Damage")) {
                            tooltip.text = tooltip.text.Insert(tooltip.text.IndexOf(' '), " "+Language.GetText("explosive"));
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
