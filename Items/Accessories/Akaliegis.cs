using MonoMod.Cil;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shield)]
	public class Akaliegis : ModItem {
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.defense = 3;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().akaliegis = true;
			DoEnduranceBuff(player);
		}
		public static void DoEnduranceBuff(Player player) {
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (projectile.owner != player.whoAmI || projectile.ModProjectile is not IArtifactMinion) continue;
				player.endurance += (1 - player.endurance) * 0.15f;
				break;
			}
		}
		public static void IL_Player_UpdateLifeRegen(ILContext il) {
			ILCursor c = new(il);
			try {
				c.GotoNext(MoveType.After,
					i => i.MatchLdarg0(),
					i => i.MatchLdfld<Player>(nameof(Player.statLife)),
					i => i.MatchLdarg0(),
					i => i.MatchLdfld<Player>(nameof(Player.statLifeMax2)),
					i => i.MatchBle(out _),
					i => i.MatchLdarg0(),
					i => i.MatchLdarg0(),
					i => i.MatchLdfld<Player>(nameof(Player.statLifeMax2)),
					i => i.MatchStfld<Player>(nameof(Player.statLife))
				);
				c.MoveAfterLabels();
				c.EmitLdarg0();
				c.EmitDelegate((Player player) => {
					if (player.statLife >= player.statLifeMax2 && player.OriginPlayer().akaliegis) {
						foreach (Projectile projectile in Main.ActiveProjectiles) {
							if (projectile.owner != player.whoAmI || projectile.ModProjectile is not IArtifactMinion artifactMinion) continue;
							artifactMinion.Life++;
							if (artifactMinion.Life > artifactMinion.MaxLife) artifactMinion.Life = artifactMinion.MaxLife;
						}
					}
				});
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(IL_Player_UpdateLifeRegen), e)) throw;
			}
		}
	}
}
