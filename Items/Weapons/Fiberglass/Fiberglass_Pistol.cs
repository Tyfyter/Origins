using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Fiberglass {
	public class Fiberglass_Pistol : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Pistol");
			Tooltip.SetDefault("Be careful, it's sharp");
		}
		public override void SetDefaults() {
			item.damage = 11;
			item.ranged = true;
			item.width = 18;
			item.height = 36;
			item.useTime = 12;
			item.useAnimation = 12;
			item.useStyle = 5;
			item.knockBack = 1;
			item.value = 5000;
			item.shootSpeed = 14;
			item.autoReuse = true;
            item.useAmmo = AmmoID.Bullet;
            item.shoot = ProjectileID.Bullet;
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item11;
		}
        public override Vector2? HoldoutOffset() {
            return new Vector2(-0.5f,0);
        }
		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox){
			hitbox.Width = player.itemWidth;
			hitbox.Height = player.itemHeight;
            hitbox.X = (int)player.itemLocation.X;
            hitbox.Y = (int)player.itemLocation.Y;
		}
    }
}
