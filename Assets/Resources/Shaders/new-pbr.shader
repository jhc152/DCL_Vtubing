Shader "Unlit/new-pbr"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
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
                float3 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = WorldSpaceViewDir(v.vertex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //// sample the texture
                fixed4 finalColor = tex2D(_MainTex, i.uv);
                //// apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //return col;



                float angleY = 3.14 / 2.0;

                float3x3 rotateY = float3x3(
                    cos(angleY), 0, sin(angleY),
                    0, 1, 0,
                    -sin(angleY), 0, cos(angleY)
                );

                // view angle DOT normal of the position
               /* float3 tray = normalize(i.uv.z * rotateY);
                float3 rayDirection = normalize(tray - i.vertex);
                float t = (dot(normalW, rayDirection) + 1.0) / 2.0;*/

                float ToonThresholds[3];
                ToonThresholds[0] = 0.90;
                ToonThresholds[1] = 0.80;
                ToonThresholds[2] = 0.20;

                float ToonBrightnessLevels[4];
                ToonBrightnessLevels[0] = 1.2;
                ToonBrightnessLevels[1] = 1.0;
                ToonBrightnessLevels[2] = 1.0;
                ToonBrightnessLevels[3] = 0.8;

        /*        if (t > ToonThresholds[0])
                {
                    finalColor.rgb *= ToonBrightnessLevels[0];
                }
                else if (t > ToonThresholds[1])
                {
                    finalColor.rgb *= ToonBrightnessLevels[1];
                }
                else if (t > ToonThresholds[2])
                {
                    finalColor.rgb *= ToonBrightnessLevels[2];
                }
                else
                {
                    finalColor.rgb *= ToonBrightnessLevels[3];
                }*/


                finalColor.rgb *= ToonBrightnessLevels[0];


                return finalColor;


            }
            ENDCG
        }
    }
}
