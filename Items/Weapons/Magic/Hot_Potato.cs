using Microsoft.Xna.Framework;
using Origins.Items.Other.Consumables.Food;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Tyfyter.Utils;
using PegasusLib;
namespace Origins.Items.Weapons.Magic {
	public class Hot_Potato : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Snowball);
			Item.maxStack = 1;
			Item.damage = 16;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.consumable = true;
			Item.autoReuse = true;
			Item.mana = 8;
			Item.ammo = AmmoID.None;
			Item.shoot = ModContent.ProjectileType<Hot_Potato_P>();
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Orange;
		}
		public override bool ConsumeItem(Player player) => false;
		public override bool? CanBeChosenAsAmmo(Item weapon, Player player) {
			return weapon.useAmmo == ModContent.ItemType<Potato>() && player.CheckMana(Item, pay: false);
		}
		public override bool CanBeConsumedAsAmmo(Item weapon, Player player) {
			player.CheckMana(Item, pay: true);
			player.manaRegenDelay = (int)player.maxRegenDelay;
			return false;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.FallenStar)
			.AddIngredient(ItemID.HellstoneBar, 8)
			.AddIngredient(ModContent.ItemType<Potato>())
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Hot_Potato_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = 25;
			Projectile.hide = false;
			Projectile.alpha = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 0;
			Projectile.scale = 1f;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.localAI[0] = -1;
			Projectile.localAI[1] = -1;
			Projectile.localAI[2] = Projectile.velocity.Length();
		}
		public override bool? CanHitNPC(NPC target) {
			if (target.whoAmI == (int)Projectile.localAI[0] || target.whoAmI == (int)Projectile.localAI[1]) return false;
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			float maxDist = 256 * 256;
			Vector2 nextDirection = default;
			bool foundNextTarget = false;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC currentTarget = Main.npc[i];
				if (i == target.whoAmI || i == (int)Projectile.localAI[0] || !currentTarget.CanBeChasedBy()) continue;

				Vector2 currentDiff = currentTarget.Center - Projectile.Center;
				float currentDist = currentDiff.LengthSquared();

				if (currentDist < maxDist) {
					maxDist = currentDist;
					nextDirection = currentDiff;
					foundNextTarget = true;
				}
			}
			if (foundNextTarget) {
				bool goHigh = Main.rand.NextBool(7);
				if (GeometryUtils.AngleToTarget(nextDirection, Projectile.localAI[2], 0.06f, high: goHigh) is float angle) {
					Projectile.velocity = GeometryUtils.Vec2FromPolar(Projectile.localAI[2], angle);
					Rectangle rect = new(0, 0, Projectile.width, Projectile.height);
					Vector2 pos = Projectile.position;
					Vector2 velocity = Projectile.velocity;
					Vector2 targetPos = Projectile.Center + nextDirection;
					for (int i = 0; i < 240; i++) {
						pos += velocity;
						if (pos.DistanceSQ(targetPos) < Projectile.localAI[2] * Projectile.localAI[2] * 4) {
							break;
						}
						rect.X = (int)pos.X;
						rect.Y = (int)pos.Y;
						if (rect.OverlapsAnyTiles()) {
							float? mangle = GeometryUtils.AngleToTarget(nextDirection, Projectile.localAI[2], 0.06f, high: !goHigh);
							if (mangle.HasValue) {
								Projectile.velocity = GeometryUtils.Vec2FromPolar(Projectile.localAI[2], mangle.Value);
							}
							break;
						}
						velocity.Y += 0.06f;
					}
				}
			}
			Projectile.damage += 2;
			Projectile.localAI[1] = Projectile.localAI[0];
			Projectile.localAI[0] = target.whoAmI;

			target.AddBuff(BuffID.OnFire, 180);
		}
		public override void AI() {
			if (Main.rand.NextBool(3)) Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
			Projectile.velocity.Y += 0.06f;
		}
	}
}
