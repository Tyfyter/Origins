using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	public class Welding_Torch : ModItem {
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.consumeAmmoOnFirstShotOnly = true;
			Item.useAmmo = AmmoID.Gel;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.autoReuse = true;
			Item.knockBack = 0.425f;
			Item.noMelee = true;

			Item.DamageType = DamageClasses.RangedSummon;
			Item.damage = 11;
			Item.useAnimation = 20;
			Item.useTime = 5;
			Item.width = 36;
			Item.height = 16;
			Item.shoot = ModContent.ProjectileType<Welding_Torch_P>();
			Item.shootSpeed = 4f;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.ArmorPenetration = 25;
			Item.UseSound = SoundID.Item34.WithPitch(0.5f).WithVolume(0.75f);
		}
		public override Vector2? HoldoutOffset() => new Vector2(2, 0);
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.ItemAnimationEndingOrEnded) player.altFunctionUse = 0;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 unit = velocity.Normalized(out _);
			position += unit * 16 + 12 * new Vector2(unit.Y, -unit.X) * player.direction;
		}
		public override bool AltFunctionUse(Player player) {
			if (player.controlUseTile) player.controlUseItem = true;
			return true;
		}
	}
	public class Welding_Torch_P : ModProjectile {
		AutoLoadingTexture altTexture = typeof(Welding_Torch_P).GetDefaultTMLName("2");
		public static float Lifetime => 60f;
		public static float MinSize => 30f;
		public static float MaxSize => 6f;
		private readonly float[] sizes = new float[21];
		protected Welding_Torch_P() : base() {
			healCooldown ??= new int[Main.maxProjectiles];
		}
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 7;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 21;
			OriginsSets.Projectiles.FireProjectiles[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.RangedSummon;
			Projectile.width = Projectile.height = 6;
			Projectile.penetrate = 4;
			Projectile.friendly = true;
			Projectile.alpha = 255;
			Projectile.extraUpdates = 3;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			for (int i = 0; i < Projectile.oldPos.Length; i++)
				Projectile.oldRot[i] = Main.rand.NextFloatDirection();
		}
		float Size => Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize, MaxSize);
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent { Entity: Player player }) Projectile.ai[1] = player.altFunctionUse;
		}
		public override void AI() {
			if (Projectile.ai[1] != 0) {
				Lighting.AddLight(Projectile.Center, 0.1f, 0f, 0.85f);
			} else {
				Lighting.AddLight(Projectile.Center, 0.85f, 0.4f, 0f);
			}
			Projectile.ai[0]++;
			if (Projectile.velocity == default) Projectile.ai[0]++;
			for (int i = sizes.Length - 1; i > 0; i--) {
				sizes[i] = sizes[i - 1];
			}
			sizes[0] = Size;
			Projectile.scale = Utils.Remap(Projectile.ai[0], 0f, Lifetime, MinSize / 96f, MaxSize / 96f);
			Projectile.alpha = (int)(200 * (1 - (Projectile.ai[0] / Lifetime)));
			Projectile.rotation += 0.3f * Projectile.direction;
			Projectile.velocity *= 0.97f;
			if (Projectile.ai[0] > Lifetime) Projectile.Kill();
			for (int i = 0; i < healCooldown.Length; i++) {
				if (healCooldown[i] > 0) healCooldown[i]--;
			}
			Rectangle hitbox = Projectile.Hitbox;
			ProjectileLoader.ModifyDamageHitbox(Projectile, ref hitbox);
			DoHealing(hitbox);
		}
		public virtual void DoHealing(Rectangle hitbox) {
			bool doSound = false;
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (healCooldown[other.whoAmI] > 0) continue;
				if (Projectile.Colliding(hitbox, other.Hitbox) && other.ModProjectile is IArtifactMinion artifactMinion && artifactMinion.Life < artifactMinion.MaxLife) {
					float oldHealth = artifactMinion.Life;
					artifactMinion.Life += Projectile.damage * 0.15f + 0.004f * artifactMinion.MaxLife;
					if (artifactMinion.Life > artifactMinion.MaxLife) artifactMinion.Life = artifactMinion.MaxLife;
					CombatText.NewText(other.Hitbox, CombatText.HealLife, (int)Math.Round(artifactMinion.Life - oldHealth), true, dot: true);
					healCooldown[other.whoAmI] = 20;
					if (Projectile.ai[1] == 2 && other.owner == Projectile.owner) other.GetGlobalProjectile<ArtifactMinionGlobalProjectile>().stayStillSoICanHealYouTime = 6;
					doSound = true;
				}
			}
			if (doSound) {
				Player player = Main.player[Projectile.owner];
				OriginPlayer originPlayer = player.OriginPlayer();
				originPlayer.weldingTorchSound.PlaySoundIfInactive(Origins.Sounds.WeldingTorch, player.MountedCenter, sound => {
					sound.Position = player.MountedCenter;
					return true;
				});
				originPlayer.weldingTorchSoundTime = 10;
			}
		}
		protected int[] healCooldown;
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			int scale = (int)((Size - hitbox.Width) / 2);
			hitbox.Inflate(scale, scale);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, hit.Crit ? 360 : 180);
		}
		public override bool PreDraw(ref Color lightColor) {
			bool healing = Projectile.ai[1] != 0;
			float progress = (Projectile.ai[0] / Lifetime);
			Flamethrower_Drawer.Draw(
				Projectile,
				float.Pow(1 - progress, 0.5f),
				healing ? altTexture : TextureAssets.Projectile[Type].Value,
				Color.Black,
				sizes,
				brightnessColorExponent: 1.75f,
				smokeAmount: 0,
				sizeProgressOverride: _ => progress * 0.5f
			);
			return false;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = Vector2.Zero;
			return false;
		}
	}
}
