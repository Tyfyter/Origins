using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Origins.Projectiles.Weapons;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Other {
    public class Firespit : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Firespit");
            Tooltip.SetDefault("Uses fireblossoms as ammo");
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            glowmask = Origins.AddGlowMask(this);
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.damage = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.crit = 1;
            Item.useAnimation = 2;
            Item.useTime = 38;
            Item.width = 58;
            Item.height = 22;
            Item.useAmmo = ItemID.Fireblossom;
            Item.shoot = ModContent.ProjectileType<Lava_Shot>();
            Item.shootSpeed = 8.75f;
            Item.UseSound = SoundID.Item20;
            Item.reuseDelay = 9;
            Item.rare = ItemRarityID.Orange;
            Item.glowMask = glowmask;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            if (player.itemAnimationMax - player.itemAnimation > 9) return;
            Vector2 offset = Vector2.Normalize(velocity);
            offset = offset * 24 + offset.RotatedBy(-MathHelper.PiOver2 * player.direction) * 8;
            position += offset;
            velocity = velocity.RotatedByRandom(0.5);
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		    if(player.itemAnimationMax-player.itemAnimation > 9)return false;
            SoundEngine.PlaySound(SoundID.Item20, position);
            Lava_Shot.damageType = DamageClass.Ranged;
            return true;
            //Projectile projectile = Projectile.NewProjectileDirect(, type, damage, knockBack, player.whoAmI);
        }
    }
}
