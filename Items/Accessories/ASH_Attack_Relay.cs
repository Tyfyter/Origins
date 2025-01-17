using Origins.Dev;
using Origins.Layers;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class ASH_Attack_Relay : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override void SetStaticDefaults() {
			Accessory_Glow_Layer.AddGlowMask<Back_Glow_Layer>(Item.backSlot, Texture + "_Back_Glow");
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 34);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ModContent.ProjectileType<ASH_Attack_Relay_Dust>();
			Item.damage = 17;
			Item.ArmorPenetration = 6;
			Item.knockBack = 6;
			Item.useTime = 12;
			Item.useAnimation = 6;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<OriginPlayer>().cinderSealItem = Item;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) *= 1.05f;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(
				player.GetSource_Accessory_OnHurt(source.Item, attacker: null),
				position,
				new Vector2(1, 0).RotatedByRandom(MathHelper.Pi),
				type,
				damage,
				knockback,
				player.whoAmI
			);
			return false;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Comb>()
			.AddIngredient<Seal_Of_Cinders>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class ASH_Attack_Relay_Dust : ModProjectile {
		public override string Texture => "Origins/Items/Accessories/Seal_Of_Cinders_Dust";
		public override void SetDefaults() {
			Projectile.tileCollide = false;
			Projectile.friendly = false;
			Projectile.timeLeft = 15;
			Projectile.width = Projectile.height = 6;
		}
		public override void AI() {
			Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ash, Alpha: 100).noGravity = true;
			Projectile.velocity *= 0.95f;
			float targetWeight = 300;
			Vector2 targetPos = default;
			bool foundTarget = false;
			for (int i = 0; i < 200; i++) {
				NPC currentNPC = Main.npc[i];
				if (currentNPC.CanBeChasedBy(this)) {
					Vector2 currentPos = currentNPC.Center;
					float dist = Math.Abs(Projectile.Center.X - currentPos.X) + Math.Abs(Projectile.Center.Y - currentPos.Y);
					if (dist < targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, currentNPC.position, currentNPC.width, currentNPC.height)) {
						targetWeight = dist;
						targetPos = currentPos;
						foundTarget = true;
					}
				}
			}
			if (foundTarget) {
				Vector2 targetVelocity = (targetPos - Projectile.Center).SafeNormalize(-Vector2.UnitY) * 8;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.083333336f);
			}
		}
		public override void OnKill(int timeLeft) {
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item38.WithVolumeScale(0.5f), Projectile.Center);
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45, Projectile.Center);
			Projectile.NewProjectile(
				Projectile.GetSource_Death(),
				Projectile.Center,
				Vector2.Zero,
				ModContent.ProjectileType<ASH_Attack_Relay_Explosion>(),
				Projectile.originalDamage,
				Projectile.knockBack,
				Projectile.owner
			);
		}
	}
	public class ASH_Attack_Relay_Explosion : ModProjectile {
		public override string Texture => "Origins/Items/Accessories/Seal_Of_Cinders_Explosion";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 19;
		}
		public override void SetDefaults() {
			Projectile.tileCollide = false;
			Projectile.width = Projectile.height = 110;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
		}
		public override void AI() {
			if (++Projectile.frameCounter > 1) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.timeLeft = 0;
			}
			Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.1f, 0f) * MathHelper.Min(Projectile.frame / 8f, 1));
		}
		public override bool PreDraw(ref Color lightColor) {
			lightColor = new(1f, 1f, 1f, 0f);
			return true;
		}
	}
}