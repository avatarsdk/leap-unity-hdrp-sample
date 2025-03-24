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
using UnityEngine;

namespace AvatarSDK.Leap
{
	public class ScalpTextureBlender
	{
		private Material blendingMaterial;

		private RenderTexture blendedTexture;

		public ScalpTextureBlender(Texture2D scalpTexture)
		{
			blendingMaterial = Resources.Load<Material>("Materials/scalp_blending_material");
			blendingMaterial.SetTexture("_ScalpTex", scalpTexture);
			blendedTexture = LeapUtils.CreateHeadRenderTexture();
		}

		public void Blend(RenderTexture headTexture)
		{
			blendingMaterial.SetTexture("_HeadTex", headTexture);
			Graphics.Blit(headTexture, blendedTexture, blendingMaterial);
		}

		public RenderTexture GetBlendedTexture()
		{
			return blendedTexture;
		}

		public void Dispose()
		{
			if (blendedTexture != null)
			{
				RenderTexture.ReleaseTemporary(blendedTexture);
				blendedTexture = null;
			}
		}
	}
}
