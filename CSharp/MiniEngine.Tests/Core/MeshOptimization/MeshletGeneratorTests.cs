using MiniEngine.AssetDefinitions;
using MiniEngine.AssetImporters;
using MiniEngine.MeshOptimization;
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
            var meshLetContainers = meshletGenerator.Generate(originalMesh);


            Assert.AreEqual(1, meshLetContainers.Length);
            Assert.AreEqual(1, meshLetContainers[0].Meshlets.Length);
            Assert.AreEqual(originalMesh.SubMeshes[0].Indices.Length, meshLetContainers[0].Indices.Length);
            Assert.AreEqual((ushort)36, meshLetContainers[0].Meshlets[0].IndicesCount);
            Assert.AreEqual((byte)8, meshLetContainers[0].Meshlets[0].VertexCount);

        }

        /// <summary>
        /// AntiqueCeramicVaseTest
        /// </summary>
        [TestMethod]
        public void AntiqueCeramicVaseTest()
        {

            var mesh = (MockMesh)_context.Asset.Get<Mesh>(@"..\..\Assets\Tests\AntiqueCeramicVase\antique_ceramic_vase_01_4k.obj.asset");

            MeshletGenerator meshletGenerator = new MeshletGenerator();

            var meshLetContainers = meshletGenerator.Generate(mesh.MeshDefinition);

            Assert.AreEqual(1, meshLetContainers.Length);
            Assert.AreEqual(55, meshLetContainers[0].Meshlets.Length);
            Assert.AreEqual(mesh.MeshDefinition.SubMeshes[0].Indices.Length, meshLetContainers[0].Indices.Length);
            Assert.AreEqual((uint)516, meshLetContainers[0].Meshlets[0].IndicesCount);
            Assert.AreEqual((uint)123, meshLetContainers[0].Meshlets[0].VertexCount);

        }
    }
}