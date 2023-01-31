﻿using Microsoft.Xna.Framework;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    public class Boiler : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Boiler");
            Tooltip.SetDefault("Uses fireblossoms as ammo");
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            glowmask = Origins.AddGlowMask(this);
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Gatligator);
            Item.damage = 38;
            Item.useAnimation = 18;
            Item.useTime = 12;
            Item.width = 38;
            Item.height = 22;
            Item.useAmmo = ItemID.Fireblossom;
            Item.shoot = ModContent.ProjectileType<Lava_Shot>();
            Item.shootSpeed*=1.75f;
            Item.UseSound = SoundID.Item41;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Orange;
            Item.glowMask = glowmask;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            Vector2 offset = Vector2.Normalize(velocity);
            offset = offset*24+offset.RotatedBy(-MathHelper.PiOver2*player.direction)*8;
            SoundEngine.PlaySound(SoundID.Item41, position+offset);
            position+=offset;
            Item.reuseDelay = 36;
            Lava_Shot.damageType = DamageClass.Ranged;
        }
    }
}