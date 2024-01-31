using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Magic {
    public class Bad_Hand : ModItem {
		
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.VampireKnives);
			Item.damage = 48;
			Item.crit = 4;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.useTime = 6;
			Item.useAnimation = 24;
			Item.shootSpeed = 5;
			Item.mana = 8;
			Item.knockBack = 1;
			Item.useAmmo = AmmoID.None;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Blue;
            Item.reuseDelay = 9;
        }
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(0.25f);
            switch (Main.rand.Next(2)) {
                case 0:
                type = ModContent.ProjectileType<Bad_Hand_Black_P>();
                break;

                case 1:
                type = ModContent.ProjectileType<Bad_Hand_Red_P>();
                break;
            }
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.itemAnimation != 0 && !player.CheckMana(Item, pay: true)) {
				return false;
			}
			SoundEngine.PlaySound(SoundID.Item43, position);
			return true;
		}
	}
    public class Bad_Hand_Black_P : ModProjectile {
        public override void SetStaticDefaults() {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults() {
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.width = 12;
            Projectile.height = 14;
            Projectile.timeLeft = 600;
            Projectile.aiStyle = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }
        public override void AI() {
            Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, 0, 0, 255, new Color(20, 200, 30), 1f);
            dust.noGravity = false;
            dust.velocity *= 3f;
            Projectile.extraUpdates = 0;
            if (Projectile.ai[0] > 0) {
                int targetID = (int)Projectile.ai[0];
                NPC target = Main.npc[targetID];
                if (target.active && Projectile.localNPCImmunity[targetID] <= 0) {
                    PolarVec2 velocity = (PolarVec2)Projectile.velocity;
                    PolarVec2 diff = (PolarVec2)(target.Center - Projectile.Center);
                    OriginExtensions.AngularSmoothing(ref velocity.Theta, diff.Theta, diff.R < 96 ? 0.35f : 0.25f);
                    Projectile.velocity = (Vector2)velocity;
                    Projectile.extraUpdates = 1;
                } else {
                    Projectile.ai[0] = -1;
                }
            } else {
                float distanceFromTarget = 300f;
                for (int i = 0; i < Main.maxNPCs; i++) {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy() && Projectile.localNPCImmunity[i] <= 0) {
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool inRange = between < distanceFromTarget;
                        if (inRange) {
                            distanceFromTarget = between;
                            Projectile.ai[0] = npc.whoAmI + 0.1f;
                        }
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor) {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame),
                lightColor,
                Projectile.rotation,
                new Vector2(36, 6),
                Projectile.scale,
                Projectile.direction < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None,
            0);
            return false;
        }
    }
    public class Bad_Hand_Red_P : ModProjectile {
        public override void SetStaticDefaults() {
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults() {
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.width = 12;
            Projectile.height = 14;
            Projectile.timeLeft = 600;
            Projectile.aiStyle = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }
        public override void AI() {
            Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, 0, 0, 255, new Color(20, 200, 30), 1f);
            dust.noGravity = false;
            dust.velocity *= 3f;
            Projectile.extraUpdates = 0;
            if (Projectile.ai[0] > 0) {
                int targetID = (int)Projectile.ai[0];
                NPC target = Main.npc[targetID];
                if (target.active && Projectile.localNPCImmunity[targetID] <= 0) {
                    PolarVec2 velocity = (PolarVec2)Projectile.velocity;
                    PolarVec2 diff = (PolarVec2)(target.Center - Projectile.Center);
                    OriginExtensions.AngularSmoothing(ref velocity.Theta, diff.Theta, diff.R < 96 ? 0.35f : 0.25f);
                    Projectile.velocity = (Vector2)velocity;
                    Projectile.extraUpdates = 1;
                } else {
                    Projectile.ai[0] = -1;
                }
            } else {
                float distanceFromTarget = 300f;
                for (int i = 0; i < Main.maxNPCs; i++) {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy() && Projectile.localNPCImmunity[i] <= 0) {
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool inRange = between < distanceFromTarget;
                        if (inRange) {
                            distanceFromTarget = between;
                            Projectile.ai[0] = npc.whoAmI + 0.1f;
                        }
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor) {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame),
                lightColor,
                Projectile.rotation,
                new Vector2(36, 6),
                Projectile.scale,
                Projectile.direction < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None,
            0);
            return false;
        }
    }
}
