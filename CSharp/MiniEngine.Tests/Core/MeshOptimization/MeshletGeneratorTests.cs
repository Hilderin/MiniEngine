using MiniEngine.AssetDefinitions;
using MiniEngine.AssetImporters;
using MiniEngine.Core.MeshOptimization;
using MiniEngine.Drivers.Vulkan;
using MiniEngine.Rendering.Vulkan;
using MiniEngine.Tests.Mocks;

namespace MiniEngine.Tests.Core.MeshOptimization
{
    [TestClass]
    public class MeshletGeneratorTests
    {
        private Context _context = null;
        private MockRenderer _renderer = null;

        [TestInitialize]
        public void Init()
        {
            if (_context == null)
            {
                _renderer = new MockRenderer();
                _context = new Context();
                _context.SetRenderer(_renderer)
                           .Init();
            }
        }

        /// <summary>
        /// BasicCubeTest
        /// </summary>
        [TestMethod]
        public void BasicCubeTest()
        {

            MeshletGenerator meshletGenerator = new MeshletGenerator();

            var originalMesh = Primitives.CreateCubeMeshDefinition();
            var newMesh = meshletGenerator.Convert(originalMesh);


            Assert.AreEqual(1, newMesh.SubMeshes.Count);
            Assert.AreEqual(1, newMesh.SubMeshes[0].Meshlets.Length);
            Assert.AreEqual(originalMesh.SubMeshes[0].Indices.Length, newMesh.SubMeshes[0].Indices.Length);
            Assert.AreEqual((uint)36, newMesh.SubMeshes[0].Meshlets[0].indices_count);
            Assert.AreEqual((uint)8, newMesh.SubMeshes[0].Meshlets[0].vertex_count);

        }

        /// <summary>
        /// AntiqueCeramicVaseTest
        /// </summary>
        [TestMethod]
        public void AntiqueCeramicVaseTest()
        {

            var mesh = (MockMesh)_context.Asset.Get<Mesh>(@"..\..\Assets\Tests\AntiqueCeramicVase\antique_ceramic_vase_01_4k.obj.asset");

            MeshletGenerator meshletGenerator = new MeshletGenerator();

            var newMesh = meshletGenerator.Convert(mesh.MeshDefinition);

            Assert.AreEqual(1, newMesh.SubMeshes.Count);
            Assert.AreEqual(1, newMesh.SubMeshes[0].Meshlets.Length);
            Assert.AreEqual(mesh.MeshDefinition.SubMeshes[0].Indices.Length, newMesh.SubMeshes[0].Indices.Length);
            Assert.AreEqual((uint)36, newMesh.SubMeshes[0].Meshlets[0].indices_count);
            Assert.AreEqual((uint)8, newMesh.SubMeshes[0].Meshlets[0].vertex_count);

        }
    }
}