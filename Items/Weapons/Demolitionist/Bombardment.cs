using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo.Canisters;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Bombardment : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Launcher",
			"CanistahUser"
		];
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Bombardment_P>(2, 40, 9f, 48, 32);
			Item.useTime = 8;
			Item.knockBack = 4f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 20);
			Item.UseSound = null;
			Item.reuseDelay = 60;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-6f, 0);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
			velocity = velocity.RotatedByRandom(0.25f);
			SoundEngine.PlaySound(SoundID.Item61.WithPitch(0.4f), position);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 10)
			.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 15)
			.AddTile(TileID.Anvils)
			.Register();
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
