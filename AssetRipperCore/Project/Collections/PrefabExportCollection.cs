﻿using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.PrefabInstance;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Exporters;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Project.Collections
{
	public sealed class PrefabExportCollection : AssetsExportCollection
	{
		public PrefabExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, UnityObjectBase asset) : this(assetExporter, virtualFile, GetAssetRoot(asset)) { }

		private PrefabExportCollection(IAssetExporter assetExporter, VirtualSerializedFile virtualFile, GameObject root) : this(assetExporter, root.File, PrefabInstance.CreateVirtualInstance(virtualFile, root)) { }

		private PrefabExportCollection(IAssetExporter assetExporter, IAssetContainer file, PrefabInstance prefab) : base(assetExporter, prefab)
		{
			foreach (EditorExtension asset in prefab.FetchObjects(file))
			{
				AddAsset(asset);
			}
		}

		public static bool IsValidAsset(UnityObjectBase asset)
		{
			if (asset.ClassID == ClassIDType.GameObject)
			{
				return true;
			}
			Component component = (Component)asset;
			return component.GameObject.FindAsset(component.File) != null;
		}

		protected override string GetExportExtension(UnityObjectBase asset)
		{
			return PrefabInstance.PrefabKeyword;
		}

		private static GameObject GetAssetRoot(UnityObjectBase asset)
		{
			GameObject go;
			if (asset.ClassID == ClassIDType.GameObject)
			{
				go = (GameObject)asset;
			}
			else
			{
				Component component = (Component)asset;
				go = component.GameObject.GetAsset(component.File);
			}

			return go.GetRoot();
		}
		public override ISerializedFile File => m_file;
		public override TransferInstructionFlags Flags => base.Flags | TransferInstructionFlags.SerializeForPrefabSystem;

#warning TODO:
		// HACK: prefab's assets may be stored in different files
		// Need to find a way to set a file for current asset nicely
		public override IEnumerable<UnityObjectBase> Assets
		{
			get
			{
				m_file = m_exportIDs.Keys.First().File;
				yield return Asset;

				foreach (UnityObjectBase asset in m_assets)
				{
					m_file = asset.File;
					yield return asset;
				}
			}
		}

		private ISerializedFile m_file;
	}
}
