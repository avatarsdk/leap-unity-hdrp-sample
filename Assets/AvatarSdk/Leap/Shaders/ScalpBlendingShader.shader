// Copyright (C) Itseez3D, Inc. - All Rights Reserved
// You may not use this file except in compliance with an authorized license
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
// See the License for the specific language governing permissions and limitations under the License.
// Written by Itseez3D, Inc. <support@avatarsdk.com>, November 2024

Shader "Avatar SDK/Leap/Scalp Blending"
{
	Properties
	{
		_HeadTex("Head Texture", 2D) = "white" {}
		_ScalpTex("Scalp Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			fixed4 linearToGamma(fixed4 c)
			{
				half gamma = 1 / 2.2;
				return pow(c, gamma);
			}

			fixed4 gammaToLinear(fixed4 c)
			{
				half gamma = 2.2;
				return pow(c, gamma);
			}

			fixed4 tex2DInLinear(sampler2D tex, float2 uv)
			{
				fixed4 col = tex2D(tex, uv);
				#if UNITY_COLORSPACE_GAMMA
					col = gammaToLinear(col);
				#endif
				return col;
			}

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _HeadTex;
			sampler2D _ScalpTex;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 headTex = tex2DInLinear(_HeadTex, i.uv);
				fixed4 scalpTex = tex2DInLinear(_ScalpTex, i.uv);

				headTex.rgb = scalpTex.rgb * scalpTex.a + headTex.rgb * (1 - scalpTex.a);

				#if defined(UNITY_COLORSPACE_GAMMA)
					headTex = linearToGamma(headTex);
				#endif

				return headTex;
			}

			ENDCG
		}
	}
}
