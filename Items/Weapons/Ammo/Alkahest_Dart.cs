using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.Dev;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using System;
using Origins.NPCs;
namespace Origins.Items.Weapons.Ammo {
	public class Alkahest_Dart : ModItem, ICustomWikiStat, ITornSource {
		public static float TornSeverity => 0.25f;
		float ITornSource.Severity => TornSeverity;
		public string[] Categories => [
			WikiCategories.Dart
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CursedDart);
			Item.damage = 10;
			Item.shoot = ModContent.ProjectileType<Alkahest_Dart_P>();
			Item.shootSpeed = 3f;
			Item.knockBack = 2.2f;
			Item.value = Item.sellPrice(copper: 6);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 100)
			.AddIngredient(ModContent.ItemType<Alkahest>())
			.Register();
		}
		public override void HoldItem(Player player) {
		}
	}
	public class Alkahest_Dart_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ammo/Alkahest_Dart";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CursedDart);
		}
		public override void AI() {
			Projectile.localAI[0] += 1f;
			if (Projectile.localAI[0] > 3f)
				Projectile.alpha = 0;

			if (Projectile.ai[0] >= 20f) {
				Projectile.ai[0] = 20f;
				Projectile.velocity.Y += 0.075f;
			}
			int auraProjIndex = (int)Projectile.ai[1] - 1;
			if (auraProjIndex < 0) {
				if (Projectile.owner == Main.myPlayer) Projectile.ai[1] = Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.position,
					default,
					Alkahest_Dart_Aura.ID,
					Projectile.damage / 2,
					0,
					Projectile.owner,
					Projectile.whoAmI
				) + 1;
			} else {
				Projectile auraProj = Main.projectile[auraProjIndex];
				if (auraProj.active && auraProj.type == Alkahest_Dart_Aura.ID) {
					auraProj.position = Projectile.Center;
					auraProj.rotation = Projectile.rotation;
				} else {
					Projectile.ai[1] = 0;
				}
			}
		}
		public override Color? GetAlpha(Color lightColor) {
			return Projectile.alpha == 0 ? new Color(255, 255, 255, 200) : Color.Transparent;
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
			SoundEngine.PlaySound(SoundID.Shatter.WithVolume(0.5f), Projectile.position);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 180, 180, Alkahest_Dart.TornSeverity, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
	}
	public class Alkahest_Dart_Aura : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 30;
			ProjectileID.Sets.TrailingMode[Type] = -1;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 16 * 400;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.hide = true;
			Projectile.width = Projectile.height = 0;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 15;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			int auraProj = (int)Projectile.ai[0];
			if (auraProj >= 0) {
				Projectile ownerProj = Main.projectile[auraProj];
				if (ownerProj.active) {
					Projectile.scale = ownerProj.scale;
					Projectile.Center = ownerProj.Center;
					Projectile.rotation = ownerProj.rotation;
				} else {
					Projectile.Center = ownerProj.Center;
					Projectile.ai[0] = -1;
				}
			}
			for (int i = Projectile.oldPos.Length - 1; i > 0; i--) {
				Projectile.oldPos[i] = Projectile.oldPos[i - 1];
				Projectile.oldRot[i] = Projectile.oldRot[i - 1];
			}
			Projectile.oldPos[0] = Projectile.position;
			Projectile.oldRot[0] = Projectile.rotation;
			if (Projectile.oldPos[^1] == Projectile.position && Projectile.oldRot[^1] == Projectile.rotation) Projectile.Kill();
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 width = new(8, 0);
			Vector2 rot = width.RotatedBy(Projectile.rotation);
			Vector2 lastPos0 = Projectile.position - rot;
			Vector2 lastPos1 = Projectile.position + rot;
			Vector2 nextPos0 = default, nextPos1 = default;
			List<(Vector2 start, Vector2 end)> _lines = [(lastPos1, lastPos0)];
			for (int i = 0; i < Projectile.oldPos.Length; i++) {
				Vector2 nextPos = Projectile.oldPos[i];
				if (nextPos == default) break;
				rot = width.RotatedBy(Projectile.oldRot[i]);
				nextPos0 = nextPos - rot;
				nextPos1 = nextPos + rot;

				_lines.Add((lastPos0, nextPos0));
				_lines.Add((nextPos1, lastPos1));
				lastPos0 = nextPos0;
				lastPos1 = nextPos1;
			}
			_lines.Add((nextPos0, nextPos1));
			return CollisionExtensions.PolygonIntersectsRect(_lines.ToArray(), targetHitbox);
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindProjectiles.Add(index);
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 30, 180, Alkahest_Dart.TornSeverity, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:AnimatedTrail"];

			Vector2[] positions = new Vector2[Projectile.oldPos.Length - 1];
			float[] rotations = new float[Projectile.oldPos.Length - 1];
			int count = 0;
			for (int i = 1; i < Projectile.oldPos.Length; i++) {
				positions[i - 1] = Projectile.oldPos[i];
				rotations[i - 1] = Projectile.oldRot[i] + MathHelper.PiOver2;
				if (Projectile.oldPos[i] == default) break;
				count++;
			}
			float frameStart = 1 - count / (float)Projectile.oldPos.Length;
			miscShaderData.UseImage0(TextureAssets.Projectile[Type]);
			miscShaderData.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
			//miscShaderData.UseImage0(TextureAssets.Extra[189]);
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 1, 1, 0));
			miscShaderData.Shader.Parameters["uAlphaMatrix1"].SetValue(new Vector4(0.7f, 0, 0, 0));
			miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(frameStart, 0, 1 - frameStart, 1));
			miscShaderData.Shader.Parameters["uSourceRect1"].SetValue(new Vector4(frameStart, MathF.Sin((float)Main.timeForVisualEffects * 0.1f) * 0.2f, 1 - frameStart, 1));
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (_) => Color.Wheat * 0.7f, (_) => 8f, -Main.screenPosition, true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();

			miscShaderData.Shader.Parameters["uSourceRect1"].SetValue(new Vector4(frameStart, (float)Main.timeForVisualEffects * -0.1f, 1 - frameStart, 1));
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (_) => new Color(0.4f, 0.5f, 0f, 0.5f), (_) => 8f, -Main.screenPosition, true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			/*
			Vector2 width = new(8, 0);
			Vector2 rot = width.RotatedBy(Projectile.rotation);
			Vector2 lastPos0 = Projectile.position - rot;
			Vector2 lastPos1 = Projectile.position + rot;
			Vector2 nextPos0, nextPos1;
			for (int i = 0; i < Projectile.oldPos.Length; i++) {
				Vector2 nextPos = Projectile.oldPos[i];
				if (nextPos == default) break;
				rot = width.RotatedBy(Projectile.oldRot[i]);
				nextPos0 = nextPos - rot;
				nextPos1 = nextPos + rot;
				(Vector2 start, Vector2 end)[] lines = [
					(lastPos0, lastPos1),
					(nextPos0, lastPos0),
					(lastPos1, nextPos1),
					(nextPos1, nextPos0)
				];
				Color color = collisionPos == i ? Color.Green : Color.Red;
				for (int j = 0; j < lines.Length; j++) {
					OriginExtensions.DrawDebugLineSprite(lines[j].start, lines[j].end, color, -Main.screenPosition);
					Vector2 diff = lines[j].start - lines[j].end;
					Vector2 center = lines[j].end + diff * 0.5f;
					OriginExtensions.DrawDebugLineSprite(center, center - (new Vector2(diff.Y, -diff.X).SafeNormalize(default) * 4), color, -Main.screenPosition);
				}
				//OriginExtensions.DrawDebugLineSprite(lastPos0, nextPos0, color, -Main.screenPosition);
				//OriginExtensions.DrawDebugLineSprite(nextPos1, lastPos1, color, -Main.screenPosition);
				lastPos0 = nextPos0;
				lastPos1 = nextPos1;
			}*/
			return false;
		}
	}
}
