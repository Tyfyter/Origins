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
            Item.CloneDefaults(ItemID.Katana);
            Item.damage = 407;
            Item.DamageType = DamageClass.Melee;
            Item.noUseGraphic = false;
            Item.noMelee = false;
            Item.width = 70;
            Item.height = 70;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.knockBack = 9.5f;
            Item.value = 500000;
            Item.shoot = ProjectileID.None;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
            Item.scale = 1f;
        }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) {
            if(!(target.boss || NPCID.Sets.ShouldBeCountedAsBoss[target.type])) {
                int quarterHealth = target.lifeMax / 4;
                if(target.life<=quarterHealth) {
                    damage = Math.Max(target.life + (target.defense / 2), damage);
                }
            }
        }
    }
}
