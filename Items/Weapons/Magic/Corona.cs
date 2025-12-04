using Origins.Dev;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Corona : ModItem, ICustomWikiStat {
		public override string Texture => typeof(Blast_Furnace).GetDefaultTMLName();
		public string[] Categories => [
			WikiCategories.SpellBook
		];
		public override void SetStaticDefaults() {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.OnFire3];
		}
		public override void SetDefaults() {
			Item.DamageType = DamageClass.Magic;
			Item.damage = 30;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 23;
			Item.useAnimation = 23;
			Item.shoot = ModContent.ProjectileType<Corona_P>();
			Item.shootSpeed = 16f;
			Item.mana = 10;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item103;
			Item.autoReuse = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = 5; i > 0; i--) {
				Projectile.NewProjectile(
					source,
					position,
					velocity.RotatedByRandom(1f),
					type,
					damage,
					knockback
				);
			}
			return false;
		}
	}
	public class Corona_P : ModProjectile {
		public override string Texture => typeof(Blast_Furnace).GetDefaultTMLName();
		public virtual float FadeFrames => 25f;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 25;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.timeLeft = 65;
			Projectile.extraUpdates = 2;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool ShouldUpdatePosition() => false;
		public record struct Wave(float Frequency, float Amplitude, float Phase) {
			public readonly double Sample(double position) => Math.Sin(position * Frequency + Phase) * Amplitude;
			public override readonly string ToString() => $"sin(x * {Frequency} + {Phase}) * {Amplitude}";
		}
		public Vector2 originalDirection;
		public NPC target;
		public Wave[] waves;
		public Vector2 HeadOffset {
			get => new(Projectile.ai[0], Projectile.ai[1]);
			set => (Projectile.ai[0], Projectile.ai[1]) = value;
		}
		public override void OnSpawn(IEntitySource source) {
			waves = new Wave[Main.rand.Next(13, 21)];
			for (int i = 0; i < waves.Length; i++) {
				waves[i] = new(Main.rand.NextFloat(0.1f, 0.5f), Main.rand.NextFloat(0.02f, 0.1f), Main.rand.NextFloat(MathHelper.TwoPi));
			}
			originalDirection = Projectile.DirectionTo(Main.MouseWorld);
			float distanceFromTarget = 16 * 5f;
			distanceFromTarget *= distanceFromTarget;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.CanBeChasedBy()) {
					Vector2 pos = Main.MouseWorld;
					float between = pos.Clamp(npc.Hitbox).DistanceSQ(pos);
					if (distanceFromTarget > between) {
						distanceFromTarget = between;
						target = npc;
					}
				}
			}
		}
		public override void AI() {
			Projectile.Opacity = Projectile.timeLeft / FadeFrames;
			if (Projectile.TryGetOwner(out Player player)) Projectile.position = player.MountedCenter;
			if (++Projectile.ai[2] >= Projectile.oldPos.Length) return;
			Projectile.ai[0] += Projectile.velocity.X;
			Projectile.ai[1] += Projectile.velocity.Y;
			ProcessTick();
			double value = 0;
			for (int i = 0; i < waves.Length; i++) value += waves[i].Sample(Projectile.ai[2]);
			Projectile.velocity = Projectile.velocity.RotatedBy(value);
			if (target is not null) {
				if (Projectile.localNPCImmunity[target.whoAmI] != 0 || !target.CanBeChasedBy(Projectile)) target = null;
			}
			float speed = Projectile.velocity.Length();
			Vector2? targetDir = target?.DirectionFrom(Projectile.position + HeadOffset);
			Projectile.velocity = (Vector2.Lerp(
				(targetDir ?? originalDirection) * speed,
				Projectile.velocity,
				float.Clamp(Vector2.Dot((targetDir ?? originalDirection), HeadOffset.Normalized(out _)) * 2, 0, 1)
			) + (targetDir ?? Vector2.Zero) * speed * 0.25f).Normalized(out _) * speed;
			SetHitboxCache();
			void ProcessTick() {
				Projectile.oldPos[(int)Projectile.ai[2]] = HeadOffset;
				Projectile.oldRot[(int)Projectile.ai[2]] = Projectile.velocity.ToRotation() + MathHelper.Pi;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (polygonCache is null) return false;
			targetHitbox.Offset((-Projectile.position).ToPoint());
			return CollisionExtensions.PolygonIntersectsRect(polygonCache, targetHitbox);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, Main.rand.Next(60, 181));
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.WriteVector2(originalDirection);
			writer.Write((short)(target?.whoAmI ?? -1));
			writer.Write((byte)waves.Length);
			for (int i = 0; i < waves.Length; i++) {
				writer.Write(waves[i].Frequency);
				writer.Write(waves[i].Amplitude);
				writer.Write(waves[i].Phase);
			}
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			originalDirection = reader.ReadVector2();
			target = Main.npc.GetIfInRange(reader.ReadInt16());
			waves = new Wave[reader.ReadByte()];
			for (int i = 0; i < waves.Length; i++) {
				waves[i] = new Wave(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			}
		}
		(Vector2 start, Vector2 end)[] polygonCache;
		private void SetHitboxCache() {
			Vector2 width = new(BladeWidth, 0);
			Vector2 rot = width.RotatedBy(Projectile.rotation);
			Vector2 lastPos0 = -rot;
			Vector2 lastPos1 = rot;
			Vector2 nextPos0 = default, nextPos1 = default;
			int count = int.Min((int)Projectile.ai[2], Projectile.oldPos.Length);
			polygonCache = new (Vector2 start, Vector2 end)[count * 2 + 2];
			int lineIndex = 0;
			polygonCache[lineIndex++] = (lastPos1, lastPos0);
			for (int i = 0; i < count; i++) {
				Vector2 nextPos = Projectile.oldPos[i];
				rot = width.RotatedBy(Projectile.oldRot[i]);
				nextPos0 = nextPos - rot;
				nextPos1 = nextPos + rot;

				polygonCache[lineIndex++] = (lastPos0, nextPos0);
				polygonCache[lineIndex++] = (nextPos1, lastPos1);
				lastPos0 = nextPos0;
				lastPos1 = nextPos1;
			}
			polygonCache[lineIndex++] = (nextPos0, nextPos1);
		}
		private static readonly VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			Vector2[] oldPos = new Vector2[int.Min((int)Projectile.ai[2], Projectile.oldPos.Length)];
			for (int i = 0; i < oldPos.Length; i++) {
				oldPos[i] = Projectile.oldPos[i] + Projectile.position;
			}
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:LaserBlade"];
			miscShaderData.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
			miscShaderData.UseImage0(TextureAssets.Extra[ExtrasID.FlameLashTrailShape]);
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 1, 1, 0));
			miscShaderData.UseSaturation(-1);
			miscShaderData.UseOpacity(2);
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(oldPos, Projectile.oldRot, BladeSecondaryColors, BladeWidth,  -Main.screenPosition, true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();

			miscShaderData.UseSaturation(0.5f);
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(oldPos, Projectile.oldRot, BladeColors, BladeWidth, -Main.screenPosition, true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			float BladeWidth(float progressOnStrip) => this.BladeWidth;
			return false;
		}
		public virtual Color BladeColors(float progressOnStrip) => new Color(255, 69, 0, 32) * Projectile.Opacity;
		public virtual Color BladeSecondaryColors(float progressOnStrip) => new Color(218, 165, 32, 32) * Projectile.Opacity;
		public virtual int BladeWidth => 12;
	}
}
