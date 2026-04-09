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
        private LandmarkStructure _landmarkStructure;

        public LandmarkMeshRenderer(
            LandmarkReference landmark,
            Celestial celestial,
            LandmarkStructure landmarkStructure)
        {
            _landmark = landmark;
            _landmarkStructure = landmarkStructure;
            _celestial = celestial;

            var pbr = (IMeshRenderer<InstanceData>)
                Program.Instance.SuperMeshRenderSystem.MeshRendererStaticPbr;
            var prepass = (IMeshRenderer<InstanceData>)
                Program.Instance.SuperMeshRenderSystem.MeshRendererStaticPrePass;

            // Load the glTF asset the same way CharacterAvatar does
            var gltf = Program.Instance.SuperMeshRenderSystem.GltfSystem.GetOrLoad((AssetName)landmarkStructure.MeshID);

            _mesh = new StaticMeshRenderable(pbr, gltf.Id, prepass);

            DefaultCategory.Log.Info($"LandmarkMeshRenderer: Initialized mesh '{landmarkStructure.MeshID}' at " + $"Lat={landmark.Latitude.ToStringDegrees()} Lon={landmark.Longitude.ToStringDegrees()}",
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
            // Points up from the surface
            float3 surfaceUp = float3.Pack(in forwardCcf);
            // Points east
            float3 surfaceEast = float3.Normalize(float3.Cross(surfaceUp, new float3(0, 0, 1)));
            // Points north
            float3 surfaceNorth = float3.Normalize(float3.Cross(surfaceEast, surfaceUp));

            // Convert to EGO (camera-relative) space like PbrSpheres does
            Camera camera = viewport.GetCamera();
            double3 surfacePosEcl = _celestial.GetSurfacePositionEclFromCce(forwardCcf.Transform(_celestial.GetCcf2Cce()), false);

            double3 egoPos = camera.EclToEgo(surfacePosEcl);
            // Position of the mesh
            float3 positionEgo = float3.Pack(in egoPos);

            // Build and apply rotation
            /*
            floatQuat rotX = floatQuat.CreateFromAxisAngle(surfaceEast, _landmarkStructure.Rotation.X);
            floatQuat rotY = floatQuat.CreateFromAxisAngle(surfaceNorth, _landmarkStructure.Rotation.Y);
            floatQuat rotZ = floatQuat.CreateFromAxisAngle(surfaceUp, _landmarkStructure.Rotation.Z);
            floatQuat combined = rotZ * rotY * rotX;
            float4x4 rotation = float4x4.CreateFromQuaternion(combined);

            surfaceEast = float3.Transform(surfaceEast, rotation);
            surfaceNorth = float3.Transform(surfaceNorth, rotation);
            surfaceUp = float3.Transform(surfaceUp, rotation);
            */

            // Apply scale
            surfaceNorth = surfaceNorth * _landmarkStructure.Scale.X;
            surfaceUp = surfaceUp * _landmarkStructure.Scale.Y;
            surfaceEast = surfaceEast * _landmarkStructure.Scale.Z;

            // Apply position offset
            positionEgo = positionEgo + _landmarkStructure.Position;

            return new float4x4(
                surfaceEast.X, surfaceEast.Y, surfaceEast.Z, 0,
                surfaceUp.X, surfaceUp.Y, surfaceUp.Z, 0,
                surfaceNorth.X, surfaceNorth.Y, surfaceNorth.Z, 0,
                positionEgo.X, positionEgo.Y, positionEgo.Z, 1
            );
        }

        public void Dispose() => _mesh.Dispose();
    }
}