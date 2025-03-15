using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Pets;
using Origins.NPCs.Fiberglass;
using Origins.Tiles;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
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
		Vector2[] legTargets;
		const float upperLegLength = 70.1f / 4.5f;
		const float lowerLegLength = 76f / 4.5f;
		const float totalLegLength = upperLegLength + lowerLegLength;
		public override void SetStaticDefaults() {
			Terlet_Paper.projectileID = Projectile.type;

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
			if (distanceToIdlePosition > 12f) {
				// The immediate range around the player (when it passively floats about)

				// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
			} else if (Projectile.velocity == Vector2.Zero) {
				Projectile.velocity *= 0.95f;
			}
			#endregion

			#region Animation and visuals
			// So it will lean slightly towards the direction it's moving
			AngularSmoothing(ref Projectile.rotation, Projectile.AngleTo(Projectile.Center + Projectile.velocity) + MathHelper.PiOver2, 0.1f);

			// This is a simple "loop through all frames from top to bottom" animation

			// Some visuals here
			if (legs is null) {
				legs = new Arm[8];
				legTargets = new Vector2[8];
				for (int i = 0; i < 8; i++) {
					legs[i] = new Arm() {
						bone0 = new PolarVec2(upperLegLength, 0),
						bone1 = new PolarVec2(lowerLegLength, 0)
					};
					switch (i / 2) {
						case 0:
						legs[i].start = new Vector2(i % 2 == 0 ? -17 : 17, -30);
						break;
						case 1:
						legs[i].start = new Vector2(i % 2 == 0 ? -24 : 24, -13);
						break;
						case 2:
						legs[i].start = new Vector2(i % 2 == 0 ? -24 : 24, 7);
						break;
						case 3:
						legs[i].start = new Vector2(i % 2 == 0 ? -24 : 24, 30);
						break;
					}
					legTargets[i] = ((legs[i].start + new Vector2(0, 5)) * new Vector2(1, 1)).RotatedBy(Projectile.rotation) * 1.3f + Projectile.Center;
				}
			}
			//for (int i = 0; i < 8; i++) DrawDebugLine(legs[i].start.RotatedBy(Projectile.rotation) + Projectile.Center, legTargets[i]);
			switch ((int)Projectile.ai[0]) {
				case 0: {
					for (int i = 0; i < 8; i++) {
						Vector2 legStart = legs[i].start.RotatedBy(Projectile.rotation) + Projectile.Center;
						if (legStart.DistanceSQ(legTargets[i]) > (totalLegLength * totalLegLength)) {
							legTargets[i] = Fiberglass_Weaver.GetStandPosition(
								((legs[i].start + new Vector2(0, 5)) * new Vector2(1, 1)).RotatedBy(Projectile.rotation) * 1.3f + Projectile.Center,
								legStart,
								totalLegLength
							);
						}
					}
					if (++Projectile.ai[1] > 240) {
						Projectile.ai[1] = 0;
						Projectile.ai[0] = Main.rand.Next(1, 3);
					}
					break;
				}
				case 1: {
					Projectile.ai[1] += 1f;
					float leg0Factor = (float)Math.Sin(Projectile.ai[1] / 3);
					float leg0Factor2 = (float)Math.Cos(Projectile.ai[1] / 3);
					float leg1Factor = (float)Math.Sin(Projectile.ai[1] / 3 + Math.PI);
					float leg1Factor2 = (float)Math.Cos(Projectile.ai[1] / 3 + 0.1);
					legTargets[0] = Projectile.Center + (Vector2)new PolarVec2(84 + (8 * leg0Factor), Projectile.rotation - MathHelper.PiOver2 + 0.09f + leg0Factor2 * 0.10f);
					legTargets[1] = Projectile.Center + (Vector2)new PolarVec2(86 + (8 * leg1Factor), Projectile.rotation - MathHelper.PiOver2 - 0.09f - leg1Factor2 * 0.10f);
					for (int i = 2; i < 8; i++) {
						Vector2 legStart = legs[i].start.RotatedBy(Projectile.rotation) + Projectile.Center;
						if (legStart.DistanceSQ(legTargets[i]) > (totalLegLength * totalLegLength)) {
							legTargets[i] = Fiberglass_Weaver.GetStandPosition(
								((legs[i].start + new Vector2(0, 5)) * new Vector2(1, 1)).RotatedBy(Projectile.rotation) * 1.3f + Projectile.Center,
								legStart,
								totalLegLength
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
			#endregion
		}
		public override bool PreDraw(ref Color lightColor) {
			if (legs is null) return false;
			for (int i = 0; i < 8; i++) {
				bool flip = (i % 2 != 0) == i < 4;
				Vector2 baseStart = legs[i].start;
				legs[i].start = legs[i].start.RotatedBy(Projectile.rotation) + Projectile.Center;
				float[] targets = legs[i].GetTargetAngles(legTargets[i], flip);
				AngularSmoothing(ref legs[i].bone0.Theta, targets[0], 0.3f);
				AngularSmoothing(ref legs[i].bone1.Theta, targets[1], Projectile.ai[0] == 2 && Projectile.ai[2] == i ? 1f : 0.3f);

				Vector2 screenStart = legs[i].start - Main.screenPosition;
				Main.EntitySpriteDraw(UpperLegTexture, screenStart, null, lightColor, legs[i].bone0.Theta, new Vector2(5, flip ? 3 : 9), 1f, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
				Main.EntitySpriteDraw(LowerLegTexture, screenStart + (Vector2)legs[i].bone0, null, lightColor, legs[i].bone0.Theta + legs[i].bone1.Theta, new Vector2(6, flip ? 2 : 6), 1f, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
				legs[i].start = baseStart;
			}
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(34, 70), 1f, SpriteEffects.None, 0);
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