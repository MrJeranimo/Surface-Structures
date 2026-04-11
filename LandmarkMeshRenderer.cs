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

                DefaultCategory.Log.Info($"LandmarkMeshRenderer: Initialized mesh '{landmarkStructure.MeshID}' at " + $"Lat={landmark.Latitude.ToStringDegrees()} Lon={landmark.Longitude.ToStringDegrees()}");
            }
            catch (Exception ex)
            {
                DefaultCategory.Log.Error($"Surface Structures - Could not load mesh: {landmarkStructure.MeshID}. Exception\n{ex}");
            }
        }

        private bool _hasLoggedFirstDraw = false;

        public void Draw(Viewport viewport)
        {
            if (_mesh == null) return;
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
                _hasLoggedFirstDraw = true;
            }

            _mesh.Draw();
        }

        private float4x4 BuildSurfaceTransform(Viewport viewport)
        {
            double3 forwardCcf = _landmark.ForwardCcf;

            // Build basis in CCF (Z = north pole in CCF, which is correct per the docs)
            double3 surfaceUpCcf = double3.Normalize(forwardCcf);
            double3 surfaceEastCcf = double3.Normalize(double3.Cross(surfaceUpCcf, new double3(0, 0, 1)));
            double3 surfaceNorthCcf = double3.Normalize(double3.Cross(surfaceUpCcf, surfaceEastCcf));

            // Rotate basis from CCF into ECL-aligned space so offsets/rotations work correctly
            // ccf2Cce rotation brings CCF axes into ECL-axis alignment (same axes as ECL, just origin on celestial)
            floatQuat ccf2CceQuat = floatQuat.Pack(_celestial.GetCcf2Cce());
            float3 surfaceUp = float3.Normalize(float3.TransformNormal(float3.Pack(surfaceUpCcf), float4x4.CreateFromQuaternion(ccf2CceQuat)));
            float3 surfaceEast = float3.Normalize(float3.TransformNormal(float3.Pack(surfaceEastCcf), float4x4.CreateFromQuaternion(ccf2CceQuat)));
            float3 surfaceNorth = float3.Normalize(float3.TransformNormal(float3.Pack(surfaceNorthCcf), float4x4.CreateFromQuaternion(ccf2CceQuat)));

            // Get surface position in EGO space (this part was fine)
            Camera camera = viewport.GetCamera();
            double3 surfacePosEcl = _celestial.GetSurfacePositionEclFromCce(forwardCcf.Transform(_celestial.GetCcf2Cce()), false);

            //surfacePosEcl += new double3(surfaceNorth.X, surfaceNorth.Y, surfaceNorth.Z) * _landmarkStructure.Position.Y;
            //surfacePosEcl += new double3(surfaceUp.X, surfaceUp.Y, surfaceUp.Z) * _landmarkStructure.Position.Z;
            surfacePosEcl += new double3(surfaceEast.X, surfaceEast.Y, surfaceEast.Z) * _landmarkStructure.Position.X;

            double3 egoPos = camera.EclToEgo(surfacePosEcl);
            float3 positionEgo = float3.Pack(in egoPos);

            // Apply rotation to unit basis vectors FIRST, then scale
            floatQuat rotX = floatQuat.CreateFromAxisAngle(surfaceEast, _landmarkStructure.Rotation.X);
            floatQuat rotY = floatQuat.CreateFromAxisAngle(surfaceNorth, _landmarkStructure.Rotation.Y);
            floatQuat rotZ = floatQuat.CreateFromAxisAngle(surfaceUp, _landmarkStructure.Rotation.Z);
            floatQuat combined = rotZ * rotY * rotX;
            float4x4 rotMat = float4x4.CreateFromQuaternion(combined);

            surfaceEast = float3.Normalize(float3.TransformNormal(surfaceEast, rotMat));
            surfaceNorth = float3.Normalize(float3.TransformNormal(surfaceNorth, rotMat));
            surfaceUp = float3.Normalize(float3.TransformNormal(surfaceUp, rotMat));

            // Scale AFTER rotation
            float3 col0 = -surfaceEast * _landmarkStructure.Scale.X;
            float3 col1 = surfaceUp * _landmarkStructure.Scale.Y;
            float3 col2 = surfaceNorth * _landmarkStructure.Scale.Z;

            // Position offset applied in EGO space (offset directions now correct since basis is ECL-aligned)
            positionEgo = positionEgo + _landmarkStructure.Position;

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