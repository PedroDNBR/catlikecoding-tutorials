using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace VALUENOISE
{
    using static Noise;

    public class NoiseVisualization : Visualization
    {
        static ScheduleDelegate[] noiseJobs =
        {
            Job<Lattice1D>.ScheduleParallel,
            Job<Lattice2D>.ScheduleParallel,
            Job<Lattice3D>.ScheduleParallel,
        };
        static int
            noiseId = Shader.PropertyToID("_Noise");

        [SerializeField]
        int seed;

        [SerializeField, Range(1, 3)]
        int dimensions = 3;

        [SerializeField]
        HASHINGSPACE.SpaceTRS domain = new HASHINGSPACE.SpaceTRS
        {
            scale = 8f,
        };

        NativeArray<float4> noise;

        ComputeBuffer noiseBuffer;

        protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock)
        {
            noise = new NativeArray<float4>(dataLength, Allocator.Persistent);
            noiseBuffer = new ComputeBuffer(dataLength * 4, 4);
            propertyBlock ??= new MaterialPropertyBlock();
            propertyBlock.SetBuffer(noiseId, noiseBuffer);
        }

        protected override void DisableVisualization()
        {
            noise.Dispose();
            noiseBuffer.Release();
            noiseBuffer = null;
        }

        protected override void UpdateVisualization(
            NativeArray<float3x4> positions, int resolution, JobHandle handle
        )
        {
            noiseJobs[dimensions - 1](
                positions, noise, seed, domain, resolution, handle
            ).Complete();

            noiseBuffer.SetData(noise.Reinterpret<float>(4 * 4));
        }
    }

}
