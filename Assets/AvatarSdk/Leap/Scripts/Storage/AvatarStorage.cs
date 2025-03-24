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
using System.IO;
using UnityEngine;

namespace AvatarSDK.Leap
{
	public enum DataLocation
	{
		PersistentStorage,
		StreamingAssets,
		Custom
	}

	public static class AvatarStorage
	{
		public static DataLocation AvatarsDataLocation { get; set; } = DataLocation.PersistentStorage;

		public static string CustomAvatarsDirPath { get; set; } = string.Empty;

		public static string GetAvatarsDirectory()
		{
			switch (AvatarsDataLocation)
			{
				case DataLocation.StreamingAssets:
					return Path.Combine(Application.streamingAssetsPath, "itseez3d", "avatar_sdk_leap", "avatars");
				case DataLocation.Custom:
					return CustomAvatarsDirPath;
			}
			
			return Path.Combine(Application.persistentDataPath, "avatars");
		}

		public static string GetAvatarDirectory(string avatarCode)
		{
			return Path.Combine(GetAvatarsDirectory(), avatarCode);
		}

		public static string GetAvatarSourceDirectory(string avatarCode)
		{
			return Path.Combine(GetAvatarDirectory(avatarCode), "source");
		}

		public static string GetAvatarDataDirectory(string avatarCode)
		{
			return Path.Combine(GetAvatarDirectory(avatarCode), "data");
		}

		public static string GetAvatarExportDirectory(string avatarCode)
		{
			return Path.Combine(GetAvatarDirectory(avatarCode), "export");
		}

		public static string GetAvatarCloudExportDirectory(string avatarCode)
		{
			return Path.Combine(GetAvatarDirectory(avatarCode), "cloud_export");
		}

		public static string GetAvatarModelFilePath(string avatarCode)
		{
			return Path.Combine(GetAvatarExportDirectory(avatarCode), "avatar", "model.glb");
		}

		public static string GetCloudAvatarModelFilePath(string avatarCode)
		{
			return Path.Combine(GetAvatarCloudExportDirectory(avatarCode), "model.glb");
		}

		public static string videoTextureStaticFilePath = string.Empty;

		public static string GetVideoTextureFilePath(string avatarCode)
		{
			if (!string.IsNullOrEmpty(videoTextureStaticFilePath))
				return videoTextureStaticFilePath;

			return Path.Combine(GetAvatarDataDirectory(avatarCode), "textures.mp4");
		}

		public static string GetSourceVideoFilePath(string avatarCode)
		{
			return Path.Combine(GetAvatarSourceDirectory(avatarCode), "ARVideo.mp4");
		}

		public static string GetSampleAvatarArchivePath()
		{
			return Path.Combine(Application.streamingAssetsPath, "itseez3d", "avatar_sdk_leap", "sample_avatar", "avatarsdk_leap_source.zip");
		}
	}
}
