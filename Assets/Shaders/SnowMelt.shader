// Neve che si scioglie, per lo sprite intero del pupazzo (sm_body). Con _Melt da 0 a 1:
// - ogni colonna si abbassa verso terra (i bordi prima del centro, con un'ondulazione irregolare): la neve
//   si affloscia e cola, non viene "tagliata";
// - vicino a terra la massa si allarga, come neve bagnata che si spande nella pozzanghera;
// - il colore diventa azzurrino e bagnato, con striature che colano e un bordo lucido in cima;
// - un leggero tremolio liquido.
// La stessa formula è replicata in SnowmanMelter.cs per far seguire alla neve gli oggetti attaccati.
Shader "Gotcha/SnowMelt"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Melt ("Melt", Range(0, 1)) = 0
        _Ground ("Ground (v)", Range(0, 1)) = 0.076
        _Seed ("Seed", Float) = 0
        _WetTint ("Wet tint", Color) = (0.78, 0.9, 1, 1)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "False" }
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
            float _Melt;
            float _Ground;
            float _Seed;
            fixed4 _WetTint;

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

            float Noise(float u)
            {
                return 0.5 + 0.3 * sin(u * 19.0 + _Seed) + 0.2 * sin(u * 41.0 + 2.0 + _Seed * 0.7);
            }

            float Keep(float u, float m)   // quanta altezza resta alla colonna u (1 = intatta)
            {
                float edge = abs(u - 0.5) * 2.0;
                return clamp(1.0 - m * (1.0 + 0.18 * Noise(u) + 0.7 * pow(edge, 1.5)), 0.0001, 1.0);
            }

            fixed4 Sample(float2 uv)
            {
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return fixed4(0, 0, 0, 0);
                return tex2D(_MainTex, uv);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float m = _Melt;
                float u = i.uv.x, v = i.uv.y;
                float s = Keep(u, m);
                float h = v - _Ground;
                float spread = 1.0 + m * 1.6 * exp(-max(h, 0.0) * 9.0);
                float us = 0.5 + (u - 0.5) / spread + sin(v * 25.0 + _Time.y * 6.0) * 0.004 * m;
                float vs = h > 0.0 ? _Ground + h / s : v;
                fixed4 c = Sample(float2(us, vs));
                if (m <= 0.0001) return c * i.color;

                // bagnata: più azzurra, con striature che colano
                c.rgb *= lerp(fixed3(1, 1, 1), _WetTint.rgb, saturate(m * 1.2));
                float streak = pow(0.5 + 0.5 * sin(u * 140.0 + Noise(u) * 9.0), 12.0) * m * 0.35;
                c.rgb *= 1.0 - streak * fixed3(0.35, 0.15, 0.0);
                // bordo lucido in cima alla neve che si affloscia
                float above = Sample(float2(us, vs + 0.035 / max(s, 0.05))).a;
                float rim = (above < 0.5 && c.a > 0.5) ? saturate(m * 3.0) * 0.6 : 0.0;
                c.rgb += (1.0 - c.rgb) * rim;
                return c * i.color;
            }
            ENDCG
        }
    }
}
