using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using System.IO;
namespace Origins.Items.Weapons.Ranged {
	public class Longbone : ModItem, ICustomWikiStat {
		internal static int t = ProjectileID.WoodenArrowFriendly;
        public string[] Categories => [
            "Bow"
        ];
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GoldBow);
			Item.damage = 28;
			Item.knockBack = 5;
			Item.crit = 4;
			Item.useTime = Item.useAnimation = 16;
			Item.shoot = ProjectileID.BoneArrow;
			Item.shootSpeed = 9;
			Item.width = 24;
			Item.height = 56;
			Item.autoReuse = false;
			Item.value = Item.sellPrice(silver: 35);
			Item.rare = ItemRarityID.Green;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8f, 0);
	}
	public class Longbone_Global : GlobalProjectile {
		int longboneType = 0;
		bool initialized = false;
		public override bool InstancePerEntity => true;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.arrow;
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (source is EntitySource_ItemUse_WithAmmo withAmmo && withAmmo.Item.type == Longbone.ID) {
				longboneType = 1;
			} else if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parent && parent.TryGetGlobalProjectile(out Longbone_Global longboneProj) && longboneProj.longboneType == 1) {
				longboneType = 2;
			}
		}
		public override void AI(Projectile projectile) {
			if (longboneType != 0 && !initialized) {
				initialized = true;
				projectile.timeLeft = longboneType == 1? 30 : 120;
				projectile.extraUpdates = 1;
				projectile.localNPCHitCooldown = 10;
				projectile.usesLocalNPCImmunity = true;
			}
		}
		public override void OnKill(Projectile projectile, int timeLeft) {
			if (longboneType == 1) {
				for (int i = 0; i < 3; i++) {
					projectile.localNPCImmunity.CopyTo(
						Projectile.NewProjectileDirect(
							projectile.GetSource_Death(),
							projectile.Center,
							projectile.velocity.RotatedByRandom(0.3f),
							projectile.type,
							projectile.damage / 5,
							2,
							projectile.owner
						).localNPCImmunity,
					0);
				}
				SoundEngine.PlaySound(SoundID.NPCHit2.WithVolume(0.75f).WithPitchRange(0.1f, 0.2f), projectile.Center);
			}
		}
		public override bool PreDraw(Projectile projectile, ref Color lightColor) {
			switch (longboneType) {
				case 1 or 2: {
					switch ((int)projectile.localAI[0]) {
						case ProjectileID.FireArrow:
						lightColor.R += (byte)Math.Min(80, 255 - lightColor.R);
						lightColor.G += (byte)Math.Min(60, 255 - lightColor.G);
						lightColor.B += (byte)Math.Min(10, 255 - lightColor.B);
						Dust.NewDust(projectile.Center, 0, 0, DustID.Torch);
						break;
						case ProjectileID.FrostburnArrow:
						lightColor.G += (byte)Math.Min(60, 255 - lightColor.G);
						lightColor.B += (byte)Math.Min(80, 255 - lightColor.B);
						Dust.NewDust(projectile.Center, 0, 0, DustID.IceTorch);
						break;
						case ProjectileID.CursedArrow:
						lightColor.R += (byte)Math.Min(30, 255 - lightColor.R);
						lightColor.G += (byte)Math.Min(80, 255 - lightColor.G);
						Dust.NewDustDirect(projectile.Center, 0, 0, DustID.BubbleBurst_Green, Scale: 0.75f).velocity *= 0.5f;
						break;
						case ProjectileID.IchorArrow:
						lightColor.R += (byte)Math.Min(80, 255 - lightColor.R);
						lightColor.G += (byte)Math.Min(80, 255 - lightColor.G);
						Dust.NewDust(projectile.Center, 0, 0, DustID.GoldFlame);
						break;
						case ProjectileID.VenomArrow:
						Dust.NewDustDirect(projectile.Center, 0, 0, DustID.Water_Corruption).noGravity = true;
						break;
						case ProjectileID.HellfireArrow:
						lightColor.R += (byte)Math.Min(80, 255 - lightColor.R);
						lightColor.G += (byte)Math.Min(60, 255 - lightColor.G);
						lightColor.B += (byte)Math.Min(10, 255 - lightColor.B);
						Dust.NewDust(projectile.Center, 0, 0, DustID.Torch);
						break;
					}
					int projDrawType = longboneType == 1 ? ProjectileID.BoneArrow : Bone_Shard.ID;
					Main.instance.LoadProjectile(projDrawType);
					Texture2D texture = TextureAssets.Projectile[projDrawType].Value;
					for (int i = 1; i < 5; i++) {
						float x = projectile.velocity.X * i;
						float y = projectile.velocity.Y * i;
						Color color = projectile.GetAlpha(lightColor);
						float a = 0f;
						switch (i) {
							case 1:
							a = 0.4f;
							break;
							case 2:
							a = 0.3f;
							break;
							case 3:
							a = 0.2f;
							break;
							case 4:
							a = 0.1f;
							break;
						}
						color.R = (byte)(color.R * a);
						color.G = (byte)(color.G * a);
						color.B = (byte)(color.B * a);
						color.A = (byte)(color.A * a);
						Main.EntitySpriteDraw(texture, new Vector2(projectile.position.X - Main.screenPosition.X - x, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY - y), new Rectangle(0, 0, texture.Width, texture.Height), color, projectile.rotation, new Vector2(texture.Width / 2f, texture.Height / 2f), projectile.scale, SpriteEffects.None, 0);
					}
					return false;
				}
				default:
				return true;
			}
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			if (longboneType != 0) bitWriter.WriteBit(longboneType == 2);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			if (bitReader.MaxBits != 0) longboneType = bitReader.ReadBit() ? 2 : 1;
		}
	}
	public class Bone_Shard : ModProjectile {
		public static int ID { get; private set; } = -1;
		public override string Texture => "Origins/Projectiles/Weapons/BoneS_hard";
		public override void SetStaticDefaults() {
			ID = Type;
		}
	}
}
