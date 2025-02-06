// https://gamedev.stackexchange.com/a/18459/147473
float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRaydir) {
    float3 tmin = (boundsMin - rayOrigin)*invRaydir;
    float3 tmax = (boundsMax - rayOrigin)*invRaydir;

    float3 t1 = min(tmin, tmax);
    float3 t2 = max(tmin, tmax);

    // Find entry and exit points along the ray
    float tNear = max(t1.x, max(t1.y, t1.z));
    float tFar = min(t2.x, min(t2.y, t2.z));

    return float2(tNear, tFar);
}


float Unity_RandomRange_float(float2 Seed)
{
  uint hash = asuint(Seed.x) ^ asuint(Seed.y) * 0x45d9f3bU;
    hash = (hash ^ (hash >> 16)) * 0x45d9f3bU;
    return (float)(hash & 0x7FFFFFFFU) / 2147483648.0 - 0.5;
} 

float3 Unity_HashRandomRange_float3(float2 Seed)
{
    float3 constants = float3(127.1, 311.7, 74.7); // Unique constants for each dimension
    float3 random;
    
    random.x = frac(sin(dot(Seed, constants.xy)) * 43758.5453) - 0.5;
    random.y = frac(sin(dot(Seed, constants.yz)) * 43758.5453) - 0.5;
    random.z = frac(sin(dot(Seed, constants.zx)) * 43758.5453) - 0.5;

    return random;
}

SAMPLER(sampler_linear_mirror);
SAMPLER(sampler_linear_repeat);
float SamplePoint (
  float3 p
) {
  
  float n3D = SAMPLE_TEXTURE3D(_Noise3D, sampler_linear_mirror, p).x;

  float n = (SAMPLE_TEXTURE2D(_Noise2D, sampler_linear_repeat , p.xz * _Noise2DScale).x  * 2 - 1) * _Noise2DPower * 0.01;
  float noise = n + n3D ; 

  float noiseAdjusted = (noise - _Threshold*0.01) * _Multiplier;
  return saturate(noiseAdjusted);
}

void volumeFog_float(
    float MeshDistance,
    float3 Position,
    float3 View,
    out float Fog,
    out float Shadow
    ) { 
    float3 objectPosition = SHADERGRAPH_OBJECT_POSITION;
    float3 objectScale = float3(length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                             length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                             length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z)));

    float3 Size = float3(_Size, _Size * _YtoSizeRation, _Size) * 0.001;

    _Offset *= 0.01;
    
    float3 cameraPosition = _WorldSpaceCameraPos;

    float3 boundsMin = objectPosition - objectScale / 2;
    float3 boundsMax = objectPosition + objectScale / 2;
    int Samples = _Samples;

    float2 rayToContainerInfo = rayBoxDst(boundsMin, boundsMax, cameraPosition, 1/View);

    bool isCameraInsideCube = 
      cameraPosition.x > boundsMin.x && cameraPosition.x < boundsMax.x &&
      cameraPosition.y > boundsMin.y && cameraPosition.y < boundsMax.y &&
      cameraPosition.z > boundsMin.z && cameraPosition.z < boundsMax.z;

    float3 entryPoint;
    float3 exitPoint;

    if (isCameraInsideCube) {
      exitPoint = cameraPosition + max(rayToContainerInfo.x, -MeshDistance) * View;
      entryPoint  = cameraPosition; // Use the camera position as the entry point
    } else {
      exitPoint = cameraPosition + max(rayToContainerInfo.x, -MeshDistance) * View;
      entryPoint  = cameraPosition + max(rayToContainerInfo.y, -MeshDistance) * View;
    }

    float3 rayPos = entryPoint;
    float rayLength = distance(entryPoint, exitPoint);
    float stepSize = rayLength / Samples;

    float density = 0.0;

    Shadow = 0;
    [loop]
    for (int i = 0; i < Samples; i++)
    {
      float3 samplePos = rayPos - i * stepSize * View;

      float3 randomOffset = Unity_HashRandomRange_float3(samplePos.yz + samplePos.zz + float2(0,_Time.y)*0.01)* _Randomness;
  
      samplePos += randomOffset;
      float ShadowAtten;
      #ifdef SHADERGRAPH_PREVIEW
        ShadowAtten = 0;
      #else
        #if SHADOWS_SCREEN
          float4 clipPos = TransformWorldToHClip(samplePos);
          float4 shadowCoord = ComputeScreenPos(clipPos);
        #else
          float4 shadowCoord = TransformWorldToShadowCoord(samplePos);
        #endif
        ShadowAtten = smoothstep(.45, .5,MainLightRealtimeShadow(shadowCoord));
      #endif



      float3 distanceToMinBounds = abs(samplePos - boundsMin);
      float3 distanceToMaxBounds = abs(samplePos - boundsMax);

      samplePos -= _Offset;
      samplePos *= Size;
      samplePos += _Offset;

      float noiseValue = SamplePoint(samplePos);

      float bordersFade = (min(
        min(distanceToMinBounds.x, distanceToMaxBounds.x),
        min(min(distanceToMinBounds.y, distanceToMaxBounds.y),
        min(distanceToMinBounds.z, distanceToMaxBounds.z))
      ));
      float fadeFactor = smoothstep(0,max(0, _BoundsFade), bordersFade * (_BoundsFadeRange/10.0));
      float pointDensity = noiseValue * stepSize * fadeFactor;
      density += pointDensity;
      Shadow += ( (ShadowAtten*_ShadowMulti  + _ShadowAdd)) *  pointDensity;
      if(density > _LoopStop) break;
    }
    Shadow = saturate(Shadow);
    Fog = (1.0 - exp(-(density)));
}