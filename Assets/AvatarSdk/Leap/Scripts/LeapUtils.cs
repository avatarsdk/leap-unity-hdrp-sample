/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, November 2024
*/

using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using UnityEngine;

namespace AvatarSDK.Leap
{
	public static class LeapUtils
	{
		public static byte[] PrepareArchiveForCloudComputations(string avatarCode)
		{
			string sourceDirPath = AvatarStorage.GetAvatarSourceDirectory(avatarCode);

			Dictionary<string, string> localPathToZipEntryMap = new Dictionary<string, string>();

			string faceGeometryFilename = "faceGeometry1.obj";
			string faceGeometryPath = Path.Combine(sourceDirPath, faceGeometryFilename);
			if (File.Exists(faceGeometryPath))
				localPathToZipEntryMap.Add(faceGeometryPath, faceGeometryFilename);
			else
				Debug.LogErrorFormat("{0} file not found", faceGeometryFilename);

			string dataFilename = "data.bin";
			string dataPath = Path.Combine(sourceDirPath, dataFilename);
			if (File.Exists(dataPath))
				localPathToZipEntryMap.Add(dataPath, dataFilename);
			else
			{
				dataFilename = "data.txt";
				dataPath = Path.Combine(sourceDirPath, dataFilename);
				if (File.Exists(dataPath))
					localPathToZipEntryMap.Add(dataPath, dataFilename);
				else
					Debug.LogErrorFormat("{0} file not found", dataFilename);
			}

			string imageFilename = "photo.jpg";
			string imagePath = Path.Combine(sourceDirPath, imageFilename);
			if (File.Exists(imagePath))
				localPathToZipEntryMap.Add(imagePath, imageFilename);
			else
			{
				imagePath = Path.Combine(sourceDirPath, "1.jpg");
				if (File.Exists(imagePath))
					localPathToZipEntryMap.Add(imagePath, imageFilename);
				else
					Debug.LogErrorFormat("{0} file not found", imageFilename);
			}

			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
				{
					foreach(var pairMap in localPathToZipEntryMap)
					{
						ZipArchiveEntry entry = archive.CreateEntry(pairMap.Value, System.IO.Compression.CompressionLevel.Optimal);
						using (Stream entryStream = entry.Open())
						{
							using (FileStream fileStream = new FileStream(pairMap.Key, FileMode.Open, FileAccess.Read))
								fileStream.CopyTo(entryStream);
						}
					}
				}

				return memoryStream.ToArray();
			}
		}

		public static RenderTexture CreateHeadRenderTexture()
		{
			RenderTexture renderTexture = RenderTexture.GetTemporary(2048, 2048, 0, RenderTextureFormat.ARGB32,
				QualitySettings.activeColorSpace == ColorSpace.Linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB, 8);
			renderTexture.filterMode = FilterMode.Point;
			return renderTexture;
		}
	}
}
