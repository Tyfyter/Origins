using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
    public class Bomboomstick : ModItem, ICustomDrawItem {
        public static AutoCastingAsset<Texture2D> UseTexture { get; private set; }
        public override void Unload() {
            UseTexture = null;
        }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bomboomstick");
            if (!Main.dedServ) {
                UseTexture = Mod.Assets.Request<Texture2D>("Items/Weapons/Demolitionist/Bomboomstick_Use");
            }
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Boomstick);
            Item.useAmmo = ItemID.Grenade;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Pink;
        }
        public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
            Player drawPlayer = drawInfo.drawPlayer;
            float itemRotation = drawPlayer.itemRotation;

            Vector2 pos = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

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

            drawInfo.DrawDataCache.Add(new DrawData(
                UseTexture,
                pos,
                new Rectangle(0, 24*(frame), 96, 22),
                Item.GetAlpha(lightColor),
                itemRotation,
                drawOrigin,
                drawPlayer.GetAdjustedItemScale(Item),
                drawInfo.itemEffect,
                0));
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            for(int i = Main.rand.Next(3,5); i-->0;) {
                Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.3f), type, damage, knockback, player.whoAmI);
            }
            return false;
        }
    }
}
