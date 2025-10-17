using Origins.Dev;
using Origins.Items.Materials;
using Origins.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Rubber_Dynamite : ModItem {
        public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Dynamite);
			Item.damage = 65;
			Item.shoot = ModContent.ProjectileType<Rubber_Dynamite_P>();
			Item.shootSpeed *= 1.75f;
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.Dynamite, 8)
			.AddIngredient<Rubber>()
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Rubber_Dynamite_P : ModProjectile {
		public override string Texture => typeof(Rubber_Dynamite).GetDefaultTMLName();
		public override LocalizedText DisplayName => Language.GetOrRegister($"Mods.Origins.Items.{nameof(Rubber_Dynamite)}.DisplayName");
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			Origins.MagicTripwireDetonationStyle[Type] = 1;
			ProjectileID.Sets.Explosive[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Dynamite);
			Projectile.timeLeft = 360;
			Projectile.friendly = false;
			AIType = ProjectileID.Dynamite;
			DrawOriginOffsetY = -16;
		}
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.TryGetGlobalProjectile(out ExplosiveGlobalProjectile global)) global.selfDamageModifier /= 4;
		}
		public override void PrepareBombToBlow() {
			Projectile.Resize(250, 250);
			Projectile.ai[1] = 0f;
			Projectile.friendly = true;
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile.Hitbox, SoundID.Item14);
		}
	}
}
