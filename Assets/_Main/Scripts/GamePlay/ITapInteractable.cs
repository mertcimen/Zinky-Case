namespace _Main.Scripts.GamePlay
{
	public interface ITapInteractable
	{
		bool CanHandleTap(TapInputContext inputContext);
		void HandleTap(TapInputContext inputContext);
	}
}
