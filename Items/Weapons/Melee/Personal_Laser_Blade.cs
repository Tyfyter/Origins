using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Tiles.Other;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Terraria.GameContent;
using Tyfyter.Utils;
using System.Linq;

namespace Origins.Items.Weapons.Melee {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Personal_Laser_Blade : ModItem, IElementalItem, ICustomWikiStat {
		public const int max_charge = 75;
		public string[] Categories => [
			"Sword"
		];
		public ushort Element => Elements.Fire;
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.damage = 104;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.DamageType = DamageClass.Melee;
			Item.width = 42;
			Item.height = 42;
			Item.crit = 10;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.noMelee = true;
			Item.shoot = Personal_Laser_Blade_P.ID;
			Item.shootSpeed = 1f;
			Item.knockBack = 1;
			Item.autoReuse = false;
			Item.useTurn = false;
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
			ref int laserBladeCharge = ref player.GetModPlayer<OriginPlayer>().laserBladeCharge;
			//damage += (laserBladeCharge * damage) / max_charge;
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			laserBladeCharge = 0;
			return false;
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			ref int laserBladeCharge = ref player.GetModPlayer<OriginPlayer>().laserBladeCharge;
			if (laserBladeCharge < max_charge) {
				laserBladeCharge = Math.Min(laserBladeCharge + 2, max_charge + 1);// increments by 2 since it's decrementing by 1 at the same rate
				if (laserBladeCharge >= max_charge) {
					/*for (int i = 0; i < 20; i++) {
						Dust.NewDust(
							player.position,
							player.width,
							player.height,
							DustID.GoldFlame
						);
					}*/
				}
			} else {
				laserBladeCharge = max_charge + 1;
			}
		}
		public override void UseItemFrame(Player player) {
			player.handon = Item.handOnSlot;
		}
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
		protected virtual float Rotation => Projectile.rotation + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.ai[1]);
		protected virtual Vector2 Origin => new(14, 25 + 11 * Projectile.ai[1]);
		protected virtual int HitboxSteps => 5;
		protected virtual float Startup => 0.25f;
		protected virtual float End => 0.25f;
		protected virtual float SwingStartVelocity => 1f;
		protected virtual float SwingEndVelocity => 1f;
		protected virtual float TimeoutVelocity => 1f;
		protected virtual float MinAngle => -2.5f;
		protected virtual float MaxAngle => 2.5f;
		protected Rectangle lastHitHitbox;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
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
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (!player.active || player.dead) {
				Projectile.Kill();
				return;
			}
			if (player.channel) {
				Projectile.timeLeft = player.itemTimeMax * Projectile.MaxUpdates;
				if (Projectile.owner == Main.myPlayer) {
					Projectile.velocity = (new Vector2(Player.tileTargetX, Player.tileTargetY).ToWorldCoordinates() - Projectile.Center).SafeNormalize(default);
				}
				player.SetDummyItemTime(player.itemTimeMax - 1);
				Projectile.ai[0] += 1f / player.itemTimeMax;
				if (Projectile.ai[0] >= 1) {
					Projectile.ai[0] = 1;
					player.TryCancelChannel(Projectile);
					Projectile.timeLeft += Projectile.timeLeft / 2;
				}
				Projectile.width = (int)(16 * (1 + Projectile.ai[0]));
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
			) * Projectile.ai[1];

			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			Projectile.Center = player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation);
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
			for (int j = 0; j <= HitboxSteps; j++) {
				Projectile.EmitEnchantmentVisualsAt(Projectile.position + vel * j, Projectile.width, Projectile.height);
			}
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + (Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width * 0.9f * HitboxSteps);
			Utils.PlotTileLine(Projectile.Center, end, Projectile.width, DelegateMethods.CutTiles);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			//Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.95f;
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width;
			for (int j = 0; j <= HitboxSteps; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
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
			trailDrawer.TrailColor = new(0, 35, 35, 0);
			trailDrawer.BladeColor = new(0, 255, 255, 128);
			trailDrawer.Length = Projectile.velocity.Length() * Projectile.width * 0.9f * HitboxSteps;
			trailDrawer.Draw(Projectile);
			return false;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 1 + Projectile.ai[0] * 0.5f;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.SourceDamage *= 1 + Projectile.ai[0] * 0.5f;
		}
		public struct LaserBladeDrawer {

			private static VertexStrip _vertexStrip = new();

			public Color TrailColor;
			public Color BladeColor;

			public float Length;

			int[] spriteDirections;
			public void Draw(Projectile proj) {
				{
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
					Vector2[] positions = new Vector2[16];
					for (int i = 0; i < positions.Length; i++) {
						positions[i] = proj.Center + velocity * (i / (float)positions.Length);
					}
					float[] rotations = [..Enumerable.Repeat(proj.velocity.ToRotation() + proj.rotation, positions.Length)];
					miscShaderData.UseImage1(TextureAssets.Extra[193]);
					miscShaderData.UseImage0(TextureAssets.Extra[189]);
					miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 1, 1, 0));
					miscShaderData.UseSaturation(-1);
					miscShaderData.UseOpacity(2);
					miscShaderData.Apply();
					_vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, BladeColors, BladeWidth, -Main.screenPosition, true);
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
			private readonly float BladeWidth(float progressOnStrip) {
				return 24 - 8 * progressOnStrip;
			}
		}
	}
}
