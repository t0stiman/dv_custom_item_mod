using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace custom_item_components
{
	public class GadgetBase : MonoBehaviour
	{
		public enum GadgetRemovalMethod : short
		{
			None=0,
			Remover=1,
			EmptyHand=2,
			FromCode=3,
			Any=-1
		}
		public GameObject hudPrefab;
		public Vector3 boundsCenter;
		public Vector3 boundsSize;
		public GadgetBase.GadgetRemovalMethod removalMethod;
		public MeshFilter[] highlightMeshes;
		public AudioClip soundOnPlaced;
		public AudioClip soundOnRemoved;
		public int requiredMountPoints;

	}
}
