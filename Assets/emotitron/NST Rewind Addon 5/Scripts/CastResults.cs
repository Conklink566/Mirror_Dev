//Copyright 2018, Davin Carten, All rights reserved

using System.Collections.Generic;
using UnityEngine;
using emotitron.Utilities.BitUtilities;
using emotitron.Utilities.GenericCast;
using emotitron.Compression;

namespace emotitron.NST
{
	//// These structs might need a better home
	///// <summary>
	///// Struct for client rewind casts that are pending. Calls to cast put casts on a queue that fires on the next FrameUpdate.
	///// </summary>
	//public struct RewindCastQueueItem
	//{
	//	public CastType castType;
	//	public int castDefId; // The List<CastDefinition> item from the NST
	//	public float dist;
	//	public int layers;
	//}

	public class CastResults
	{
		/// List of NSTs that registerd as hit, and a parallel list of masks, indicating which hitgroups were hit for that NST (headshots, etc)
		public List<NetworkSyncTransform> nsts;
		public List<int> hitGroupMasks;

		private CastDefinition _castDef;
		public CastDefinition CastDef
		{
			get { return _castDef; }
			set
			{
				_castDef = value;
				useMaskForNsts = (_castDef == null) ? false : CastDef.sendHitsAsMask && _bitsForNstId <= 6;
			}
		}

		public bool wasCast;
		public bool isConfirmed;

		public Frame frame;

		public static CastResults reusableResults;
		
		// Cached values
		private static int _bitsForNstId;
		private static int _maxNstIds;
		private static int _hitGroupTagsCount;
		private bool useMaskForNsts;

		// static construct - the static class is just used for the tempUnconfirmed buffer, which is used by the server to hold incoming claims before confirming
		static CastResults()
		{
			reusableResults = new CastResults(null) { /*isConfirmed = false*/ };
			_bitsForNstId = HeaderSettings.Single.BitsForNstId;
			_maxNstIds = (int)HeaderSettings.Single.MaxNSTObjects;
			_hitGroupTagsCount = HitGroupSettings.single.hitGroupTags.Count;
		}

		//Constructor
		public CastResults(CastDefinition _castDef)
		{
			CastDef = _castDef;

			nsts = new List<NetworkSyncTransform>();
			hitGroupMasks = new List<int>();
		}

		/// <summary>
		/// writes a castresults to the stream...
		/// </summary>
		/// <param name="bitstream"></param>
		public void Serialize(ref UdpBitStream bitstream, Replicate replication)
		{
			if (replication < Replicate.Hits)
				return;

			// Mask can only be used if:
			// Mask is selected in settings &&
			// Total number of NstIds is less than 64 (the limit of a uint mask)
			int layercount = (replication == Replicate.HitsWithHitGroups) ? _hitGroupTagsCount : 1;

			if (useMaskForNsts)
			{
				ulong hitmask = 0;

				// Make note of where the mask needs to be written in the bitstream, will come back and write it after we finish (we may be writing hitmasks first)
				int maskPosInBitstream = bitstream.ptr;
				bitstream.ptr += _maxNstIds;

				int maskcount = nsts.Count;
				for (int i = 0; i < maskcount; ++i)
				{
					((int)(nsts[i].NstId)).SetBitInMask(ref hitmask, true);
					// write the hitgroup mask if we are doing confirmed
					if (replication == Replicate.HitsWithHitGroups)
					{
						bitstream.WriteInt(hitGroupMasks[i], layercount);
					}
				}
				bitstream.WriteULongAtPos(hitmask, _maxNstIds, maskPosInBitstream);
				return;
			}

			// Send the total number of hits

			// Write the ids for hits on this layer
			int count = nsts.Count;
			for (int i = 0; i < count; ++i)
			{
				bitstream.WriteBool(true);

				bitstream.WriteUInt(nsts[i].NstId, _bitsForNstId);

				if (layercount > 1)
				{
					bitstream.WriteInt(hitGroupMasks[i], layercount);
				}
			}
			// End of hits
			bitstream.WriteBool(false);
		}

		/// <summary>
		/// Expected stream is in form of [notEOS] [nstid][hitmask], [nstid][hitmask], [nstid][hitmask] etc
		/// with mask... [NstIdMask] [hitmask][hitmask][hitmask] etc
		/// </summary>
		/// <param name="bitstream"></param>
		public void Deserialize(ref UdpBitStream bitstream, Replicate replicate)
		{
			nsts.Clear();
			hitGroupMasks.Clear();
			isConfirmed = replicate == Replicate.HitsWithHitGroups;
			// Nothing to deserialize?
			if (replicate < Replicate.Hits)
				return;
			
			int layercount = (replicate == Replicate.HitsWithHitGroups) ? _hitGroupTagsCount : 1;

			if (useMaskForNsts)
			{
				// Read hitNsts as mask
				ulong mask = bitstream.ReadULong(_maxNstIds);

				// For each flagged NST in the mask, create an Nsts entry and corresponding hitGroupMask entry (if this is confirmed)
				for (int i = 0; i < _maxNstIds; ++i)
				{
					if (mask.GetBitInMask(i))
					{
						nsts.Add(NSTTools.GetNstFromId((uint)i));
						if (replicate == Replicate.HitsWithHitGroups)
						{
							hitGroupMasks.Add(bitstream.ReadInt(layercount));
						}
					}
				}
			}
			else
			{
				while (bitstream.ReadBool())
				{
					nsts.Add(NSTTools.GetNstFromId(bitstream.ReadUInt(_bitsForNstId)));

					if (layercount > 1)
					{
						hitGroupMasks.Add(bitstream.ReadInt(layercount));
					}
				}
			}
		}

		/// <summary>
		/// Determine if hit on an NST involved a hit on the collider type of hitGroupName.
		/// </summary>
		/// <param name="nstIndex">The index of the NST in nsts of this cast result. nsts and hitGroupTags correspond to one another.</param>
		/// <param name="hitGroupName">The name of the HitGroupTag we are checking for a hit against.</param>
		/// <returns></returns>
		public bool WasHitGroupHit(int nstIndex, int hitGroupNumber)
		{
			return (hitGroupMasks[nstIndex] & (1 << hitGroupNumber)) != 0;
		}

		/// <summary>
		/// Determine if hit on an NST involved a hit on the collider type of hitGroupName. This involves a slower lookup than giving hitGroupNumber,
		/// so the overload of this function that takes the hitGroupNumber is recommended if serious performance is a concern.
		/// </summary>
		/// <param name="nstIndex">The index of the NST in nsts of this cast result. nsts and hitGroupTags correspond to one another.</param>
		/// <param name="hitGroupName">The name of the HitGroupTag we are checking for a hit against.</param>
		/// <returns></returns>
		public bool WasHitGroupHit(int nstIndex, string hitGroupName)
		{
			var hgtags = HitGroupSettings.Single.rewindLayerTagToId;

			if (hgtags.ContainsKey(hitGroupName))
				return (hitGroupMasks[nstIndex] & (1 << hgtags[hitGroupName])) != 0;

			Debug.LogError("No Hit Group Tag named '" + hitGroupName + "' exists in 'Hit Group Settings'");
			return false;
		}

		public void Clear()
		{
			nsts.Clear();
			hitGroupMasks.Clear();
		}

		public override string ToString()
		{
			string str =  CastDef + " type: " + ((isConfirmed) ? " w/ HitGroups" : " no HitGroups") + " Results:\n";

			if (nsts.Count == 0)
				str += "<b><no hits></b>";
			else
			{
				int count = nsts.Count;
				for (int i = 0; i < count; ++i)
				{
					if (nsts[i] == null)
					{
						str += "<b>ERROR NULL id:</b> " + i;
					}
					else
					{
						str += "<b>HIT: </b>'" + nsts[i].name + " nstid:" + nsts[i].NstId + " mask: " + ((i < hitGroupMasks.Count) ? BitTools.PrintBitMask((uint)hitGroupMasks[i], -1, 8) : "null") + "' \n";
					}
				}
			}

			return str;
		}
	}
}





