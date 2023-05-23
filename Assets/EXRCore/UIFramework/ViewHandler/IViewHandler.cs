using System;

namespace EXRCore.UIFramework {
	public interface IViewHandler : IDisposable {
		public void InjectUIService(IUIService uiService);
		public void OnShow();
		public void OnClose();
	}
}