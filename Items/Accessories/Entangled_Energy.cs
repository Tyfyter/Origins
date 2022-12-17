using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Entangled_Energy : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Entangled Energy");
            Tooltip.SetDefault("Fiberglass weapons gain damage based on defense\nFiberglass weapons gain lifesteal");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.rare = ItemRarityID.Master;
            Item.master = true;
            Item.value = Item.buyPrice(gold: 5);
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().entangledEnergy = true;
        }
    }
    public class Entangled_Energy_Lifesteal : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public override void SetDefaults() {
            Projectile.timeLeft = 300;
        }
        public override void AI() {
            Player player = Main.player[Projectile.owner];
            if (player.dead || !player.active) Projectile.Kill();
            if (player.Hitbox.Contains(Projectile.Center.ToPoint())) {
                player.statLife += Projectile.damage;
                player.HealEffect(Projectile.damage);
                Projectile.Kill();
                return;
            }
            Vector2 unit = (player.Center - Projectile.Center).WithMaxLength(12);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, unit, 0.1f);
            Dust.NewDustPerfect(Projectile.Center, DustID.Glass, Vector2.Zero);
        }
    }
}
