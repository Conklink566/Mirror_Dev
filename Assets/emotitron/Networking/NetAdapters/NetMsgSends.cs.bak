using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using emotitron.Debugging;
using emotitron.Compression;


#if PUN_2_OR_NEWER
using Photon;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

#else
using UnityEngine.Networking;

#endif


namespace emotitron.Networking
{

	public enum ReceiveGroup { Others, All, Master }

	/// <summary>
	/// Unified code for sending network messages across different Network Libraries.
	/// </summary>
	public static class NetMsgSends
	{
#if PUN_2_OR_NEWER

		private static RaiseEventOptions[] opts = new RaiseEventOptions[3]
		{
			new RaiseEventOptions() { Receivers = ReceiverGroup.Others },
			new RaiseEventOptions() { Receivers = ReceiverGroup.All },
			new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient }
		};

		private static SendOptions sendOpts = new SendOptions();

		public static void Send(this byte[] buffer, int bytecount, byte msgId, ReceiveGroup rcvGrp)
		{
			//TODO replace this GC generating mess with something prealloc
			byte[] streambytes = new byte[bytecount];
			Array.Copy(buffer, streambytes, bytecount);
			PhotonNetwork.NetworkingClient.OpRaiseEvent(msgId, streambytes, opts[(int)rcvGrp], sendOpts);
			PhotonNetwork.NetworkingClient.Service();
		}

		public static void Send(this byte[] buffer, byte msgId, ReceiveGroup rcvGrp)
		{
			//TODO replace this GC generating mess with something prealloc
			int bytecount = buffer.Length;
			byte[] streambytes = new byte[bytecount];
			Array.Copy(buffer, streambytes, bytecount);
			PhotonNetwork.NetworkingClient.OpRaiseEvent(msgId, streambytes, opts[(int)rcvGrp], sendOpts);
			PhotonNetwork.NetworkingClient.Service();
		}

		public static void Send(ref Bitstream bitstream, byte msgId, ReceiveGroup rcvGrp)
		{
			//TODO replace this GC generating mess with something prealloc
			int bytecount = bitstream.BytesUsed;
			byte[] streambytes = new byte[bytecount];
			//Debug.Log("SND bytecount " + bytecount + " " + bitstream.WritePtr);

			bitstream.ReadOut(streambytes);

			//int bitposition = 0;
			//for (int i = 0; i < bytecount; ++i)
			//	streambytes.Write(bitstream.GetByte(i), ref bitposition, 8);

			//Debug.Log("SND " + bitstream + "  bytes: " + bytecount + " " + streambytes[8] + ":" + streambytes[9] + ":" + streambytes[10] + ":" + streambytes[11]);
			//int bitposition = 0;
			//	bitstream.WriteBytes(streambytes, ref bitposition);

			/////TEST
			//bitstream.Reset();
			//bitstream.WriteFromByteBuffer(streambytes);
			//Debug.Log("Reconstruct " + bitstream);

			//Debug.Log("SEND bytes" + bytecount + " " + rcvGrp);
			PhotonNetwork.NetworkingClient.OpRaiseEvent(msgId, streambytes, opts[(int)rcvGrp], sendOpts);
			PhotonNetwork.NetworkingClient.Service();
		}
#else

#pragma warning disable CS0618 // UNET is obsolete

		static readonly NetworkWriter reusableunetwriter = new NetworkWriter();

		public static void Send(this byte[] buffer, int bytecount, short msgId, ReceiveGroup rcvGrp)
		{
			reusableunetwriter.StartMessage(msgId);
			reusableunetwriter.Write(buffer, bytecount);
			reusableunetwriter.FinishMessage();

			/// Server send to all. Owner client send to server.
			SendWriter(reusableunetwriter, rcvGrp);
		}

		public static void Send(this byte[] buffer, short msgId, ReceiveGroup rcvGrp)
		{
			int bytecount = buffer.Length;
			reusableunetwriter.StartMessage(msgId);
			reusableunetwriter.Write(buffer, bytecount);
			reusableunetwriter.FinishMessage();

			/// Server send to all. Owner client send to server.
			SendWriter(reusableunetwriter, rcvGrp);
		}

		public static void Send(ref Bitstream bitstream, short msgId, ReceiveGroup rcvGrp)
		{
			reusableunetwriter.StartMessage(msgId);
			reusableunetwriter.Write(ref bitstream);
			reusableunetwriter.FinishMessage();

			/// Server send to all. Owner client send to server.
			SendWriter(reusableunetwriter, rcvGrp);
		}

		/// <summary>
		/// Sends byte[] to each client, making any needed per client alterations, such as changing the frame offset value in the first byte.
		/// </summary>
		public static void SendWriter(NetworkWriter unetwriter, ReceiveGroup rcvGrp, int channel = Channels.DefaultUnreliable)
		{

			/// Server send to all. Owner client send to server.
			if (rcvGrp == ReceiveGroup.All)
			{
				if (NetworkServer.active)
					NetworkServer.SendWriterToReady(null, reusableunetwriter, Channels.DefaultUnreliable);
			}
			else if (rcvGrp == ReceiveGroup.Master)
			{
				if (NetworkClient.active)
					ClientScene.readyConnection.SendWriter(reusableunetwriter, Channels.DefaultUnreliable);
			}
			else
			{
				if (NetworkServer.active)
					foreach (NetworkConnection nc in NetworkServer.connections)
					{
						if (nc == null)
							continue;

						/// Don't send to self if Host
						if (nc.connectionId == 0)
							continue;

						if (nc.isReady)
							nc.SendWriter(unetwriter, channel);
					}
			}

			//nc.FlushChannels();
		}

#pragma warning restore CS0618 // UNET is obsolete

#endif
	}

}

