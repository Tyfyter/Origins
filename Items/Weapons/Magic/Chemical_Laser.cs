using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Shaders;

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
			Item.shootSpeed = 8f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = Origins.Sounds.EnergyRipple.WithPitchRange(1.7f, 2f);
		}
		public bool? Hardmode => true;
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 20)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 unit = velocity.SafeNormalize(default);
			position += unit * 56 + new Vector2(unit.Y, -unit.X) * player.direction * 2;
		}
		public override Vector2? HoldoutOffset() => Vector2.Zero;
	}
	public class Laseer : ModProjectile {
		public override string Texture => typeof(Chemical_Laser).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ShadowBeamFriendly);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.timeLeft = 250;
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.hide = true;
		}
		public override void AI() {
			int offScreenDist = 128;
			if (Projectile.position.X < Main.screenPosition.X - offScreenDist || Projectile.position.Y < Main.screenPosition.Y - offScreenDist
				|| Projectile.position.X > Main.screenPosition.X + Main.screenWidth + offScreenDist || Projectile.position.Y > Main.screenPosition.Y + Main.screenHeight + offScreenDist) return;
			ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(ItemID.AcidDye);
			for (int i = 0; i < 2; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, 1, 1, DustID.Electric);
				dust.position = Projectile.position - Projectile.velocity * (i * 0.5f);
				dust.position.X += Projectile.width / 2;
				dust.position.Y += Projectile.height / 2;
				dust.scale = Main.rand.NextFloat(0.65f, 0.65f);
				dust.velocity = dust.velocity * 0.2f + Projectile.velocity * 0.1f;
				dust.shader = shader;
				dust.noGravity = false;
				dust.noLight = true;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, 80);
		}
		public override void OnKill(int timeLeft) {
			if (Projectile.owner != Main.myPlayer) {
				if (Projectile.hide) {
					Projectile.hide = false;
					try {
						Projectile.active = true;
						Projectile.timeLeft = timeLeft;
						Projectile.Update(Projectile.whoAmI);
					} finally {
						Projectile.active = false;
						Projectile.timeLeft = 0;
					}
				}
				return;
			}
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			int t = ModContent.ProjectileType<Brine_Droplet>();
			for (int i = Main.rand.Next(1); i < 3; i++) Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Main.rand.NextVector2Unit() * 4) + (Projectile.velocity / 8), t, Projectile.damage / 5, 6, Projectile.owner, ai1: -0.5f).scale = 0.85f;
		}
	}
}
