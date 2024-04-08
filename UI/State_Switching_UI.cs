using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria.UI;

namespace Origins.UI {
	public class State_Switching_UI : UIState {
		readonly List<SwitchableUIState> states = new();
		SwitchableUIState currentState;
		public State_Switching_UI(params SwitchableUIState[] states) : base() {
			this.states = states.ToList();
		}
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
				}
			}
			if (currentState is null) return;
			currentState.Update(gameTime);
		}
		public override void Draw(SpriteBatch spriteBatch) {
			if (currentState is null) return;
			currentState.Draw(spriteBatch);
		}
	}
	public abstract class SwitchableUIState : UIState {
		public abstract bool IsActive();
	}
}