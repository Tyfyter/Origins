using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Melee;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles.Weapons {
	public class Defiled_Spike_Explosion : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.timeLeft = 600;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.hide = true;
			Projectile.rotation = Main.rand.NextFloatDirection();
			Projectile.tileCollide = false;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj) {
				Projectile.npcProj = parentProj.npcProj;
				Projectile.hostile = false;
				Projectile.friendly = parentProj.friendly;
				Projectile.DamageType = parentProj.DamageType;
			} else if (source is EntitySource_OnHit onHit) {
				switch (onHit.Context) {
					case nameof(Spiker_Sword):
					Projectile.friendly = true;
					Projectile.DamageType = DamageClass.Melee;
					break;
				}
			} else if (source is EntitySource_ItemUse itemUse) {
				Projectile.friendly = true;
				Projectile.DamageType = itemUse.Item.DamageType;
			}
		}
		public override bool? CanHitNPC(NPC target) => false;
		public override bool CanHitPlayer(Player target) => false;
		public override bool CanHitPvp(Player target) => false;
		public override void AI() {
			if (Projectile.ai[0] > 0) {
				Projectile.ai[0]--;
				Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					(Vector2)new PolarVec2(Main.rand.NextFloat(8, 16), Projectile.ai[1]++),
					Defiled_Spike_Explosion_Spike.ID,
					Projectile.damage,
					0,
					Projectile.owner,
					ai1: Projectile.whoAmI,
					ai2: Projectile.ai[2]
				);
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(new BitsByte(Projectile.npcProj, Projectile.hostile, Projectile.friendly));
			writer.Write(Projectile.DamageType.Type);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			BitsByte bits = reader.ReadByte();
			Projectile.npcProj = bits[0];
			Projectile.hostile = bits[1];
			Projectile.friendly = bits[2];
			Projectile.DamageType = DamageClassLoader.GetDamageClass(reader.ReadInt32());
		}
	}
	public class Defiled_Spike_Explosion_Spike : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
		public static int ID { get; private set; }
		Vector2 realPosition;
		public override void SetStaticDefaults() {
			ID = Projectile.type;
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.timeLeft = Main.rand.Next(22, 25);
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 0;
			Projectile.hide = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj) {
				Projectile.npcProj = parentProj.npcProj;
				Projectile.hostile = parentProj.npcProj && !parentProj.friendly;
				Projectile.friendly = parentProj.friendly;
				Projectile.DamageType = parentProj.DamageType;
			}
			if (Projectile.ai[2] == 0) Projectile.ai[2] = 1;
			realPosition = Projectile.Center;
		}
		public Projectile ParentProjectile => Main.projectile[(int)Projectile.ai[1]];
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Projectile.Center = realPosition - Projectile.velocity;
			if (movementFactor == 0f) {
				movementFactor = 1f;
				Projectile.netUpdate = true;
			}
			if (Projectile.timeLeft > 18) {
				movementFactor += 1f;
			}
			Projectile.position += Projectile.velocity * (movementFactor + 1) * Projectile.ai[2];
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.rotation += MathHelper.PiOver2;
			ParentProjectile.timeLeft = 7;
		}
		public override bool? CanHitNPC(NPC target) {
			if (ParentProjectile.localNPCImmunity[target.whoAmI] == 0) {
				return null;
			}
			return false;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
			overWiresUI.Add(index);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			ParentProjectile.localNPCImmunity[target.whoAmI] = -1;
		}
		public override bool PreDraw(ref Color lightColor) {
			float totalLength = Projectile.velocity.Length() * movementFactor;
			int avg = (lightColor.R + lightColor.G + lightColor.B) / 3;
			lightColor = Color.Lerp(lightColor, new Color(avg, avg, avg), 0.5f);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 18, (int)Math.Min(58, totalLength)), lightColor, Projectile.rotation, new Vector2(9, 0), Projectile.ai[2], SpriteEffects.None, 0);
			Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.Zero);
			Texture2D texture = Mod.Assets.Request<Texture2D>("Projectiles/Weapons/Dismay_Mid").Value;
			Vector2 pos;
			for (float i = 58 * Projectile.ai[2]; i < totalLength; i += Math.Min(totalLength - i, 58 * Projectile.ai[2])) {
				pos = (Projectile.Center - Main.screenPosition) - (offset * i * Projectile.ai[2]);
				//lightColor = Projectile.GetAlpha(new Color(Lighting.GetColor((pos + Projectile.velocity * 2).ToTileCoordinates()).ToVector4()));
				int frameSize = Math.Min(58, (int)(totalLength - i));
				Main.EntitySpriteDraw(texture, pos, new Rectangle(0, 58 - frameSize, 18, frameSize), lightColor, Projectile.rotation, new Vector2(9, 0), Projectile.ai[2], SpriteEffects.None, 0);
			}
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(new BitsByte(Projectile.npcProj, Projectile.hostile, Projectile.friendly));
			writer.Write(Projectile.DamageType.Type);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			BitsByte bits = reader.ReadByte();
			Projectile.npcProj = bits[0];
			Projectile.hostile = bits[1];
			Projectile.friendly = bits[2];
			Projectile.DamageType = DamageClassLoader.GetDamageClass(reader.ReadInt32());
		}
	}
}
