using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.DataStructures;

namespace Origins.Items.Weapons.Other {
    public class Eyndum_Scar : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Eyndum Scar");
            Tooltip.SetDefault("");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Katana);
            item.damage = 407;
            item.melee = true;
            item.noUseGraphic = false;
            item.noMelee = false;
            item.width = 70;
            item.height = 70;
            item.useStyle = 1;
            item.useTime = 20;
            item.useAnimation = 20;
            item.knockBack = 9.5f;
            item.value = 500000;
            item.shoot = ProjectileID.None;
            item.rare = ItemRarityID.Purple;
            item.autoReuse = true;
            item.scale = 1f;
        }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) {
            if(!(target.boss || NPCID.Sets.TechnicallyABoss[target.type])) {
                int quarterHealth = target.lifeMax / 4;
                if(target.life<=quarterHealth) {
                    damage = Math.Max(target.life + (target.defense / 2), damage);
                }
            }
        }
    }
}
