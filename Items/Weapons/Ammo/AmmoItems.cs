using Origins.Dusts;
using Origins.Projectiles;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Metal_Slug : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MusketBall);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.shootSpeed = 0f;
			Item.damage = 25;
			Item.shoot = Metal_Slug_P.ID;
			Item.ammo = Item.type;
			Item.value = Item.sellPrice(silver: 3);
			Item.ArmorPenetration += 2;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 10)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddRecipeGroup(RecipeGroupID.IronBar)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Metal_Slug_P : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ExplosiveBullet);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.aiStyle = 0;
			Projectile.width = 10;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.penetrate = 7;
			Projectile.timeLeft = 900;
			Projectile.alpha = 0;
			Projectile.hide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.extraUpdates = 4;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.velocity.Y += 0.02f;
		}
		public override void OnKill(int timeLeft) {
			if (Projectile.GetGlobalProjectile<ExplosiveGlobalProjectile>().novaCascade) return;
			Projectile.NewProjectile(
				Projectile.GetSource_Death(),
				Projectile.Center,
				default,
				ModContent.ProjectileType<Metal_Slug_Explosion>(),
				Projectile.damage,
				Projectile.knockBack
			);
			/*if (Projectile.GetGlobalProjectile<ExplosiveGlobalProjectile>().acridHandcannon) return;
			int t = ModContent.ProjectileType<Acid_Shot>();
			for (int i = Main.rand.Next(1); i < 3; i++) Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Main.rand.NextVector2Unit() * 4) + (Projectile.velocity / 8), t, Projectile.damage / 5, 6, Projectile.owner, ai1: -0.5f).scale = 0.85f;
			);*/
		}
	}
	public class Metal_Slug_Explosion : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Sonorous_Shredder_P";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.width = 72;
			Projectile.height = 72;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62, fireDustAmount: 0);
				Projectile.ai[0] = 1;
			}
			ExplosiveGlobalProjectile.DealSelfDamage(Projectile);
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding => true;
	}
	public class Gray_Solution : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.PurpleSolution] = ItemID.RedSolution;
			ItemID.Sets.ShimmerTransformToItem[ItemID.RedSolution] = Type;
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Teal_Solution>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Teal_Solution>()] = ModContent.ItemType<Orange_Solution>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Orange_Solution>()] = ItemID.PurpleSolution;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GreenSolution);
			Item.shoot = ModContent.ProjectileType<Gray_Solution_P>();
			Item.value = Item.sellPrice(silver: 3);
		}
		public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
			type -= ProjectileID.PureSpray;
		}
	}
	public class Gray_Solution_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PureSpray);
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			OriginGlobalProj.ClentaminatorAI<Defiled_Wastelands_Alt_Biome>(Projectile, ModContent.DustType<Solution_D>(), new Color(156, 156, 160));
		}
		public override bool? CanCutTiles() {
			return false;
		}
	}
	public class Teal_Solution : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GreenSolution);
			Item.shoot = ModContent.ProjectileType<Teal_Solution_P>();
			Item.value = Item.sellPrice(silver: 3);
		}
		public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
			type -= ProjectileID.PureSpray;
		}
	}
	public class Teal_Solution_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PureSpray);
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			OriginGlobalProj.ClentaminatorAI<Riven_Hive_Alt_Biome>(Projectile, ModContent.DustType<Solution_D>(), new Color(0, 180, 255));
		}
		public override bool? CanCutTiles() {
			return false;
		}
	}
	public class Orange_Solution : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GreenSolution);
			Item.shoot = ModContent.ProjectileType<Orange_Solution_P>();
			Item.value = Item.sellPrice(silver: 3);
		}
		public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
			type -= ProjectileID.PureSpray;
		}
	}
	public class Orange_Solution_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PureSpray);
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			OriginGlobalProj.ClentaminatorAI<Ashen_Alt_Biome>(Projectile, ModContent.DustType<Solution_D>(), new Color(255, 156, 160));
		}
		public override bool? CanCutTiles() {
			return false;
		}
	}
}
