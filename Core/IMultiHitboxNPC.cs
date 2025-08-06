using MonoMod.Cil;
using ReLogic.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Core {
	class MultiHitboxNPC : ILoadable {
		public void Load(Mod mod) {
			IL_Projectile.Damage += IL_Projectile_Damage;
		}
		public void Unload() { }
		static void IL_Projectile_Damage(ILContext il) {
			ILCursor c = new(il);
			ILLabel normalNPC = default;
			int i = -1;
			int colliding = -1;
			int projHitbox = -1;
			c.GotoNext(
				il => il.MatchLdsfld<Main>(nameof(Main.npc)),
				il => il.MatchLdloc(out i),
				il => il.MatchLdelemRef(),
				il => il.MatchLdfld<NPC>(nameof(NPC.type)),
				il => il.MatchLdcI4(NPCID.SolarCrawltipedeTail),
				il => il.MatchBneUn(out normalNPC)
			);
			c.GotoLabel(normalNPC);
			Debug.Assert(c.Previous.Previous.MatchStloc(out colliding));
			Debug.Assert(c.Previous.MatchBr(out ILLabel notNormalNPC));
			Debug.Assert(c.Next.Next.MatchLdloc(out projHitbox));
			c.EmitLdarg0();
			c.EmitLdloc(i);
			c.EmitLdloca(colliding);
			c.EmitLdloc(projHitbox);
			c.EmitDelegate((Projectile self, int i, ref bool colliding, Rectangle projHitbox) => {
				if (Main.npc[i].ModNPC is IMultiHitboxNPC multiHitboxNPC) {
					bool isColliding = colliding;
					int minX = int.MaxValue;
					int maxX = int.MinValue;
					int minY = int.MaxValue;
					int maxY = int.MinValue;
					for (int j = 0; j < multiHitboxNPC.Hitboxes.Length; j++) {
						Rectangle box = multiHitboxNPC.Hitboxes[j];
						Min(ref minX, box.X);
						Max(ref maxX, box.Right);
						Min(ref minY, box.Y);
						Max(ref maxY, box.Bottom);
					}
					if (!self.Colliding(projHitbox, new(minX, minY, maxX - minX, maxY - minY))) return true;
					FastParallel.For(0, multiHitboxNPC.Hitboxes.Length, (min, max, _) => {
						for (int i = min; i < max && !isColliding; i++) {
							if (self.Colliding(projHitbox, multiHitboxNPC.Hitboxes[i])) {
								isColliding = true;
								return;
							}
						}
					});
					colliding = isColliding;
					return true;
				}
				return false;
			});
			c.EmitBrtrue(notNormalNPC);
		}
		static void Min(ref int current, int @new) {
			if (current > @new) current = @new;
		}
		static void Max(ref int current, int @new) {
			if (current < @new) current = @new;
		}
	}
	public interface IMultiHitboxNPC {
		public Rectangle[] Hitboxes { get; }
	}
}
