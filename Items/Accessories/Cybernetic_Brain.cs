using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Cybernetic_Brain : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Cybernetic Brain");
            Tooltip.SetDefault("Increases mana regeneration speed\nGrants immunity to most debuffs");
            glowmask = Origins.AddGlowMask(this);
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.glowMask = glowmask;
        }
        public override void UpdateEquip(Player player) {
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Darkness] = true;
            player.buffImmune[BuffID.Blackout] = true;
            player.buffImmune[BuffID.Silenced] = true;
            player.buffImmune[BuffID.Horrified] = true;
            player.buffImmune[BuffID.Cursed] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.Weak] = true;
            player.buffImmune[Toxic_Shock_Debuff.ID] = true;
            player.buffImmune[Solvent_Debuff.ID] = true;
        }
    }
}
