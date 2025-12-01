using Origins.Dev;
using Origins.Items.Weapons.Ranged;
using Origins.Projectiles;
using PegasusLib;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Front)]
	public class Flak_Jacket : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override void SetStaticDefaults() {
			OriginsSets.Armor.Front.DrawsInNeckLayer[Item.frontSlot] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.damage = 30;
			Item.knockBack = 4f;
			Item.defense = 1;
			Item.shoot = ModContent.ProjectileType<Flak_Jacket_Explosion>();
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.endurance += (1 - player.endurance) * 0.05f;
			player.OriginPlayer().flakJacketItem = Item;
		}
		public override bool WeaponPrefix() => true;
	}
	public class Flak_Jacket_Explosion : ExplosionProjectile {
		public override DamageClass DamageType => DamageClasses.Explosive;
		public override int Size => 80;
		public override bool DealsSelfDamage => false;
		public override void SetStaticDefaults() {
			OriginsSets.Projectiles.NoMultishot[Type] = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				Flak_Jacket_Flak_1[] projectiles = Mod.GetContent<Flak_Jacket_Flak_1>().ToArray();
				int count = 5 + Main.rand.Next(4);
				float rot = MathHelper.TwoPi / count;
				for (int i = count; i > 0; i--) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center,
						GeometryUtils.Vec2FromPolar(14, rot * i + Main.rand.NextFloat(0.3f) * Main.rand.NextBool().ToDirectionInt()) + Main.rand.NextVector2Unit(),
						Main.rand.Next(projectiles).Type,
						Projectile.damage / 2,
						Projectile.knockBack * 0.25f,
						Projectile.owner
					);
				}
			}
			base.AI();
		}
	}
	public class Flak_Jacket_Flak_1 : ModProjectile {
		public override string Texture => typeof(Shardcannon_P1).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 10;
			Projectile.extraUpdates = 1;
			Projectile.aiStyle = 0;
			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.shimmerWet) {
				int num = (int)(Projectile.Center.X / 16f);
				int num2 = (int)(Projectile.position.Y / 16f);
				if (WorldGen.InWorld(num, num2) && Main.tile[num, num2] != null && Main.tile[num, num2].LiquidAmount == byte.MaxValue && Main.tile[num, num2].LiquidType == LiquidID.Shimmer && WorldGen.InWorld(num, num2 - 1) && Main.tile[num, num2 - 1] != null && Main.tile[num, num2 - 1].LiquidAmount > 0 && Main.tile[num, num2 - 1].LiquidType == LiquidID.Shimmer) {
					Projectile.Kill();
				} else if (Projectile.velocity.Y > 0f) {
					Projectile.velocity.Y *= -1f;
					Projectile.netUpdate = true;
					if (Projectile.timeLeft > 600)
						Projectile.timeLeft = 600;

					Projectile.timeLeft -= 60;
					Projectile.shimmerWet = false;
					Projectile.wet = false;
				}
			}
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
	}
	public class Flak_Jacket_Flak_2 : Flak_Jacket_Flak_1 {
		public override string Texture => typeof(Shardcannon_P2).GetDefaultTMLName();
	}
	public class Flak_Jacket_Flak_3 : Flak_Jacket_Flak_1 {
		public override string Texture => typeof(Shardcannon_P3).GetDefaultTMLName();
	}
}
