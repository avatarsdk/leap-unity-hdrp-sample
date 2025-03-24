/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, January 2025
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AvatarSDK.Leap
{
	public static class LeapStreamingAssetsStorage
	{
		private static readonly string leapExecutableName = "avatar_sdk_leap.exe";
		private static readonly string exportExecutableName = "export_avatar.exe";
		private static readonly string getLicenseExecutableName = "read_license_credentials.exe";

		private static readonly string licenseFileName = "res_I7IFyeGGMPc=.bytes";

		public static string GetLeapStreamingAssetsDirPath()
		{
			return Path.Combine(Application.streamingAssetsPath, "itseez3d", "avatar_sdk_leap");
		}

		public static string GetLeapResourcesDirPath()
		{
			return Path.Combine(GetLeapStreamingAssetsDirPath(), "resources");
		}

		public static string GetLeapParamsDirPath()
		{
			return Path.Combine(GetLeapStreamingAssetsDirPath(), "params");
		}

		public static string GetLeapExectablePath()
		{
			return Path.Combine(GetLeapStreamingAssetsDirPath(), leapExecutableName);
		}

		public static string GetExportExectablePath()
		{
			return Path.Combine(GetLeapStreamingAssetsDirPath(), exportExecutableName);
		}

		public static string GetReadLicenseExectablePath()
		{
			return Path.Combine(GetLeapStreamingAssetsDirPath(), getLicenseExecutableName);
		}

		public static string GetLicenseFilePath()
		{
			return Path.Combine(GetLeapResourcesDirPath(), licenseFileName);
		}
	}
}
