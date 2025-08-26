using Humanizer;
using MonoMod.Cil;
using ReLogic.Threading;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Core {
	class MultiHitboxNPC : ILoadable {
		public void Load(Mod mod) {
			IMultiHitboxNPC.SpawningEnabled = true;
			try {
				IL_Projectile.Damage += IL_Projectile_Damage;
				IL_Player.ProcessHitAgainstNPC += IL_Player_ProcessHitAgainstNPC;
			} catch (Exception e) {
				IMultiHitboxNPC.SpawningEnabled = false;
				if (Origins.LogLoadingILError($"{nameof(MultiHitboxNPC)}", e)) throw;
			}
		}

		public void Unload() { }
		static void IL_Projectile_Damage(ILContext il) {
			ILCursor c = new(il);
			ILLabel normalNPC = default;
			ILLabel notNormalNPC = default;
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
			Debugging.Assert(c.Previous.Previous.MatchStloc(out colliding), new Exception("Could not find x = Projectile.Colliding"));
			Debugging.Assert(c.Previous.MatchBr(out notNormalNPC), new Exception("Could not find x = Projectile.Colliding"));
			Debugging.Assert(c.Next.Next.MatchLdloc(out projHitbox), new Exception("Could not find x = Projectile.Colliding"));
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

		static void IL_Player_ProcessHitAgainstNPC(ILContext il) {
			ILCursor c = new(il);
			int itemRectangle = -1;
			int intersected = -1;
			c.GotoNext(MoveType.After,
				il => il.MatchLdarga(out itemRectangle),
				il => il.MatchLdloc(out _),
				il => il.MatchCall<Rectangle>(nameof(Rectangle.Intersects))
			);
			static bool DoCollisionCheck(bool intersected, Rectangle itemRectangle, int i) {
				if (Main.npc[i].ModNPC is IMultiHitboxNPC multiHitboxNPC) {
					for (int j = 0; j < multiHitboxNPC.Hitboxes.Length; j++) {
						if (itemRectangle.Intersects(multiHitboxNPC.Hitboxes[j])) return true;
					}
					return false;
				}
				return intersected;
			}
			if (c.Next.MatchStloc(out intersected)) {
				c.Index++;
				c.EmitLdloc(intersected);
				c.EmitLdarg(itemRectangle);
				c.EmitLdarg(5);
				c.EmitDelegate(DoCollisionCheck);
				c.EmitStloc(intersected);
			} else {
				c.EmitLdarg(itemRectangle);
				c.EmitLdarg(5);
				c.EmitDelegate(DoCollisionCheck);
			}
		}
		static void Min(ref int current, int @new) {
			if (current > @new) current = @new;
		}
		static void Max(ref int current, int @new) {
			if (current < @new) current = @new;
		}
	}
	public interface IMultiHitboxNPC {
		public static bool SpawningEnabled = true;
		public Rectangle[] Hitboxes { get; }
	}
}
