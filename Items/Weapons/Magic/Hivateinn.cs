using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Buffs;
using Origins.CrossMod;

namespace Origins.Items.Weapons.Magic {
	public class Hivateinn : ModItem {
		public override void SetStaticDefaults() {
			Item.staff[Item.type] = true;
			Item.ResearchUnlockCount = 1;
			Origins.DamageBonusScale[Type] = 1.5f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [Electrified_Debuff.ID];
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
		public override bool? UseItem(Player player) {
			Vector2 position = player.itemLocation;
			SoundEngine.PlaySound(Main.rand.Next(Origins.Sounds.LightningSounds), position);
			return null;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
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
			Projectile.extraUpdates = 5;
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
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float size = Projectile.width * Projectile.scale;
			float _ = 0;
			for (int i = 1; i < Projectile.oldPos.Length; i++) {
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.oldPos[i - 1], Projectile.oldPos[i], size, ref _)) return true;
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<LightningImmuneFixBuff>(), 6);
			target.AddBuff(Electrified_Debuff.ID, 240);
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
