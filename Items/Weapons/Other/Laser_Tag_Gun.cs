using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
	public class Laser_Tag_Gun : IAnimatedItem, IElementalItem {
        public ushort Element => Elements.Earth;
        static DrawAnimationManual animation;
        public override DrawAnimation Animation => animation;
        public override Color? GlowmaskTint => Main.teamColor[Main.player[item.owner].team];
        static short glowmask;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Laser Tag Gun");
			Tooltip.SetDefault("‘Once you're tagged follow through the safety guidelines and walk out of the chamber.’");
            animation = new DrawAnimationManual(1);
			Main.RegisterItemAnimation(item.type, animation);
            glowmask = Origins.AddGlowMask("Weapons/Other/Laser_Tag_Gun_Glow");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.SpaceGun);
			item.damage = 1;
			item.magic = true;
			item.ranged = true;
			item.noMelee = true;
            item.crit = 46;
			item.width = 42;
			item.height = 14;
			item.useTime = 16;
			item.useAnimation = 16;
			item.mana = 10;
			item.value = 70000;
            item.shoot = ModContent.ProjectileType<Laser_Tag_Laser>();
			item.rare = ItemRarityID.Green;
            item.glowMask = glowmask;
            item.scale = 1;
		}
        public override void UpdateInventory(Player player) {
            OriginPlayer modPlayer = player.GetModPlayer<OriginPlayer>();
            int a = item.prefix;
            item.SetDefaults(item.type);
            if((modPlayer.oldBonuses&1)!=0||modPlayer.fiberglassSet) {
                item.crit = 0;
            }else if((modPlayer.oldBonuses&2)!=0||modPlayer.felnumSet) {
                item.crit = -14;
            }
            item.Prefix(a);
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(3-(11*Main.player[item.owner].direction),0);
        }
        public override void HoldItem(Player player) {
            if(player.itemAnimation!=0) {
                player.GetModPlayer<OriginPlayer>().ItemLayerWrench = true;
            }
        }
    }
    public class Laser_Tag_Laser : ModProjectile {
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.GreenLaser);
            projectile.light = 0;
            projectile.aiStyle = 0;
            projectile.extraUpdates++;
			projectile.magic = true;
			projectile.ranged = true;
        }
        public override void AI() {
            projectile.rotation = projectile.velocity.ToRotation();
            try {
                Color color = Main.teamColor[Main.player[projectile.owner].team];
                Lighting.AddLight(projectile.Center, Vector3.Normalize(color.ToVector3())*3);
            } catch(Exception) { }
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(crit)damage*=199;
        }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit) {
            if(crit)damage*=199;
        }
        public override void OnHitPvp(Player target, int damage, bool crit) {
            target.AddBuff(BuffID.Cursed, 600);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Color color = Main.teamColor[Main.player[projectile.owner].team];
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center-Main.screenPosition, null, color, projectile.rotation, new Vector2(42,1), projectile.scale, SpriteEffects.None, 1);
            return false;
        }
    }
}
