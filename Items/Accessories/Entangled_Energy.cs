using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Dusts;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Entangled_Energy : ModItem {
		/// <summary>
		/// The maximum amount of seconds of reduction per second
		/// </summary>
		public static float MaxSecondsPerSecond => 1f;
		/// <param name="damage">the damage dealt by the hit which triggered the healing orb</param>
		/// <param name="trueMelee">whether or not the hit is "true melee" (directly uses the item hitbox, spears, etc.)</param>
		public static float RestorationPerHit(int damage, bool trueMelee) => (trueMelee ? 10 : 7) + MathF.Pow(damage, 0.66f) / (trueMelee ? 1.5f : 2);
		public static float DamageBonus(int missingLife) => Math.Clamp(MathF.Pow(missingLife / 350f, 1.5f), 0, 1) * 15;
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 30);
			Item.rare = ItemRarityID.Blue;
			Item.expert = true;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().entangledEnergy = true;
		}
	}
	public class Entangled_Energy_Lifesteal : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.tileCollide = false;
			Projectile.timeLeft = 300;
			Projectile.extraUpdates = 1;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active) Projectile.Kill();
			if (player.Hitbox.Contains(Projectile.Center.ToPoint())) {
				for (int i = 0; i < 12; i++) {
					Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, ModContent.DustType<Solution_D>(), 0f, 0f, 40, new(100, 255, 230), 1.1f);
					//Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Glass, 0f, 0f, 40, new(0.7f, 1f, 0.7f, 0f), 1.1f);
					dust.noGravity = true;
					dust.velocity += (dust.position - player.Center).SafeNormalize(default) * 4;
				}
				ref float entangledEnergyCount = ref player.OriginPlayer().entangledEnergyCount;
				int reduction = int.Min(Main.rand.RandomRound(Entangled_Energy.RestorationPerHit(Projectile.damage, Projectile.ai[1] == 1)), (int)entangledEnergyCount);
				player.potionDelay = int.Max(player.potionDelay - reduction, 0);
				entangledEnergyCount -= reduction;

				int sicknessDebuff = player.FindBuffIndex(BuffID.PotionSickness);
				if (sicknessDebuff != -1) player.buffTime[sicknessDebuff] = player.potionDelay;
				Projectile.Kill();
				return;
			}
			Vector2 direction = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
			float timeFactor = (++Projectile.ai[2] * 0.01f);
			direction *= (2 + timeFactor * timeFactor) - Vector2.Dot(Projectile.velocity.SafeNormalize(Vector2.Zero), direction);
			Projectile.velocity = (Projectile.velocity + direction).WithMaxLength(10);
			Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Solution_D>(), Projectile.velocity, 40, new(100, 255, 230), 0.8f).noGravity = true;
		}
	}
}
