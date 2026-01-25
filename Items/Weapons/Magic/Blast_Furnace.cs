using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.UI;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.Items.Weapons.Magic {
	public class Blast_Furnace : ModItem, ICustomWikiStat {
		public const int max_charges = 5;
		public string[] Categories => [
			WikiCategories.SpellBook
		];
		public override void SetStaticDefaults() {
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.DamageType = DamageClass.Magic;
			Item.damage = 64;
			Item.noMelee = true;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 70;
			Item.useAnimation = 70;
			Item.shoot = ModContent.ProjectileType<Blast_Furnace_Charge>();
			Item.shootSpeed = 8f;
			Item.mana = 7;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item82;
			Item.autoReuse = true;
			Item.channel = true;
		}
		public override bool AltFunctionUse(Player player) => player.OriginPlayer().blastFurnaceCharges < max_charges;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse != 2) type = ModContent.ProjectileType<Blast_Furnace_P>();
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) => damage *= Utils.Remap(player.OriginPlayer().blastFurnaceCharges, 0, max_charges, 1, 2);
		public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) => knockback *= Utils.Remap(player.OriginPlayer().blastFurnaceCharges, 0, max_charges, 1, 5);
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) return base.Shoot(player, source, position, velocity, type, damage, knockback);
			ref int blastFurnaceCharges = ref player.OriginPlayer().blastFurnaceCharges;
			for (int i = 3 + (blastFurnaceCharges + 1) / 3; i > 0; i--) {
				Projectile.NewProjectile(
					source,
					position,
					velocity.RotatedByRandom(0.25f),
					type,
					damage,
					knockback,
					ai1: blastFurnaceCharges
				);
			}
			if (blastFurnaceCharges > 0) blastFurnaceCharges--;
			return false;
		}
	}
	public class Blast_Furnace_UI : SwitchableUIState {
		public override void AddToList() => OriginSystem.Instance.ItemUseHUD.AddState(this);
		public override bool IsActive() => Main.LocalPlayer.HeldItem.ModItem is Blast_Furnace;
		public override InterfaceScaleType ScaleType => InterfaceScaleType.Game;
		readonly AutoLoadingAsset<Texture2D> texture = typeof(Blast_Furnace_UI).GetDefaultTMLName();
		public override void Draw(SpriteBatch spriteBatch) {
			Rectangle frame = texture.Frame(verticalFrames: Blast_Furnace.max_charges + 1, frameY: Main.LocalPlayer.OriginPlayer().blastFurnaceCharges);
			spriteBatch.Draw(
				texture,
				Main.LocalPlayer.Top.Floor() - Vector2.UnitY * (12 - Main.LocalPlayer.gfxOffY) - frame.Size() * new Vector2(0.5f, 1f) - Main.screenPosition,
				frame,
				Color.White
			);
		}
	}
	public class Blast_Furnace_Charge : ModProjectile {
		public static float ChargeTimeMultiplier => 0.175f;
		public override string Texture => typeof(Blast_Furnace).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse { Player: Player player }) Projectile.ai[0] = player.itemTimeMax * ChargeTimeMultiplier;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (!Projectile.TryGetOwner(out Player player)) {
				Projectile.Kill();
				return;
			}
			Projectile.position = player.MountedCenter;
			if (!player.channel) {
				Projectile.Kill();
				return;
			}
			player.SetDummyItemTime(5);
			if (--Projectile.ai[0] <= 0) {
				if (player.HeldItem?.ModItem?.AltFunctionUse(player) != true) {
					Projectile.Kill();
					return;
				}
				player.OriginPlayer().blastFurnaceCharges++;
				Projectile.ai[0] += CombinedHooks.TotalUseTime(player.HeldItem.useTime, player, player.HeldItem) * ChargeTimeMultiplier;
			}
		}
	}
	public class Blast_Furnace_P : ModProjectile {
		public float Lifetime => 40f + Projectile.ai[1];
		public float MinSize => 6f + Projectile.ai[1];
		public float MaxSize => 60f + Projectile.ai[1] * 5;
		private readonly float[] sizes = new float[21];
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = sizes.Length;
		}
		float Size => float.Lerp(MinSize, MaxSize, float.Pow(Utils.GetLerpValue(0f, Lifetime, Projectile.ai[0]), 0.8f));
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
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
		public override void AI() {
			float brightnessMult = 1;
			if (Projectile.ai[2] != 0) brightnessMult = Utils.GetLerpValue(0f, Lifetime, Projectile.ai[0]);
			Lighting.AddLight(Projectile.Center, 0.85f * brightnessMult, 0.4f * brightnessMult, 0f);
			if (Projectile.ai[2] != 0) {
				Projectile.velocity = default;
				if (--Projectile.ai[0] < 0) {
					Projectile.Kill();
				}
			} else {
				if (++Projectile.ai[0] > Lifetime) {
					Projectile.ai[2] = 1;
				}
			}
			//Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FrostStaff);

			for (int i = sizes.Length - 1; i > 0; i--) {
				sizes[i] = sizes[i - 1];
			}
			sizes[0] = Size;
			Projectile.alpha = (int)(200 * (1 - (Projectile.ai[0] / Lifetime)));
			Projectile.rotation += 0.3f * Projectile.direction;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			int scale = (int)(Size / 2) - hitbox.Width;
			hitbox.Inflate(scale, scale);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire, 240);
		}
		public override bool PreDraw(ref Color lightColor) {
			float progress = (Projectile.ai[0] / Lifetime);
			Flamethrower_Drawer.Draw(Projectile, float.Pow(1 - progress, 2f), TextureAssets.Projectile[Type].Value, Color.Black, sizes, brightnessColorExponent: 1.75f, smokeAmount: 0, sizeProgressOverride: _ => progress * 0.5f);
			return false;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.ai[2] = 2;
			return false;
		}
	}
}
