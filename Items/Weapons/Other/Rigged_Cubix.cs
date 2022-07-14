using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Other {
    public class Rigged_Cubix : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Rigged Cubix");
			Tooltip.SetDefault("");
			ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
			glowmask = Origins.AddGlowMask(this);
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 88;
			Item.crit = -3;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 3;
			Item.useAnimation = 54;
			Item.shootSpeed = 14;
			Item.mana = 4;
			Item.value = 5000;
            Item.shoot = ModContent.ProjectileType<Rigged_Cubix_P>();
			Item.rare = ItemRarityID.Green;
			Item.UseSound = null;
			Item.glowMask = glowmask;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(0.075f);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.itemAnimation != 0 && !player.CheckMana(Item, pay:true)) {
				return false;
			}
			SoundEngine.PlaySound(SoundID.Item12, position);
			return true;
		}
	}
    public class Rigged_Cubix_P : ModProjectile {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Rigged Cubix");
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.width = 26;
			Projectile.height = 26;
			//projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.timeLeft = 600;
			Projectile.localNPCHitCooldown = Projectile.timeLeft / 10;
			Projectile.aiStyle = 0;
			Projectile.tileCollide = false;
		}
        public override void AI() {
			//projectile.rotation += 16f * projectile.direction;
			Dust.NewDustPerfect(Projectile.Center, DustID.JungleTorch, Projectile.velocity).noGravity = true;
			Projectile.extraUpdates = 0;
			if (Projectile.ai[0] > 0) {
				int targetID = (int)Projectile.ai[0];
				NPC target = Main.npc[targetID];
				if (target.active && Projectile.localNPCImmunity[targetID] <= 0) {
					PolarVec2 velocity = (PolarVec2)Projectile.velocity;
					PolarVec2 diff = (PolarVec2)(target.Center - Projectile.Center);
					OriginExtensions.AngularSmoothing(ref velocity.Theta, diff.Theta, diff.R < 96 ? 0.35f : 0.25f);
					Projectile.velocity = (Vector2)velocity;
					//projectile.rotation += 0f * projectile.direction;
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
    }
}
