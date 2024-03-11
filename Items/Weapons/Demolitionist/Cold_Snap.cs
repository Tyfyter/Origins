using Microsoft.Xna.Framework;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
    public class Cold_Snap : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "Launcher",
            "MineUser"
        };
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.SniperRifle);
            Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
            Item.damage = 6;
            Item.crit = 0;
            Item.useAnimation = 32;
            Item.useTime = 32;
            Item.useAmmo = ModContent.ItemType<Ammo.Resizable_Mine_One>();
            Item.shoot = ModContent.ProjectileType<Cold_Snap_P>();
            Item.shootSpeed = 12;
            Item.reuseDelay = 6;
            Item.autoReuse = true;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Orange;
        }
        public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.IceBlock, 30);
            recipe.AddIngredient(ItemID.Shiverthorn, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            type = Item.shoot;
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(-2, 0);
        }
    }
    public class Cold_Snap_P : ModProjectile {
        public override void SetStaticDefaults() {
            Origins.MagicTripwireRange[Type] = 32;
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if (Projectile.velocity.X == 0f) {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y == 0f) {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            Projectile.timeLeft = 1;
            return true;
        }
        public override void OnKill(int timeLeft) {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.StardustGuardianExplosion, 0, 0, Projectile.owner, -1, 1);
            Projectile.damage = (int)(Projectile.damage * 0.75f);
            Projectile.knockBack = 16f;
            Projectile.position = Projectile.Center;
            Projectile.width = (Projectile.height = 72);
            Projectile.Center = Projectile.position;
            Projectile.Damage();
            ExplosiveGlobalProjectile.DealSelfDamage(Projectile);
        }
        public override void AI() {
            Dust.NewDust(Projectile.Center, 0, 0, DustID.IceTorch, 0, 0, 155, default, 0.75f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(BuffID.Frostburn, 300);
        }
    }
}
