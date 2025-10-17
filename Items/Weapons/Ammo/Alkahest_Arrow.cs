using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.NPCs;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ammo {
	public class Alkahest_Arrow : ModItem, ICustomWikiStat, ITornSource {
		public static float TornSeverity => 0.5f;
		float ITornSource.Severity => TornSeverity;
		public string[] Categories => [
			WikiCategories.Arrow
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WoodenArrow);
			Item.damage = 17;
			Item.shoot = ModContent.ProjectileType<Alkahest_Arrow_P>();
			Item.shootSpeed = 4f;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(copper: 9);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 150)
			.AddIngredient(ItemID.WoodenArrow, 150)
			.AddIngredient(ModContent.ItemType<Alkahest>())
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Alkahest_Arrow_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ammo/Alkahest_Arrow_P";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.penetrate = 1;
			Projectile.width = 14;
			Projectile.height = 32;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 300, 180, Alkahest_Arrow.TornSeverity, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
	}
}
