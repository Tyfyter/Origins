using Microsoft.Xna.Framework;
using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
    public class Hellfire_Bomb : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => new string[] {
            "ThrownExplosive",
			"IsBomb"
        };
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Bomb);
			Item.damage = 65;
			Item.shoot = ModContent.ProjectileType<Hellfire_Bomb_P>();
			Item.value *= 9;
			Item.rare = ItemRarityID.Orange;
			Item.glowMask = glowmask;
            Item.ArmorPenetration += 1;
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 15);
			recipe.AddIngredient(ItemID.Bomb, 15);
			recipe.AddIngredient(ItemID.Fireblossom);
			recipe.AddIngredient(ItemID.Hellstone, 5);
			recipe.Register();
		}
	}
	public class Hellfire_Bomb_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Hellfire_Bomb";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.penetrate = 1;
			Projectile.timeLeft = 135;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.InfernoFriendlyBlast;
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
			Vector2 v;
			for (int i = 4; i-- > 0;) {
				v = Main.rand.NextVector2Unit() * 6;
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + v * 8, v, ModContent.ProjectileType<Impeding_Shrapnel_Shard>(), Projectile.damage / 2, Projectile.knockBack / 4, Projectile.owner);
			}
		}
	}
}
