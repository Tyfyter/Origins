using Microsoft.Xna.Framework;
using Origins.Items.Other.Consumables.Food;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
    public class Hot_Potato : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Hot Potato");
			Tooltip.SetDefault("'Be careful, it's hot'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Snowball);
			Item.maxStack = 1;
			Item.damage = 16;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.consumable = false;
			Item.autoReuse = true;
			Item.mana = 8;
			Item.shoot = ModContent.ProjectileType<Hot_Potato_P>();
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.FallenStar);
			recipe.AddIngredient(ItemID.HellstoneBar, 8);
			recipe.AddIngredient(ModContent.ItemType<Potato>());
			recipe.AddTile(TileID.Anvils);
		}
	}
	public class Hot_Potato_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RubyBolt);
			Projectile.extraUpdates = 1;
			Projectile.penetrate = 25;
			Projectile.hide = false;
			Projectile.alpha = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 0;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[0] = -1;
			Projectile.ai[1] = -1;
		}
		public override bool? CanHitNPC(NPC target) {
			if (target.whoAmI == (int)Projectile.ai[0] || target.whoAmI == (int)Projectile.ai[1]) return false;
			return null;
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			float maxDist = 256 * 256;
			Vector2 nextDirection = default;
			bool foundNextTarget = false;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC currentTarget = Main.npc[i];
				if (i == target.whoAmI || i == (int)Projectile.ai[0] || !currentTarget.CanBeChasedBy()) continue;

				Vector2 currentDiff = currentTarget.Center - Projectile.Center;
				float currentDist = currentDiff.LengthSquared();

				if (currentDist < maxDist) {
					maxDist = currentDist;
					nextDirection = currentDiff;
					foundNextTarget = true;
				}
			}
			if (foundNextTarget) {
				Projectile.velocity = nextDirection.SafeNormalize(default) * Projectile.velocity.Length();
			}
			Projectile.damage += 2;
			Projectile.ai[1] = Projectile.ai[0];
			Projectile.ai[0] = target.whoAmI;
		}
		public override void AI() {
			if (Main.rand.NextBool(3)) Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Torch);
		}
	}
}
