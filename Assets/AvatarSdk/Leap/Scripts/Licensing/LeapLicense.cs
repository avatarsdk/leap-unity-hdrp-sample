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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AvatarSDK.Leap
{
	public static class LeapLicense
	{
		public static async Task<ClientCredentials> GetClientCredentials()
		{
			string readLicenseExePath = LeapStreamingAssetsStorage.GetReadLicenseExectablePath();

			if (!File.Exists(readLicenseExePath))
			{
				Debug.LogError("Executable not found at path: " + readLicenseExePath);
				return null;
			}

			string licenseFilePath = LeapStreamingAssetsStorage.GetLicenseFilePath();
			if (!File.Exists(licenseFilePath))
			{
				Debug.LogErrorFormat("License file not found: {0}", licenseFilePath);
				return null;
			}

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = readLicenseExePath,
				Arguments = string.Format("\"{0}\"", licenseFilePath),
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using (Process licenseProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
			{
				ClientCredentials clientCredentials = new ClientCredentials();

				string clientIdPrefix = "Client ID: ";
				string clientSecretPrefix = "Client Secret: ";

				licenseProcess.OutputDataReceived += (sender, e) =>
				{
					if (!string.IsNullOrEmpty(e.Data))
					{
						if (e.Data.StartsWith(clientIdPrefix))
						{
							clientCredentials.clientId = e.Data.Substring(clientIdPrefix.Length);
						}
						else if (e.Data.StartsWith(clientSecretPrefix))
						{
							clientCredentials.clientSecret = e.Data.Substring(clientSecretPrefix.Length);
						}
					}
				};

				licenseProcess.ErrorDataReceived += (sender, e) =>
				{
					if (!string.IsNullOrEmpty(e.Data))
					{
						Debug.LogError(e.Data);
					}
				};

				licenseProcess.Start();
				licenseProcess.BeginOutputReadLine();
				licenseProcess.BeginErrorReadLine();

				await Task.Run(() => licenseProcess.WaitForExit());

				int exitCode = licenseProcess.ExitCode;

				if (exitCode == 0)
				{
					return clientCredentials;
				}
				else
				{
					Debug.LogErrorFormat("Failed to read license credentials. Exit code: {0}", exitCode);
					return null;
				}
			}
		}
	}
}
