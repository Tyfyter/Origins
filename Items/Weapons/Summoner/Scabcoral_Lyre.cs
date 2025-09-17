using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.World.BiomeData;
using PegasusLib;
using ReLogic.Content;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
namespace Origins.Items.Weapons.Summoner {
	public class Scabcoral_Lyre : ModItem, ICustomWikiStat {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.damage = 37;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 24;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Guitar;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item26.WithPitchRange(0.0f, 0.1f);
			Item.shoot = ModContent.ProjectileType<Scabcoral_Lyre_P>();
			Item.shootSpeed = 1;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.sentry = true;
			Item.glowMask = glowmask;
		}
		public override bool CanUseItem(Player player) => Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY).HasFullSolidTile();
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Item177.WithPitchRange(-1, -0.9f), player.itemLocation);
			return null;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse != 2) {
				Projectile.NewProjectile(source, position, default, type, Item.damage, Item.knockBack, player.whoAmI, Player.tileTargetX, Player.tileTargetY);
				player.UpdateMaxTurrets();
			}
			return false;
		}
	}
	public class Scabcoral_Lyre_P : ModProjectile {
		public override string Texture => typeof(Scabcoral_Lyre).GetDefaultTMLName() + "_Node";
		static readonly AutoLoadingAsset<Texture2D> glowTexture = typeof(Scabcoral_Lyre).GetDefaultTMLName() + "_Node_Glow";
		static readonly AutoLoadingAsset<Texture2D> branchTexture = typeof(Scabcoral_Lyre).GetDefaultTMLName() + "_Branch";

		public override void SetStaticDefaults() {
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults() {
			//Projectile.CloneDefaults(ProjectileID.FrostHydra);
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 46;
			Projectile.height = 44;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.sentry = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = Projectile.SentryLifeTime;
		}
		Arm[] arms;
		Arm[] Arms {
			get {
				if (arms is null) {
					arms = new Arm[7];
					Vector2 pos = new(Projectile.ai[0] * 16 + 8, Projectile.ai[1] * 16 + 8);
					for (int i = 0; i < arms.Length - 1; i++) {
						arms[i] = new() { start = pos };
						pos -= Vector2.UnitY * 32;
						arms[i].end = pos;
					}
					arms[^1] = new() {
						start = pos,
						end = pos - Vector2.UnitY
					};
				}
				return arms;
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Vector2 basePos = new(Projectile.ai[0] * 16 + 8, Projectile.ai[1] * 16 + 8);
			_ = Arms;
			#region General behavior
			if (player.dead || !player.active) {
				Projectile.Kill();
				return;
			}
			#endregion

			#region Find target
			// Starting search distance
			float distanceFromTarget = 32 * 7;
			Vector2 targetCenter = default;
			int target = -1;
			bool hasPriorityTarget = false;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				if ((isPriorityTarget || !hasPriorityTarget) && npc.CanBeChasedBy()) {
					Vector2 pos = Projectile.position;
					Vector2 offset = Vector2.Zero;
					int dir = Math.Sign(npc.Center.X - (pos.X + offset.X));
					float between = Vector2.Distance(npc.Center, pos + offset);
					bool closer = distanceFromTarget > between;
					bool lineOfSight = Collision.CanHitLine(pos + offset, 8, 8, npc.position, npc.width, npc.height);
					if (closer && lineOfSight) {
						distanceFromTarget = between;
						targetCenter = npc.Center;
						target = npc.whoAmI;
						foundTarget = true;
						hasPriorityTarget = isPriorityTarget;
					}
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);

			#endregion

			#region Movement
			Vector2 armTarget;
			if (foundTarget) {
				const float move_speed = 3;
				armTarget = targetCenter;
				for (int i = 1; i <= arms.Length; i++) {
					arms[^i].DoIKMove(armTarget, move_speed);
					armTarget = arms[^i].start;
				}
			}
			armTarget = basePos;

			for (int i = 0; i < arms.Length; i++) {
				arms[i].MoveByStart(armTarget);
				arms[i].end = arms[i].end.RotatedBy(MathF.Sin(Projectile.timeLeft * 0.02f + MathHelper.PiOver4) * 0.001f * i, arms[i].start);
				armTarget = arms[i].end;
			}
			Projectile.Center = arms[^1].end;
			#endregion
		}
		public override bool PreDraw(ref Color lightColor) {
			default(Scabcoral_Branch_Drawer).Draw(Arms);
			float rotation = (arms[^1].end - arms[^1].start).ToRotation();
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				rotation,
				texture.Size() * 0.5f,
				1,
				0
			);
			Main.EntitySpriteDraw(
				glowTexture,
				Projectile.Center - Main.screenPosition,
				null,
				Riven_Hive.GetGlowAlpha(lightColor),
				rotation,
				texture.Size() * 0.5f,
				1,
				0
			);
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)Arms.Length);
			for (int i = 0; i < arms.Length; i++) {
				writer.Write(arms[i].end.X);
				writer.Write(arms[i].end.Y);
			}
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			byte armCount = reader.ReadByte();
			if (Arms.Length != armCount) arms = new Arm[armCount];
			Vector2 start = new(Projectile.ai[0] * 16 + 8, Projectile.ai[1] * 16 + 8);
			for (int i = 0; i < armCount; i++) {
				arms[i].end = new(reader.ReadSingle(), reader.ReadSingle());
				arms[i].start = start;
				start = arms[i].end;
			}
		}
		public class Arm {
			public Vector2 start;
			public Vector2 end;
			public void MoveByStart(Vector2 target) {
				Vector2 diff = target - start;
				start += diff;
				end += diff;
			}
			public void MoveByEnd(Vector2 target) {
				Vector2 diff = target - end;
				start += diff;
				end += diff;
			}
			public void DoIKMove(Vector2 target, float maxSpeed) {
				Vector2 diff = target - end;
				Vector2 newPos = end + diff.WithMaxLength(maxSpeed);
				float angleDiff = GeometryUtils.AngleDif((end - start).ToRotation(), (target - start).ToRotation(), out int dir);
				end = end.RotatedBy(angleDiff * dir, start);
				MoveByEnd(newPos);
			}
		}
		public struct Scabcoral_Branch_Drawer {
			private static VertexStrip _vertexStrip = new VertexStrip();
			Color[] colors;
			int length;
			public void Draw(Arm[] arms) {
				MiscShaderData miscShaderData = GameShaders.Misc["Origins:Beam"];
				length = arms.Length;
				if (length == 0) return;
				float[] rot = new float[length];
				Vector2[] pos = new Vector2[length];
				colors = new Color[length + 1];
				for (int i = 0; i < length; i++) {
					rot[i] = (arms[i].end - arms[i].start).ToRotation();
					pos[i] = arms[i].start;
					colors[i] = Lighting.GetColor(arms[i].start.ToTileCoordinates());
				}
				if (length == 0) return;
				colors[^1] = Lighting.GetColor(arms[^1].end.ToTileCoordinates());
				Asset<Texture2D> texture = branchTexture;
				miscShaderData.UseImage0(texture);
				miscShaderData.UseShaderSpecificData(texture.UVFrame());
				float endLength = (16f / 32f) / length;
				miscShaderData.Shader.Parameters["uLoopData"].SetValue(new Vector2(
					3,
					0
				));
				miscShaderData.Apply();
				_vertexStrip.PrepareStrip(pos, rot, StripColors, StripWidth, -Main.screenPosition, length, includeBacksides: true);
				_vertexStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			}

			private Color StripColors(float progressOnStrip) => colors[(int)(progressOnStrip * length)];
			private float StripWidth(float progressOnStrip) => 12;
		}
	}
}
