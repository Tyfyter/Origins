using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.NPCs;
using Origins.World.BiomeData;
using PegasusLib.Sets;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	[LegacyName("Riven_Dungeon_Chest_Placeholder_Item")]
	public class Plasma_Cutter : ModItem, ICustomWikiStat, ITornSource {
		public static float TornSeverity => 0.15f;
		float ITornSource.Severity => TornSeverity;
		public const int baseDamage = 64;
		public string[] Categories => [
			"MagicGun"
		];
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.damage = 60;
			Item.DamageType = DamageClass.Magic;
			Item.shoot = ModContent.ProjectileType<Plasma_Cutter_P>();
			Item.knockBack = 6;
			Item.shootSpeed = 9f;
			Item.mana = 28;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.channel = false;
			Item.noUseGraphic = false;
			Item.autoReuse = true;
			Item.reuseDelay = 0;
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(gold: 20);
			Item.UseSound = Origins.Sounds.EnergyRipple.WithPitch(0.5f);
		}
		public override Vector2? HoldoutOffset() => Vector2.Zero;
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Item36.WithPitch(0.5f), player.itemLocation);
			return null;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Vector2 perp = velocity.SafeNormalize(default);
			position += perp * 48;
			perp = new(-perp.Y, perp.X);
			for (int i = -1; i <= 1; i++) {
				Projectile.NewProjectile(
					source,
					position + perp * i * 8,
					velocity,
					type,
					damage,
					knockback,
					player.whoAmI
				);
			}
			return false;
		}
	}
	public class Plasma_Cutter_P : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Daybreak);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = Projectile.height = 10;
			Projectile.extraUpdates = 12;
			Projectile.aiStyle = 0;
			Projectile.alpha = 0;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
			Projectile.ArmorPenetration = 2;
			Projectile.hide = false;
			Projectile.frame = Main.rand.Next(4);
		}
		public override void AI() {
			Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Teleporter, 0, 0, 0, new Color(100, 255, 000), 1f);
			dust.velocity *= 0.25f;
			dust.noGravity = true;
			if (Projectile.numUpdates == -1) {
				Projectile.rotation = Projectile.velocity.ToRotation();
				if (++Projectile.frameCounter >= 6) {
					Projectile.frameCounter = 0;
					if (++Projectile.frame >= 4) Projectile.frame = 0;
				}
			}
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item167, Projectile.position);

			Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Teleporter, 0, 0, 255, new Color(100, 255, 000), 1f);
			dust.noGravity = false;
			//dust.velocity *= 8f;

			dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Firework_Yellow, 0, 0, 255, new Color(210, 255, 160));
			dust.noGravity = false;
			//dust.velocity *= 8f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 300, 180, Plasma_Cutter.TornSeverity, Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			float tornCurrentSeverity = target.GetGlobalNPC<OriginGlobalNPC>().tornCurrentSeverity;
			if (tornCurrentSeverity > 0) {
				modifiers.FinalDamage /= 1 - tornCurrentSeverity;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Color color = Riven_Hive.GetGlowAlpha(lightColor);
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame),
				color,
				Projectile.rotation,
				new(21, 5),
				Projectile.scale,
				SpriteEffects.None
			);
			return false;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 6;
			height = 6;
			fallThrough = true;
			return true;
		}
	}
}
