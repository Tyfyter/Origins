using Origins.Dev;
using Origins.Items.Materials;
using Origins.Tiles.Other;
using PegasusLib;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Personal_Laser_Blade : ModItem, IElementalItem, ICustomWikiStat {
		public const int max_charge = 75;
		public string[] Categories => [
			WikiCategories.Sword
		];
		public ushort Element => Elements.Fire;
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			OriginsSets.Items.SwungNoMeleeMelees[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 185;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.DamageType = DamageClass.Melee;
			Item.width = 42;
			Item.height = 42;
			Item.crit = 10;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = Personal_Laser_Blade_P.ID;
			Item.shootSpeed = 1f;
			Item.knockBack = 1;
			Item.autoReuse = false;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item45;
			Item.glowMask = glowmask;
			Item.channel = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 13)
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddIngredient(ModContent.ItemType<Rotor>(), 2)
			.AddIngredient(ModContent.ItemType<Rubber>(), 8)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			return true;
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			ref int laserBladeCharge = ref player.GetModPlayer<OriginPlayer>().laserBladeCharge;
		}
		public override void UseItemFrame(Player player) {
			player.handon = Item.handOnSlot;
		}
		public override bool MeleePrefix() => true;
	}
	public class Personal_Laser_Blade_P : ModProjectile, IElementalProjectile {
		public const int trail_length = 20;
		public ushort Element => Elements.Fire;
		public override string Texture => "Origins/Items/Weapons/Melee/Personal_Laser_Blade";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = trail_length * 2;
			ID = Type;
		}
		protected const int HitboxSteps = 5;
		protected const float Startup = 0.25f;
		protected const float End = 0.25f;
		protected const float SwingStartVelocity = 1f;
		protected const float SwingEndVelocity = 1f;
		protected const float TimeoutVelocity = 1f;
		protected const float MinAngle = -2.5f;
		protected const float MaxAngle = 2.5f;
		protected Rectangle lastHitHitbox;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 3;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
			Projectile.noEnchantmentVisuals = true;
			DrawHeldProjInFrontOfHeldItemAndArms = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				Projectile.scale *= itemUse.Item.scale;
				itemUse.Player.ApplyMeleeScale(ref Projectile.scale);
				Projectile.ai[1] = itemUse.Player.direction;
			}
		}
		protected float SwingFactor {
			get => Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (!player.active || player.dead) {
				Projectile.Kill();
				return;
			}
			if (player.channel) {
				Projectile.timeLeft = player.itemTimeMax * Projectile.MaxUpdates;
				if (Projectile.owner == Main.myPlayer) {
					Vector2 newVel = (Main.MouseWorld - Projectile.Center).SafeNormalize(default);
					if (Projectile.velocity != newVel) {
						Projectile.velocity = newVel;
						Projectile.netUpdate = true;
					}
				}
				player.SetDummyItemTime(player.itemTimeMax - 1);
				Projectile.ai[0] += 1f / Projectile.timeLeft;
				if (Projectile.ai[0] >= 1) {
					Projectile.ai[0] = 1;
					player.channel = false;
					Projectile.timeLeft += Projectile.timeLeft / 2;
				}
				Projectile.width = (int)(16 * (1 + Projectile.ai[0] * Projectile.ai[0]));
				Projectile.height = Projectile.width;
			}
			if (player.itemTime <= 2) {
				Projectile.localAI[2] = 1;
			}
			float updateOffset = (Projectile.MaxUpdates - (Projectile.numUpdates + 1)) / (float)(Projectile.MaxUpdates + 1);
			SwingFactor = ((player.itemTime - updateOffset) / (float)player.itemTimeMax) * (1 + Startup + End) - End;
			if (SwingFactor > 0) SwingFactor = MathHelper.Lerp(MathF.Pow(SwingFactor, 2f), MathF.Pow(SwingFactor, 0.5f), SwingFactor * SwingFactor);
			if (Projectile.localAI[2] == 1) {
				player.SetDummyItemTime(2);
				SwingFactor = 0;
			}
			Projectile.rotation = MathHelper.Lerp(
				MaxAngle,
				MinAngle,
				MathHelper.Clamp(SwingFactor, 0, 1)
			) * Projectile.ai[1] * player.gravDir;

			float realRotation = Projectile.rotation * player.gravDir + Projectile.velocity.ToRotation() * player.gravDir;
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			Projectile.Center = player.GetCompositeArmPosition(false);
			player.itemLocation = Projectile.Center + GeometryUtils.Vec2FromPolar(26, realRotation + 0.3f * player.direction);
			player.itemRotation = player.compositeFrontArm.rotation;
			player.direction = Math.Sign(Projectile.velocity.X);
			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
			}
			EmitEnchantmentVisuals();
		}
		public virtual void EmitEnchantmentVisuals() {
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width * 0.95f;
			float velocityMult = 0;
			float rotMult = 0.15f;
			if (Projectile.localAI[2] == 0) {
				if (Main.player[Projectile.owner].channel) {
					velocityMult = 2;
				} else {
					velocityMult = 8;
					rotMult = 0.05f;
				}
			}
			Color dustColor = Color.Magenta;
			switch (GetBladeColor()) {
				case BladeColor.DEFAULT:
				dustColor = new(0, 225, 255, 64);
				break;
				case BladeColor.STUN:
				dustColor = new(255, 255, 0, 64);
				break;
				case BladeColor.PULSE:
				dustColor = new(80, 255, 219, 64);
				break;
				case BladeColor.CORAL:
				dustColor = new(255, 32, 20, 64);
				break;
				case BladeColor.CHRYSALIS:
				dustColor = new(12, 168, 10, 32);
				break;
				case BladeColor.FAILURE:
				dustColor = new(156, 191, 255, 64);
				break;
			}
			for (int j = 0; j <= HitboxSteps; j++) {
				Projectile.EmitEnchantmentVisualsAt(Projectile.position + vel * j, Projectile.width, Projectile.height);
				if (j > 1 && Main.rand.NextFloat(2 * Projectile.MaxUpdates) < 1 + Projectile.ai[0]) {
					Dust dust = Dust.NewDustDirect(
						Projectile.position + vel * j,
						Projectile.width, Projectile.height,
						DustID.PortalBoltTrail,
						newColor: dustColor
					);
					dust.velocity = dust.velocity * 0.25f + Projectile.velocity.RotatedBy(Projectile.rotation * rotMult) * velocityMult;
					dust.position += dust.velocity * 2;
					dust.noGravity = true;
				}
			}
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + (Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width * HitboxSteps);
			Utils.PlotTileLine(Projectile.Center, end, Projectile.width, DelegateMethods.CutTiles);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width;
			Vector2 additionalOffset = vel.SafeNormalize(default) * 12;
			for (int j = 0; j <= HitboxSteps; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j + additionalOffset;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					lastHitHitbox = hitbox;
					return true;
				}
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			LaserBladeDrawer trailDrawer = default;
			switch (GetBladeColor()) {
				case BladeColor.DEFAULT:
				trailDrawer.TrailColor = new(0, 35, 35, 0);
				trailDrawer.BladeColor = new(0, 255, 255, 128);
				trailDrawer.BladeSecondaryColor = new(0, 180, 255, 64);
				break;
				case BladeColor.STUN:
				trailDrawer.TrailColor = new(35, 35, 0, 0);
				trailDrawer.BladeColor = new(255, 255, 0, 128);
				trailDrawer.BladeSecondaryColor = new(255, 255, 130, 64);
				break;
				case BladeColor.PULSE:
				trailDrawer.TrailColor = new(15, 35, 30, 0);
				trailDrawer.BladeColor = new(80, 255, 219, 128);
				trailDrawer.BladeSecondaryColor = new(130, 255, 255, 64);
				break;
				case BladeColor.CORAL:
				trailDrawer.TrailColor = new(35, 17, 11, 0);
				trailDrawer.BladeColor = new(240, 128, 128, 128);
				trailDrawer.BladeSecondaryColor = new(255, 127, 80, 64);
				break;
				case BladeColor.CHRYSALIS:
				trailDrawer.TrailColor = new(11, 84, 91, 255);
				trailDrawer.BladeColor = new(88, 196, 84, 64);
				trailDrawer.BladeSecondaryColor = new(11, 84, 91, 64);
				break;
				case BladeColor.FAILURE:
				trailDrawer.TrailColor = new(156, 191, 255, 255);
				trailDrawer.BladeColor = new(255, 255, 255, 64);
				trailDrawer.BladeSecondaryColor = new(156, 191, 255, 64);
				break;
			}
			trailDrawer.Length = Projectile.velocity.Length() * Projectile.width * 0.9f * HitboxSteps;
			trailDrawer.Draw(Projectile);
			return false;
		}
		public BladeColor GetBladeColor() {
			switch ((Main.player.GetIfInRange(Projectile.owner)?.name ?? "").ToLower()) {
				default:
				return BladeColor.DEFAULT;
				case "ceroba":
				return BladeColor.STUN;
				case "reivax" or "dio":
				return BladeColor.CORAL;
				case "jennifer" or "jennifer_alt" or "faust":
				return BladeColor.CHRYSALIS;
				case "chee" or "xiqi" or "chrersis":
				return BladeColor.FAILURE;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 1 + Projectile.ai[0] * 0.5f;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.SourceDamage *= 1 + Projectile.ai[0] * 0.5f;
		}
		public enum BladeColor {
			DEFAULT,
			STUN,
			PULSE,
			CORAL,
			CHRYSALIS,
			FAILURE
		}
		public struct LaserBladeDrawer {

			private static VertexStrip _vertexStrip = new();

			public Color TrailColor;
			public Color BladeColor;
			public Color BladeSecondaryColor;

			public float Length;

			int[] spriteDirections;
			public void Draw(Projectile proj) {
				if (!Main.player[proj.owner].channel) {
					MiscShaderData miscShaderData = GameShaders.Misc["EmpressBlade"];
					int num = 1;//1
					int num2 = 0;//0
					int num3 = 0;//0
					float w = 0.6f;//0.6f
					miscShaderData.UseShaderSpecificData(new Vector4(num, num2, num3, w));
					miscShaderData.Apply();
					float[] oldRot = new float[proj.oldRot.Length];
					Vector2[] oldPos = new Vector2[proj.oldPos.Length];
					float baseRot = proj.velocity.ToRotation() + MathHelper.PiOver2;
					Vector2 move = new(Length - 30, 0);
					for (int i = 0; i < oldPos.Length; i++) {
						if (proj.oldPos[i] == default) {
							Array.Resize(ref oldRot, i);
							Array.Resize(ref oldPos, i);
							if (i == 0) return;
							break;
						}
						oldRot[i] = proj.oldRot[i] + baseRot;
						oldPos[i] = proj.oldPos[i] + move.RotatedBy(oldRot[i] - MathHelper.PiOver2);
					}
					spriteDirections = proj.oldSpriteDirection;
					_vertexStrip.PrepareStrip(oldPos, oldRot, AfterimageColors, AfterimageWidth, -Main.screenPosition + proj.Size / 2f, oldPos.Length, includeBacksides: true);
					_vertexStrip.DrawTrail();
					Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				}
				{
					MiscShaderData miscShaderData = GameShaders.Misc["Origins:LaserBlade"];
					Vector2 velocity = proj.velocity.RotatedBy(proj.rotation) * Length * 1.333f;
					Vector2[] positions = new Vector2[15];
					for (int i = 0; i < positions.Length; i++) {
						positions[i] = proj.Center + velocity * ((i + 1) / (float)(positions.Length + 1));
					}
					float[] rotations = [..Enumerable.Repeat(proj.velocity.ToRotation() + proj.rotation, positions.Length)];
					miscShaderData.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
					miscShaderData.UseImage0(TextureAssets.Extra[ExtrasID.FlameLashTrailShape]);
					miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 1, 1, 0));
					miscShaderData.UseSaturation(-1);
					miscShaderData.UseOpacity(2);
					miscShaderData.Apply();
					_vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, BladeSecondaryColors, BladeWidth, -Main.screenPosition, true);
					_vertexStrip.DrawTrail();
					Main.pixelShader.CurrentTechnique.Passes[0].Apply();

					miscShaderData.UseSaturation(0.5f);
					miscShaderData.Apply();
					_vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, BladeColors, BladeWidth, -Main.screenPosition, true);
					_vertexStrip.DrawTrail();
					Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				}
			}

			private readonly Color AfterimageColors(float progressOnStrip) {
				if (float.IsNaN(progressOnStrip)) return default;
				Color result = TrailColor * MathHelper.Lerp(1f, 0.5f, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, clamped: true));
				result.A /= 2;
				result *= spriteDirections[Math.Max((int)(progressOnStrip * spriteDirections.Length) - 1, 0)];
				return result;
			}
			private readonly float AfterimageWidth(float progressOnStrip) {
				return 60;
			}

			private readonly Color BladeColors(float progressOnStrip) {
				return BladeColor * (progressOnStrip == 0 ? 0 : 1);
			}
			private readonly Color BladeSecondaryColors(float progressOnStrip) {
				return BladeSecondaryColor * (progressOnStrip == 0 ? 0 : 1);
			}
			private readonly float BladeWidth(float progressOnStrip) {
				return 24 - 8 * progressOnStrip;
			}
		}
	}
}
