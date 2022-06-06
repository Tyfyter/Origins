using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
	public class Tiny_Sniper : ModItem {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tiny Sniper");
			Tooltip.SetDefault("Comically small, comically powerful");
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.SniperRifle);
            Item.damage = 96;
            Item.crit = 36;
            Item.useAnimation = 33;
            Item.useTime = 33;
            Item.width = 34;
            Item.height = 12;
            //item.scale = 0.5f;
		}
        public override Vector2? HoldoutOffset() {
            return new Vector2(-1,0);
        }
        public override void HoldItemFrame(Player player) {
            if(Item.holdStyle==4) {
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
            }
        }
        public override void HoldStyle(Player player) {
            Item.holdStyle = ItemHoldStyleID.HoldFront;
            if(player.scope&&(PlayerInput.UsingGamepad?(PlayerInput.GamepadThumbstickRight.Length() != 0f||!Main.SmartCursorIsUsed):Main.mouseRight)) {
                Item.holdStyle = 4;
				player.itemLocation.X = player.Center.X - 17f - (player.direction * 2);
				player.itemLocation.Y = player.MountedCenter.Y - 6f;
                Vector2 diff = Main.MouseWorld - player.MountedCenter;
                player.direction = System.Math.Sign(diff.X);
                player.itemRotation = diff.ToRotation()+(diff.X>0?0:MathHelper.Pi);
            }
        }
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		    if(type==ProjectileID.Bullet)type = ProjectileID.BulletHighVelocity;
            player.velocity-=velocity*0.2f;
            SoundEngine.PlaySound(SoundID.Item,(int)position.X,(int)position.Y,36, 0.75f);
        }
    }
}
