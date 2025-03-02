using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Weapons.Crossmod;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine.Boss {
	[AutoloadBossHead]
	public class Lost_Diver : Brine_Pool_NPC {
		internal static IItemDropRule normalDropRule;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.CantTakeLunchMoney[Type] = false;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() {
				//CustomTexturePath = "Origins/NPCs/Brine/Boss/Rock_Bottom", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(0f, 0f),
				PortraitPositionXOverride = 0f,
				PortraitPositionYOverride = 0f
			};
		}
		public override void Unload() {
			normalDropRule = null;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.boss = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 14;
			NPC.lifeMax = 25000;
			NPC.defense = 26;
			NPC.aiStyle = 0;
			NPC.width = 20;
			NPC.height = 42;
			NPC.knockBackResist = 0f;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-0.8f);
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.value = Item.buyPrice(gold: 5);
		}
		public override void AI() {
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			normalDropRule = ItemDropRule.OneFromOptionsNotScalingWithLuck(1,
				ModContent.ItemType<Boat_Rocker>(),
				ModContent.ItemType<Depth_Charge>(),
				ModContent.ItemType<Torpedo_Tube>(),
				ModContent.ItemType<Mildew_Whip>(),
				Watered_Down_Keytar.ID
			).WithOnSuccess(ItemDropRule.Common(ModContent.ItemType<Lost_Diver_Helmet>(), 10)
			.WithOnSuccess(ItemDropRule.Common(ModContent.ItemType<Lost_Diver_Chest>()))
			.WithOnSuccess(ItemDropRule.Common(ModContent.ItemType<Lost_Diver_Greaves>()))
			);
			//These need to be in the mildew carrion's loot, it's realistically simplest to make the two separate NPCs
			/*npcLoot.Add(new DropBasedOnExpertMode(
				normalDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ModContent.ItemType<Lost_Diver_Bag>(), 1, 1, 1, null)
			));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Faith_Beads>(), 4));
			npcLoot.Add(new DropInstancedPerClient(ModContent.ItemType<Crown_Jewel>()));*/
		}
		public Rectangle bodyFrame = new(0, 0, 40, 56);
		public Rectangle legFrame = new(0, 0, 40, 56);
		AutoLoadingAsset<Texture2D> headTexture = typeof(Lost_Diver).GetDefaultTMLName() + "_Head";
		AutoLoadingAsset<Texture2D> bodyTexture = typeof(Lost_Diver).GetDefaultTMLName() + "_Body";
		AutoLoadingAsset<Texture2D> legTexture = typeof(Lost_Diver).GetDefaultTMLName() + "_Legs";
		AutoLoadingAsset<Texture2D> backTexture = typeof(Lost_Diver).GetDefaultTMLName() + "_Back";
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			//conveniently, the drop it has that'd use the proper composite arms is the keytar, so nothing it's definitely going to do needs me to support them
			SpriteEffects effect = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Vector2 headVect = legFrame.Size() * new Vector2(0.5f, 0.4f);
			Vector2 bodyVect = legFrame.Size() * new Vector2(0.5f, 0.5f);
			Vector2 legVect = legFrame.Size() * new Vector2(0.5f, 0.75f);
			Color headColor = Lighting.GetColorClamped((int)NPC.Center.X / 16, (int)(NPC.position.Y + NPC.height * 0.25) / 16, Color.White);
			Color bodyColor = Lighting.GetColorClamped((int)NPC.Center.X / 16, (int)(NPC.position.Y + NPC.height * 0.75) / 16, Color.White);
			Color legColor = Lighting.GetColorClamped((int)NPC.Center.X / 16, (int)(NPC.position.Y + NPC.height * 0.75) / 16, Color.White);
			if (NPC.IsABestiaryIconDummy) {
				headColor = Color.White;
				bodyColor = Color.White;
				legColor = Color.White;
			}

			#region composite data
			Vector2 vector = new Vector2((int)(NPC.position.X - (bodyFrame.Width / 2) + (NPC.width / 2)), (int)(NPC.position.Y + NPC.height - bodyFrame.Height + 4f)) + new Vector2(bodyFrame.Width / 2, bodyFrame.Height / 2) - screenPos;
			Vector2 vector2 = Main.OffsetsPlayerHeadgear[bodyFrame.Y / bodyFrame.Height];
			vector2.Y -= 2f;
			vector -= vector2;
			Vector2 compositeOffset_BackArm = new(6 * NPC.direction, 2);
			Vector2 backArmPosition = vector + compositeOffset_BackArm;
			Vector2 backArmOrigin = bodyVect + compositeOffset_BackArm;
			Vector2 compositeOffset_FrontArm = new(-5 * NPC.direction, 0);
			Vector2 frontArmPosition = vector + compositeOffset_FrontArm;
			Vector2 frontArmOrigin = bodyVect + compositeOffset_FrontArm;
			Vector2 frontShoulderPosition = frontArmPosition + compositeOffset_FrontArm;
			Point backShoulderFrameIndex = new(1, 1);
			Point frontShoulderFrameIndex = new(0, 1);
			Point backArmframeIndex = default;
			Point frontArmframeIndex = default;
			Point torsoFrameIndex = default;
			static Rectangle CreateCompositeFrameRect(Point pt) {
				return new Rectangle(pt.X * 40, pt.Y * 56, 40, 56);
			}
			int shoulderDrawPosition = 1; // -1 if hideCompositeShoulders would be true, 0 if compShoulderOverFrontArm would be false, 1 if compShoulderOverFrontArm would be true
			switch (bodyFrame.Y / bodyFrame.Height) {
				case 0:
				frontArmframeIndex.X = 2;
				break;
				case 1:
				frontArmframeIndex.X = 3;
				shoulderDrawPosition = 0;
				break;
				case 2:
				frontArmframeIndex.X = 4;
				shoulderDrawPosition = 0;
				break;
				case 3:
				frontArmframeIndex.X = 5;
				shoulderDrawPosition = 1;
				break;
				case 4:
				frontArmframeIndex.X = 6;
				shoulderDrawPosition = 1;
				break;
				case 5:
				frontArmframeIndex.X = 2;
				frontArmframeIndex.Y = 1;
				torsoFrameIndex.X = 1;
				shoulderDrawPosition = -1;
				break;
				case 6:
				frontArmframeIndex.X = 3;
				frontArmframeIndex.Y = 1;
				break;
				case 7:
				case 8:
				case 9:
				case 10:
				frontArmframeIndex.X = 4;
				frontArmframeIndex.Y = 1;
				break;
				case 11:
				case 12:
				case 13:
				frontArmframeIndex.X = 3;
				frontArmframeIndex.Y = 1;
				break;
				case 14:
				frontArmframeIndex.X = 5;
				frontArmframeIndex.Y = 1;
				break;
				case 15:
				case 16:
				frontArmframeIndex.X = 6;
				frontArmframeIndex.Y = 1;
				break;
				case 17:
				frontArmframeIndex.X = 5;
				frontArmframeIndex.Y = 1;
				break;
				case 18:
				case 19:
				frontArmframeIndex.X = 3;
				frontArmframeIndex.Y = 1;
				break;
			}
			backArmframeIndex.X = frontArmframeIndex.X;
			backArmframeIndex.Y = frontArmframeIndex.Y + 2;
			Rectangle compBackShoulderFrame = CreateCompositeFrameRect(backShoulderFrameIndex);
			Rectangle compBackArmFrame = CreateCompositeFrameRect(backArmframeIndex);
			Rectangle compFrontShoulderFrame = CreateCompositeFrameRect(frontShoulderFrameIndex);
			Rectangle compFrontArmFrame = CreateCompositeFrameRect(frontArmframeIndex);
			Rectangle compTorsoFrame = CreateCompositeFrameRect(torsoFrameIndex);
			if (compFrontArmFrame.X / compFrontArmFrame.Width >= 7) {
				frontArmPosition += new Vector2(NPC.direction, 1);
			}
			#endregion

			//index 10, back acc
			Vector2 vec = NPC.position + new Vector2(NPC.width / 2, NPC.height - bodyFrame.Height / 2) + new Vector2(0f, 4f) - screenPos;
			vec = vec.Floor();
			spriteBatch.Draw(
				backTexture,
				vec,
				bodyFrame,
				bodyColor,
				0,
				bodyVect,
				1f,
				effect,
			0);

			//index 12, back arm
			spriteBatch.Draw(
				bodyTexture,
				vector,
				compBackShoulderFrame,
				bodyColor,
				0,
				bodyVect,
				1f,
				effect,
			0);
			spriteBatch.Draw(
				bodyTexture,
				backArmPosition,
				compBackArmFrame,
				bodyColor,
				0,
				backArmOrigin,
				1f,
				effect,
			0);

			//index 13, legs
			spriteBatch.Draw(
				legTexture,
				new Vector2((int)(NPC.position.X - (legFrame.Width / 2) + (NPC.width / 2)), (int)(NPC.position.Y + NPC.height - legFrame.Height + 4f)) + legVect - screenPos,
				legFrame,
				legColor,
				0,
				legVect,
				1f,
				effect,
			0);

			//index 17, torso
			spriteBatch.Draw(
				bodyTexture,
				vector,
				compTorsoFrame,
				bodyColor,
				0,
				bodyVect,
				1f,
				effect,
			0);

			//index 21, head
			Rectangle headFrame = this.bodyFrame;
			headFrame.Height -= 4;
			spriteBatch.Draw(
				headTexture,
				new Vector2((int)(NPC.position.X - (bodyFrame.Width / 2) + (NPC.width / 2)), (int)(NPC.position.Y + NPC.height - bodyFrame.Height + 4f)) + headVect - screenPos,
				headFrame,
				headColor,
				0,
				headVect,
				1f,
				effect,
			0);

			//index 28, front arm
			if (shoulderDrawPosition == 0) {
				spriteBatch.Draw(
					bodyTexture,
					frontShoulderPosition,
					compFrontShoulderFrame,
					bodyColor,
					0,
					bodyVect,
					1f,
					effect,
				0);
			}
			spriteBatch.Draw(
				bodyTexture,
				frontArmPosition,
				compFrontArmFrame,
				bodyColor,
				0,
				frontArmOrigin,
				1f,
				effect,
			0);
			if (shoulderDrawPosition == 1) {
				spriteBatch.Draw(
					bodyTexture,
					frontShoulderPosition,
					compFrontShoulderFrame,
					bodyColor,
					0,
					bodyVect,
					1f,
					effect,
				0);
			}
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
			}
		}
		public override void OnKill() {
			Boss_Tracker.Instance.downedLostDiver = true;
			NetMessage.SendData(MessageID.WorldData);
		}
	}
}
