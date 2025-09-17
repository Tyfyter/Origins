using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Pets;
using Origins.NPCs.Fiberglass;
using PegasusLib;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;
using static Tyfyter.Utils.KinematicUtils;

namespace Origins.Items.Pets {
	public class Terlet_Paper : ModItem, ICustomWikiStat {
		internal static int projectileID = 0;
		internal static int buffID = 0;
		public override void SetDefaults() {
			Item.DefaultToVanitypet(projectileID, buffID);
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Master;
			Item.master = true;
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
				player.AddBuff(Item.buffType, 3600);
			}
		}
	}
	public class Terlet_Paper_P : ModProjectile {
		public override string Texture => "Origins/NPCs/Fiberglass/Fiberglass_Weaver_Arachnophobia";
		static AutoLoadingAsset<Texture2D> UpperLegTexture = "Origins/NPCs/Fiberglass/Fiberglass_Weaver_Leg_Upper_Arachnophobia";
		static AutoLoadingAsset<Texture2D> LowerLegTexture = "Origins/NPCs/Fiberglass/Fiberglass_Weaver_Leg_Lower_Arachnophobia";
		Arm[] legs;
		bool[] legsGrounded;
		Vector2[] legTargets;
		static float UpperLegLength => 70.1f / 3f;
		static float LowerLegLength => 76f / 3f;
		static float TotalLegLength => UpperLegLength + LowerLegLength;
		public override void SetStaticDefaults() {
			Terlet_Paper.projectileID = Projectile.type;

			ProjectileID.Sets.CharacterPreviewAnimations[Type].WithCode(TerletSpooderAnim);

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults() {
			Projectile.timeLeft = 5;
			Projectile.width = Projectile.height = 28;
			Projectile.tileCollide = false;
			Projectile.friendly = false;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		private Vector2 GetTarget(int i) {
			Vector2 targetTarget = (legs[i].start + new Vector2(0, 0)) * new Vector2(3f, 7f);
			Vector2 targetWithinRange = ((targetTarget - legs[i].start).WithMaxLength(TotalLegLength * 0.6f) + legs[i].start);
			return targetWithinRange.RotatedBy(Projectile.rotation) + Projectile.Center;
		}
		private void CreateLegs() {
			if (legs is null) {
				legs = new Arm[8];
				legsGrounded = new bool[8];
				legTargets = new Vector2[8];
				for (int i = 0; i < 8; i++) {
					legsGrounded[i] = true;
					legs[i] = new Arm() {
						bone0 = new PolarVec2(UpperLegLength, 0),
						bone1 = new PolarVec2(LowerLegLength, 0)
					};
					switch (i / 2) {
						case 0:
						legs[i].start = new Vector2(i % 2 == 0 ? -22 : 22, -30);
						break;
						case 1:
						legs[i].start = new Vector2(i % 2 == 0 ? -29 : 29, -14);
						break;
						case 2:
						legs[i].start = new Vector2(i % 2 == 0 ? -29 : 29, 7);
						break;
						case 3:
						legs[i].start = new Vector2(i % 2 == 0 ? -25 : 25, 24);
						break;
					}
					legTargets[i] = GetTarget(i);
				}
			}
		}
		private SettingsForCharacterPreview.CustomAnimationCode TerletSpooderAnim = new((proj, walking) => {
			if (proj.ModProjectile is not Terlet_Paper_P paper) return;
			if (paper.legs is null) return;
			for (int i = 0; i < 8; i++) {
				Vector2 target = paper.GetTarget(i);
				Vector2 endPoint = paper.GetLegEndPoint(i);
				if (walking) {
					float inertia = 48f;
					proj.velocity = proj.velocity * (inertia - 1) / inertia;
				} else paper.legTargets[i] = target;
				if (!endPoint.WithinRange(target, 48)) {
					proj.velocity *= 0.7f;
					paper.legsGrounded[i] = false;
				} else if (paper.AdjacentLegsGrounded(i) && !endPoint.WithinRange(target, proj.velocity.IsWithin(Vector2.Zero, 0.1f) ? 4 : 16)) {
					paper.legsGrounded[i] = false;
				}
				if (!paper.legsGrounded[i]) {
					paper.legTargets[i] = target;
				}
			}
		});
		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Terlet_Paper.buffID);
			}
			if (player.HasBuff(Terlet_Paper.buffID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Top;
			idlePosition.X -= 68f * player.direction;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
					else Projectile.velocity.X += overlapVelocity;

					if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
					else Projectile.velocity.Y += overlapVelocity;
				}
			}


			#endregion
			#region Movement

			float speed;
			float inertia;
			if (distanceToIdlePosition > 600f) {
				speed = 16f;
				inertia = 36f;
			} else {
				speed = 6f;
				inertia = 48f;
			}
			if (distanceToIdlePosition > 24f) {
				// The immediate range around the player (when it passively floats about)

				// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
			} else {
				Projectile.velocity *= 0.95f;
				if (Projectile.velocity.LengthSquared() <= 0.1f * 0.1f) Projectile.velocity = Vector2.Zero;
			}
			#endregion

			#region Animation and visuals
			if (Projectile.velocity != Vector2.Zero) AngularSmoothing(ref Projectile.rotation, Projectile.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);

			if (!OriginsModIntegrations.CheckAprilFools()) {
				CreateLegs();
				// Some visuals here
				switch ((int)Projectile.ai[0]) {
					case 0: {
						for (int i = 0; i < 8; i++) {
							Vector2 target = GetTarget(i);
							Vector2 endPoint = GetLegEndPoint(i);
							/*Dust.NewDustPerfect(target, 6, Vector2.Zero).noGravity = true;
							Dust.NewDustPerfect(endPoint, endPoint.WithinRange(target, 4) ? 27 : 29, Vector2.Zero).noGravity = true;*/
							if (!endPoint.WithinRange(target, 48)) {
								Projectile.velocity *= 0.7f;
								legsGrounded[i] = false;
							} else if (AdjacentLegsGrounded(i) && !endPoint.WithinRange(target, Projectile.velocity.IsWithin(Vector2.Zero, 0.1f) ? 4 : 16)) {
								legsGrounded[i] = false;
							}
							if (!legsGrounded[i]) {
								legTargets[i] = target;
							}
						}
						if (++Projectile.ai[1] > 240) {
							Projectile.ai[1] = 0;
							//Projectile.ai[0] = Main.rand.Next(0, 2);
						}
						break;
					}
					case 1: {
						Projectile.ai[1] += 1f;
						float leg0Factor = (float)Math.Sin(Projectile.ai[1] / 3);
						float leg0Factor2 = (float)Math.Cos(Projectile.ai[1] / 3);
						float leg1Factor = (float)Math.Sin(Projectile.ai[1] / 3 + Math.PI);
						float leg1Factor2 = (float)Math.Cos(Projectile.ai[1] / 3 + 0.1);
						//legTargets[0] = Projectile.Center + (Vector2)new PolarVec2(84 + (8 * leg0Factor), Projectile.rotation - MathHelper.PiOver2 + 0.09f + leg0Factor2 * 0.10f);
						//legTargets[1] = Projectile.Center + (Vector2)new PolarVec2(86 + (8 * leg1Factor), Projectile.rotation - MathHelper.PiOver2 - 0.09f - leg1Factor2 * 0.10f);
						for (int i = 2; i < 8; i++) {
							Vector2 legStart = legs[i].start.RotatedBy(Projectile.rotation) + Projectile.Center;
							if (legStart.DistanceSQ(legTargets[i]) > (TotalLegLength * TotalLegLength)) {
								legTargets[i] = Fiberglass_Weaver.GetStandPosition(
									((legs[i].start + new Vector2(0, ((i % 2 == 0) ^ (i % 4 < 2) ? -5 : 5))) * new Vector2(1, 1)).RotatedBy(Projectile.rotation) * 1.7f + Projectile.Center,
									legStart,
									TotalLegLength
								);
							}
						}
						if (Projectile.ai[1] > 90) {
							if (Main.netMode != NetmodeID.MultiplayerClient) {
								Projectile.ai[1] = 0;
								Projectile.ai[0] = 0;
							}
						}
						break;
					}
					default: {
						Projectile.ai[0] = 0;
						goto case 0;
					}
				}
			}

			Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
			/*
			for (int i = 0; i < 8; i++) {
				Vector2 start = legs[i].start.RotatedBy(Projectile.rotation) + Projectile.Center;
				Vector2 a = start + (Vector2)legs[i].bone0;
				Vector2 b = a + (Vector2)(legs[i].bone1 with { Theta = legs[i].bone1.Theta + legs[i].bone0.Theta });
				DrawDebugLine(start, a, dustType: 29);
				DrawDebugLine(a, b, dustType: 29);
			}
			for (int i = 0; i < 8; i++) DrawDebugLine(legs[i].start.RotatedBy(Projectile.rotation) + Projectile.Center, legTargets[i], dustType: (legsGrounded[i] ? 6 : 27));
			//*/
			#endregion
		}
		public Vector2 GetLegEndPoint(int leg) {
			Vector2 start = legs[leg].start.RotatedBy(Projectile.rotation) + Projectile.Center;
			Vector2 a = start + (Vector2)legs[leg].bone0;
			PolarVec2 polar = legs[leg].bone1;
			polar.Theta += legs[leg].bone0.Theta;
			return a + (Vector2)polar;
		}
		public bool AdjacentLegsGrounded(int leg) => LegGrounded(leg - 2) && LegGrounded(leg ^ 1) && LegGrounded(leg + 2);
		public bool LegGrounded(int leg) => (!legs.IndexInRange(leg)) || legsGrounded[leg];
		public static AutoLoadingAsset<Texture2D> normalTexture = typeof(Fiberglass_Weaver).GetDefaultTMLName() + "_Arachnophobia";
		public static AutoLoadingAsset<Texture2D> afTexture = typeof(Terlet_Paper_P).GetDefaultTMLName() + "_AF";
		public override bool PreDraw(ref Color lightColor) {
			float rotation = Projectile.rotation;
			float scale = 1;
			SpriteEffects effect = SpriteEffects.None;
			if (Projectile.isAPreviewDummy) {
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				Projectile.position -= new Vector2(0, 10);
			}
			if (OriginsModIntegrations.CheckAprilFools()) {
				TextureAssets.Projectile[Type] = afTexture;
				rotation = 0;
				effect = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			} else {
				CreateLegs();
				TextureAssets.Projectile[Type] = normalTexture;
				for (int i = 0; i < 8; i++) {
					bool flip = (i % 2 != 0) == i < 4;
					Vector2 baseStart = legs[i].start;
					legs[i].start = legs[i].start.RotatedBy(Projectile.rotation) + Projectile.Center;
					float[] targets = legs[i].GetTargetAngles(legTargets[i], flip);
					if (AngularSmoothing(ref legs[i].bone0.Theta, targets[0], 0.3f) && AngularSmoothing(ref legs[i].bone1.Theta, targets[1], 0.5f)) {
						legsGrounded[i] = true;
					}

					Vector2 screenStart = legs[i].start - Main.screenPosition;
					Main.EntitySpriteDraw(UpperLegTexture, screenStart, null, lightColor, legs[i].bone0.Theta, new Vector2(0, 4), 1f, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);

					Main.EntitySpriteDraw(LowerLegTexture, screenStart + (Vector2)legs[i].bone0, null, lightColor, legs[i].bone0.Theta + legs[i].bone1.Theta, new Vector2(0, 6), 1f, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
					legs[i].start = baseStart;
				}
			}
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, lightColor, rotation, TextureAssets.Projectile[Type].Size() / 2, scale, effect, 0);
			return false;
		}
	}
}

namespace Origins.Buffs {
	public class Terlet_Paper_Buff : ModBuff {
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
			Terlet_Paper.buffID = Type;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.buffTime[buffIndex] = 18000;

			int projType = Terlet_Paper.projectileID;

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				var entitySource = player.GetSource_Buff(buffIndex);

				Projectile.NewProjectile(entitySource, player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
			}
		}
	}
}