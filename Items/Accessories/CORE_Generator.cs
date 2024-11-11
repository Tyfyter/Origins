using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using PegasusLib;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Waist)]
	public class CORE_Generator : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"ExplosiveBoostAcc"
		];
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[Type] = AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher];
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 26);
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.LightPurple;
			Item.accessory = true;
			Item.damage = 10;
			Item.DamageType = DamageClasses.Explosive;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.shootSpeed = 5;
			Item.useAmmo = AmmoID.Rocket;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Last_Descendant>())
			.AddIngredient(ModContent.ItemType<Missile_Armcannon>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			player.GetAttackSpeed(DamageClasses.Explosive) += 0.1f;
			originPlayer.explosiveBlastRadius += 0.15f;
			player.GetKnockback(DamageClasses.Explosive) += 0.2f;
			originPlayer.explosiveProjectileSpeed += 0.3f;

			originPlayer.destructiveClaws = true;
			originPlayer.gunGlove = true;
			originPlayer.gunGloveItem = Item;
			originPlayer.guardedHeart = true;
			originPlayer.coreGenerator = true;
			originPlayer.coreGeneratorItem = Item;
			player.longInvince = true;
			player.starCloakItem = Item;
			player.starCloakItem_starVeilOverrideItem = Item;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (CanisterGlobalItem.GetCanisterType(source.AmmoItemIdUsed) != -1) {
				type = ModContent.ProjectileType<CORE_Generator_P>();
				float speed = velocity.Length();
				for (int i = 0; i < 5; i++) {
					Projectile.NewProjectile(source, position, GeometryUtils.Vec2FromPolar(speed, MathHelper.TwoPi * (i / 5f) - MathHelper.PiOver2), type, damage, knockback, player.whoAmI);
				}
				SoundEngine.PlaySound(Item.UseSound, position);
				return false;
			}
			return true;
		}
	}
	public class CORE_Generator_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Resizable_Mine_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Resizable_Mine_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.timeLeft = 120;
			Projectile.scale = 0.85f;
			Projectile.penetrate = 1;
		}
		public override void AI() {
			Projectile.velocity.Y += 0.2f;
			Projectile.rotation += Projectile.velocity.X * 0.1f;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.timeLeft > 105) {
				if (Projectile.velocity.X == 0f) {
					Projectile.velocity.X = -oldVelocity.X;
				}
				if (Projectile.velocity.Y == 0f) {
					Projectile.velocity.Y = -oldVelocity.Y;
				}
				return false;
			}
			return true;
		}
	}
}
