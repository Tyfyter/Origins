using Microsoft.Xna.Framework;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Origins.Items.Weapons.Ammo;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ammo.Canisters;
namespace Origins.Items.Weapons.Demolitionist {
    public class Cold_Snap : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "Launcher",
            "CanistahUser"
        };
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.GrenadeLauncher);
            Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
            Item.damage = 15;
            Item.crit = 0;
            Item.useAnimation = 32;
            Item.useTime = 32;
            Item.useAmmo = ModContent.ItemType<Resizable_Mine_One>();
            Item.shoot = ModContent.ProjectileType<Cold_Snap_P>();
            Item.shootSpeed = 12;
            Item.reuseDelay = 6;
            Item.autoReuse = true;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Orange;
            Item.ArmorPenetration += 3;
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
    public class Cold_Snap_P : ModProjectile, ICanisterProjectile {
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Cold_Snap_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Cold_Snap_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
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
        public override void AI() {
            Dust.NewDust(Projectile.Center, 0, 0, DustID.IceTorch, 0, 0, 155, default, 0.75f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(BuffID.Frostburn, 300);
        }
    }
}
