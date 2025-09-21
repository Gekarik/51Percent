Shader "Custom/HexSides"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Height ("Raise Height", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Cull Back
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float _Height;

            v2f vert(appdata v)
            {
                v2f o;
                v.vertex.y += (v.vertex.y < 0.01) ? _Height : 0;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 _Color;

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
