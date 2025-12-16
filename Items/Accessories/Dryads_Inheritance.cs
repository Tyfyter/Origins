using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Layers;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Dryads_Inheritance : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.GenericBoostAcc
		];
        static short glowmask;
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<Neck_Glow_Layer>(Item.neckSlot, Texture + "_Neck_Glow");
        }
        public override void SetDefaults() {
			Item.DefaultToAccessory(26, 26);
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Lime;
			Item.expert = true;
			Item.shoot = ModContent.ProjectileType<Dryad_Ward>();
			Item.accessory = true;
            Item.glowMask = glowmask;
        }
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SporeSac)
			.AddIngredient(ModContent.ItemType<Last_Descendant>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) *= 1.05f;
			player.longInvince = true;
			player.SporeSac(Item);
			player.sporeSac = true;
			player.starCloakItem = Item;
			player.starCloakItem_starVeilOverrideItem = Item;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.guardedHeart = true;
			originPlayer.dryadNecklace = true;
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[Item.shoot] <= 0) {
				Projectile.NewProjectile(
					player.GetSource_Accessory(Item),
					player.Center,
					default,
					Item.shoot,
					0,
					0,
					player.whoAmI
				);
			}
		}
	}
	public class Dryad_Ward : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DryadsWardCircle;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.DryadsWardCircle);
			Projectile.aiStyle = 0;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (owner.GetModPlayer<OriginPlayer>().dryadNecklace) {
				Projectile.Center = owner.MountedCenter;
				if (owner.immuneTime > 0) Projectile.ai[0] += 0.5f + owner.immuneTime / 30f;
				Projectile.timeLeft = 5;
				AI_111_DryadsWard();
			}
		}
		void AI_111_DryadsWard() {
			if (Projectile.ai[0] < 100f) {
				Projectile.ai[0] = Math.Min(Projectile.ai[0] + 1.5f, 100f);
			} else if (Projectile.ai[0] > 100f) {
				Projectile.ai[1] += (Projectile.ai[0] - 100f) * 0.0005f;
				Projectile.ai[1] %= MathHelper.TwoPi;
				Projectile.ai[0] = Math.Max(Projectile.ai[0] - 1f, 100f);
			}
			Projectile.rotation += MathHelper.Pi / 300f;
			Projectile.scale = Projectile.ai[0] / 100f;
			if (Projectile.scale > 1f) {
				Projectile.scale = 1f;
			}
			Projectile.alpha = (int)(255f * (1f - Projectile.scale));
			float radius = Projectile.ai[0] * 2f;
			if (Projectile.ai[0] >= 30f) {
				if (Main.netMode != NetmodeID.Server) {
					Player player = Main.player[Main.myPlayer];
					if (player.active && !player.dead && Projectile.Distance(player.Center) <= radius && player.FindBuffIndex(BuffID.DryadsWard) == -1) {
						player.AddBuff(BuffID.DryadsWard, 120);
					}
				}
				if (Projectile.ai[2] % 10f == 0f && Main.netMode != NetmodeID.MultiplayerClient) {
					for (int i = 0; i < 200; i++) {
						NPC npc = Main.npc[i];
						if (AppliesToEnemy(npc, radius)) {
							npc.AddBuff(BuffID.DryadsWardDebuff, 120);
						}
					}
				}
			}
			if (++Projectile.frameCounter >= 6) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= 5) {
					Projectile.frame = 0;
				}
			}
			if (++Projectile.ai[2] >= 10) {
				Projectile.ai[2] = 0;
			}
		}
		bool AppliesToEnemy(NPC npc, float radius) {
			if (OriginsSets.NPCs.TargetDummies[npc.type] || !npc.active) return false;
			if (npc.friendly || npc.lifeMax <= 5 || npc.dontTakeDamage) return false;
			if (Projectile.Distance(Projectile.Center.Clamp(npc.Hitbox)) >= radius) return false;
			int buffIndex = npc.FindBuffIndex(BuffID.DryadsWardDebuff);
			if (buffIndex != -1 && npc.buffTime[buffIndex] > 20) return false;
			if (!npc.dryadBane && !Collision.CanHit(Projectile.Center, 1, 1, npc.position, npc.width, npc.height)) return false;
			return true;
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.ai[2] == 1f) {
				Color value = new(lightColor.R / 2, 0, lightColor.G);
				float amount = (float)Math.Sin(Projectile.ai[0] % 120f * ((float)Math.PI * 2f) / 120f) * 0.5f + 0.5f;
				lightColor = Color.Lerp(lightColor, value, amount);
				return Color.Lerp(lightColor, Color.Lerp(Color.White, value, amount), 0.75f);
			}

			return Color.Lerp(lightColor, Color.White, 0.75f);
		}
		public override bool PreDraw(ref Color lightColor) {
			float radius = Projectile.ai[0] * 2f;
			float hurtDiff = Projectile.ai[0] - 100;
			float centerRad = 0.4f + hurtDiff * 0.003f;

			float baseRotation = Projectile.rotation;
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			int num239 = Projectile.frame;
			Vector2 offset = new(0f, radius);
			for (int i = 0; i < 10f; i++) {
				Rectangle frame = texture.Frame(1, 5, 0, (num239 + i) % 5);
				float rotation = baseRotation + Projectile.ai[1] + MathHelper.Pi / 5f * i;
				Vector2 position = offset.RotatedBy(rotation) * centerRad + Projectile.Center;
				Color alpha = Projectile.GetAlpha(Lighting.GetColor(position.ToTileCoordinates()));
				alpha.A /= 2;
				Main.EntitySpriteDraw(texture, position - Main.screenPosition, frame, alpha, rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
			}

			for (int i = 0; i < 20f; i++) {
				Rectangle frame = texture.Frame(1, 5, 0, (num239 + i) % 5);
				float rotation = -baseRotation + MathHelper.Pi / 10f * i;
				rotation *= 2f;
				Vector2 position = offset.RotatedBy(rotation) + Projectile.Center;
				Color alpha = Projectile.GetAlpha(Lighting.GetColor(position.ToTileCoordinates()));
				alpha.A /= 2;
				Main.EntitySpriteDraw(texture, position - Main.screenPosition, frame, alpha, rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
			}
			return false;
		}
	}
}
