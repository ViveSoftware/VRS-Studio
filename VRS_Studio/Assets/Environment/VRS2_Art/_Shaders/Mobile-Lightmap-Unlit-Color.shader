// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

//Modified Unlit shader, supports lightmaps and color tinting

Shader "HTC/Unlit_Lightmapped_Color" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
	_Color ("Color", Color) = (1,1,1,1)
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 200




    // Lightmapped
    Pass
    {
        //Tags{ "LIGHTMODE" = "VertexLM" "RenderType" = "Opaque" }

        CGPROGRAM

        #pragma vertex vert addshadow fullforwardshadows
        #pragma fragment frag
        #pragma target 3.0
        #include "UnityCG.cginc"
 
        // uniforms
        float4 _MainTex_ST;
		fixed4 _Color;
		
struct Input {
    float2 uv_MainTex : TEXCOORD0;
			};

        // vertex shader input data
        struct appdata
        {
            float3 pos : POSITION;
            float3 uv1 : TEXCOORD1;
            float3 uv0 : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        // vertex-to-fragment interpolators
        struct v2f
        {
            float2 uv0 : TEXCOORD0;
            float2 uv1 : TEXCOORD1;
            float4 pos : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        // vertex shader
        v2f vert(appdata IN)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(IN);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            // compute texture coordinates
            o.uv0 = IN.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
            o.uv1 = IN.uv0.xy * _MainTex_ST.xy + _MainTex_ST.zw;

            // transform position
            o.pos = UnityObjectToClipPos(IN.pos);
            return o;
        }

        // textures
        sampler2D _MainTex;

        // fragment shader
        fixed4 frag(v2f IN) : SV_Target
        {
            fixed4 col, tex;

            // Fetch lightmap
            half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.uv0.xy);
            col.rgb = DecodeLightmap(bakedColorTex);

            // Fetch color texture
            tex = tex2D(_MainTex, IN.uv1.xy);
            col.rgb = tex.rgb * col.rgb * _Color;
            col.a = 1;


            return col;
        }

        ENDCG
    }
}
}
