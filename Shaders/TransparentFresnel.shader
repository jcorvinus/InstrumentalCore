Shader "Instrumental/TransparentFresnel" 
{
    Properties 
    {
		_RimColor ("Rim Color", Color) = (1,1,1,1)
		_RimPower ("Rim Power", Range(0, 8.0)) = 3.0
    }
    SubShader
    {
        Tags 
        {
            "RenderType"="Transparent"
            "Queue" = "Transparent"
        }
        LOD 100

        Pass 
		{
			Name "Depth"
			ZWrite On
			ColorMask 0
		}

        Pass
        {
            Name "Main"
            Cull Back
		    Blend SrcAlpha OneMinusSrcAlpha
		    ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float fresnel : TEXCOORD0;
            };

            float4 _RimColor;
		    float _RimPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 viewDirection = normalize(ObjSpaceViewDir(v.vertex));
                float dotProduct = dot (viewDirection, v.normal);
                o.fresnel = 1.0 - saturate(dotProduct);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float power = pow(i.fresnel, _RimPower);
                fixed4 col = fixed4(
                    _RimColor.r * power,
                    _RimColor.g * power,
                    _RimColor.b * power, i.fresnel);
                return col;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}