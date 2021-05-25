using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
	public class Tiny_Sniper : ModItem {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tiny Sniper");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.SniperRifle);
            item.damage = 60;
            item.crit = 36;
            item.useAnimation = 33;
            item.useTime = 33;
            item.width = 34;
            item.height = 12;
            //item.scale = 0.5f;
		}
        public override Vector2? HoldoutOffset() {
            return new Vector2(-1,0);
        }
        public override bool HoldItemFrame(Player player) {
            if(item.holdStyle==4) {
                float rot = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero).Y;//player.itemRotation * player.direction;
		        player.bodyFrame.Y = player.bodyFrame.Height * 3;
                bool reverseGrav = player.gravDir == -1f;
		        if (rot < -0.65){
			        player.bodyFrame.Y = player.bodyFrame.Height * 2;
			        if (reverseGrav){
				        player.bodyFrame.Y = player.bodyFrame.Height * 4;
			        }
		        }
		        if (rot > 0.6){
			        player.bodyFrame.Y = player.bodyFrame.Height * 4;
			        if (reverseGrav){
				        player.bodyFrame.Y = player.bodyFrame.Height * 2;
			        }
		        }
                return true;
            }
            return false;
        }
        public override void HoldStyle(Player player) {
            item.holdStyle = ItemHoldStyleID.Default;
            if(player.scope&&(PlayerInput.UsingGamepad?(PlayerInput.GamepadThumbstickRight.Length() != 0f||!Main.SmartCursorEnabled):Main.mouseRight)) {
                item.holdStyle = 4;
				player.itemLocation.X = player.Center.X - 17f - (player.direction * 2);
				player.itemLocation.Y = player.MountedCenter.Y - 6f;
                Vector2 diff = Main.MouseWorld - player.MountedCenter;
                player.direction = System.Math.Sign(diff.X);
                player.itemRotation = diff.ToRotation()+(diff.X>0?0:MathHelper.Pi);
            }
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            if(type==ProjectileID.Bullet)type = ProjectileID.BulletHighVelocity;
            player.velocity.X-=speedX/5;
            player.velocity.Y-=speedY/5;
            Main.PlaySound(SoundID.Item,(int)position.X,(int)position.Y,36, 0.75f);
            return true;
        }
    }
}
