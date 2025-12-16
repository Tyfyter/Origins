using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Armor.Fiberglass;
using Origins.Items.Other.LootBags;
using Origins.Items.Pets;
using Origins.Items.Vanity.BossMasks;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.NPCs.MiscE;
using Origins.Tiles.BossDrops;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Origins.OriginExtensions;
using static Tyfyter.Utils.KinematicUtils;

namespace Origins.NPCs.Fiberglass {
	[AutoloadBossHead]
	public class Fiberglass_Weaver : ModNPC, IMeleeCollisionDataNPC, ICustomWikiStat {
		static AutoLoadingAsset<Texture2D> UpperLegTexture = "Origins/NPCs/Fiberglass/Fiberglass_Weaver_Leg_Upper";
		static AutoLoadingAsset<Texture2D> LowerLegTexture = "Origins/NPCs/Fiberglass/Fiberglass_Weaver_Leg_Lower";
		Arm[] legs;
		Vector2[] legTargets;
		internal static IItemDropRule armorDropRule;
		internal static IItemDropRule weaponDropRule;
		const float upperLegLength = 70.1f;
		const float lowerLegLength = 76f;
		const float totalLegLength = upperLegLength + lowerLegLength;
		Vector2? spawnPosition = null;
		public static int DifficultyMult => Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);
		string ICustomWikiStat.CustomSpritePath => WikiPageExporter.GetWikiImagePath("UI/Fiberglass_Weaver_Preview");
		public override void SetStaticDefaults() {
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			OriginsSets.NPCs.CustomGroundedCheck[Type] = _ => true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() { // Influences how the NPC looks in the Bestiary
				CustomTexturePath = "Origins/UI/Fiberglass_Weaver_Preview", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(0f, -32f),
				PortraitPositionXOverride = 0f,
				PortraitPositionYOverride = -32f
			};
			NPCID.Sets.BossBestiaryPriority.Add(Type);
		}
		public override void Unload() {
			armorDropRule = null;
			weaponDropRule = null;
		}
		public override void SetDefaults() {
			NPC.boss = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 25;
			NPC.lifeMax = 2000;
			NPC.defense = 26;
			NPC.aiStyle = Terraria.ID.NPCAIStyleID.FaceClosestPlayer;
			NPC.width = NPC.height = 68;
			NPC.knockBackResist = 0.1f;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.value = Item.buyPrice(gold: 5);
			NPC.npcSlots = 200;
			SpawnModBiomes = [
				ModContent.GetInstance<Fiberglass_Undergrowth>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void AI() {
			NPCAimedTarget target = NPC.GetTargetData();
			if (legs is null) {
				legs = new Arm[8];
				legTargets = new Vector2[8];
				NPC.rotation = NPC.AngleTo(target.Center) + MathHelper.PiOver2;
				for (int i = 0; i < 8; i++) {
					legs[i] = new Arm() {
						bone0 = new PolarVec2(upperLegLength, 0),
						bone1 = new PolarVec2(lowerLegLength, 0)
					};
					switch (i / 2) {
						case 0:
						legs[i].start = new Vector2(i % 2 == 0 ? -15 : 15, -38);
						break;
						case 1:
						legs[i].start = new Vector2(i % 2 == 0 ? -15 : 15, -30);
						break;
						case 2:
						legs[i].start = new Vector2(i % 2 == 0 ? -17 : 17, -16);
						break;
						case 3:
						legs[i].start = new Vector2(i % 2 == 0 ? -15 : 15, -7);
						break;
					}
					legTargets[i] = ((legs[i].start + new Vector2(0, 20)) * new Vector2(1, 1.5f)).RotatedBy(NPC.rotation) * 4 + NPC.Center + new Vector2(0, (((i / 2) % 2) == i % 2 ? 12f : -12f));
				}
			}
			float jumpSpeed = 10 + DifficultyMult * 2;
			spawnPosition ??= NPC.Center;
			if (target.Type == NPCTargetType.Player && !Main.player[NPC.target].InModBiome<Fiberglass_Undergrowth>()) {
				if (NPC.ai[3] < 180) NPC.ai[3] += 0.333f;
			} else {
				NPC.ai[3] = 0;
			}
			if (target.Invalid) {
				target.Position = spawnPosition.Value;
			}
			NPC.spriteDirection = Math.Sign(NPC.velocity.X);
			switch ((int)NPC.ai[0]) {
				case 0: {
					AngularSmoothing(ref NPC.rotation, NPC.AngleTo(target.Center) + MathHelper.PiOver2, 0.05f);
					for (int i = 0; i < 8; i++) {
						Vector2 legStart = legs[i].start.RotatedBy(NPC.rotation) + NPC.Center;
						if (NPC.ai[0] != 0 && i == NPC.ai[2]) {
							continue;
						}
						if (i <= 1 && legStart.DistanceSQ(target.Center) < (totalLegLength * totalLegLength * 0.75f)) {
							NPC.ai[1] = 0;
							NPC.ai[0] = 3;
							NPC.ai[2] = i;
						} else if (legStart.DistanceSQ(legTargets[i]) > (totalLegLength * totalLegLength)) {
							legTargets[i] = GetStandPosition(
								((legs[i].start + new Vector2(0, 15.75f + (i / 2) * 0.0625f)) * new Vector2(0.85f, 2.04f)).RotatedBy(NPC.rotation) * 4 + NPC.Center,
								legStart,
								totalLegLength
							);
						}
					}
					NPC.velocity = Vector2.Lerp(NPC.velocity, (Vector2)new PolarVec2(3 + DifficultyMult, NPC.rotation - MathHelper.PiOver2), 0.1f);
					if (++NPC.ai[1] > 240 - NPC.ai[3]) {
						NPC.ai[1] = 0;
						NPC.ai[0] = Main.rand.Next(1, 3);
						if (!NPC.WithinRange(target.Center, 16 * 100)) {
							NPC.ai[0] = 1; // always lunge when over 100 blocks away
						}
					}
					break;
				}
				case 1: {
					if (NPC.ai[1] == 0) {
						AngularSmoothing(ref NPC.rotation, NPC.AngleTo(target.Center) + MathHelper.PiOver2, 0.05f);
						for (int i = 0; i < 8; i++) {
							Vector2 legStart = legs[i].start.RotatedBy(NPC.rotation) + NPC.Center;
							if (legStart.Distance(legTargets[i]) > totalLegLength - 8) {
								NPC.ai[1] = -1;
								break;
							}
						}
						NPC.velocity = Vector2.Lerp(NPC.velocity, (Vector2)new PolarVec2(1.5f, NPC.rotation + MathHelper.PiOver2), 0.5f);
					} else if (NPC.ai[1] < 0) {
						NPC.velocity = Vector2.Lerp(NPC.velocity, (Vector2)new PolarVec2(0.5f, NPC.rotation + MathHelper.PiOver2), 0.5f);
						if (--NPC.ai[1] < -30) {
							NPC.ai[1] = 1;
						}
					} else if (NPC.ai[1] == 1) {
						float targetDist = NPC.Distance(target.Center);
						NPC.ai[1] = 300 - (targetDist / jumpSpeed);
						NPC.velocity = Vec2FromPolar(jumpSpeed, NPC.rotation - MathHelper.PiOver2);
						legTargets[0] = legTargets[1] = Vec2FromPolar(NPC.rotation - MathHelper.PiOver2, targetDist);

					} else if (++NPC.ai[1] >= 300) {
						NPC.ai[1] = 0;
						NPC.ai[0] = 0;
					} else {
						NPC.velocity = (Vector2)new PolarVec2(jumpSpeed, NPC.rotation - MathHelper.PiOver2);
					}
					break;
				}
				case 2: {
					NPC.velocity = Vector2.Lerp(NPC.velocity, default, 0.1f);
					NPC.ai[1] += 1f;
					float leg0Factor = (float)Math.Sin(NPC.ai[1] / 3);
					float leg0Factor2 = (float)Math.Cos(NPC.ai[1] / 3);
					float leg1Factor = (float)Math.Sin(NPC.ai[1] / 3 + Math.PI);
					float leg1Factor2 = (float)Math.Cos(NPC.ai[1] / 3 + 0.1);
					legTargets[0] = NPC.Center + (Vector2)new PolarVec2(84 + (8 * leg0Factor), NPC.rotation - MathHelper.PiOver2 + 0.09f + leg0Factor2 * 0.10f);
					legTargets[1] = NPC.Center + (Vector2)new PolarVec2(86 + (8 * leg1Factor), NPC.rotation - MathHelper.PiOver2 - 0.09f - leg1Factor2 * 0.10f);
					if (NPC.ai[1] > 180 - DifficultyMult * 30) {
						if (Main.netMode != NetmodeID.MultiplayerClient) {
							int projectileType = ModContent.ProjectileType<Fiberglass_Thread>();
							for (int i = 5; i-- > 0;) {
								PolarVec2 vec = new(Main.rand.Next(8 - DifficultyMult, 12 - DifficultyMult) * 64, Main.rand.NextFloat(0, MathHelper.TwoPi));
								Collision.AimingLaserScan(target.Center, target.Center + (Vector2)vec, 2, 3, out Vector2 position, out float[] samples);
								position = position.SafeNormalize(Vector2.One) * samples.Average() + target.Center;
								Vector2 velocity = (Vector2)vec.RotatedBy(MathHelper.PiOver2 + Main.rand.NextFloat(-0.1f, 0.1f)).WithLength(16);
								Projectile.NewProjectile(
									NPC.GetSource_FromAI(),
									position,
									velocity,
									projectileType,
									(int)(10 * ContentExtensions.DifficultyDamageMultiplier),
									0,
									Main.myPlayer,
									ai0: -velocity.X,
									ai1: -velocity.Y
								);
							}
							NPC.ai[1] = 0;
							NPC.ai[0] = 0;
						}
					}
					break;
				}
				case 3: {
					if (++NPC.ai[1] > 16 - DifficultyMult) {
						legTargets[(int)NPC.ai[2]] = target.Center;
						if (NPC.ai[1] > 48 - DifficultyMult * 6) {
							NPC.ai[1] = 0;
							NPC.ai[0] = 0;
						}
					}
					goto case 0;
				}
			}
			if (NPC.HasValidTarget) NPC.DiscourageDespawn(60 * 5);
			else NPC.EncourageDespawn(60);
		}
		public static Vector2 GetStandPosition(Vector2 target, Vector2 legStart, float legth) {//candidate
			HashSet<Point> checkedPoints = new HashSet<Point>();
			Queue<Point> candidates = new Queue<Point>();
			candidates.Enqueue(target.ToWorldCoordinates().ToPoint());
			Tile tile;
			Point current;
			while (candidates.Count > 0) {
				current = candidates.Dequeue();
				checkedPoints.Add(current);
				if (legStart.DistanceSQ(current.ToWorldCoordinates()) > (legth * legth)) {
					continue;
				}
				tile = Main.tile[current];
				if ((tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])) || tile.WallType != WallID.None) {
					return current.ToWorldCoordinates();
				}
				if (!checkedPoints.Contains(current + new Point(1, 0))) candidates.Enqueue(current + new Point(1, 0));
				if (!checkedPoints.Contains(current + new Point(-1, 0))) candidates.Enqueue(current + new Point(-1, 0));
				if (!checkedPoints.Contains(current + new Point(0, 1))) candidates.Enqueue(current + new Point(0, 1));
				if (!checkedPoints.Contains(current + new Point(0, -1))) candidates.Enqueue(current + new Point(0, -1));
			}
			return target;
		}
		public void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult) {
			if (legs is null) return;
			Rectangle legHitbox = new(-4, -4, 8, 8);
			for (int i = 0; i < 8; i++) {
				Rectangle hitbox = legHitbox;
				Vector2 legStart = legs[i].start.RotatedBy(NPC.rotation) + NPC.Center;
				Vector2 offset = legStart + (Vector2)legs[i].bone0 + (Vector2)legs[i].bone1;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(victimHitbox)) {
					npcRect = hitbox;
					damageMultiplier = NPC.ai[0] == 3 ? 1 : 1.5f;
					return;
				}
			}
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			armorDropRule = ItemDropRule.FewFromOptionsNotScalingWithLuck(2, 1, ModContent.ItemType<Fiberglass_Helmet>(), ModContent.ItemType<Fiberglass_Body>(), ModContent.ItemType<Fiberglass_Legs>());
			weaponDropRule = ItemDropRule.OneFromOptionsNotScalingWithLuck(1,
				ModContent.ItemType<Dream_Catcher>(),
				ModContent.ItemType<Fiberglass_Bow>(),
				ModContent.ItemType<Fiberglass_Sword>(),
				ModContent.ItemType<Fiberglass_Pistol>(),
				ModContent.ItemType<Glass_Cannon>(),
				ModContent.ItemType<Thread_Rod>()
			);
			armorDropRule.OnSuccess(weaponDropRule);
			armorDropRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Fiberglass_Weaver_Head>(), 10));
			armorDropRule.OnSuccess(ItemDropRule.Common(TrophyTileBase.ItemType<Fiberglass_Weaver_Trophy>(), 10));

			npcLoot.Add(new DropBasedOnExpertMode(
				armorDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ModContent.ItemType<Fiberglass_Weaver_Bag>(), 1, 1, 1, null)
			));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Fiberglass_Dagger>(), 4));
			npcLoot.Add(ItemDropRule.MasterModeCommonDrop(RelicTileBase.ItemType<Fiberglass_Weaver_Relic>()));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Terlet_Paper>(), 4));
			npcLoot.Add(new LeadingConditionRule(new Conditions.IsMasterMode()).WithOnSuccess(weaponDropRule));
		}
		public static AutoLoadingAsset<Texture2D> normalTexture = typeof(Fiberglass_Weaver).GetDefaultTMLName();
		public static AutoLoadingAsset<Texture2D> afTexture = typeof(Fiberglass_Weaver).GetDefaultTMLName() + "_AF";
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			float rotation = NPC.rotation;
			float scale = 1;
			SpriteEffects effect = SpriteEffects.None;
			if (OriginsModIntegrations.CheckAprilFools()) {
				TextureAssets.Npc[Type] = afTexture;
				rotation = 0;
				effect = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			} else {
				TextureAssets.Npc[Type] = normalTexture;
				Main.CurrentDrawnEntityShader = Terraria.Graphics.Shaders.GameShaders.Armor.GetShaderIdFromItemId(ItemID.ReflectiveDye);
				if (legs is null) return false;
				for (int i = 0; i < 8; i++) {
					bool flip = (i % 2 != 0) == i < 4;
					Vector2 baseStart = legs[i].start;
					legs[i].start = legs[i].start.RotatedBy(NPC.rotation) + NPC.Center;
					float[] targets = legs[i].GetTargetAngles(legTargets[i], flip);
					AngularSmoothing(ref legs[i].bone0.Theta, targets[0], 0.3f);
					AngularSmoothing(ref legs[i].bone1.Theta, targets[1], NPC.ai[0] == 2 && NPC.ai[2] == i ? 1f : 0.3f);

					Vector2 screenStart = legs[i].start - screenPos;
					Main.EntitySpriteDraw(UpperLegTexture, screenStart, null, drawColor, legs[i].bone0.Theta, new Vector2(5, flip ? 3 : 9), 1f, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
					Main.EntitySpriteDraw(LowerLegTexture, screenStart + (Vector2)legs[i].bone0, null, drawColor, legs[i].bone0.Theta + legs[i].bone1.Theta, new Vector2(6, flip ? 2 : 6), 1f, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
					legs[i].start = baseStart;
				}
			}
			Main.EntitySpriteDraw(TextureAssets.Npc[Type].Value, NPC.Center - screenPos, null, drawColor, rotation, TextureAssets.Npc[Type].Size() / 2, scale, effect, 0);
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			NPC.velocity.X += hit.HitDirection * 3;
			if (NPC.life <= 0) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG1_Gore");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG2_Gore");
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, "Gores/NPCs/FG3_Gore");
			} else if (hit.Damage > NPC.lifeMax * 0.1f) {
				Mod.SpawnGoreByName(NPC.GetSource_Death(), NPC.position, NPC.velocity, $"Gores/NPCs/FG{Main.rand.Next(3) + 1}_Gore");
			}
		}
		public override void OnKill() {
			Boss_Tracker.Instance.downedFiberglassWeaver = true;
			NetMessage.SendData(MessageID.WorldData);
		}
	}
	public class Fiberglass_Thread : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public Vector2 OtherEndPos {
			get => new(Projectile.localAI[0], Projectile.localAI[1]);
			set {
				Projectile.localAI[0] = value.X;
				Projectile.localAI[1] = value.Y;
			}
		}
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = ushort.MaxValue;
		}
		public override void SetDefaults() {
			Projectile.hostile = true;
			Projectile.timeLeft = 600 + Main.rand.Next(60, 120) * Fiberglass_Weaver.DifficultyMult;
			Projectile.width = Projectile.height = 8;
		}
		public override void AI() {
			if (Projectile.ai[2] == 0) {
				Projectile.ai[2] = 1;
				Projectile.localAI[0] = Projectile.position.X;
				Projectile.localAI[1] = Projectile.position.Y;
			}
			Vector2 vel = Collision.AnyCollisionWithSpecificTiles(
				OtherEndPos,
				new(Projectile.ai[0], Projectile.ai[1]),
				8,
				8,
				Main.tileSolid.CombineSets(Main.tileSolidTop, (a, b) => a && !b)
			);
			if (Projectile.ai[0] != vel.X || Projectile.ai[1] != vel.Y) {
				Projectile.ai[0] = 0;
				Projectile.ai[1] = 0;
				Projectile.netUpdate = true;
			}
			OtherEndPos += vel;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			return Collision.CheckAABBvLineCollision2(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, OtherEndPos + new Vector2(4));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (Main.rand.NextBool(32 >> Fiberglass_Weaver.DifficultyMult)) {
				target.AddBuff(BuffID.Webbed, 10 * Fiberglass_Weaver.DifficultyMult);
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = default;
			Projectile.netUpdate = true;
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[0]);
			writer.Write(Projectile.localAI[1]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[0] = reader.ReadSingle();
			Projectile.localAI[1] = reader.ReadSingle();
		}
		readonly float frameA = Main.rand.NextFloat();
		readonly float frameB = Main.rand.NextFloat();
		private static readonly VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			Vector2 diff = OtherEndPos + new Vector2(4) - Projectile.Center;
			Vector2 scale = new Vector2(diff.Length(), 2);

			MiscShaderData miscShaderData = GameShaders.Misc["Origins:AnimatedTrail"];
			int num = 1;//1
			int num2 = 0;//0
			int num3 = 0;//0
			float w = 0f;//0.6f
			miscShaderData.UseShaderSpecificData(new Vector4(num, num2, num3, w));
			miscShaderData.UseImage0(TextureAssets.Extra[ExtrasID.MagicMissileTrailShape]);
			miscShaderData.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
			//miscShaderData.UseImage0(TextureAssets.Extra[189]);
			float uTime = (float)Main.timeForVisualEffects / 22;
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1.5f, 0, 0, 0));
			miscShaderData.Shader.Parameters["uAlphaMatrix1"].SetValue(new Vector4(1.05f, 0, 0, 0));
			miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(frameA, 0, 1, 1));
			miscShaderData.Shader.Parameters["uSourceRect1"].SetValue(new Vector4(frameB, 0, 1, 1));
			miscShaderData.Apply();
			const int verts = 128;
			float[] rot = new float[verts + 1];
			Vector2[] pos = new Vector2[verts + 1];
			Vector2 start = new Vector2(Projectile.Center.X, Projectile.Center.Y);
			Vector2 end = OtherEndPos;
			float rotation = (end - start).ToRotation();
			for (int i = 0; i < verts + 1; i++) {
				rot[i] = rotation;
				pos[i] = Vector2.Lerp(start, end, i / (float)verts);
			}
			_vertexStrip.PrepareStrip(pos, rot, progress => Lighting.GetColor(Vector2.Lerp(start, end, progress).ToTileCoordinates()), _ => 8, -Main.screenPosition, pos.Length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			return false;
		}
	}
}
