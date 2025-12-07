using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
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
					int projDrawType = longboneType == 1 ? ProjectileID.BoneArrow : Bone_Shard.ID;
					Main.instance.LoadProjectile(projDrawType);
					Texture2D texture = TextureAssets.Projectile[projDrawType].Value;
					Color color = projectile.GetAlpha(lightColor);
					for (int i = 1; i < 5; i++) {
						Main.EntitySpriteDraw(
							texture,
							new Vector2(projectile.position.X, projectile.position.Y + (projectile.height / 2) + projectile.gfxOffY)
								- projectile.velocity * i
								- Main.screenPosition,
							null,
							color * (0.5f - i * 0.1f),
							projectile.rotation,
							texture.Size() * 0.5f,
							projectile.scale,
							SpriteEffects.None,
						0);
					}
					return false;
				}
				default:
				return true;
			}
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			binaryWriter.Write((byte)longboneType);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			longboneType = binaryReader.ReadByte();
		}
	}
	public class Bone_Shard : ModProjectile {
		public static int ID { get; private set; }
		public override string Texture => "Origins/Projectiles/Weapons/BoneS_hard";
		public override void SetStaticDefaults() {
			ID = Type;
		}
	}
}
