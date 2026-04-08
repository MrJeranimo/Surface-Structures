using Brutal.Logging;
using Brutal.Numerics;
using Core;
using KSA;

namespace Surface_Structures
{
    public class LandmarkMeshRenderer : IDisposable
    {
        private readonly StaticMeshRenderable _mesh;
        private readonly LandmarkReference _landmark;
        private readonly Celestial _celestial;
        public static double HeightOffset { get; set; } = 0;

        public LandmarkMeshRenderer(
            LandmarkReference landmark,
            Celestial celestial,
            string gltfId)
        {
            _landmark = landmark;
            _celestial = celestial;

            var pbr = (IMeshRenderer<InstanceData>)
                Program.Instance.SuperMeshRenderSystem.MeshRendererStaticPbr;
            var prepass = (IMeshRenderer<InstanceData>)
                Program.Instance.SuperMeshRenderSystem.MeshRendererStaticPrePass;

            // Load the glTF asset the same way CharacterAvatar does
            var gltf = Program.Instance.SuperMeshRenderSystem.GltfSystem.GetOrLoad((AssetName)gltfId);

            _mesh = new StaticMeshRenderable(pbr, gltf.Id, prepass);

            DefaultCategory.Log.Info($"LandmarkMeshRenderer: Initialized mesh '{gltfId}' at " + $"Lat={landmark.Latitude.ToStringDegrees()} Lon={landmark.Longitude.ToStringDegrees()}",
            "LandmarkMeshRenderer", nameof(LandmarkMeshRenderer), 0);
        }

        private bool _hasLoggedFirstDraw = false;

        public void Draw(Viewport viewport)
        {
            _mesh.Transform = BuildSurfaceTransform(viewport);

            if (!_hasLoggedFirstDraw)
            {
                float4x4 t = _mesh.Transform;
                DefaultCategory.Log.Info(
                    $"LandmarkMeshRenderer: Transform position = " +
                    $"({t.M41}, {t.M42}, {t.M43})",
                    "Draw", nameof(LandmarkMeshRenderer), 0);
                DefaultCategory.Log.Info(
                    $"LandmarkMeshRenderer: SurfaceRadius = {_celestial.RenderData.SurfaceRadius}",
                    "Draw", nameof(LandmarkMeshRenderer), 0);
                DefaultCategory.Log.Info(
                    $"LandmarkMeshRenderer: ForwardCcf = {_landmark.ForwardCcf}",
                    "Draw", nameof(LandmarkMeshRenderer), 0);

                DefaultCategory.Log.Info(
                    $"MeshBucketHandles length: {_mesh.MeshBucketHandles.Length}, " +
                    $"Handle[0] valid: {_mesh.MeshBucketHandles[0]}",
                    "Draw", nameof(LandmarkMeshRenderer), 0);
                _hasLoggedFirstDraw = true;
            }

            _mesh.Draw();
        }

        private float4x4 BuildSurfaceTransform(Viewport viewport)
        {
            // ForwardCcf is the surface normal at the landmark position
            double3 forwardCcf = _landmark.ForwardCcf;
            float3 up = float3.Pack(in forwardCcf);

            // Build orthonormal basis on the surface
            float3 right = float3.Normalize(float3.Cross(up, new float3(0, 0, 1)));
            float3 forward = float3.Cross(right, up);

            // Convert to EGO (camera-relative) space like PbrSpheres does
            Camera camera = viewport.GetCamera();
            double3 surfacePosEcl = _celestial.GetSurfacePositionEclFromCce(forwardCcf.Transform(_celestial.GetCcf2Cce()), false);

            double3 egoPos = camera.EclToEgo(surfacePosEcl);
            float3 positionEgo = float3.Pack(in egoPos);

            return new float4x4(
                right.X, right.Y, right.Z, 0,
                up.X, up.Y, up.Z, 0,
                forward.X, forward.Y, forward.Z, 0,
                positionEgo.X, positionEgo.Y, positionEgo.Z, 1
            );
        }

        public void Dispose() => _mesh.Dispose();
    }
}