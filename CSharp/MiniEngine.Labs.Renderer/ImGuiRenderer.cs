using ImGuiNET;
using MiniEngine.Drivers.Vulkan;
using MiniEngine.Rendering.Vulkan;

namespace MiniEngine.Labs.Renderer
{
    public class ImGuiRenderer
    {

        public ImGuiRenderer(Device device)
        {
            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            ImGui.GetIO().Fonts.AddFontDefault();
            ImGui.GetIO().Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;


        }

    }
}
