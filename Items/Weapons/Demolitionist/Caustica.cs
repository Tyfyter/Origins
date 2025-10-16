using Origins.Buffs;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
	public class Caustica : ModItem, IElementalItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.OtherExplosive
		];
		public ushort Element => Elements.Acid;

		public override void SetStaticDefaults() {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.CursedInferno, BuffID.Venom, Toxic_Shock_Debuff.ID];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 270;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.reuseDelay = 8;
			Item.mana = 24;
			Item.shoot = ModContent.ProjectileType<Caustica_P>();
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Lime;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int a = Main.rand.Next(5, 7);
			for (int i = 0; ++i < a; a = Main.rand.Next(5, 7)) {
				Projectile.NewProjectile(source, position, velocity.RotatedBy(((i - a / 2f) / a) * 0.35), type, damage / 2, knockback, player.whoAmI, 0, 12f);
			}
			return false;
		}
	}
	public class Caustica_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 0;
			Projectile.width = Projectile.height = 10;
			Projectile.light = 0;
			Projectile.timeLeft = 120;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.ignoreWater = true;
		}
		public override void AI() {
			if (Projectile.ai[0] > 0) {
				Projectile.ai[0]++;
				Projectile.timeLeft++;
				if (Main.npc[(int)Projectile.ai[1]].active && !Projectile.Hitbox.Intersects(Main.npc[(int)Projectile.ai[1]].Hitbox)) {
					Projectile.ai[0] = 15;
				}
				if (Projectile.ai[0] >= 15) {
					Projectile.ai[0] = -1;
					Projectile.Kill();
				}
			}
			Dust dust = Dust.NewDustPerfect(Projectile.Center, 226, Projectile.velocity.RotatedByRandom(1.4) / 4f, 100, Projectile.timeLeft % 2 == 0 ? Color.LightSeaGreen : Color.Coral, Projectile.scale * (Projectile.ai[0] > 0 ? 0.6f : 1));
			dust.shader = GameShaders.Armor.GetSecondaryShader(Projectile.timeLeft % 2 == 0 ? 90 : 1, Main.LocalPlayer);
			dust.noGravity = false;
			dust.noLight = true;
		}
		public override bool? CanHitNPC(NPC target) {
			return ((int)Projectile.ai[0] <= 0) ? null : ((bool?)false);
		}
		public override void OnKill(int timeLeft) {
			Projectile.ai[0] = 0;
			for (int i = 0; i < 9; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, 10, 10, DustID.Electric, 0, 0, 100, i % 2 == 0 ? Color.LightSeaGreen : Color.Coral, Projectile.scale);
				dust.shader = GameShaders.Armor.GetSecondaryShader(i % 2 == 0 ? 90 : 1, Main.LocalPlayer);
				dust.velocity *= 4;
				dust.noGravity = true;
				dust.noLight = true;
			}
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = (int)(96 * Projectile.scale);
			Projectile.height = (int)(96 * Projectile.scale);
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.damage = (int)(Projectile.damage * 0.75f);
			Projectile.Damage();
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[0] == 0) {
				Projectile.ai[0]++;
				Projectile.ai[1] = target.whoAmI;
			}
			target.AddBuff(Toxic_Shock_Debuff.ID, 480);
			target.AddBuff(BuffID.CursedInferno, 480);
			target.AddBuff(BuffID.Venom, 480);
			for (int i = 5; 0 < --i;) {
				Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Electric, 0, 0, 100, i % 2 == 0 ? Color.LightSeaGreen : Color.Coral, Projectile.scale);
				dust.shader = GameShaders.Armor.GetSecondaryShader(i % 2 == 0 ? 90 : 1, Main.LocalPlayer);
				dust.noGravity = false;
				dust.noLight = true;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			return false;
		}
	}
}
