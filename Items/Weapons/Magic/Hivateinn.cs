using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Magic {
	public class Hivateinn : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Wand"
        ];
        public override void SetStaticDefaults() {
			Item.staff[Item.type] = true;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrystalVileShard);
			Item.shoot = ModContent.ProjectileType<Felnum_Lightning>();
			Item.damage = 78;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.shootSpeed /= 2;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Lime;
			Item.mana = 17;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SkyFracture)
			.AddIngredient(ModContent.ItemType<Valkyrum_Bar>(), 15)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage = damage.Scale(1.5f);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), position);
			Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.9f, 1.1f), type, damage, knockback, player.whoAmI, velocity.ToRotation(), Main.rand.NextFloat());
			return false;
		}
	}
	public class Felnum_Lightning : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_466";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CultistBossLightningOrbArc);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 0;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.timeLeft /= 3;
			Projectile.penetrate = -1;
		}
		public override void AI() {
			Projectile.type = ProjectileID.CultistBossLightningOrbArc;
			Vector2 targetPos = Projectile.Center;
			bool foundTarget = false;
			Vector2 testPos;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC target = Main.npc[i];
				if (target.CanBeChasedBy() && !target.HasBuff(ModContent.BuffType<LightningImmuneFixBuff>())) {
					testPos = Projectile.Center.Clamp(target.Hitbox);
					Vector2 difference = testPos - Projectile.Center;
					float distance = difference.Length();
					bool closest = Vector2.Distance(Projectile.Center, targetPos) > distance;
					bool inRange = distance < 96 && (difference.SafeNormalize(Vector2.Zero) * Projectile.velocity.SafeNormalize(Vector2.Zero)).Length() > 0.1f;//magRange;
					if ((!foundTarget || closest) && inRange) {
						targetPos = testPos;
						foundTarget = true;
					}
				}
			}
			if (foundTarget) {
				Vector2 direction = targetPos - Projectile.Center;
				direction.Normalize();
				direction *= Projectile.velocity.Length();
				Projectile.velocity = direction;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<LightningImmuneFixBuff>(), 4);
		}
		public override bool? CanHitNPC(NPC target) {
			return target.HasBuff(ModContent.BuffType<LightningImmuneFixBuff>()) ? false : base.CanHitNPC(target);
		}
	}
	public class LightningImmuneFixBuff : ModBuff, ICustomWikiStat {
		public bool CanExportStats => false;
		public override string Texture => "Terraria/Images/Buff_204";
	}
}
