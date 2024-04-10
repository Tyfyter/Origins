using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
    public class Chlorodynamite : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "ThrownExplosive",
			"IsDynamite",
            "SpendableWeapon"
        };
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Dynamite);
			Item.damage = 186;
			Item.shoot = ModContent.ProjectileType<Chlorodynamite_P>();
			Item.shootSpeed *= 1.5f;
			Item.value = Item.sellPrice(silver: 22);
			Item.rare = ItemRarityID.Lime;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 5);
			recipe.AddIngredient(ItemID.ChlorophyteBar);
			recipe.AddIngredient(ItemID.Dynamite, 5);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();

			recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ChlorophyteOre);
			recipe.AddIngredient(ItemID.Dynamite);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
	public class Chlorodynamite_P : ModProjectile, IIsExplodingProjectile {
		const int explosion_delay_time = 60;
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Chlorodynamite";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.penetrate = -1;
			Projectile.width = Projectile.height = 24;
			Projectile.timeLeft = 300;
		}
		public override void AI() {
			if (Projectile.timeLeft == explosion_delay_time) {
				const float maxDist = 240 * 240;
				List<(Vector2 pos, float weight)> targets = new();
				NPC npc;
				for (int i = 0; i < Main.maxNPCs; i++) {
					npc = Main.npc[i];
					if (npc.active && npc.damage > 0 && !npc.friendly) {
						Vector2 currentPos = npc.Hitbox.ClosestPointInRect(Projectile.Center);
						Vector2 diff = currentPos - Projectile.Center;
						float dist = diff.LengthSquared();
						if (dist > maxDist) continue;
						float currentWeight = (1.5f - Vector2.Dot(npc.velocity, diff.SafeNormalize(default))) * (dist / maxDist);
						if (targets.Count >= 3) {
							for (int j = 0; j < 3; j++) {
								if (targets[j].weight < currentWeight) {
									targets.Insert(j, (currentPos, currentWeight));
									break;
								}
							}
						} else {
							targets.Add((currentPos, currentWeight));
						}
					}
				}
				for (int i = 0; i < 3; i++) {
					if (i >= targets.Count) break;
					Vector2 currentPos = targets[i].pos;
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						(currentPos - Projectile.Center).WithMaxLength(12),
						Chlorodynamite_Vine.ID,
						Projectile.damage / 9,
						10,
						Projectile.owner,
						Projectile.Center.X,
						Projectile.Center.Y
					);
				}
			}
		}
		public void Explode(int delay = 0) {
			if (delay < 0) delay = 0;
			delay += explosion_delay_time + 1;
			if (Projectile.timeLeft > delay) Projectile.timeLeft = delay;
			Projectile.aiStyle = 0;
			Projectile.velocity = Vector2.Zero;
		}
		public bool IsExploding() => Projectile.timeLeft <= explosion_delay_time;
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			Projectile.aiStyle = ProjAIStyleID.Explosive;
			return true;
		}
		public override bool? CanHitNPC(NPC target) => null;//Projectile.type == ProjectileID.Grenade ? null : false;
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (Projectile.type != ProjectileID.Grenade) {
				modifiers.SourceDamage /= 12;
			}
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 200;
			Projectile.height = 200;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
	}
	public class Chlorodynamite_Vine : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.penetrate = -1;
			Projectile.width = Projectile.height = 16;
			Projectile.timeLeft = 60;
			Projectile.tileCollide = false;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.velocity != default) {
				target.velocity -= Vector2.Normalize(Projectile.velocity) * hit.Knockback * target.knockBackResist;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Vector2 ownerCenter = new Vector2(Projectile.ai[0], Projectile.ai[1]);
			Vector2 center = Projectile.Center;
			Vector2 distToProj = ownerCenter - Projectile.Center;
			float projRotation = distToProj.ToRotation() - 1.57f;
			float distance = distToProj.Length();
			distToProj.Normalize();
			distToProj *= 8f;
			DrawData data;
			while (distance > 8f && !float.IsNaN(distance)) {
				center += distToProj;
				distance = (ownerCenter - center).Length();
				Color drawColor = lightColor;
				Rectangle frame;
				switch (Main.rand.Next(4)) {
					default:
					frame = new Rectangle(2, 2, 10, 10);
					break;

					case 1:
					frame = new Rectangle(2, 0, 10, 14);
					break;

					case 2:
					frame = new Rectangle(2, 18, 10, 10);
					break;

					case 3:
					frame = new Rectangle(14, 18, 10, 10);
					break;
				}
				data = new DrawData(
					TextureAssets.Projectile[Type].Value,
					center - Main.screenPosition,
					frame,
					drawColor,
					projRotation,
					frame.Size() * 0.5f,
					new Vector2(1, 1),
					SpriteEffects.None, 
				0);
				Main.EntitySpriteDraw(data);
			}
			return false;
		}
	}
}
