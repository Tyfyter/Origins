using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.UI {
	public class State_Switching_UI(params SwitchableUIState[] states) : UIState() {
		readonly List<SwitchableUIState> states = states.ToList();
		SwitchableUIState currentState;

		public void AddState(SwitchableUIState state) {
			states.Add(state);
		}
		public void AddStates(params SwitchableUIState[] states) {
			this.states.AddRange(states);
		}
		public override void Update(GameTime gameTime) {
			currentState = null;
			for (int i = 0; i < states.Count; i++) {
				if (states[i].IsActive()) {
					currentState = states[i];
					states[i].Activate();
				} else {
					states[i].Deactivate();
				}
				if (!this.Children.Contains(states[i])) {
					this.Append(states[i]);
				}
			}
			if (currentState is null) return;
			currentState.Update(gameTime);
		}
	}
	public abstract class SwitchableUIState : UIState, ILoadable {
		public void Load(Mod mod) {
			if (OriginSystem.queuedUIStates is null) {
				AddToList();
			} else {
				OriginSystem.queuedUIStates.Add(this);
			}
		}
		public void Unload() { }
		public abstract void AddToList();
		public abstract bool IsActive();
		private bool isActive = false;
		public override void OnActivate() {
			isActive = true;
		}
		public override void OnDeactivate() {
			isActive = false;
		}
		public override bool ContainsPoint(Vector2 point) => isActive && base.ContainsPoint(point);
		public override void Update(GameTime gameTime) {
			if (isActive) base.Update(gameTime);
		}
		public override void Draw(SpriteBatch spriteBatch) {
			if (isActive) base.Draw(spriteBatch);
		}
	}
}