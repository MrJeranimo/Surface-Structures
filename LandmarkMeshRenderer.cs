using Brutal.Logging;
using Brutal.Numerics;
using Core;
using KSA;
using System.Runtime.InteropServices.Swift;

namespace Surface_Structures
{
    public class LandmarkMeshRenderer : IDisposable
    {
        private StaticMeshRenderable? _mesh;
        private readonly LandmarkReference _landmark;
        private readonly Celestial _celestial;
        private LandmarkStructure _landmarkStructure;
        private const float deg2rad = (MathF.PI / 180f);

        public LandmarkMeshRenderer(
            LandmarkReference landmark,
            Celestial celestial,
            LandmarkStructure landmarkStructure)
        {
            _landmark = landmark;
            _landmarkStructure = landmarkStructure;
            _celestial = celestial;

            IMeshRenderer<InstanceData> pbr = Program.Instance.SuperMeshRenderSystem.MeshRendererStaticPbr;
            IMeshRenderer<InstanceData> prepass = Program.Instance.SuperMeshRenderSystem.MeshRendererStaticPrePass;

            // Load the glTF asset the same way CharacterAvatar does
            try
            {
                GltfPbrAssetRef gltf = Program.Instance.SuperMeshRenderSystem.GltfSystem.GetOrLoad((AssetName)landmarkStructure.MeshID);
                _mesh = new StaticMeshRenderable(pbr, gltf.Id, prepass);

                DefaultCategory.Log.Info($"Surface Structures - Initialized mesh '{landmarkStructure.MeshID}' at Lat={landmark.Latitude.ToStringDegrees()} Lon={landmark.Longitude.ToStringDegrees()}");
            }
            catch (Exception ex)
            {
                DefaultCategory.Log.Error($"Surface Structures - Could not load mesh: {landmarkStructure.MeshID}. Exception\n{ex}");
            }
        }

        public void Draw(Viewport viewport)
        {
            if (_mesh == null || !_landmarkStructure.Visible) return;
            _mesh.Transform = BuildSurfaceTransform(viewport);
            _mesh.Draw();
        }

        private float4x4 BuildSurfaceTransform(Viewport viewport)
        {
            double3 forwardCcf = _landmark.ForwardCcf;

            // Build surface basis in CCF - Z axis is north pole in CCF
            double3 surfaceUpCcf = double3.Normalize(forwardCcf);
            double3 surfaceEastCcf = double3.Normalize(double3.Cross(new double3(0, 0, 1), surfaceUpCcf));
            // Pole guard
            if (surfaceEastCcf.Length() < 0.001)
                surfaceEastCcf = new double3(1, 0, 0);
            surfaceEastCcf = double3.Normalize(surfaceEastCcf);
            double3 surfaceNorthCcf = double3.Normalize(double3.Cross(surfaceEastCcf, surfaceUpCcf));

            // Rotate basis from CCF into CCE (ECL-aligned axes, celestial origin)
            floatQuat ccf2CceQuat = floatQuat.Pack(_celestial.GetCcf2Cce());
            float4x4 ccf2CceMat = float4x4.CreateFromQuaternion(ccf2CceQuat);
            float3 surfaceUp = float3.Normalize(float3.TransformNormal(float3.Pack(surfaceUpCcf), ccf2CceMat));
            float3 surfaceEast = float3.Normalize(float3.TransformNormal(float3.Pack(surfaceEastCcf), ccf2CceMat));
            float3 surfaceNorth = float3.Normalize(float3.TransformNormal(float3.Pack(surfaceNorthCcf), ccf2CceMat));

            // Get surface position in ECL, apply positional offsets along surface basis
            Camera camera = viewport.GetCamera();
            double3 surfacePosEcl = _celestial.GetSurfacePositionEclFromCce(forwardCcf.Transform(_celestial.GetCcf2Cce()), false);

            surfacePosEcl += new double3(surfaceEast.X, surfaceEast.Y, surfaceEast.Z) * _landmarkStructure.Position.X;
            surfacePosEcl += new double3(surfaceNorth.X, surfaceNorth.Y, surfaceNorth.Z) * -_landmarkStructure.Position.Y;
            surfacePosEcl += new double3(surfaceUp.X, surfaceUp.Y, surfaceUp.Z) * _landmarkStructure.Position.Z;

            double3 egoPos = camera.EclToEgo(surfacePosEcl);
            float3 positionEgo = float3.Pack(in egoPos);

            // Apply landmark rotation around each surface axis
            floatQuat rotX = floatQuat.CreateFromAxisAngle(surfaceNorth, _landmarkStructure.Rotation.X * deg2rad);
            floatQuat rotY = floatQuat.CreateFromAxisAngle(-surfaceEast, _landmarkStructure.Rotation.Y * deg2rad);
            floatQuat rotZ = floatQuat.CreateFromAxisAngle(-surfaceUp, _landmarkStructure.Rotation.Z * deg2rad);
            floatQuat combined = rotZ * rotY * rotX;
            float4x4 rotMat = float4x4.CreateFromQuaternion(combined);

            surfaceEast = float3.Normalize(float3.TransformNormal(surfaceEast, rotMat));
            surfaceNorth = float3.Normalize(float3.TransformNormal(surfaceNorth, rotMat));
            surfaceUp = float3.Normalize(float3.TransformNormal(surfaceUp, rotMat));

            // Scale basis vectors - no negation
            float3 col0 = -surfaceNorth * _landmarkStructure.Scale.X;
            float3 col1 = surfaceUp * _landmarkStructure.Scale.Y;
            float3 col2 = surfaceEast * _landmarkStructure.Scale.Z;

            return new float4x4(
                col0.X, col0.Y, col0.Z, 0,
                col1.X, col1.Y, col1.Z, 0,
                col2.X, col2.Y, col2.Z, 0,
                positionEgo.X, positionEgo.Y, positionEgo.Z, 1
            );
        }

        public void Dispose() => _mesh?.Dispose();
    }
}