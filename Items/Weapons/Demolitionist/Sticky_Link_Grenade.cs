using Origins.Dev;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Sticky_Link_Grenade : ModItem {
        public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 55;
			Item.useTime = (int)(Item.useTime * 0.5);
			Item.useAnimation = (int)(Item.useAnimation * 0.5);
			Item.shoot = ModContent.ProjectileType<Sticky_Link_Grenade_P>();
			Item.shootSpeed *= 1.25f;
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(copper: 35);
			Item.rare = ItemRarityID.Blue;
        }
		public override void AddRecipes() {
			Recipe.Create(Type, 5)
			.AddIngredient(ItemID.Gel, 1)
			.AddIngredient<Link_Grenade>(5)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Sticky_Link_Grenade_P : Link_Grenade_P {
		public override string Texture => typeof(Sticky_Link_Grenade).GetDefaultTMLName();
		Vector2 stickPos = default;
		float stickRot = 0;
		public override void AI() {
			if (Projectile.ai[2] == 0) {
				Rectangle hitbox = Projectile.Hitbox;
				foreach (NPC npc in Main.ActiveNPCs) {
					if (!npc.friendly && hitbox.Intersects(npc.Hitbox)) {
						Projectile.ai[2] = npc.whoAmI + 1;
						stickPos = (Projectile.Center - npc.Center).RotatedBy(-npc.rotation);
						stickRot = Projectile.rotation - npc.rotation;
						Projectile.netUpdate = true;
						break;
					}
				}
			} else if (Projectile.ai[2] == -1) {
				Projectile.Center = stickPos;
				Projectile.rotation = stickRot;
				Projectile.velocity.X = 0;
				Projectile.velocity.Y = 0;
			} else {
				NPC npc = Main.npc[(int)Projectile.ai[2] - 1];
				if (npc.active) {
					Projectile.Center = npc.Center + stickPos.RotatedBy(npc.rotation);
					Projectile.rotation = stickRot + npc.rotation;
					Projectile.velocity.X = 0;
					Projectile.velocity.Y = 0;
				} else {
					Projectile.ai[2] = 0;
				}
			}
			if (Projectile.timeLeft <= 3) return;
			Vector2 center = Projectile.Center;
			for (int i = 0; i < ExplosiveGlobalProjectile.explodingProjectiles.Count; i++) {
				if (ExplosiveGlobalProjectile.explodingProjectiles[i].IsWithin(center, 16 * 12)) {
					Projectile.timeLeft = 3;
					break;
				}
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.timeLeft == 0 && !Projectile.IsNPCIndexImmuneToProjectileType(Type, target.whoAmI)) return false;
			return null;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.ai[2] = -1;
			stickPos = Projectile.Center;
			stickRot = Projectile.rotation;
			Projectile.netUpdate = true;
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(stickPos.X);
			writer.Write(stickPos.Y);
			writer.Write(stickRot);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			stickPos.X = reader.ReadSingle();
			stickPos.Y = reader.ReadSingle();
			stickRot = reader.ReadSingle();
		}
	}
}
