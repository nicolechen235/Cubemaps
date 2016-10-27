Shader "Unlit/CubemapCameraShader"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_deferred
			#pragma fragment frag
			
			#include "UnityCG.cginc"
#include "UnityDeferredLibrary.cginc"
			
			struct mrt_frag {
				fixed4 col;
				half4 normal;
			};
			sampler2D _CameraGBufferTexture0;
			sampler2D _CameraGBufferTexture1;
			sampler2D _CameraGBufferTexture2;
			
			float4 frag (unity_v2f_deferred i) : SV_Target
			{
				mrt_frag o;
				o.col = tex2D(_CameraGBufferTexture0, i.uv);
				o.normal = tex2D(_CameraGBufferTexture2, i.uv);

				//return o;
				return float4(1, 0, 0, 1);
			}
			ENDCG
		}
	}
}
