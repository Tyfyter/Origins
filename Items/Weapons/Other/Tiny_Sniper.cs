using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
    public class Tiny_Sniper : ModItem {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tiny Sniper");
			Tooltip.SetDefault("Comically small, comically powerful");
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.SniperRifle);
            Item.damage = 96;
            Item.crit = 36;
            Item.useAnimation = 33;
            Item.useTime = 33;
            Item.width = 34;
            Item.height = 12;
            Item.scale = 0.5f;
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
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		    if(type==ProjectileID.Bullet)type = ProjectileID.BulletHighVelocity;
            player.velocity-=velocity*0.2f;
            SoundEngine.PlaySound(SoundID.Item36.WithVolume(0.75f), position);
        }
    }
}
