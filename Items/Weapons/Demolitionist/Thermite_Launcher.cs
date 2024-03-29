using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
    public class Thermite_Launcher : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "Launcher",
			"CanistahUser"
        };
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GrenadeLauncher);
			Item.damage = 40;
			Item.width = 44;
			Item.height = 18;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.shoot = ModContent.ProjectileType<Napalm_Canister_P>();
            Item.useAmmo = ModContent.ItemType<Resizable_Mine_One>();
            Item.knockBack = 2f;
			Item.shootSpeed = 12f;
			Item.autoReuse = false;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.ArmorPenetration += 3;
		}
		//can't just chain rules since OneFromOptionsNotScaledWithLuckDropRule drops all the items directly
		//but that's fine since other bosses that drop a ranged weapon don't show the ammo in the bestiary
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemOpen or EntitySource_Loot) {
				Main.timeItemSlotCannotBeReusedFor[Item.whoAmI] = 1;
				int index = Item.NewItem(source, Item.position, ModContent.ItemType<Napalm_Canister>(), Main.rand.Next(60, 100));
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
				}
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			CanisterGlobalItem.ItemToCanisterID.TryGetValue(source.AmmoItemIdUsed, out type);
			Projectile.NewProjectile(
				source,
				position,
				velocity,
				Item.shoot,
				damage,
				knockback,
				player.whoAmI,
				ai2: type
			);
			return false;
		}
	}
	public class Napalm_Canister_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ammo/Napalm_Canister";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 900;
			Projectile.ArmorPenetration += 3;
            Projectile.scale = 0.5f;
            //projectile.aiStyle = 14;
            //projectile.usesLocalNPCImmunity = true;
            //projectile.localNPCHitCooldown = 7;
        }
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X == 0f) {
				Projectile.velocity.X = -oldVelocity.X;
			}
			if (Projectile.velocity.Y == 0f) {
				Projectile.velocity.Y = -oldVelocity.Y;
			}
			Projectile.timeLeft = 1;
			return true;
		}
		public override void OnKill(int timeLeft) {//TODO: make actually different projectile
			Projectile.damage = (int)(Projectile.damage * 0.75f);
			Projectile.knockBack = 16f;
			Projectile.position = Projectile.Center;
			Projectile.width = Projectile.height = 52;
			Projectile.Center = Projectile.position;
			Projectile.Damage();
            ExplosiveGlobalProjectile.DealSelfDamage(Projectile);
			if (Projectile.ai[2] == CanisterGlobalItem.GetCanisterType(typeof(Napalm_Canister))) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 0, 0, Projectile.owner, -1, 1);
			} else {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
			}
			for (int i = 0; i < 5; i++) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.velocity / 2) + Vec2FromPolar((i / Main.rand.NextFloat(5, 7)) * MathHelper.TwoPi, Main.rand.NextFloat(2, 4)), ModContent.ProjectileType<Thermite_P>(), (int)(Projectile.damage * 0.65f), 0, Projectile.owner);
			}
		}
		public override void AI() {
			Dust.NewDust(Projectile.Center, 0, 0, DustID.Torch);
		}
	}
	public class Thermite_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Ammo/Napalm_Pellet_P";
		
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.friendly = true;
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.aiStyle = 1;
			Projectile.penetrate = 25;
			Projectile.timeLeft = Main.rand.Next(300, 451);
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 15;
		}
		public override void AI() {
			float v = 0.75f + (float)(0.125f * (Math.Sin(Projectile.timeLeft / 5f) + 2 * Math.Sin(Projectile.timeLeft / 60f)));
			Lighting.AddLight(Projectile.Center, v, v * 0.5f, 0);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = height = 2;
			fallThrough = true;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.ai[0] == 0f) {
				Projectile.ai[0] = 1f;
				Projectile.aiStyle = 0;
				//Projectile.tileCollide = false;
				//Projectile.position+=Vector2.Normalize(oldVelocity)*2;
			}
			Projectile.velocity *= 0.9f;
			//Projectile.velocity = Vector2.Zero;
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire, Main.rand.Next(300, 451));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.OnFire, Main.rand.Next(300, 451));
		}
		public override Color? GetAlpha(Color lightColor) {
			int v = 200 + (int)(25 * (Math.Sin(Projectile.timeLeft / 5f) + Math.Sin(Projectile.timeLeft / 60f)));
			return new Color(v + 20, v + 25, v - 150, 0);
		}
	}
}
