using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;
using PegasusLib;
using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Origins.Buffs;
namespace Origins.Items.Weapons.Summoner {
	public class Thread_Rod : ModItem, IElementalItem {
		public ushort Element => Elements.Fiberglass;
		public override void SetStaticDefaults() {
			Item.staff[Item.type] = true;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [Slow_Debuff.ID];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.DamageType = DamageClass.Summon;
			Item.damage = 5;
			Item.noMelee = true;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 37;
			Item.useAnimation = 37;
			Item.shoot = ModContent.ProjectileType<Thread_Rod_P>();
			Item.shootSpeed = 12f;
			Item.mana = 13;
			Item.knockBack = 0.1f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item177;
			Item.autoReuse = false;
			Item.sentry = true;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			if (player.altFunctionUse == 2) mult = 0;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				foreach (Projectile projectile in Main.ActiveProjectiles) {
					if (projectile.owner == player.whoAmI && projectile.type == Item.shoot) {
						projectile.Kill();
					}
				}
			} else {
				Projectile.NewProjectile(source, Main.MouseWorld, default, type, Item.damage, Item.knockBack, player.whoAmI, Player.tileTargetX, Player.tileTargetY);
				player.UpdateMaxTurrets();
				//SoundEngine.PlaySound(SoundID.Item177.WithPitchRange(-1, -0.9f), position);
			}
			return false;
		}
	}
	public class Thread_Rod_P : ModProjectile, IElementalProjectile {
		public ushort Element => Elements.Fiberglass;
		public const int thread_count = 5;
		public override string Texture => "Origins/Projectiles/Pixel";
		public readonly struct Thread(Vector2 posA, Vector2 posB) {
			public readonly Vector2 posA = posA;
			public readonly Vector2 posB = posB;
			public bool Valid { get; init; } = true;
			readonly float frameA = Main.rand.NextFloat();
			readonly float frameB = Main.rand.NextFloat();
			private static readonly VertexStrip _vertexStrip = new();
			public static Thread Create(Vector2 pos, Vector2 direction) {
				const float max_length = 1024;
				float lengthA = 0;
				float lengthB = 0;
				if (direction != Vector2.Zero) {
					lengthA = CollisionExt.Raymarch(pos, direction, max_length);
					lengthB = CollisionExt.Raymarch(pos, -direction, max_length);
				}
				return new(pos + direction * lengthA, pos - direction * lengthB) {
					Valid = lengthA != max_length && lengthB != max_length
				};
			}
			public readonly void Draw() {
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
				miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 0, 0, 0));
				miscShaderData.Shader.Parameters["uAlphaMatrix1"].SetValue(new Vector4(0.7f, 0, 0, 0));
				miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(frameA, 0, 1, 1));
				miscShaderData.Shader.Parameters["uSourceRect1"].SetValue(new Vector4(frameB, 0, 1, 1));
				miscShaderData.Apply();
				const int verts = 128;
				float[] rot = new float[verts + 1];
				Vector2[] pos = new Vector2[verts + 1];
				Vector2 start = posA;
				Vector2 end = posB;
				float rotation = (end - start).ToRotation();
				for (int i = 0; i < verts + 1; i++) {
					rot[i] = rotation;
					pos[i] = Vector2.Lerp(start, end, i / (float)verts);
				}
				_vertexStrip.PrepareStrip(pos, rot, progress => Lighting.GetColor(Vector2.Lerp(start, end, progress).ToTileCoordinates()), _ => 8, -Main.screenPosition, pos.Length, includeBacksides: true);
				_vertexStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			}
		}
		Thread[] threads;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1024 * 2;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.sentry = true;
			Projectile.friendly = true;
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.timeLeft = 5 * 60 * 60;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			Projectile.timeLeft = Projectile.SentryLifeTime;
		}
		public override void OnSpawn(IEntitySource source) {
			threads = new Thread[thread_count];
			bool anyValid = false;
			for (int i = thread_count; i-- > 0;) {
				Vector2 vec = Main.rand.NextVector2CircularEdge(1, 1);
				vec = vec * (CollisionExt.Raymarch(Projectile.Center, vec, Main.rand.Next(8, 12) * 64) - 16) + Projectile.Center;
				Vector2 velocity = vec.RotatedByRandom(2).SafeNormalize(default);
				threads[i] = Thread.Create(vec, velocity);
				anyValid |= threads[i].Valid;
			}
			if (!anyValid) Projectile.timeLeft = 1;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			for (int i = thread_count; i-- > 0;) {
				if (threads[i].Valid && Collision.CheckAABBvLineCollision2(targetHitbox.TopLeft(), targetHitbox.Size(), threads[i].posA, threads[i].posB)) {
					return true;
				}
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Slow_Debuff.ID, Main.rand.Next(30, 60));
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override void SendExtraAI(BinaryWriter writer) {
			for (int i = 0; i < thread_count; i++) {
				writer.WriteVector2(threads[i].posA);
				writer.WriteVector2(threads[i].posB);
				writer.Write(threads[i].Valid);
			}
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			threads ??= new Thread[thread_count];
			for (int i = 0; i < thread_count; i++) {
				threads[i] = new Thread(reader.ReadVector2(), reader.ReadVector2()) {
					Valid = reader.ReadBoolean()
				};
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			for (int i = thread_count; i-- > 0;) {
				if (threads[i].Valid) threads[i].Draw();
			}
			return false;
		}
	}
}
