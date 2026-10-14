// Biglia d'acciaio realistica per sprite. Lo sprite fornisce solo la forma (alpha); il colore è tutto calcolato:
// - riflesso dell'ambiente (_ReflectionTex, ripreso da ChromeBall.cs) deformato come in uno specchio sferico
// - "studio" virtuale sopra il piano (cielo tenue + softbox) che dà la lucentezza dell'acciaio
// - fresnel (ai bordi riflette di più), due luci speculari nette, bordo di contatto scuro.
Shader "Gotcha/ChromeBall"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _ReflectionTex ("Reflection", 2D) = "black" {}
        _Reflectivity ("Reflectivity", Range(0, 2)) = 1.3
        _Studio ("Studio light", Range(0, 1)) = 0.5
        _Radius ("Sphere radius in UV", Range(0.1, 0.5)) = 0.463
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
            float _Studio;
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

                float2 p = (i.uv - 0.5) / _Radius;
                float r2 = saturate(dot(p, p));
                float r = sqrt(r2);
                float3 n = float3(p, sqrt(1.0 - r2));

                // 1. ambiente riflesso: al centro ciò che è vicino, verso il bordo ciò che è lontano (compresso)
                float2 ruv = 0.5 + p * (0.14 + 0.36 * r2);
                float3 env = tex2D(_ReflectionTex, ruv).rgb;

                // 2. vista dall'alto: il centro della biglia riflette il soffitto (studio luminoso), i bordi radenti
                //    riflettono ciò che sta attorno sul piano (pareti al neon, fondo)
                float sky = saturate(n.y * 0.5 + 0.5);
                float3 studio = lerp(float3(0.30, 0.33, 0.38), float3(0.86, 0.89, 0.93), sky);

                // softbox riflesso: macchia di luce allungata e morbida in alto a sinistra
                float2 q = p - float2(-0.3, 0.38);
                float c = 0.8, s = 0.6;   // rotazione di circa 37 gradi
                float2 qr = float2(q.x * c + q.y * s, -q.x * s + q.y * c);
                float softbox = exp(-(qr.x * qr.x / 0.05 + qr.y * qr.y / 0.007));

                // 3. miscela soffitto/ambiente secondo l'angolo (fresnel): ai bordi domina l'ambiente
                float wEnv = smoothstep(0.2, 0.92, r);
                float3 col = lerp(studio * _Studio * 2.0, env * _Reflectivity, wEnv) + env * 0.25 * (1.0 - wEnv);
                col += softbox * 0.9;

                // 4. luci speculari nette (lampada principale e riflesso secondario in basso a destra)
                float3 L1 = normalize(float3(-0.42, 0.5, 0.76));
                float3 L2 = normalize(float3(0.55, -0.45, 0.7));
                col += pow(saturate(dot(n, L1)), 260.0) * 1.6;
                col += pow(saturate(dot(n, L2)), 120.0) * 0.35;

                // 5. bordo di contatto scuro e leggera tinta fredda dell'acciaio
                col *= lerp(1.0, 0.45, smoothstep(0.88, 1.0, r));
                col *= float3(0.97, 0.99, 1.03);

                return fixed4(saturate(col), baseCol.a);
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
