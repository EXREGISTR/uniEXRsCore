using System.Collections.Generic;

namespace EXRCore.UIFramework {
	public interface IScreensContainer {
		public IReadOnlyCollection<IScreen> GetScreens();
	}
}