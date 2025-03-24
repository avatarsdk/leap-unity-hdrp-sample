/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, November 2024
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AvatarSDK.Leap
{
	public class LeapProcess
	{
		private Dictionary<AvatarGender, List<string>> paramsFilesMap = new Dictionary<AvatarGender, List<string>>()
		{
			{ AvatarGender.Male, new List<string>(){ "params_male.json", "export_male_1.json" } },
			{ AvatarGender.Female, new List<string>(){ "params_female.json", "export_female_1.json" } },
		};

		private Process currentProcess;

		public delegate void ProgressCallback(float progressInPercents);

		public LeapProcess()
		{
			Application.quitting += OnApplicationQuit;
		}

		public string AvatarCode { get; private set; }

		public bool IsGenerating { get; private set; }  = false;

		public bool IsAvatarRady { get; private set; } = false;

		public async Task<AvatarGenerationStatusCode> GenerateAvatar(string zipArchivePath, AvatarGender avatarGender, ProgressCallback progressCallback = null)
		{
			IsGenerating = true;
			AvatarGenerationStatusCode result = await GenerateAvatarInternal(zipArchivePath, avatarGender, progressCallback);
			IsGenerating = false;
			return result;
		}

		private async Task<AvatarGenerationStatusCode> GenerateAvatarInternal(string zipArchivePath, AvatarGender avatarGender, ProgressCallback progressCallback = null)
		{
			IsAvatarRady = false;
			AvatarCode = Guid.NewGuid().ToString();

			string avatarSrcDir = AvatarStorage.GetAvatarSourceDirectory(AvatarCode);

			bool isArchiveExtracted = ExtractArchive(zipArchivePath, avatarSrcDir);
			if (!isArchiveExtracted)
			{
				Debug.LogError("Failed to extract archive");
				return AvatarGenerationStatusCode.ArchiveExtractionError;
			}

			progressCallback?.Invoke(1);

			string paramsDirPath = LeapStreamingAssetsStorage.GetLeapParamsDirPath();
			string resourcesPath = LeapStreamingAssetsStorage.GetLeapResourcesDirPath();
			string generationParamsPath = Path.Combine(paramsDirPath, GetAvatarGenerationParamsFile(avatarGender));
			string exportParamsPath = Path.Combine(paramsDirPath, GetAvatarExportParamsFile(avatarGender));

			string avatarDirectory = AvatarStorage.GetAvatarDirectory(AvatarCode);
			if (!Directory.Exists(avatarDirectory))
				Directory.CreateDirectory(avatarDirectory);

			string avatarDataDir = AvatarStorage.GetAvatarDataDirectory(AvatarCode);
			string avatarExportDir = AvatarStorage.GetAvatarExportDirectory(AvatarCode);

			ProgressCallback generationProgressHandler = p => progressCallback?.Invoke(1 + p * 0.8f);

			int resultCode = await RunAvatarGenerationProcess(avatarSrcDir, avatarDataDir, resourcesPath, generationParamsPath, generationProgressHandler);
			if (resultCode != 0)
			{
				Debug.LogErrorFormat("Unable to generate avatar. Error code: {0}", resultCode);
				return (AvatarGenerationStatusCode)resultCode;
			}

			resultCode = await RunAvatarExportProcess(avatarDataDir, avatarExportDir, resourcesPath, exportParamsPath);
			if (resultCode != 0)
			{
				Debug.LogErrorFormat("Unable to export avatar. Error code: {0}", resultCode);
				return AvatarGenerationStatusCode.ExportMinError + resultCode;
			}

			progressCallback?.Invoke(100);

			IsAvatarRady = true;

			return AvatarGenerationStatusCode.Success;
		}

		public void StopAvatarGeneration()
		{
			if (currentProcess != null && !currentProcess.HasExited)
			{
				currentProcess.Kill();
				UnityEngine.Debug.Log("Process has been canceled.");
			}
		}

		private bool ExtractArchive(string zipArchivePath, string extractToDir)
		{
			if (!File.Exists(zipArchivePath))
			{
				Debug.LogErrorFormat("Archive doesn't exist: {0}", zipArchivePath);
				return false;
			}

			try
			{
				if (!Directory.Exists(extractToDir))
					Directory.CreateDirectory(extractToDir);

				ZipFile.ExtractToDirectory(zipArchivePath, extractToDir);

				var directories = Directory.GetDirectories(extractToDir);
				var files = Directory.GetFiles(extractToDir);

				if (directories.Length == 1 && files.Length == 0)
				{
					string singleDirPath = directories[0];
					MoveDirectoryContents(singleDirPath, extractToDir);
					Directory.Delete(singleDirPath, true);
				}

				string oldFilePath = Path.Combine(extractToDir, "1.jpg");
				string newFilePath = Path.Combine(extractToDir, "photo.jpg");
				if (File.Exists(oldFilePath))
					File.Move(oldFilePath, newFilePath);

				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"Failed to extract archive: {ex.Message}");
				return false;
			}
		}

		private void MoveDirectoryContents(string sourceDir, string targetDir)
		{
			foreach (var file in Directory.GetFiles(sourceDir))
			{
				string destFile = Path.Combine(targetDir, Path.GetFileName(file));
				File.Move(file, destFile);
			}

			foreach (var dir in Directory.GetDirectories(sourceDir))
			{
				string destDir = Path.Combine(targetDir, Path.GetFileName(dir));
				Directory.Move(dir, destDir);
			}
		}

		private async Task<int> RunAvatarGenerationProcess(string inputDataPath, string outputDataPath, string resourcesPath, string paramsPath, 
			ProgressCallback progressCallback)
		{
			string leapExePath = LeapStreamingAssetsStorage.GetLeapExectablePath();

			if (!File.Exists(leapExePath))
			{
				Debug.LogError("Executable not found at path: " + leapExePath);
				return -1;
			}

			string arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"", inputDataPath, outputDataPath, resourcesPath, paramsPath);

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = leapExePath,
				Arguments = arguments,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using (currentProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
			{
				currentProcess.OutputDataReceived += (sender, e) =>
				{
					if (!string.IsNullOrEmpty(e.Data))
					{
						Debug.Log(e.Data);
						if (progressCallback != null)
						{
							float progress = ParseProgress(e.Data);
							if (progress >= 0)
							{
								progressCallback(progress);
							}
						}
					}
				};

				currentProcess.ErrorDataReceived += (sender, e) =>
				{
					if (!string.IsNullOrEmpty(e.Data))
					{
						if (e.Data.Contains("OpenH264 Video Codec provided by Cisco Systems, Inc."))
							return;
						Debug.LogError(e.Data);
					}
				};

				currentProcess.Start();
				currentProcess.BeginOutputReadLine();
				currentProcess.BeginErrorReadLine();

				await Task.Run(() => currentProcess.WaitForExit());

				int exitCode = currentProcess.ExitCode;
				currentProcess = null;
				return exitCode;
			}
		}

		private float ParseProgress(string log)
		{
			const string progressPrefix = "Progress: ";
			if (log.StartsWith(progressPrefix))
			{
				string progressStr = log.Substring(progressPrefix.Length).TrimEnd('%');
				if (float.TryParse(progressStr, out float progress))
				{
					return progress;
				}
			}
			return -1;
		}

		private async Task<int> RunAvatarExportProcess(string inputDataPath, string outputDataPath, string resourcesPath, string paramsPath)
		{
			string exportExePath = LeapStreamingAssetsStorage.GetExportExectablePath();

			if (!File.Exists(exportExePath))
			{
				Debug.LogError("Executable not found at path: " + exportExePath);
				return -1;
			}

			string arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"", inputDataPath, paramsPath, outputDataPath, resourcesPath);

			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = exportExePath,
				Arguments = arguments,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using (currentProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
			{
				currentProcess.OutputDataReceived += (sender, e) =>
				{
					if (!string.IsNullOrEmpty(e.Data))
					{
						Debug.Log(e.Data);
					}
				};

				currentProcess.ErrorDataReceived += (sender, e) =>
				{
					if (!string.IsNullOrEmpty(e.Data))
					{
						Debug.LogError(e.Data);
					}
				};

				currentProcess.Start();
				currentProcess.BeginOutputReadLine();
				currentProcess.BeginErrorReadLine();

				await Task.Run(() => currentProcess.WaitForExit());

				int exitCode = currentProcess.ExitCode;
				currentProcess = null;
				return exitCode;
			}
		}

		private string GetAvatarGenerationParamsFile(AvatarGender avatarGender)
		{
			return paramsFilesMap[avatarGender][0];
		}

		private string GetAvatarExportParamsFile(AvatarGender avatarGender)
		{
			return paramsFilesMap[avatarGender][1];
		}

		private void OnApplicationQuit()
		{
			StopAvatarGeneration();
		}
	}
}
