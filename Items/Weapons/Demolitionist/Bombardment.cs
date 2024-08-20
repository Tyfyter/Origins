using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Items.Weapons.Ammo.Canisters;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
namespace Origins.Items.Weapons.Demolitionist {
	public class Bombardment : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Launcher",
			"CanistahUser"
		];
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Bombardment_P>(3, 36, 13f, 48, 32);
			Item.useTime = 6;
			Item.knockBack = 4f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 20);
			Item.UseSound = null;
			Item.reuseDelay = 60;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
			velocity = velocity.RotatedByRandom(0.3f);
			SoundEngine.PlaySound(SoundID.Item61.WithPitch(0.25f), position);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 10);
			recipe.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return !Main.rand.NextBool(2);
		}
	}
	public class Bombardment_P : ModProjectile, ICanisterProjectile, IIsExplodingProjectile {
		public static AutoLoadingAsset<Texture2D> outerTexture = typeof(Bombardment_P).GetDefaultTMLName() + "_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = typeof(Bombardment_P).GetDefaultTMLName() + "_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 30;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.timeLeft = 420;
			Projectile.scale = 0.75f;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			Projectile.velocity *= 0.96f;
		}
		public bool IsExploding() => Projectile.timeLeft <= 0;
	}
}
