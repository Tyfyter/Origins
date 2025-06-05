using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using PegasusLib;
using Terraria;
using Origins.Projectiles;
namespace Origins.Items.Weapons.Melee {
	public class Fiberglass_Sword : ModItem, IElementalItem, ICustomWikiStat {
		public ushort Element => Elements.Fiberglass;
		public string[] Categories => [
			"Sword"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 18;
			Item.DamageType = DamageClass.Melee;
			Item.width = 42;
			Item.height = 42;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
			Item.shoot = ModContent.ProjectileType<Fiberglass_Sword_P>();
			Item.shootSpeed = 11;
			Item.value = Item.buyPrice(silver: 30);
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void UseItemFrame(Player player) {
			if (player.altFunctionUse != 0) player.itemLocation = default;
		}
		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) => noHitbox = player.altFunctionUse != 0;
		public override bool CanShoot(Player player) => player.altFunctionUse != 0;
		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
	}
	public class Fiberglass_Sword_P : ModProjectile, IElementalProjectile {
		public ushort Element => Elements.Fiberglass;
		public override string Texture => typeof(Fiberglass_Sword).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.aiStyle = ProjAIStyleID.Boomerang;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.width = 44;
			Projectile.height = 44;
			Projectile.scale = 1f;
			DrawOriginOffsetX = 0;
			DrawOriginOffsetY = 0;
		}
		public override void PostAI() {
			Projectile.velocity *= 0.99f;
			Projectile.rotation -= 0.2f * Projectile.direction;
			Projectile.spriteDirection = Projectile.direction;
			if (Projectile.numUpdates == 0) {
				Projectile.soundDelay++;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.velocity = Projectile.oldVelocity;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 27;
			height = 27;
			return true;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			float scale = (Projectile.scale - 1) * 0.5f;
			hitbox.Inflate((int)(hitbox.Width * scale), (int)(hitbox.Height * scale));
		}
	}
}
