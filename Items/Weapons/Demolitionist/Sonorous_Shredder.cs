#if true ///TODO: unfalse if thorium
using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Projectiles;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;
using ThoriumMod.Projectiles.Bard;

namespace Origins.Items.Weapons.Demolitionist {
	[ExtendsFromMod("ThoriumMod")]
	public class Sonorous_Shredder : BardItem, IBardDamageClassOverride {
		public DamageClass DamageType => DamageClasses.ExplosiveVersion[ThoriumDamageBase<BardDamage>.Instance];
		public override void SetStaticDefaults() {
			Empowerments.AddInfo<InvincibilityFrames>(2);
			Empowerments.AddInfo<FlightTime>(2);
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override void SetBardDefaults() {
			Item.damage = 54;
			Item.knockBack = 1f;
			Item.useAnimation = 16;
			Item.useTime = 16;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.shootSpeed = 18;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.shoot = ModContent.ProjectileType<Sonorous_Shredder_Projectile>();
			Item.UseSound = Origins.Sounds.RivenBass;
			InspirationCost = 1;
		}
		public override void ModifyEmpowermentPool(Player player, Player target, EmpowermentPool empPool) {
			OriginsThoriumPlayer originsThoriumPlayer = player.GetModPlayer<OriginsThoriumPlayer>();
			if (originsThoriumPlayer.altEmpowerment) {
				empPool.Remove(EmpowermentLoader.EmpowermentType<InvincibilityFrames>());
			} else {
				empPool.Remove(EmpowermentLoader.EmpowermentType<FlightTime>());
			}
			base.ModifyEmpowermentPool(player, target, empPool);
		}
	}
	public class Sonorous_Shredder_Unloaded : ModItem, ICustomWikiStat {
		public override string Name => base.Name[..^"_Unloaded".Length];
		public override LocalizedText Tooltip => this.GetLocalization("UnloadedTooltip");
		public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("ThoriumMod");
		public IEnumerable<WikiProvider> GetWikiProviders() => [];
	}
	[ExtendsFromMod("ThoriumMod")]
	public class Sonorous_Shredder_Projectile : BardProjectile, IBardDamageClassOverride {
		public DamageClass DamageType => DamageClasses.ExplosiveVersion[ThoriumDamageBase<BardDamage>.Instance];
		public override BardInstrumentType InstrumentType => BardInstrumentType.String;
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Sonorous_Shredder_P";
		public override void SetBardDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
		}
		public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.GetGlobalNPC<OriginsThoriumGlobalNPC>().SonorousShredderHit()) {
				if (Projectile.owner == Main.myPlayer) {
					ref bool altEmpowerment = ref Main.LocalPlayer.GetModPlayer<OriginsThoriumPlayer>().altEmpowerment;
					altEmpowerment = true;
					try {
						ContentInstance<Sonorous_Shredder>.Instance.NetApplyEmpowerments(Main.LocalPlayer, 0);
					} finally {
						altEmpowerment = false;
					}
					Projectile.NewProjectile(
						Projectile.GetSource_OnHit(target),
						Projectile.Center,
						default,
						ModContent.ProjectileType<Sonorous_Shredder_Explosion>(),
						Projectile.damage * 2,
						Projectile.knockBack * 2,
						Main.myPlayer
					);
				}
			}
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * (Projectile.direction - 1);
			Projectile.spriteDirection = Projectile.direction;
		}
		public override Color? GetAlpha(Color lightColor) => Riven_Hive.GetGlowAlpha(lightColor);
	}
	[ExtendsFromMod("ThoriumMod")]
	public class Sonorous_Shredder_Explosion : BardProjectile, IBardDamageClassOverride, IIsExplodingProjectile {
		public DamageClass DamageType => DamageClasses.ExplosiveVersion[ThoriumDamageBase<BardDamage>.Instance];
		public override BardInstrumentType InstrumentType => BardInstrumentType.Percussion;
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Sonorous_Shredder_P";
		public override void SetBardDefaults() {
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound:SoundID.Item14);
				Projectile.ai[0] = 1;
			}
		}
		public void Explode(int delay = 0) {}
		public bool IsExploding() => true;
	}
}
#endif