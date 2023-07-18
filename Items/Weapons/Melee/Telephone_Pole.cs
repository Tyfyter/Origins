using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Telephone_Pole : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Telephone Pole");
			// Tooltip.SetDefault("'We already have Cellphones'");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TerraBlade);
			Item.damage = 110;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = false;
			Item.noMelee = false;
			Item.width = 104;
			Item.height = 110;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.knockBack = 14f;
			Item.shoot = ProjectileID.None;
			Item.value = Item.buyPrice(gold: 2);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.Wood, 50);
			recipe.AddIngredient(ModContent.ItemType<Valkyrum_Bar>(), 7);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.NewProjectileDirect(
				player.GetSource_OnHit(target),
				target.Center,
				default,
				ModContent.ProjectileType<Telephone_Pole_Shock>(),
				damage,
				0,
				player.whoAmI
			).localNPCImmunity[target.whoAmI] = 30;
		}
	}
	public class Telephone_Pole_Shock : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		protected override bool CloneNewInstances => true;
		Vector2 closest;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.DamageType = DamageClass.Melee;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 3;
			Projectile.width = Projectile.height = 20;
			Projectile.penetrate = 2;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
		}
		public override void AI() {
			if (Projectile.penetrate == 1) {
				Projectile.penetrate = 2;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			closest = Projectile.Center.Clamp(targetHitbox.TopLeft(), targetHitbox.BottomRight());
			return (Projectile.Center - closest).LengthSquared() <= 240 * 240;
		}
		public override bool? CanHitNPC(NPC target) {
			return Projectile.penetrate > 1 ? base.CanHitNPC(target) : false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.damage -= (int)((Projectile.Center - closest).Length() / 16f);
			if (!Main.rand.NextBool(5)) Projectile.timeLeft += crit ? 2 : 1;
			Vector2 dest = Projectile.Center;
			Projectile.Center = Vector2.Lerp(closest, new Vector2(target.position.X + Main.rand.NextFloat(target.width), target.position.Y + Main.rand.NextFloat(target.height)), 0.5f);
			for (int i = 0; i < 16; i++) {
				Dust.NewDustPerfect(Vector2.Lerp(Projectile.Center, dest, i / 16f), 226, Main.rand.NextVector2Circular(1, 1), Scale: 0.5f);
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.spriteBatch.DrawLightningArcBetween(
				Projectile.oldPosition - Main.screenPosition,
				Projectile.position - Main.screenPosition,
				Main.rand.NextFloat(-4, 4));
			Vector2 dest = (Projectile.oldPosition - Projectile.position) + new Vector2(Projectile.width, Projectile.height) / 2;
			for (int i = 0; i < 16; i++) {
				Dust.NewDustPerfect(Vector2.Lerp(Projectile.Center, dest, i / 16f), 226, Main.rand.NextVector2Circular(1, 1), Scale: 0.5f);
			}
			return false;
		}
	}
}
