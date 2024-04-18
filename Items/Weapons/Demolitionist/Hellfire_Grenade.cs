using Microsoft.Xna.Framework;
using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
    public class Hellfire_Grenade : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => new string[] {
            "ThrownExplosive",
			"IsGrenade",
            "SpendableWeapon"
        };
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 67;
			Item.shoot = ModContent.ProjectileType<Hellfire_Grenade_P>();
			Item.value *= 9;
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 999;
			Item.glowMask = glowmask;
            Item.ArmorPenetration += 1;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 35);
			recipe.AddIngredient(ItemID.Grenade, 35);
			recipe.AddIngredient(ItemID.Fireblossom);
			recipe.AddIngredient(ItemID.Hellstone, 5);
			recipe.Register();
		}
	}
	public class Hellfire_Grenade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Hellfire_Grenade";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.penetrate = 1;
			Projectile.timeLeft = 135;
		}
		public override bool PreKill(int timeLeft) {
            Projectile.type = ProjectileID.Grenade;
            return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			Projectile.NewProjectile(
				Projectile.GetSource_Death(),
				Projectile.Center,
				default,
				ModContent.ProjectileType<Hellfire_Bomb_Fire>(),
				Projectile.damage,
				Projectile.knockBack,
				Projectile.owner
			);
		}
	}
}
