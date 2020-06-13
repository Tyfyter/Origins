using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons {
	public class Fiberglass_Bow : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Bow");
			Tooltip.SetDefault("Be careful, it's sharp");
		}
		public override void SetDefaults() {
			item.damage = 17;
			item.ranged = true;
			item.width = 18;
			item.height = 36;
			item.useTime = 14;
			item.useAnimation = 14;
			item.useStyle = 5;
			item.knockBack = 1;
			item.value = 5000;
			item.shootSpeed = 14;
			item.autoReuse = true;
            item.useAmmo = AmmoID.Arrow;
            item.shoot = ProjectileID.WoodenArrowFriendly;
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item5;
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
