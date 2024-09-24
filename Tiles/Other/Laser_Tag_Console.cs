using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.UI;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.ObjectData;
using Terraria.UI;
using static System.Net.Mime.MediaTypeNames;

namespace Origins.Tiles.Other {
	public class Laser_Tag_Console : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(81, 81, 81), CreateMapEntryName());
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => CheckInteract(true);
		public override bool RightClick(int i, int j) => CheckInteract(false);
		public static bool CheckInteract(bool justCheck) {
			//if (Main.netMode == NetmodeID.SinglePlayer) return false;
			if (!OriginPlayer.LocalOriginPlayer.laserTagVest) return false;
			if (AnyLaserTagActive) {
				if (LaserTagRules.CTG) {
					_ = Main.LocalPlayer.ownedLargeGems;
				}
				if (LaserTagRules.RespawnTime < 0) return false;

			} else {
				if (!justCheck) {
					Main.LocalPlayer.SetTalkNPC(-1);
					Main.npcChatCornerItem = 0;
					SoundEngine.PlaySound(SoundID.MenuOpen);
					IngameFancyUI.OpenUIState(new Laser_Tag_Rules_UI());
				}
			}
			return true;
		}
		public static ref int LaserTagActiveTeams => ref OriginSystem.Instance.laserTagActiveTeams;
		public static ref int LaserTagActivePlayers => ref OriginSystem.Instance.laserTagActivePlayers;
		public static bool AnyLaserTagActive => LaserTagActivePlayers > 0;
		public static bool LaserTagMultipleTeamsActive {
			get {
				if (LaserTagActiveTeams == 0) return false;
				if (LaserTagActiveTeams == 1) return LaserTagActivePlayers > 1;
				return (LaserTagActiveTeams & (LaserTagActiveTeams - 1)) != 0;
			}
		}
		public static ref Laser_Tag_Rules LaserTagRules => ref OriginSystem.Instance.laserTagRules;
		public static ref int LaserTagTimeLeft => ref OriginSystem.Instance.laserTagTimeLeft;
		public static int[] LaserTagTeamPoints => OriginSystem.Instance.laserTagTeamPoints;
		public static void ProcessLaserTag() {
			LaserTagActiveTeams = 0;
			LaserTagActivePlayers = 0;
			foreach (Player player in Main.ActivePlayers) {
				OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
				OriginPlayer.playersByGuid.TryAdd(originPlayer.guid, player.whoAmI);
				if (originPlayer.laserTagVestActive) {
					LaserTagActiveTeams |= 1 << player.team;
					LaserTagActivePlayers++;
				}
			}
			Color winnerColor = Main.teamColor[0];
			object winner = null;
			void SelectWinner(Player player = null, int team = 0) {
				if (player is not null) team = player.team;
				if (!LaserTagRules.Teams) team = 0;
				winnerColor = Main.teamColor[team];
				switch (team) {
					case 1:
					winner = Language.GetOrRegister("Mods.Origins.Status_Messages.Laser_Tag_Team_Red");
					break;
					case 2:
					winner = Language.GetOrRegister("Mods.Origins.Status_Messages.Laser_Tag_Team_Green");
					break;
					case 3:
					winner = Language.GetOrRegister("Mods.Origins.Status_Messages.Laser_Tag_Team_Blue");
					break;
					case 4:
					winner = Language.GetOrRegister("Mods.Origins.Status_Messages.Laser_Tag_Team_Yellow");
					break;
					case 5:
					winner = Language.GetOrRegister("Mods.Origins.Status_Messages.Laser_Tag_Team_Purple");
					break;
					default:
					case 0:
					winner = player?.name ?? "missingno";
					break;
				}
			}
			if (AnyLaserTagActive || LaserTagTimeLeft > 0) {
				if (LaserTagRules.RespawnTime < 0) {// elimination
					if (LaserTagMultipleTeamsActive) {
						foreach (Player player in Main.ActivePlayers) {
							player.hostile = player.OriginPlayer().laserTagVestActive;
						}
					} else if (Main.netMode == NetmodeID.Server) {
						Player survivor = null;
						foreach (Player player in Main.ActivePlayers) {
							ref bool laserTagVestActive = ref player.OriginPlayer().laserTagVestActive;
							if (laserTagVestActive) {
								laserTagVestActive = false;
								survivor ??= player;
							}
						}
						SelectWinner(survivor);
					}
				}
				if (LaserTagRules.Time > 0 && --LaserTagTimeLeft <= 0) {//any mode, end by timeout
					int bestPointsOwner = 0;
					int bestPointsCount = 0;
					int tieCount = 0;
					if (LaserTagRules.Teams) {
						for (int i = 0; i < LaserTagTeamPoints.Length; i++) {
							if (LaserTagTeamPoints[i] >= bestPointsCount) {
								tieCount = LaserTagTeamPoints[i] == bestPointsCount ? tieCount + 1 : 1;
								bestPointsOwner = i;
								bestPointsCount = LaserTagTeamPoints[i];
							}
						}
					} else {
						foreach (Player player in Main.ActivePlayers) {
							int laserTagPoints = player.OriginPlayer().laserTagPoints;
							if (laserTagPoints >= bestPointsCount) {
								tieCount = laserTagPoints == bestPointsCount ? tieCount + 1 : 1;
								bestPointsOwner = player.whoAmI;
								bestPointsCount = laserTagPoints;
							}
						}
					}
					if (tieCount <= 1) {
						if (LaserTagRules.Teams) {
							SelectWinner(team: bestPointsOwner);
						} else {
							SelectWinner(Main.player[bestPointsOwner]);
						}
						for (int i = 0; i < LaserTagTeamPoints.Length; i++) {
							LaserTagTeamPoints[i] = 0;
						}
						foreach (Player player in Main.ActivePlayers) {
							player.OriginPlayer().laserTagPoints = 0;
						}
					}
				}
			}
			if (winner is not null) {
				if (Main.netMode == NetmodeID.Server) {
					ChatHelper.BroadcastChatMessage(Language.GetOrRegister("Mods.Origins.Status_Messages.Laser_Tag_Victory").ToNetworkText(winner), winnerColor);
					ModPacket packet = Origins.instance.GetPacket();
					packet.Write(Origins.NetMessageType.end_laser_tag);
					packet.Send(-1, Main.myPlayer);
				}
				LaserTagActiveTeams = 0;
				LaserTagActivePlayers = 0;
			}
		}
	}
	public class Laser_Tag_Console_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LampPost);
			Item.createTile = ModContent.TileType<Laser_Tag_Console>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.CopperBar, 2)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 5)
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddIngredient(ModContent.ItemType<Silicon_Item>(), 8)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
	}
	public class Laser_Tag_Rules(int RespawnTime = -1, int Time = -1, int HP = 1, bool Teams = false, bool CTG = false, bool Building = false) {
		const string prefix = "Mods.Origins.Laser_Tag.";
		public int RespawnTime = RespawnTime;
		public int Time = Time;
		public int HP = HP;
		public bool Teams = Teams;
		public bool CTG = CTG;
		public bool Building = Building;
		public IEnumerable<UIElement> GetUIElements() {
			static UI_Time_Button GetTimeController(ButtonTimenessGetter variable, string name, (int radix, string format, bool alwaysShow)[] radices, int increment = 1, int indefiniteThreshold = 0, string indefiniteName = null, int maxValue = int.MaxValue) {
				UI_Time_Button timeController = new(variable, Language.GetOrRegister(prefix + name), radices, increment, indefiniteThreshold, Language.GetOrRegister(prefix + (indefiniteName ?? "Indefinite")), maxValue);
				timeController.Width.Set(-10, 100);
				timeController.Height.Set(32, 0);
				return timeController;
			}
			yield return GetTimeController(() => ref RespawnTime, "RespawnTime", [(60, Language.GetOrRegister(prefix + "Seconds").Value, true)], 30, indefiniteName: "Elimination");
			yield return GetTimeController(() => ref Time, "Time", [(60, "{0:#0}:", true), (60, "{0:00}", true)], 60 * 30, 1);
			static UI_HP_Button GetHealthController(ButtonHealthnessGetter variable, string name, Texture2D texture) {
				UI_HP_Button timeController = new(variable, Language.GetOrRegister(prefix + name), texture);
				timeController.Width.Set(-10, 100);
				timeController.Height.Set(32, 0);
				return timeController;
			}
			yield return GetHealthController(() => ref HP, "HP", TextureAssets.Heart.Value);

			static UI_Toggle_Button GetButton(ButtonTogglenessGetter variable, string name, float textScaleMax = 1, bool large = false) {
				UI_Toggle_Button button = new(variable, Language.GetOrRegister(prefix + name));
				button.Width.Set(-10, 100);
				button.Height.Set(32, 0);
				return button;
			}
			yield return GetButton(() => ref Teams, "Teams");
			yield return GetButton(() => ref CTG, "CTG");
			yield return GetButton(() => ref Building, "Building");
		}
		public void Write(BinaryWriter writer) {
			writer.Write(RespawnTime);
			writer.Write(Time);
			writer.Write(HP);
			writer.Write(Teams);
			writer.Write(CTG);
			writer.Write(Building);
		}
		public static Laser_Tag_Rules Read(BinaryReader reader) => new(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean());
	}
}
