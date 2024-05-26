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
using Origins.Items.Weapons.Ammo.Canisters;
using Microsoft.Xna.Framework.Graphics;
namespace Origins.Items.Weapons.Demolitionist {
	public class Thermite_Launcher : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"HardmodeLauncher",
			"CanistahUser"
		};
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GrenadeLauncher);
			Item.damage = 45;
			Item.width = 44;
			Item.height = 18;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.shoot = ModContent.ProjectileType<Thermite_Canister_P>();
			Item.useAmmo = ModContent.ItemType<Resizable_Mine_One>();
			Item.knockBack = 2f;
			Item.shootSpeed = 16f;
			Item.autoReuse = false;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.ArmorPenetration += 10;
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
	public class Thermite_Canister_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Canister_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Canister_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 900;
			Projectile.scale = 0.85f;
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
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
		}
		public override void AI() {
			Projectile.velocity.Y += 0.2f;
			Projectile.rotation += Projectile.velocity.X * 0.1f;
			int auraProjIndex = (int)Projectile.ai[1] - 1;
			if (auraProjIndex < 0) {
				if (Projectile.owner == Main.myPlayer) Projectile.ai[1] = Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.position,
					default,
					Thermite_P.ID,
					Projectile.damage / 2,
					0,
					Projectile.owner,
					Projectile.whoAmI
				) + 1;
			} else {
				Projectile auraProj = Main.projectile[auraProjIndex];
				if (auraProj.active && auraProj.type == Thermite_P.ID) {
					auraProj.Center = Projectile.Center;
					auraProj.rotation = Projectile.rotation;
				} else {
					Projectile.ai[1] = 0;
				}
			}
		}
	}
	public class Thermite_P : ModProjectile, ICanisterChildProjectile, IIsExplodingProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.friendly = true;
			Projectile.width = 36;
			Projectile.height = 36;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 25;
			Projectile.timeLeft = 3600;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 5;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, 1, 0.75f, 0);
			int auraProj = (int)Projectile.ai[0];
			if (auraProj < 0) {
				Projectile.scale *= 0.85f;
				Projectile.scale -= 0.15f;
				if (Projectile.scale <= 0) Projectile.Kill();
			} else {
				Projectile ownerProj = Main.projectile[auraProj];
				if (ownerProj.active && ownerProj.type == Thermite_Canister_P.ID) {
					Projectile.scale = ownerProj.scale * 2;
					Projectile.Center = ownerProj.Center;
					Projectile.rotation = ownerProj.rotation;
				} else {
					Projectile.Center = ownerProj.Center;
					Projectile.ai[0] = -1;
				}
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float range = projHitbox.Width * Projectile.scale * 0.5f;
			return Projectile.Center.DistanceSQ(Projectile.Center.Clamp(targetHitbox)) < range * range;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire, Main.rand.Next(300, 451));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.OnFire, Main.rand.Next(300, 451));
		}
		public override Color? GetAlpha(Color lightColor) {
			return new Color(255, 180, 50, 0);
		}
		public bool IsExploding() => true;
	}
}
