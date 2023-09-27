Shader "Wave/Essence/CompositorLayer/Texture2DBlitShader"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }
    SubShader
    {
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		ZWrite Off
		ColorMask RGBA

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#pragma multi_compile_local _ LINEAR_TO_SRGB_COLOR
			#pragma multi_compile_local _ LINEAR_TO_SRGB_ALPHA

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
			int _LinearColorSpaceConversionNeeded;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				
				//Linear to sRGB approximation 
				#ifdef LINEAR_TO_SRGB_COLOR
				float3 S1c = sqrt(col.rgb);
				float3 S2c = sqrt(S1c);
				float3 S3c = sqrt(S2c);
				col.rgb = 0.662002687 * S1c + 0.684122060 * S2c - 0.323583601 * S3c - 0.0225411470 * col.rgb;
				#endif

				#ifdef LINEAR_TO_SRGB_ALPHA
				float S1a = sqrt(col.a);
				float S2a = sqrt(S1a);
				float S3a = sqrt(S2a);
				col.a = 0.662002687 * S1a + 0.684122060 * S2a - 0.323583601 * S3a - 0.0225411470 * col.a;
				#endif

                return col;
            }
            ENDCG
        }
    }
}
