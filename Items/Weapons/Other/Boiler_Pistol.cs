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
using Terraria.GameContent.Creative;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Other {
    public class Boiler_Pistol : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Boiler Pistol");
            Tooltip.SetDefault("Uses fireblossoms as ammo");
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            glowmask = Origins.AddGlowMask(this);
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Gatligator);
            Item.damage = 53;
            Item.useAnimation = 18;
            Item.useTime = 12;
            Item.width = 48;
            Item.height = 26;
            Item.useAmmo = ItemID.Fireblossom;
            Item.shoot = ModContent.ProjectileType<Lava_Shot>();
            Item.shootSpeed*=1.75f;
            Item.UseSound = SoundID.Item41;
            Item.scale = 0.8f;
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
