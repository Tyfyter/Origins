using Origins.Dev;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Shadow_Flare : ModItem, ICustomWikiStat {
		public override string Texture => typeof(Blast_Furnace).GetDefaultTMLName();
		public string[] Categories => [
			WikiCategories.SpellBook
		];
		public override void SetStaticDefaults() {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Daybreak];
		}
		public override void SetDefaults() {
			Item.DamageType = DamageClass.Magic;
			Item.damage = 60;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.shoot = ModContent.ProjectileType<Shadow_Flare_P>();
			Item.shootSpeed = 16f;
			Item.mana = 14;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item103;
			Item.autoReuse = true;
			Item.useLimitPerAnimation = 3;
		}
		public override float UseTimeMultiplier(Player player) => 0.15f;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = 2; i > 0; i--) {
				Projectile.NewProjectile(
					source,
					position,
					velocity.RotatedByRandom(1f),
					type,
					damage,
					knockback
				);
			}
			return false;
		}
		public override void AddRecipes() => CreateRecipe()
			.AddIngredient<Corona>()
			.AddIngredient(ItemID.Ectoplasm, 10)
			.AddTile(TileID.Bookcases)
			.Register();
	}
	public class Shadow_Flare_P : Corona_P {
		public override string Texture => typeof(Blast_Furnace).GetDefaultTMLName();
		public override float FadeFrames => 20f;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.timeLeft = 75;
			Projectile.extraUpdates = 2;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Daybreak, Main.rand.Next(40, 121));
			if (target is null) return;
			float targetCost = 16 * 20;
			if (Main.player[Projectile.owner].DoHoming(target => {
				if (target is not NPC targetNPC || Projectile.localNPCImmunity[target.whoAmI] != 0) return false;
				Vector2 targetPos = Projectile.Center.Clamp(target.Hitbox);
				Vector2 targetDir = (targetPos - (Projectile.position + HeadOffset)).Normalized(out float dist);
				float dot = Vector2.Dot(originalDirection, targetDir);
				float dirCost = 0.5f / (1 - (dot * Math.Abs(dot) * 0.5f));
				dist /= dirCost;
				if (dist < targetCost) {
					targetCost = dist;
					this.target = targetNPC;
					return true;
				}
				return false;
			}, false)) {
				originalDirection = target.DirectionFrom(Projectile.position + HeadOffset);
			}
		}
		public override Color BladeColors(float progressOnStrip) => Color.Black * Projectile.Opacity;
		public override Color BladeSecondaryColors(float progressOnStrip) => new Color(255, 100, 16, 32) * Projectile.Opacity;
		public override int BladeWidth => 16;
	}
}
