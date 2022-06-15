Shader "Omni/Unlit/Soft Shadow"
{
	Properties {
		_Color ("Color", Color) = (0,0,0,1)
		_Softness ("Edge Softness", Range(0, 1)) = 0.1
		_XStretch ("X Stretch", Range(0, 1)) = 0.5
		_YStretch ("Y Stretch", Range(0, 1)) = 0.5
		_XPos ("X Position", Range(0,1)) = 0.5
		_YPos("Y Position", Range(0,1)) = 0.5
	}

	SubShader {
		Tags {"Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float4 normal : NORMAL;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f {
					float4 vertex : SV_POSITION;
	                float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				fixed4 _Color;
				fixed _Softness;
				fixed _XStretch;
				fixed _YStretch;
				fixed _XPos;
				fixed _YPos;

				v2f vert (appdata_t v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = v.texcoord;

					return o;
				}

				fixed get_alpha(float2 texcoord)
				{
					fixed power = 1 / _Softness;
					fixed x = pow(abs(texcoord.x - _XPos) * 2, abs(log(1 - _XStretch)/log(2)));
					fixed y = pow(abs(texcoord.y - _YPos) * 2, abs(log(1 - _YStretch)/log(2)));

					return 1 - pow(x, power) - pow(y, power);
				}

				fixed4 frag (v2f i) : COLOR
				{
					fixed4 col = _Color;
					col.a = col.a *smoothstep(0, 1,  get_alpha(i.texcoord));
					return col;
				}
			ENDCG
		}
	}
}
