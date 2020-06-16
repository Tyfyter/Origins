using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Fiberglass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs {
    public class EnchantedFiberglassBow : ModNPC {
        public override string Texture => "Origins/Items/Weapons/Fiberglass/Fiberglass_Bow";
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.PossessedArmor);
            npc.noGravity = true;
            npc.aiStyle = 0;//104;//10,
            npc.width = npc.height = 27;
        }
        public override void AI() {
            npc.TargetClosest();
		    npc.spriteDirection = npc.direction;
		    npc.rotation = (npc.Center-Main.player[npc.target].Center).ToRotation();
            if(Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1) && Main.netMode != NetmodeID.MultiplayerClient) {
                npc.localAI[0] += 1f;
                if(npc.localAI[0] >= 60f) {
                    npc.localAI[0] = 30f;
                    Vector2 speed = new Vector2(-8,0).RotatedBy(Main.rand.NextFloat(npc.rotation-0.05f,npc.rotation+0.05f));
                    Projectile.NewProjectile(npc.Center.X, npc.Center.Y, speed.X, speed.Y, ProjectileID.WoodenArrowHostile, 32, 0f);
                }
            }else npc.localAI[0] = 0f;
            if(npc.spriteDirection == 1)npc.rotation+=(float)Math.PI;
        }
        public override void NPCLoot() {
            if(Main.rand.Next(10)==0)Item.NewItem(npc.Center, ModContent.ItemType<Broken_Fiberglass_Bow>());
        }
    }
    public class EnchantedFiberglassPistol : ModNPC {
        public override string Texture => "Origins/Items/Weapons/Fiberglass/Fiberglass_Pistol";
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.PossessedArmor);
            npc.aiStyle = 0;
            npc.noGravity = true;
            npc.width = npc.height = 27;
        }
        public override void AI() {
            npc.TargetClosest();
		    npc.spriteDirection = -npc.direction;
		    npc.rotation = (npc.Center-Main.player[npc.target].Center).ToRotation();
            if(Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1) && Main.netMode != NetmodeID.MultiplayerClient) {
                npc.localAI[0] += 1f;
                if(npc.localAI[0] >= 180f) {
                    npc.localAI[0] = 0f;
                    Vector2 speed = new Vector2(-8,0).RotatedBy(Main.rand.NextFloat(npc.rotation-0.1f,npc.rotation+0.1f));
                    Projectile.NewProjectile(npc.Center.X, npc.Center.Y, speed.X, speed.Y, ProjectileID.BulletDeadeye, 32, 0f);
                    npc.life = 0;
                    npc.checkDead();
                }
            }else npc.localAI[0] = 0f;
            if(npc.spriteDirection == 1)npc.rotation+=(float)Math.PI;
        }
        public override void NPCLoot() {
            Item.NewItem(npc.Center, ModContent.ItemType<Fiberglass_Shard>(),Main.rand.Next(4)+4);
        }
    }
}
