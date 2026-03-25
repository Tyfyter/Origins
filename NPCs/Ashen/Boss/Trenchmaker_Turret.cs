using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Ashen.Boss {
	public class Trenchmaker_Turret : ModNPC {
		static AutoLoadingTexture[] gunTextures;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 2;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			// That comic by market pliers' brother?
			TurretKind[] turretKinds = Enum.GetValues<TurretKind>();
			gunTextures = new AutoLoadingTexture[turretKinds.Length];
			for (int i = 0; i < turretKinds.Length; i++) gunTextures[(int)turretKinds[i]] = $"{Texture}_{turretKinds[i]}";
		}
		public TurretKind GunType { get; private set; }
		public Vector2 GunPos => NPC.position + new Vector2(19, 11) * NPC.scale;
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.width = 38;
			NPC.height = 30;
			NPC.lifeMax = 600;
			NPC.damage = 27;
			NPC.defense = 6;
			NPC.npcSlots = 0;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-2f);
			NPC.knockBackResist = 0;
			GunType = Main.rand.Next(Enum.GetValues<TurretKind>());
		}
		public override void OnSpawn(IEntitySource source) {
			base.OnSpawn(source);
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasValidTarget) NPC.targetRect = NPC.GetTargetData().Hitbox;
			Vector2 diff = NPC.targetRect.Center() - GunPos;
			switch (GunType) {
				case TurretKind.Cannon: {
					NPC.ai[1].Cooldown();
					if (TargetAngle(diff.ToRotation()) && NPC.ai[1] == 0) {
						NPC.SpawnProjectile(null,
							GunPos,
							NPC.rotation.ToRotationVector2() * 12,
							ModContent.ProjectileType<Fire_Guns_State.Trenchmaker_Bullet_P>(),
							(int)(18 * ContentExtensions.DifficultyDamageMultiplier),
							1
						);
						NPC.ai[1] = 6;
						if (NPC.ai[2].CycleUp(3)) NPC.ai[1] = 80;
					}
					break;
				}
				case TurretKind.Launcher: {
					NPC.ai[1].Cooldown();
					if (TargetAngle(diff.ToRotation()) && NPC.ai[1] == 0) {
						NPC.SpawnProjectile(null,
							GunPos,
							NPC.rotation.ToRotationVector2() * 12,
							ModContent.ProjectileType<Fire_Cannons_State.Trenchmaker_Cannon_P>(),
							(int)(25 * ContentExtensions.DifficultyDamageMultiplier),
							1
						);
						NPC.ai[1] = 120;
					}
					break;
				}
			}
			bool TargetAngle(float direction) => GeometryUtils.AngularSmoothing(ref NPC.rotation, direction, 0.05f);
		}
		public override void FindFrame(int frameHeight) {
			NPC.frame.Y = NPC.direction == 1 ? 0 : frameHeight;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				NPC.position - screenPos,
				NPC.frame,
				drawColor
			);
			spriteBatch.Draw(
				gunTextures[(int)GunType],
				GunPos - screenPos,
				null,
				drawColor,
				NPC.rotation + MathHelper.Pi,
				new Vector2(35, 11),
				NPC.scale,
				SpriteEffects.None,
			0);
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)GunType);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			GunType = (TurretKind)reader.ReadByte();
		}
		public enum TurretKind {
			Cannon,
			Launcher
		}
	}
}
