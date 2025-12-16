using Origins.NPCs;
using Origins.Projectiles;
using PegasusLib;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Broths {
	public class Spicy_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new(232, 26, 26),
				new(105, 0, 0),
				new(141, 3, 3)
			];
		}
		public override int Duration => 4;
		public override void ModifyMinionHit(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			//if (target.oiled && ) modifiers.SetCrit();
		}
		public override void OnMinionHit(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			int debuff = BuffID.OnFire3;
			if (proj.TryGetGlobalProjectile(out OriginGlobalProj global) && global.prefix is Firestarter_Prefix) debuff = ModContent.BuffType<On_Even_More_Fire>();
			target.AddBuff(debuff, 180);
		}
	}
	public class On_Even_More_Fire : ModBuff {
		public override string Texture => typeof(Spicy_Broth).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			BuffID.Sets.GrantImmunityWith[Type] = [
				BuffID.OnFire3
			];
			BuffID.Sets.GrantImmunityWith[BuffID.OnFire].Add(Type);
			BuffID.Sets.GrantImmunityWith[BuffID.OnFire3].Add(Type);
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.onFire3 = true;
			npc.GetGlobalNPC<OriginGlobalNPC>().onSoMuchFire = true;
			if (Main.rand.NextBool(3, 8)) {
				Dust dust = Dust.NewDustDirect(new Vector2(npc.position.X - 2f, npc.position.Y - 2f), npc.width + 4, npc.height + 4, DustID.Torch, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default(Color), 3.5f);
				dust.noGravity = !Main.rand.NextBool(4);
				dust.velocity *= 1.8f;
				dust.velocity.Y -= 1f;
				if (!dust.noGravity) {
					dust.scale *= 0.5f;
				}

				dust.customData = 0;
			}

			Lighting.AddLight((int)(npc.Top.X / 16f), (int)(npc.BottomLeft.Y / 16f + 1f), 1f, 0.4f, 0.15f);
		}
	}
}
