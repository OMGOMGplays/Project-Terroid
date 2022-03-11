using Sandbox;

namespace PTR 
{
	public class PTRCamera : CameraMode 
	{
		public override void Update()
		{
			var pawn = Local.Pawn as PTRPlayer;
			var client = Local.Client;

			if (pawn == null)
				return;
		}
	}
}
