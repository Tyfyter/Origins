using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Tiles.Other;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

using Origins.Dev;
using PegasusLib;
namespace Origins.Items.Weapons.Magic {
    public class Rigged_Cubix : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => [
            "MagicGun"
        ];
        public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
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
			Item.shootSpeed = 12;
			Item.mana = 4;
			Item.shoot = ModContent.ProjectileType<Rigged_Cubix_P>();
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ButterscotchRarity.ID;
			Item.UseSound = null;
			Item.glowMask = glowmask;
		}
        public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Batholith_Item>(), 16)
            .AddIngredient(ModContent.ItemType<Formium_Bar>(), 25)
            .AddTile(TileID.LunarCraftingStation) //Interstellar Sampler
            .Register();
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(Origins.Sounds.PhaserCrash, player.itemLocation);
			return null;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(0.5f);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.itemAnimation != 0 && !player.CheckMana(Item, pay: true)) {
				return false;
			}
			player.manaRegenDelay = (int)player.maxRegenDelay;
			return true;
		}
	}
	public class Rigged_Cubix_P : ModProjectile {
		
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.width = 26;
			Projectile.height = 26;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.timeLeft = 600;
			Projectile.localNPCHitCooldown = Projectile.timeLeft / 10;
			Projectile.aiStyle = 0;
			Projectile.tileCollide = false;
		}
		public override void AI() {
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
