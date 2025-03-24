/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, March 2025
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace AvatarSDK.Leap.Sample
{
	public class LeapHDRPOfficeSample : MonoBehaviour
	{
		public LeapAnimator leapAnimator = null;

		public VideoPlayer srcVideoPlayer = null;

		public VideoClip sourceVideo = null;

		public VideoClip textureVideo = null;

		public Texture2D scalpTexture = null;

		public GameObject avatarObject = null;

		private void Start()
		{
			leapAnimator.animationEnded += () =>
			{
				if (srcVideoPlayer != null)
					srcVideoPlayer.Stop();
				leapAnimator.PlayAnimation();
			};
			leapAnimator.animationStarted += () =>
			{
				if (srcVideoPlayer != null)
					srcVideoPlayer.Play();
			};

			ClearVideoPlayerTexture();

			PrepareAvatar();

			leapAnimator.PlayAnimation();
		}

		private void OnDestroy()
		{
			ClearVideoPlayerTexture();
		}

		private void PrepareAvatar()
		{
			bool isPrepared = leapAnimator.PrepareLoadedModel(avatarObject, textureVideo, scalpTexture);

			if (!isPrepared)
			{
				Debug.LogError("Failed to prepare avatar model");
				return;
			}

			ConfigureSrcVideoPlayer();
		}

		private void ConfigureSrcVideoPlayer()
		{
			srcVideoPlayer.clip = sourceVideo;
			srcVideoPlayer.Prepare();
		}

		private void ClearVideoPlayerTexture()
		{
			if (srcVideoPlayer == null)
				return;

			RenderTexture renderTexture = srcVideoPlayer.targetTexture;
			if (renderTexture != null)
			{
				RenderTexture.active = renderTexture;
				GL.Clear(true, true, Color.clear);
				RenderTexture.active = null;
			}
		}
	}
}
