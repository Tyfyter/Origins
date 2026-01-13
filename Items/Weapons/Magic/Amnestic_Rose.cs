using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Buffs;
using Origins.Core;
using Origins.Dusts;
using Origins.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.NPCs;
using Origins.Tiles.Defiled;
using PegasusLib;
using PegasusLib.Networking;
using PegasusLib.UI;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Amnestic_Rose : ModItem {
		static bool firstLoad = false;
		public override LocalizedText DisplayName => Mod.GetLocalization($"{LocalizationCategory}.{nameof(Amnestic_Rose)}.{nameof(DisplayName)}");
		public override LocalizedText Tooltip => Mod.GetLocalization($"{LocalizationCategory}.{nameof(Amnestic_Rose)}.{nameof(Tooltip)}");
		public virtual string creditKey => "ConceptAndSpriteBy";
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = tooltips.Count - 1; i >= 0; i--) {
				if (tooltips[i].FullName.StartsWith("Terraria/Tooltip")) {
					tooltips[i].Text += $"\n{Language.GetTextValue("Mods.Origins.Items.GenericTooltip." + creditKey, "Calano")}";
					break;
				}
			}
		}
		public override void Load() {
			Mod.AddContent(new Amnestic_Rose_Layer(this));
			if (!firstLoad.TrySet(true)) return;
			try {
				IL_Player.PlayerFrame += (il) => {
					ILCursor c = new(il);
					//IL_1c74: ldarg.0
					//IL_1c75: ldflda valuetype [FNA]Microsoft.Xna.Framework.Vector2 Terraria.Entity::velocity
					//IL_1c7a: ldfld float32 [FNA]Microsoft.Xna.Framework.Vector2::X
					//IL_1c7f: call float32 [System.Runtime]System.Math::Abs(float32)
					//IL_1c84: conv.r8
					//IL_1c85: ldc.r8 1.3
					//IL_1c8e: mul
					c.GotoNext(MoveType.After,
						i => i.MatchLdarg0(),
						i => i.MatchLdflda<Entity>(nameof(Player.velocity)),
						i => i.MatchLdfld<Vector2>(nameof(Vector2.X)),
						i => i.MatchCall(typeof(Math), nameof(Math.Abs)),
						i => i.MatchConvR8(),
						i => i.MatchLdcR8(1.3),
						i => i.MatchMul()
					);
					c.EmitLdarg0();
					c.EmitDelegate((float orig, Player player) => {
						return Math.Abs(orig) * (player.direction * Math.Sign(player.velocity.X));
					});
					//IL_1b04: ldarg.0
					//IL_1b05: ldfld float64 Terraria.Player::legFrameCounter
					//IL_1b0a: ldc.r8 8
					//IL_1b13: bgt.s IL_1ad4
					c.GotoNext(MoveType.After,
						i => i.MatchLdarg0(),
						i => i.MatchLdfld<Player>(nameof(Player.legFrameCounter)),
						i => i.MatchLdcR8(8),
						i => i.MatchBgt(out _)
					);

					c.EmitLdarg0();
					c.EmitDelegate((Player player) => {
						if (player.direction != Math.Sign(player.velocity.X)) {
							while (player.legFrameCounter < 0) {
								player.legFrameCounter += 8.0;
								player.legFrame.Y -= player.legFrame.Height;
							}
						}
					});
				};
			} catch (Exception e) {
				if (Origins.LogLoadingILError("Amnestic_Rose.Fix_Legs", e)) throw;
			}
			On_Player.HorizontalMovement += (orig, self) => {
				int forceDirection = 0;
				if (self.HeldItem.ModItem is Amnestic_Rose) forceDirection = self.direction;
				orig(self);
				if (forceDirection != 0) self.direction = forceDirection;
				if (self.OriginPlayer().exoLegs) Exo_Legs.UpdateSpeeds(self);
			};
		}
		AutoLoadingAsset<Texture2D> stemTexture;
		AutoLoadingAsset<Texture2D> flowerTexture;
		AutoLoadingAsset<Texture2D> jointTexture;
		protected int thornID;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			stemTexture = Texture + "_Stem";
			flowerTexture = Texture + "_Flower";
			jointTexture = Texture + "_Thorn";
			thornID = ModContent.ProjectileType<Amnestic_Rose_Thorn>();
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.useStyle = ItemUseStyleID.HiddenAnimation;
			Item.holdStyle = 420;
			Item.damage = 45;
			Item.DamageType = DamageClass.Magic;
			Item.shoot = ModContent.ProjectileType<Amnestic_Rose_Flower_P>();
			Item.noMelee = true;
			Item.width = 42;
			Item.height = 42;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.mana = 16;
			Item.UseSound = Origins.Sounds.DefiledIdle.WithPitch(1);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Pink;
			Item.buffType = ModContent.BuffType<Amnestic_Rose_Buff>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SoulofNight, 20)
			.AddIngredient<Black_Bile>(10)
			.AddIngredient<Wilting_Rose_Item>()
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		static Vector2 GetStartPosition(Player player) {
			return player.MountedCenter - new Vector2(player.width * player.direction * 0.5f, -8 * player.gravDir);
		}
		static void UpdateJoints(OriginPlayer originPlayer, PolarVec2[] joints) {
			int gravDir = (int)originPlayer.Player.gravDir;
			int playerDirection = originPlayer.Player.direction * gravDir;
			float playerRotation = originPlayer.Player.fullRotation;
			float diff;
			int dir;
			for (int j = 0; j < 2; j++) {
				int atLimit = 0;
				for (int i = 0; i < joints.Length; i++) {
					float prevRot;
					float highMaxCurve = 1;
					if (i > 0) {
						prevRot = joints[i - 1].Theta;
					} else {
						prevRot = -MathHelper.PiOver2 * gravDir - playerDirection * 1.5f;
						highMaxCurve = 0.75f;
					}
					diff = GeometryUtils.AngleDif(joints[i].Theta + playerRotation, prevRot, out dir);
					float maxCurve = dir == playerDirection ? 0.2f : highMaxCurve;
					if (diff > maxCurve) {
						joints[i].Theta += (diff - maxCurve) * dir;
						atLimit++;
					} else if (dir == playerDirection || diff < 0.2f) {
						joints[i].Theta += 0.002f * playerDirection;
					}else if (dir != playerDirection && diff > 0.8f) {
						joints[i].Theta -= 0.002f * playerDirection;
					}
				}
				Vector2 pos = GetEndPos(originPlayer.Player, joints);
				diff = GeometryUtils.AngleDif(joints[^1].Theta + playerRotation, (originPlayer.relativeTarget + originPlayer.Player.Bottom - pos).ToRotation(), out dir);
				float remainingDiff = diff;
				for (int i = 0; i < joints.Length; i++) {
					float bonus = 1f;
					if (i > 0) {
						bonus = 1 + float.Max(0, GeometryUtils.AngleDif(joints[i].Theta, joints[i - 1].Theta, out int dir2) - 0.5f) * dir2;
					}
					float change = diff * 0.01f * (bonus + (atLimit / (float)joints.Length) * 2 + i * 0.25f);
					joints[i].Theta += change * dir;
					remainingDiff -= change;
					if (remainingDiff <= 0) break;
				}
			}
		}
		static Vector2 GetEndPos(Player player, PolarVec2[] joints) {
			Vector2 startPos = player.RotatedRelativePoint(GetStartPosition(player));
			for (int i = 0; i < joints.Length; i++) {
				startPos += (Vector2)joints[i];
			}
			return startPos;
		}
		public override void HoldItem(Player player) {
			player.itemLocation = Vector2.Zero;
			OriginPlayer originPlayer = player.OriginPlayer();
			if (player.whoAmI == Main.myPlayer) new Set_Relative_Target_Action(player, Main.MouseWorld - player.Bottom).Perform();
			if ((originPlayer.relativeTarget.X > 0) != (player.direction > 0)) {
				player.ChangeDir(originPlayer.relativeTarget.X > 0 ? 1 : -1);
				originPlayer.changedDir = true;
			}
			if (originPlayer.changedDir || originPlayer.ChangedGravDir || originPlayer.amnesticRoseHoldTime <= 0) {
				float startRot = 3;
				if (player.direction == -1) startRot = MathHelper.Pi - startRot;
				float unit = player.direction * 0.9f;
				if (player.gravDir == -1) {
					startRot = MathHelper.Pi - startRot;
					unit *= player.gravDir * 2;
				}
				originPlayer.amnesticRoseJoints = [
					new(40, startRot + unit),
					new(40, startRot + unit * 2),
					new(40, startRot + unit * 3),
					new(40, startRot + unit * 4),
					new(28, startRot + unit * 5) // flower, 
				];
				if (originPlayer.changedDir || originPlayer.ChangedGravDir) {
					for (int i = 0; i < 20; i++) {
						UpdateJoints(originPlayer, originPlayer.amnesticRoseJoints);
					}
				}
			}
			originPlayer.amnesticRoseHoldTime = 3;
			UpdateJoints(originPlayer, originPlayer.amnesticRoseJoints);
			if (originPlayer.amnesticRoseBloomTime.Cooldown()) {
				new Set_Rose_Bloom_Time_Action(player, 0).Send();
				Vector2 pos = GetEndPos(originPlayer.Player, originPlayer.amnesticRoseJoints) - 8 * Vector2.One;
				for (int i = 0; i < 16; i++) {
					Dust.NewDust(pos, 16, 16, ModContent.DustType<Rose_Dust>());
				}
			}
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			OriginPlayer originPlayer = player.OriginPlayer();
			position = GetEndPos(player, originPlayer.amnesticRoseJoints);
			velocity = GeometryUtils.Vec2FromPolar(velocity.Length(), originPlayer.amnesticRoseJoints[^1].Theta + player.fullRotation);
			if (originPlayer.amnesticRoseBloomTime > 0) type = thornID;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			OriginPlayer originPlayer = player.OriginPlayer();
			if (originPlayer.amnesticRoseBloomTime > 0) {
				for (int i = 0; i < 8; i++) {
					Projectile.NewProjectile(
						source,
						position,
						velocity.RotatedBy((Main.rand.NextFloat(0.2f) + 0.2f * (i < 4).ToInt()) * (i % 4 < 2).ToDirectionInt()) * (1 + (i % 2 == 0).ToDirectionInt() * 0.1f),
						type,
						damage,
						knockback,
						player.whoAmI
					);
				}
				return false;
			}
			new Set_Rose_Bloom_Time_Action(player, (int)((60 + player.itemAnimationMax * 1.5f) * 0.5f * 8)).Perform();
			player.AddBuff(Item.buffType, originPlayer.amnesticRoseBloomTime);
			return true;
		}
		[Autoload(false)]
		public class Amnestic_Rose_Layer(Amnestic_Rose item) : PlayerDrawLayer {
			public override string Name => item.Name + "_Layer";
			public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
				return drawInfo.shadow == 0 && drawInfo.heldItem.type == item.Type;
			}
			public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.Tails, PlayerDrawLayers.Wings);
			protected override void Draw(ref PlayerDrawSet drawInfo) {
				OriginPlayer originPlayer = drawInfo.drawPlayer.OriginPlayer();
				Rectangle frame = item.stemTexture.Frame(3);
				DrawData data = new(
					item.stemTexture,
					Vector2.Zero,
					frame,
					Color.White
				) {
					origin = new(frame.Width * 0.5f, frame.Height)
				};
				DrawData jointData = new(
					item.jointTexture,
					Vector2.Zero,
					null,
					Color.White
				) {
					origin = item.jointTexture.Value.Size() * new Vector2(0.5f, 0.6f),
					shader = TangelaVisual.FakeShaderID
				};
				Vector2 pos = GetStartPosition(drawInfo.drawPlayer);
				if (!TangelaVisual.DrawOver) {
					jointData.rotation = GeometryUtils.AngleDif(0, originPlayer.amnesticRoseJoints[0].Theta, out int dir) * dir * 0.5f;
					jointData.position = pos - Main.screenPosition;
					drawInfo.DrawDataCache.Add(jointData);
				}
				for (int i = 0; i < originPlayer.amnesticRoseJoints.Length - 1; i++) {
					if (!TangelaVisual.DrawOver) {
						jointData.rotation = originPlayer.amnesticRoseJoints[i].Theta;
						jointData.rotation += GeometryUtils.AngleDif(jointData.rotation, originPlayer.amnesticRoseJoints[i + 1].Theta, out int dir) * dir * 0.5f;
						jointData.position = pos + (Vector2)originPlayer.amnesticRoseJoints[i] - Main.screenPosition;
						drawInfo.DrawDataCache.Add(jointData);
					}
					frame.X = frame.Width * (i % 3);
					data.sourceRect = frame;
					data.rotation = originPlayer.amnesticRoseJoints[i].Theta + MathHelper.PiOver2;
					data.position = pos - Main.screenPosition;
					data.color = Lighting.GetColor(pos.ToTileCoordinates());
					drawInfo.DrawDataCache.Add(data);
					pos += (Vector2)originPlayer.amnesticRoseJoints[i];
				}
				frame = item.flowerTexture.Frame(verticalFrames: 2, frameY: (originPlayer.amnesticRoseBloomTime <= 0).ToInt());
				data.texture = item.flowerTexture;
				data.sourceRect = frame;
				data.origin = new(frame.Width * 0.5f, frame.Height);
				data.rotation = originPlayer.amnesticRoseJoints[^1].Theta + MathHelper.PiOver2;
				data.position = pos - Main.screenPosition;
				data.color = Lighting.GetColor(pos.ToTileCoordinates());
				drawInfo.DrawDataCache.Add(data);
			}
		}
	}
	public class Amnestic_Rose_Thorn : ModProjectile {
		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
	}
	public class Amnestic_Rose_Flower_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.extraUpdates = 1;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				Projectile.localAI[2] = itemUse.Item.buffType;
			}
		}
		public override void AI() {
			if (Projectile.ai[0] == 1) {
				if (Projectile.ai[1] == 0) {
					SoundEngine.PlaySound(SoundID.Item105.WithPitch(1f).WithPitchVarience(0) with { MaxInstances = 0 }, Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item76.WithPitch(1).WithPitchVarience(0) with { MaxInstances = 0 }, Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item123.WithPitch(1).WithPitchVarience(0) with { MaxInstances = 0 }, Projectile.Center);
					SoundEngine.PlaySound(Origins.Sounds.defiledKill.WithPitch(1).WithPitchVarience(0) with { MaxInstances = 0 }, Projectile.Center);
				}
				Projectile.localNPCHitCooldown = -1;
				Projectile.ai[1] += 1 / 45f;
				if (Projectile.ai[1] > 1) {
					Projectile.ai[1] = 1;
					if (++Projectile.ai[2] > 10) Projectile.Kill();
				}
				Projectile.localAI[0] = MathF.Pow(Projectile.ai[1], 0.5f);
			} else {
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}
		}
		bool StartBloom() {
			if (!Projectile.ai[0].TrySet(1)) return true;
			Projectile.timeLeft = 12000;
			Projectile.velocity = Vector2.Zero;
			Projectile.tileCollide = false;
			Projectile.netUpdate = true;
			for (int i = 0; i < 16; i++) {
				Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Rose_Dust>()).velocity *= 2;
			}
			return false;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.ai[0] == 1) {
				return targetHitbox.IsWithin(Projectile.Center, (96 + 8) * Projectile.localAI[0]);
			}
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (StartBloom()) {
				target.AddBuff((int)Projectile.localAI[2], 180);
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			StartBloom();
			return false;
		}

		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) => Projectile.ai[0] != 1;
		public override void PostDraw(Color lightColor) {
			if (Projectile.ai[0] == 1) {
				if (Mask_Rasterize.QueueProjectile(Projectile.whoAmI)) return;
				Draw(Projectile.Center, MathF.Pow(Projectile.ai[1], 0.5f));
				Draw(Projectile.Center, Projectile.ai[1] * Projectile.ai[1]);
			}
		}
		static void Draw(Vector2 center, float size) {
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:Identity"];
			miscShaderData.UseImage0(TextureAssets.Extra[ExtrasID.RainbowRodTrailShape]);
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1.5f, 0, 0, 0));
			miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(0, 0, 1, 1));
			miscShaderData.Apply();
			const int verts = 64;
			float[] rot = new float[verts + 1];
			Vector2[] pos = new Vector2[verts + 1];
			Matrix matrix = Main.GameViewMatrix.ZoomMatrix;
			float length = 96 * size;
			size *= matrix.Right.X;
			for (int i = 0; i < verts + 1; i++) {
				rot[i] = (i * MathHelper.TwoPi) / verts;
				pos[i] = Vector2.Transform(center + new Vector2(length, 0).RotatedBy(rot[i] + MathHelper.PiOver2) - Main.screenPosition, matrix);
			}
			_vertexStrip.PrepareStrip(pos, rot, progress => new(MathF.Cos(progress * MathHelper.TwoPi) * 0.5f + 0.5f, MathF.Sin(progress * MathHelper.TwoPi) * 0.5f + 0.5f, 0), _ => 32 + 16 * size, Vector2.Zero, pos.Length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}
	}
	public class Amnestic_Rose_Buff : ModBuff {
		protected int projType;
		LocalizedText DebuffName;
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
			projType = ModContent.ProjectileType<Amnestic_Rose_Goo_Ball>();
			DebuffName = this.GetLocalization("DebuffName");
			Buff_Hint_Handler.ModifyTip(Type, 0, this.GetLocalization("EffectDescription").Key);
			Buff_Hint_Handler.RemoveIcon(Type);
			Buff_Hint_Handler.CombineBuffHintModifiers(Type, modifyBuffTip: (lines, item, player) => {
				if (player) return;
				lines[0] = lines[0].Replace(DisplayName.Value, DebuffName.Value);
			});
		}
		public override void Update(NPC npc, ref int buffIndex) {
			OriginGlobalNPC originGlobalNPC = npc.GetGlobalNPC<OriginGlobalNPC>();
			originGlobalNPC.amnesticRose = true;
			originGlobalNPC.amnesticRoseGooProj = projType;
			if (!npc.boss) originGlobalNPC.slowDebuff = true;
			if (Main.rand.NextBool(3)) Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<Rose_Dust>());
		}
		public override void Update(Player player, ref int buffIndex) {
			player.buffTime[buffIndex] = player.OriginPlayer().amnesticRoseBloomTime;
		}
	}
	public class Amnestic_Rose_Goo_Ball : ModProjectile {
		protected int floorGooType;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			floorGooType = ModContent.ProjectileType<Amnestic_Rose_Goo_Floor>();
		}
		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 0;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.velocity.Y += 0.12f;
			if (Main.rand.NextBool(10)) {
				Dust.NewDust(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					ModContent.DustType<Rose_Dust>()
				);
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			for (int i = 0; i < 10; i++) {
				Dust.NewDust(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					ModContent.DustType<Rose_Dust>()
				);
			}
			if (Projectile.owner != Main.myPlayer) return true;
			if (Projectile.velocity.X != oldVelocity.X) {
				Vector2 dir = new(oldVelocity.X - Projectile.velocity.X, 0);
				dir.Normalize();
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center + dir * CollisionExt.Raymarch(Projectile.Center, dir, 32),
					dir,
					floorGooType,
					Projectile.damage / 2,
					Projectile.knockBack / 10,
					Projectile.owner
				);
			}
			if (Projectile.velocity.Y != oldVelocity.Y) {
				Vector2 dir = new(0, oldVelocity.Y - Projectile.velocity.Y);
				dir.Normalize();
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center + dir * CollisionExt.Raymarch(Projectile.Center, dir, 32),
					dir,
					floorGooType,
					Projectile.damage / 2,
					Projectile.knockBack / 10,
					Projectile.owner
				);
			}
			return true;
		}
	}
	public class Amnestic_Rose_Goo_Floor : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 2;
		}
		public override void SetDefaults() {
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 60 * 10;
			Projectile.friendly = true;
			Projectile.hide = true;
			Projectile.tileCollide = false;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 0;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (Projectile.timeLeft == 60 * 10) Projectile.frame = Main.rand.Next(2);
			Rectangle hitbox = Projectile.Hitbox;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.CanBeChasedBy(Projectile)) {
					if (Projectile.Colliding(hitbox, npc.Hitbox)) {
						int index = npc.FindBuffIndex(Slow_Debuff.ID);
						if (index >= 0) {
							if (npc.buffTime[index] < 10) npc.buffTime[index] = 10;
						} else {
							npc.AddBuff(Slow_Debuff.ID, 10);
						}
					}
				}
			}
			if (Projectile.TryGetOwner(out Player owner)/* && owner.hostile*/) {
				foreach (Player other in Main.ActivePlayers) {
					if (/*other.hostile && other.team != owner.team && */Projectile.Colliding(hitbox, other.Hitbox)) {
						int index = other.FindBuffIndex(BuffID.Slow);
						if (index >= 0) {
							if (other.buffTime[index] < 10) other.buffTime[index] = 10;
						} else {
							other.AddBuff(BuffID.Slow, 10);
						}
					}
				}
			}
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				TextureAssets.Projectile[Type].Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame),
				lightColor,
				Projectile.rotation,
				new(18, 8),
				1,
				SpriteEffects.None
			);
			return false;
		}
	}
	public class Amnestic_Rose_Alt : Amnestic_Rose {
		public override string creditKey => "ConceptBy";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			thornID = ModContent.ProjectileType<Amnestic_Rose_Alt_Thorn>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Amnestic_Rose>()] = Type;
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Amnestic_Rose>();
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.buffType = ModContent.BuffType<Amnestic_Rose_Alt_Buff>();
		}
		public override void AddRecipes() { } // empty override so it doesn't register the recipe for both
	}
	public class Amnestic_Rose_Alt_Thorn : Amnestic_Rose_Thorn { }
	public class Amnestic_Rose_Alt_Flower_P : Amnestic_Rose_Flower_P { }
	public class Amnestic_Rose_Alt_Buff : Amnestic_Rose_Buff {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			projType = ModContent.ProjectileType<Amnestic_Rose_Goo_Ball>();
		}
	}
	public class Amnestic_Rose_Alt_Goo_Ball : Amnestic_Rose_Goo_Ball {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			floorGooType = ModContent.ProjectileType<Amnestic_Rose_Alt_Goo_Floor>();
		}
	}
	public class Amnestic_Rose_Alt_Goo_Floor : Amnestic_Rose_Goo_Floor { }
	public record class Set_Relative_Target_Action(Player Player, Vector2 Target) : SyncedAction {
		public Set_Relative_Target_Action() : this(default, default) { }
		protected override bool ShouldPerform => Player.OriginPlayer().relativeTarget != Target;
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			Player = Main.player[reader.ReadByte()],
			Target = reader.ReadPackedVector2()
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)Player.whoAmI);
			writer.WritePackedVector2(Target);
		}
		protected override void Perform() {
			Player.OriginPlayer().relativeTarget = Target;
		}
	}
	public record class Set_Rose_Bloom_Time_Action(Player Player, int Value) : SyncedAction {
		public Set_Rose_Bloom_Time_Action() : this(default, default) { }
		protected override bool ShouldPerform => Player.OriginPlayer().amnesticRoseBloomTime != Value;
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			Player = Main.player[reader.ReadByte()],
			Value = reader.ReadInt32()
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)Player.whoAmI);
			writer.Write(Value);
		}
		protected override void Perform() {
			Player.OriginPlayer().amnesticRoseBloomTime = Value;
		}
	}
}
