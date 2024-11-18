using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Origins.UI.Event {
	public class Laser_Tag_Hud : SwitchableUIState {
		public AutoLoadingAsset<Texture2D> texture = typeof(Laser_Tag_Hud).GetDefaultTMLName();
		public override void AddToList() => OriginSystem.Instance.SetBonusHUD.AddState(this);
		public override bool IsActive() {
			if (OriginPlayer.LocalOriginPlayer is null) return false;
			if (OriginPlayer.LocalOriginPlayer.laserTagVest) {
				return true;
			}
			return false;
		}
		public Laser_Tag_Hud() : base() {
			OverrideSamplerState = SamplerState.PointClamp;
		}
		UIHideWrapper summaryPanelWrapper;
		UIPanel summaryPanel;
		UIHideWrapper teamsListWrapper;
		UI_Hidden_Supporting_List teamsList;
		UIHideWrapper playersList;
		UIHideWrapper ActiveScoreList => Laser_Tag_Console.LaserTagRules.Teams ? teamsListWrapper : playersList;
		public override void OnInitialize() {
			summaryPanel = new() {
				Width = new(0, 1),
				Height = new(0, 1),
			};
			summaryPanelWrapper = new() {
				HAlign = 0.5f
			};
			summaryPanelWrapper.Width.Set(250, 0.05f);
			summaryPanelWrapper.Height.Set(32, 0f);
			summaryPanelWrapper.Append(summaryPanel);
			Append(summaryPanelWrapper);

			teamsList = [
				new Laser_Tag_Team_UIElement(1),
				new Laser_Tag_Team_UIElement(2),
				new Laser_Tag_Team_UIElement(3),
				new Laser_Tag_Team_UIElement(4),
				new Laser_Tag_Team_UIElement(5)
			];
			teamsList.Width.Set(0, 1);
			teamsList.Height.Set(0, 1);
			teamsList.ListPadding = 0;
			teamsList.OnUpdate += el => {
				if (el is UIList list) list.Height.Set(list.GetTotalHeight(), 0);
			};
			teamsListWrapper = new() {
				HAlign = 0.5f
			};
			teamsListWrapper.Width.Set(250, 0.05f);
			teamsListWrapper.Height.Set(64, 1f);
			teamsListWrapper.Append(teamsList);
			Append(teamsListWrapper);
		}
		public override void Update(GameTime gameTime) {
			if (summaryPanel.IsMouseHovering || teamsList.IsMouseHovering) {
				summaryPanelWrapper.isHidden = true;
				if (ActiveScoreList.isHidden) {
					if (Laser_Tag_Console.LaserTagRules.Teams) {
						teamsList.UpdateOrder();
						teamsList.Recalculate();
					} else {
						playersList.Recalculate();
					}
				}
				ActiveScoreList.isHidden = false;
			} else {
				summaryPanelWrapper.isHidden = false;
				teamsListWrapper.isHidden = true;
				//playersList.isHidden = true;
			}
			base.Update(gameTime);
		}
	}
	public class UIHideWrapper : UIElement {
		public bool isHidden = false;
		public override bool ContainsPoint(Vector2 point) => !isHidden && base.ContainsPoint(point);
		public override void Draw(SpriteBatch spriteBatch) {
			if (!isHidden) base.Draw(spriteBatch);
		}
	}
	public class Laser_Tag_Team_UIElement : UI_Hideable_Panel {
		public readonly int team;
		public Laser_Tag_Team_UIElement(int team) {
			Width.Set(0, 1);
			Height.Set(32, 0);
			BackgroundColor = Main.teamColor[team];
			this.team = team;
			this.UseImmediateMode = true;
			this.OverflowHidden = true;
		}
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			this.Hidden = Laser_Tag_Console.LaserTagTeamPlayers[team] <= 0;
			if (this.IsMouseHovering) Main.LocalPlayer.mouseInterface = true;
		}
		public override int CompareTo(object obj) {
			if (obj is not Laser_Tag_Team_UIElement other) return 0;
			if (Laser_Tag_Console.LaserTagRules.RespawnTime < 0) return -Comparer<int>.Default.Compare(Laser_Tag_Console.LaserTagTeamPlayers[team], Laser_Tag_Console.LaserTagTeamPlayers[other.team]);
			return -Comparer<int>.Default.Compare(Laser_Tag_Console.LaserTagTeamPoints[team], Laser_Tag_Console.LaserTagTeamPoints[other.team]);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			Rectangle bounds = GetInnerDimensions().ToRectangle();
			Vector2 pointsPos = bounds.Right();
			string points = Language.GetTextValue("Mods.Origins.Laser_Tag.Hud_Points", Laser_Tag_Console.LaserTagTeamPoints[team]);
			Vector2 pointsSize = FontAssets.MouseText.Value.MeasureString(points);
			pointsPos.X -= pointsSize.X;
			ChatManager.DrawColorCodedString(
				spriteBatch,
				FontAssets.MouseText.Value,
				points,
				pointsPos,
				Color.Black,
				0,
				pointsSize * Vector2.UnitY * 0.4f,
				Vector2.One
			);
			if (Laser_Tag_Console.LaserTagRules.RespawnTime < 0) {
				Vector2 headPos = bounds.TopLeft();
				foreach (Player player in Main.ActivePlayers) {
					if (player.team == team && player.OriginPlayer().laserTagVestActive) {
						int direction = player.direction;
						Rectangle headFrame = player.headFrame;
						Rectangle hairFrame = player.hairFrame;
						Rectangle bodyFrame = player.bodyFrame;
						Rectangle legFrame = player.legFrame;
						try {
							player.direction = 1;
							player.headFrame.Y = 0;
							player.hairFrame.Y = 0;
							player.bodyFrame.Y = 0;
							player.legFrame.Y = 0;
							Main.PlayerRenderer.DrawPlayerHead(
								Main.Camera,
								player,
								headPos + new Vector2(8, -2),
								scale: 0.85f
							);
						} finally {
							player.direction = direction;
							player.headFrame = headFrame;
							player.hairFrame = hairFrame;
							player.bodyFrame = bodyFrame;
							player.legFrame = legFrame;
						}
						headPos.X += 30;
						if (headPos.X > pointsPos.X - 30) break;
					}
				}
			}
		}
	}
	public class Laser_Tag_Player_List : UIHideWrapper {

	}
}
