Shader "Unlit/Color/Transparent"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 0.5)
        _DashLength("Dash Length", Range(0.01, 10)) = 1
        _GapLength("Gap Length", Range(0.01, 10)) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        LOD 100

        Pass
        {

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 worldPos : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            float4 _Color;
            float _DashLength;
            float _GapLength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _Color;
                float dashValueX = abs((int)(i.worldPos.x / (_DashLength + _GapLength)) % 2);
                float dashValueY = abs((int)(i.worldPos.y / (_DashLength + _GapLength)) % 2);
                float dashValueZ = abs((int)(i.worldPos.z / (_DashLength + _GapLength)) % 2);

                col.a *= dashValueX * dashValueY * dashValueZ;
                return col;
            }
            ENDCG
        }
    }
}
