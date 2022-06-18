using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Explosives {
	public class Shrapnel_Bomb : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Shrapnel Bomb");
			Tooltip.SetDefault("Explodes into shrapnel");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Bomb);
            Item.damage = 89;
			Item.value*=2;
			Item.useTime = (int)(Item.useTime*1.15);
			Item.useAnimation = (int)(Item.useAnimation*1.15);
            Item.shoot = ModContent.ProjectileType<Shrapnel_Bomb_P>();
			Item.shootSpeed*=0.95f;
            Item.knockBack = 13f;
			Item.rare = ItemRarityID.Green;
		}
    }
    public class Shrapnel_Bomb_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Explosives/Shrapnel_Bomb";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Shrapnel Bomb");
            //Origins.ExplosiveProjectiles[Projectile.type] = true;
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Bomb);
            Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Throwing];
            Projectile.penetrate = 1;
            Projectile.timeLeft = 135;
        }
        public override bool PreKill(int timeLeft) {
            Projectile.type = ProjectileID.Bomb;
            return true;
        }
        public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
            int center = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Ace_Shrapnel_P>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            Vector2 v;
            for(int i = 4; i-->0;) {
                v = Main.rand.NextVector2Unit()*6;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center+v*8, v, ModContent.ProjectileType<Ace_Shrapnel_Shard>(), Projectile.damage/2, Projectile.knockBack/4, Projectile.owner, center, 4);
            }
        }
    }
}
