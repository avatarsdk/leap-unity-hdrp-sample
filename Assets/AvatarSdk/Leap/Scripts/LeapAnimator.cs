/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, November 2024
*/

using AvatarSDK.MetaPerson.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

namespace AvatarSDK.Leap
{
	public class LeapAnimator : MonoBehaviour
	{
		public MetaPersonLoader metaPersonLoader = null;

		private RenderTexture headRenderTexture = null;

		private SkinnedMeshRenderer headMeshRenderer = null;

		private VideoPlayer videoPlayer = null;

		private Animation avatarAnimation = null;

		private Animator animator = null;

		private ScalpTextureBlender scalpTextureBlender = null;
		
		public event Action animationStarted;

		public event Action animationEnded;

		private void Update()
		{
			if (scalpTextureBlender != null && IsPlaying)
				scalpTextureBlender.Blend(headRenderTexture);
		}

		private void OnDisable()
		{
			if (IsPlaying)
			{
				StopAnimation();
				StopCoroutine("CheckAnimationState");
				animationEnded?.Invoke();
			}
		}

		private void OnDestroy()
		{
			if (headRenderTexture != null)
			{
				RenderTexture.ReleaseTemporary(headRenderTexture);
				headRenderTexture = null;
			}

			if (scalpTextureBlender != null)
			{
				scalpTextureBlender.Dispose();
				scalpTextureBlender = null;
			}
		}

		public async Task<bool> LoadModel(string avatarCode)
		{
			string avatarModelPath = AvatarStorage.GetAvatarModelFilePath(avatarCode);
			string videoTexturePath = AvatarStorage.GetVideoTextureFilePath(avatarCode);
			return await LoadModel(avatarModelPath, videoTexturePath);
		}

		public async Task<bool> LoadCloudModel(string avatarCode)
		{
			string avatarModelPath = AvatarStorage.GetCloudAvatarModelFilePath(avatarCode);
			string videoTexturePath = AvatarStorage.GetVideoTextureFilePath(avatarCode);
			return await LoadModel(avatarModelPath, videoTexturePath);
		}

		public async Task<bool> LoadModel(string avatarModelPath, string videoTexturePath)
		{
			bool isLoaded = await metaPersonLoader.LoadModelAsync(avatarModelPath);
			if (!isLoaded)
			{
				Debug.LogError("Failed to load avatar model");
				return false;
			}

			headMeshRenderer = FindHeadMeshRenderer();
			if (headRenderTexture != null)
				RenderTexture.ReleaseTemporary(headRenderTexture);
			headRenderTexture = LeapUtils.CreateHeadRenderTexture();

			if (videoPlayer != null)
				DestroyImmediate(videoPlayer);
			videoPlayer = CreateAndConfigureVideoPlayer(videoTexturePath, null, headRenderTexture);

			avatarAnimation = FindAndConfigureAnimation();

			if (scalpTextureBlender != null)
			{
				scalpTextureBlender.Dispose();
				scalpTextureBlender = null;
			}

			string scalpTexturePath = Path.Combine(Path.GetDirectoryName(avatarModelPath), "scalp.png");
			if (File.Exists(scalpTexturePath))
			{
				Texture2D scalpTexture = await ImageUtils.LoadTexture(scalpTexturePath, true, true);
				scalpTextureBlender = new ScalpTextureBlender(scalpTexture);
			}

			return true;
		}

		public async Task<bool> PrepareLoadedModel(GameObject avatarObject, string avatarCode)
		{
			string avatarModelPath = AvatarStorage.GetCloudAvatarModelFilePath(avatarCode);
			string videoTexturePath = AvatarStorage.GetVideoTextureFilePath(avatarCode);

			headMeshRenderer = FindHeadMeshRenderer(avatarObject);

			if (headRenderTexture != null)
				RenderTexture.ReleaseTemporary(headRenderTexture);
			headRenderTexture = LeapUtils.CreateHeadRenderTexture();

			if (videoPlayer != null)
				DestroyImmediate(videoPlayer);
			videoPlayer = CreateAndConfigureVideoPlayer(videoTexturePath, null, headRenderTexture);

			animator = avatarObject.GetComponent<Animator>();

			if (scalpTextureBlender != null)
			{
				scalpTextureBlender.Dispose();
				scalpTextureBlender = null;
			}

			string scalpTexturePath = Path.Combine(Path.GetDirectoryName(avatarModelPath), "scalp.png");
			if (File.Exists(scalpTexturePath))
			{
				Texture2D scalpTexture = await ImageUtils.LoadTexture(scalpTexturePath, true, true);
				scalpTextureBlender = new ScalpTextureBlender(scalpTexture);
			}

			return true;
		}

		public bool PrepareLoadedModel(GameObject avatarObject, VideoClip textureVideo, Texture2D scalpTexture)
		{
			headMeshRenderer = FindHeadMeshRenderer(avatarObject);

			if (headRenderTexture != null)
				RenderTexture.ReleaseTemporary(headRenderTexture);
			headRenderTexture = LeapUtils.CreateHeadRenderTexture();

			if (videoPlayer != null)
				DestroyImmediate(videoPlayer);
			videoPlayer = CreateAndConfigureVideoPlayer(null, textureVideo, headRenderTexture);

			animator = avatarObject.GetComponent<Animator>();

			if (scalpTextureBlender != null)
			{
				scalpTextureBlender.Dispose();
				scalpTextureBlender = null;
			}

			if (scalpTexture != null)
				scalpTextureBlender = new ScalpTextureBlender(scalpTexture);

			return true;
		}

		public void PlayAnimation()
		{
			if (videoPlayer == null)
			{
				Debug.LogError("Video player is not found");
				return;
			}

			videoPlayer.Play();
		}

		public void StopAnimation()
		{
			videoPlayer.Stop();

			if (avatarAnimation != null)
				avatarAnimation.Stop();

			if (animator != null)
				animator.enabled = false;
		}

		public bool IsPlaying
		{
			get { return videoPlayer != null && videoPlayer.isPlaying; }
		}

		private SkinnedMeshRenderer FindHeadMeshRenderer()
		{
			return FindHeadMeshRenderer(metaPersonLoader.avatarObject);
		}

		private SkinnedMeshRenderer FindHeadMeshRenderer(GameObject avatarObject)
		{
			var meshRenderers = avatarObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (var meshRenderer in meshRenderers)
			{
				if (meshRenderer.gameObject.name == "AvatarHead")
					return meshRenderer;
			}
			return null;
		}

		private VideoPlayer CreateAndConfigureVideoPlayer(string videoTexturePath, VideoClip videoTexture, RenderTexture outputTexture)
		{
			VideoPlayer videoPlayer = gameObject.AddComponent<VideoPlayer>();
			videoPlayer.playOnAwake = false;
			videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
			videoPlayer.skipOnDrop = true;
			videoPlayer.renderMode = VideoRenderMode.RenderTexture;
			videoPlayer.targetTexture = outputTexture;
			videoPlayer.url = videoTexturePath;
			videoPlayer.clip = videoTexture;
			videoPlayer.started += OnVideoPlayerStarted;
			videoPlayer.errorReceived += (source, message) => Debug.LogErrorFormat("Video player error: {0}", message);
			return videoPlayer;
		}

		private Animation FindAndConfigureAnimation()
		{
			Animation animation = metaPersonLoader.avatarObject.GetComponent<Animation>();
			if (animation != null)
			{
				animation.playAutomatically = false;
				foreach (AnimationState state in animation)
					state.wrapMode = WrapMode.Once;
			}
			return animation;
		}

		private void OnVideoPlayerStarted(VideoPlayer source)
		{
			if (headMeshRenderer != null)
			{
				Texture headTexture = headMeshRenderer.material.mainTexture;
				RenderTexture newHeadTexture = scalpTextureBlender != null ? scalpTextureBlender.GetBlendedTexture() : headRenderTexture;
				headMeshRenderer.material.mainTexture = newHeadTexture;
				if (headTexture != newHeadTexture && headTexture.GetInstanceID() < 0)
				{
					DestroyImmediate(headTexture);
				}
			}
			else
				Debug.LogError("Head mesh renderer is not found");

			if (avatarAnimation != null)
				avatarAnimation.Play();

			if (animator != null)
			{
				animator.enabled = true;
			}

			StartCoroutine(CheckAnimationState());

			animationStarted?.Invoke();
		}

		private IEnumerator CheckAnimationState()
		{
			while (true)
			{
				if (avatarAnimation != null)
				{
					if (!avatarAnimation.isPlaying && !videoPlayer.isPlaying)
					{
						animationEnded?.Invoke();
						break;
					}
				}

				if (animator != null)
				{
					if (!videoPlayer.isPlaying)
					{
						animator.enabled = false;
						animationEnded?.Invoke();
						break;
					}
				}

				yield return null;
			}
		}
	}
}
