using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Impact_Grenade : ModItem {
        public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 43;
			/*Item.useTime = (int)(Item.useTime * 0.75);
			Item.useAnimation = (int)(Item.useAnimation * 0.75);*/
			Item.shoot = ModContent.ProjectileType<Impact_Grenade_P>();
			Item.shootSpeed *= 1.75f;
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(copper: 35);
			Item.rare = ItemRarityID.Green;
            //Item.ArmorPenetration += 3;
        }
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.Grenade, 8)
			.AddIngredient(ModContent.ItemType<Peat_Moss_Item>())
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Impact_Grenade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Impact_Grenade";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Hand_Grenade_Launcher.AltFireAction[Type] = (player, source, position, velocity, type, damage, knockback) => {
				type = ModContent.ProjectileType<Impact_Grenade_Blast>();
				position += velocity.SafeNormalize(Vector2.Zero) * 40;
				damage *= 2;
				knockback *= 3; Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			};
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 135;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.Kill();
			return false;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
		public class Impact_Grenade_Blast : ModProjectile {

			public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2ExplosiveTrapT1Explosion;
			protected override bool CloneNewInstances => true;
			float dist;

			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.Grenade);
				Projectile.aiStyle = 0;
				Projectile.timeLeft = 8;
				Projectile.width = Projectile.height = 5;
				Projectile.penetrate = -1;
				Projectile.tileCollide = false;
				if (Main.netMode != NetmodeID.Server && !TextureAssets.Projectile[694].IsLoaded) {
					Main.instance.LoadProjectile(694);
				}
			}
			public override void AI() {
				Player player = Main.player[Projectile.owner];
				Vector2 unit = Projectile.velocity.SafeNormalize(Vector2.Zero);
				Projectile.Center = player.MountedCenter + unit * 36 + unit.RotatedBy(MathHelper.PiOver2 * player.direction) * -2;
				Projectile.rotation = Projectile.velocity.ToRotation();
				if (Projectile.soundDelay <= 0) {
					SoundEngine.PlaySound(SoundID.Item14.WithPitchRange(1, 1), Projectile.Center);
					Projectile.soundDelay = Projectile.timeLeft * 20;
				}
			}
			public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
				Vector2 closest = (Projectile.Center + Projectile.velocity * 2).Clamp(targetHitbox.TopLeft(), targetHitbox.BottomRight());
				double rot = GeometryUtils.AngleDif((closest - Projectile.Center).ToRotation(), Projectile.rotation, out _) + 0.5f;
				dist = (float)((Projectile.Center - closest).Length() * rot / 5.5f) + 1;
				return (Projectile.Center - closest).Length() <= 48 / rot;
			}
			public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
				modifiers.SourceDamage /= dist;
			}
			public override bool PreDraw(ref Color lightColor) {
				int frame = (8 - Projectile.timeLeft) / 2;
				Main.EntitySpriteDraw(TextureAssets.Projectile[694].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 80 * frame, 80, 80), lightColor, Projectile.rotation + MathHelper.PiOver2, new Vector2(40, 80), 1f, SpriteEffects.None, 0);
				return false;
			}
		}
	}
}
