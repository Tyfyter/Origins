using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Chlorodynamite : ModItem {
        public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Dynamite);
			Item.damage = 186;
			Item.knockBack = 8;
			Item.shoot = ModContent.ProjectileType<Chlorodynamite_P>();
			Item.shootSpeed *= 1.5f;
			Item.value = Item.sellPrice(silver: 22);
			Item.rare = ItemRarityID.Lime;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 5)
			.AddIngredient(ItemID.ChlorophyteBar)
			.AddIngredient(ItemID.Dynamite, 5)
			.AddTile(TileID.MythrilAnvil)
			.Register();

			Recipe.Create(Type)
			.AddIngredient(ItemID.ChlorophyteOre)
			.AddIngredient(ItemID.Dynamite)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Chlorodynamite_P : ModProjectile, IIsExplodingProjectile {
		public const int explosion_delay_time = 60;
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
			Projectile.friendly = false;
			Projectile.damage = (int)Main.player[Projectile.owner].GetTotalDamage(Projectile.DamageType).ApplyTo(Projectile.originalDamage);
			if (Projectile.timeLeft == explosion_delay_time) {
				const float maxDist = 240 * 240;
				List<(Vector2 pos, float weight)> targets = new();
				NPC npc;
				for (int i = 0; i < Main.maxNPCs; i++) {
					npc = Main.npc[i];
					if (npc.active && npc.CanBeChasedBy(Projectile)) {
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
						Projectile.knockBack / 10,
						Projectile.owner,
						Projectile.whoAmI
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
		public bool IsExploding => Projectile.timeLeft <= explosion_delay_time;
		public override bool PreKill(int timeLeft) {
			return true;
		}
		public override bool? CanHitNPC(NPC target) => null;//Projectile.type == ProjectileID.Grenade ? null : false;
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (Projectile.timeLeft > 0) {
				modifiers.SourceDamage /= 12;
			}
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 200, sound: SoundID.Item62);
		}
	}
	public class Chlorodynamite_Vine : ModProjectile {
		public static int ID { get; private set; }
		public int ParentProjectile { 
			get => (int)Projectile.ai[0]; 
			set => Projectile.ai[0] = value;
		}
		public int TargetNPC { 
			get => (int)Projectile.ai[1]; 
			set => Projectile.ai[1] = value;
		}
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.width = Projectile.height = 16;
			Projectile.timeLeft = Chlorodynamite_P.explosion_delay_time;
			Projectile.tileCollide = false;
			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void OnSpawn(IEntitySource source) {
			TargetNPC = -1;
		}
		public override void AI() {
			if (TargetNPC != -1) {
				Projectile parent = Main.projectile[ParentProjectile];
				NPC target = Main.npc[TargetNPC];
				if (!target.active) {
					Projectile.Kill();
					return;
				}
				Vector2 direction = parent.DirectionTo(target.Center);
				if (!direction.HasNaNs()) target.velocity -= direction * Projectile.ai[2];
				Projectile.Center = target.Center;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			TargetNPC = target.whoAmI;
			Projectile.ai[2] = hit.Knockback;
			Projectile.netUpdate = true;
		}
		public override bool PreDraw(ref Color lightColor) {
			Vector2 ownerCenter = Main.projectile[ParentProjectile].Center;
			Vector2 center = Projectile.Center;
			Vector2 distToProj = ownerCenter - Projectile.Center;
			float projRotation = distToProj.ToRotation() - 1.57f;
			float distance = distToProj.Length();
			distToProj.Normalize();
			distToProj *= 8f;
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
				Main.EntitySpriteDraw(new DrawData(
					TextureAssets.Projectile[Type].Value,
					center - Main.screenPosition,
					frame,
					drawColor,
					projRotation,
					frame.Size() * 0.5f,
					new Vector2(1, 1),
					SpriteEffects.None
				));
			}
			return false;
		}
	}
}
