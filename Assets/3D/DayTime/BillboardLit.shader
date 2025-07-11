Shader "Billboard/Lit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _ScaleFactor ("Scale Factor", Range(0, 1)) = 0
        _ReferenceDistance ("Referance Distance", Float) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+500" }
        LOD 100

        Pass
        {

            
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off


            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScaleFactor;
            float _ReferenceDistance;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;

                float4 world_origin = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1));
                float4 view_origin = float4(UnityObjectToViewPos(float3(0, 0, 0)), 1);

                float scale = (_ScaleFactor * (length(view_origin)/_ReferenceDistance)) + ((1 - _ScaleFactor));

                float4 world_pos = mul(UNITY_MATRIX_M, float4(scale, scale, scale, 1) * v.vertex);
                float4 flipped_world_pos = float4(-1, 1, -1, 1) * (world_pos - world_origin) + world_origin;
                float4 view_pos = flipped_world_pos - world_origin + view_origin;

                float4 clip_pos = mul(UNITY_MATRIX_P, view_pos);

                o.vertex = clip_pos;



                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
