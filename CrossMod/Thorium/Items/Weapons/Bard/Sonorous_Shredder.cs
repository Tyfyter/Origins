using Origins.Dev;
using Origins.Items.Materials;
using Origins.Projectiles;
using Origins.Tiles;
using Origins.Tiles.Ashen;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;
using ThoriumMod.Projectiles.Bard;

namespace Origins.CrossMod.Thorium.Items.Weapons.Bard {
	#region without thorium
	public class Sonorous_Shredder: ModItem {
		public static int ID { get; internal set; }
		public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("ThoriumMod");
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ID = Type;
		}
		public static void SetSharedDefaults(Item item, out int cost) {
			item.damage = 54;
			item.DamageType = DamageClasses.Explosive;
			item.knockBack = 1f;
			item.useAnimation = 16;
			item.useTime = 16;
			item.useStyle = ItemUseStyleID.Shoot;
			item.noMelee = true;
			item.shootSpeed = 18;
			item.value = Item.sellPrice(gold: 2);
			item.rare = ItemRarityID.LightRed;
			item.UseSound = Origins.Sounds.RivenBass;
			cost = 10;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SoulofNight, 15)
			.AddIngredient<Alkahest>(25)
			.AddIngredient(OriginTile.TileItem<Spug_Flesh>(), 30)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(Item.UseSound.Value.WithPitchOffset(
				Math.Min(((Main.MouseWorld - player.Center) / new Vector2(Main.screenWidth * 0.4f, Main.screenHeight * 0.4f)).Length(), 1) * 2 - 1
			), player.Center);
			return null;
		}
		public override void SetDefaults() {
			SetSharedDefaults(Item, out Item.mana);
			Item.shoot = ModContent.ProjectileType<Sonorous_Shredder_Projectile>();
		}
	}
	public class Sonorous_Shredder_Projectile : ModProjectile {
		public const int frame_time = 5;
		public const int frame_count = 7;
		public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("ThoriumMod");
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Sonorous_Shredder_P";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = frame_count;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.GetGlobalNPC<Sonorous_Shredder_Tracker>().SonorousShredderHit()) {
				if (Projectile.owner == Main.myPlayer) {
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
			if (++Projectile.frameCounter >= frame_time) {
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
			const int HalfSpriteWidth = 50 / 2;

			int HalfProjWidth = Projectile.width / 2;

			// Vanilla configuration for "hitbox towards the end"
			if (Projectile.spriteDirection == 1) {
				DrawOriginOffsetX = -(HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = (int)-DrawOriginOffsetX * 2;
				DrawOriginOffsetY = 0;
			} else {
				DrawOriginOffsetX = (HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = 0;
				DrawOriginOffsetY = 0;
			}
		}
		public override Color? GetAlpha(Color lightColor) => Riven_Hive.GetGlowAlpha(lightColor);
	}
	public class Sonorous_Shredder_Explosion : ModProjectile, IIsExplodingProjectile {
		public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("ThoriumMod");
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Sonorous_Shredder_P";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item14);
				Projectile.ai[0] = 1;
			}
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding => true;
	}
	#endregion without thorium
	#region with thorium
	[ExtendsFromMod("ThoriumMod")]
	public class Sonorous_Shredder_Thorium : BardItem, IBardDamageClassOverride, ICustomWikiStat {
		public override string Name => base.Name[..^"_Thorium".Length];
		public DamageClass DamageType => DamageClasses.ExplosiveVersion[ThoriumDamageBase<BardDamage>.Instance];
		public override void SetStaticDefaults() {
			Empowerments.AddInfo<InvincibilityFrames>(2);
			Empowerments.AddInfo<FlightTime>(2);
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
			Sonorous_Shredder.ID = Type;
		}
		public override void SetBardDefaults() {
			Sonorous_Shredder.SetSharedDefaults(Item, out int cost);
			Item.shoot = ModContent.ProjectileType<Sonorous_Shredder_Projectile_Thorium>();
			InspirationCost = cost / 10;
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
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SoulofNight, 15)
			.AddIngredient<Alkahest>(25)
			.AddIngredient(OriginTile.TileItem<Spug_Flesh>(), 30)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public string CustomStatPath => WikiPageExporter.GetWikiName(this) + "_Thorium";
	}
	[ExtendsFromMod("ThoriumMod")]
	public class Sonorous_Shredder_Projectile_Thorium : BardProjectile, IBardDamageClassOverride {
		public override string Name => base.Name[..^"_Thorium".Length];
		public DamageClass DamageType => DamageClasses.ExplosiveVersion[ThoriumDamageBase<BardDamage>.Instance];
		public override BardInstrumentType InstrumentType => BardInstrumentType.String;
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Sonorous_Shredder_P";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = Sonorous_Shredder_Projectile.frame_count;
		}
		public override void SetBardDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
		}
		public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.GetGlobalNPC<Sonorous_Shredder_Tracker>().SonorousShredderHit()) {
				if (Projectile.owner == Main.myPlayer) {
					ref bool altEmpowerment = ref Main.LocalPlayer.GetModPlayer<OriginsThoriumPlayer>().altEmpowerment;
					altEmpowerment = true;
					try {
						ContentInstance<Sonorous_Shredder_Thorium>.Instance.NetApplyEmpowerments(Main.LocalPlayer, 0);
					} finally {
						altEmpowerment = false;
					}
					Projectile.NewProjectile(
						Projectile.GetSource_OnHit(target),
						Projectile.Center,
						default,
						ModContent.ProjectileType<Sonorous_Shredder_Explosion_Thorium>(),
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
			if (++Projectile.frameCounter >= Sonorous_Shredder_Projectile.frame_time) {
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
			const int HalfSpriteWidth = 50 / 2;

			int HalfProjWidth = Projectile.width / 2;

			// Vanilla configuration for "hitbox towards the end"
			if (Projectile.spriteDirection == 1) {
				DrawOriginOffsetX = -(HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = (int)-DrawOriginOffsetX * 2;
				DrawOriginOffsetY = 0;
			} else {
				DrawOriginOffsetX = (HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = 0;
				DrawOriginOffsetY = 0;
			}
		}
		public override Color? GetAlpha(Color lightColor) => Riven_Hive.GetGlowAlpha(lightColor);
	}
	[ExtendsFromMod("ThoriumMod")]
	public class Sonorous_Shredder_Explosion_Thorium : BardProjectile, IBardDamageClassOverride, IIsExplodingProjectile {
		public override string Name => base.Name[..^"_Thorium".Length];
		public DamageClass DamageType => DamageClasses.ExplosiveVersion[ThoriumDamageBase<BardDamage>.Instance];
		public override BardInstrumentType InstrumentType => BardInstrumentType.Percussion;
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Sonorous_Shredder_P";
		public override void SetBardDefaults() {
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound:SoundID.Item14);
				Projectile.ai[0] = 1;
			}
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding => true;
	}
	#endregion with thorium
	public class Sonorous_Shredder_Tracker : GlobalNPC {
		public override bool InstancePerEntity => true;
		int sonorousShredderHitCount = 0;
		int sonorousShredderHitTime = 0;
		public override void ResetEffects(NPC npc) {
			if (sonorousShredderHitTime > 0) {
				if (--sonorousShredderHitTime <= 0) {
					sonorousShredderHitCount = 0;
				}
			}
		}
		public bool SonorousShredderHit() {
			if (sonorousShredderHitCount < 4) {
				sonorousShredderHitCount++;
				sonorousShredderHitTime = 300;
				return false;
			} else {
				sonorousShredderHitCount = 0;
				sonorousShredderHitTime = 0;
				return true;
			}
		}
	}
}