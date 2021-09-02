using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;

namespace Origins.Items.Weapons.Explosives {
    public class Bomboomstick : ModItem, ICustomDrawItem {
        public static Texture2D UseTexture { get; private set; }
        internal static void Unload() {
            UseTexture = null;
        }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bomboomstick");
            if(Main.netMode == NetmodeID.Server)return;
            UseTexture = mod.GetTexture("Items/Weapons/Explosives/Bomboomstick_Use");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Boomstick);
            item.useAmmo = ItemID.Grenade;
        }
        public void DrawInHand(Texture2D itemTexture, PlayerDrawInfo drawInfo, Vector2 itemCenter, Vector4 lightColor, Vector2 drawOrigin) {
            Player drawPlayer = drawInfo.drawPlayer;
            float itemRotation = drawPlayer.itemRotation;

            Vector2 pos = new Vector2((int)(drawInfo.itemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.itemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

            int frame = 0;
            switch(drawPlayer.itemAnimationMax-drawPlayer.itemAnimation) {
                case 3:
                frame = 1;
                break;
                case 4:
                case 2:
                frame = 2;
                break;
                default:
                frame = 0;
                break;
            }

            Main.playerDrawData.Add(new DrawData(
                UseTexture,
                pos,
                new Rectangle(0, 24*(frame), 96, 22),
                item.GetAlpha(new Color(lightColor.X, lightColor.Y, lightColor.Z, lightColor.W)),
                itemRotation,
                drawOrigin,
                item.scale,
                drawInfo.spriteEffects,
                0));
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Vector2 speed = new Vector2(speedX, speedY);
            for(int i = Main.rand.Next(3,5); i-->0;) {
                Projectile.NewProjectile(position, speed.RotatedByRandom(0.3f), type, damage, knockBack, player.whoAmI);
            }
            return false;
        }
    }
}
