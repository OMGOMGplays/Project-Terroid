using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PTR
{
	public partial class PTRGame : Game
	{
		public PTRGame()
		{
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var pawn = new PTRPlayer();
			client.Pawn = pawn;
			pawn.Respawn();
		}
	}

}
