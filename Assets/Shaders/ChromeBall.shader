// Sfera cromata per sprite: base argentata (lo sprite) + riflesso dell'ambiente (_ReflectionTex,
// ripreso da ChromeBall.cs) deformato come su uno specchio sferico + luce speculare.
Shader "Gotcha/ChromeBall"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _ReflectionTex ("Reflection", 2D) = "gray" {}
        _Reflectivity ("Reflectivity", Range(0, 1)) = 0.6
        _Radius ("Sphere radius in UV", Range(0.1, 0.5)) = 0.446
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _ReflectionTex;
            float _Reflectivity;
            float _Radius;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv) * i.color;

                // punto sulla sfera: p in [-1, 1] dentro il cerchio
                float2 p = (i.uv - 0.5) / _Radius;
                float r2 = saturate(dot(p, p));
                float3 n = float3(p, sqrt(1.0 - r2));

                // specchio sferico: al centro si vede ciò che sta vicino, verso il bordo ciò che sta lontano
                float2 ruv = 0.5 + p * (0.22 + 0.28 * r2);
                fixed3 refl = tex2D(_ReflectionTex, ruv).rgb;

                // il riflesso evita il contorno a inchiostro (bordo) e si attenua dove la base è molto scura
                float inside = 1.0 - smoothstep(0.80, 0.90, sqrt(r2));
                float fresnel = 0.75 + 0.25 * r2;
                float k = _Reflectivity * inside * fresnel;
                fixed3 col = lerp(baseCol.rgb, refl * 1.15 + baseCol.rgb * 0.25, k);

                // luce speculare in alto a sinistra
                float3 L = normalize(float3(-0.45, 0.55, 0.7));
                float spec = pow(saturate(dot(n, L)), 60.0) * inside;
                col += spec * 0.9;

                return fixed4(col, baseCol.a);
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
