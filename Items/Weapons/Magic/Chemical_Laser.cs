using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Magic {
    public class Chemical_Laser : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "MagicGun",
			"ToxicSource"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadowbeamStaff);
			Item.damage = 30;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Item.mana = 8;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.knockBack = 0;
			Item.shoot = ModContent.ProjectileType<Laseer>();
			Item.shootSpeed = 32f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = Origins.Sounds.EnergyRipple.WithPitchRange(1.7f, 2f);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 20)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Laseer : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Summoner/Minions/Flying_Exoskeleton";
		public override void SetStaticDefaults() {
			Projectile.penetrate = 1;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ShadowBeamFriendly);
			Projectile.friendly = true;
			Projectile.penetrate = 1;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<Toxic_Shock_Debuff>(), 80);
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			int t = ModContent.ProjectileType<Acid_Shot>();
			for (int i = Main.rand.Next(1); i < 3; i++) Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Main.rand.NextVector2Unit() * 4) + (Projectile.velocity / 8), t, Projectile.damage / 5, 6, Projectile.owner, ai1: -0.5f).scale = 0.85f;
		}
	}
}
