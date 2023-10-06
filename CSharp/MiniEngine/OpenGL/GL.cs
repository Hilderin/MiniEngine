using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MiniEngine.OpenGL
{
    /// <summary>
    /// Wrapper OpenGL
    /// Documentation: https://registry.khronos.org/OpenGL-Refpages/gl4/
    /// </summary>
    public unsafe static class GL
    {


        #region The OpenGL constant definitions.

        //   OpenGL Version Identifier
        public const uint GL_VERSION_1_1 = 1;

        //  AccumOp
        public const uint GL_ACCUM = 0x0100;
        public const uint GL_LOAD = 0x0101;
        public const uint GL_RETURN = 0x0102;
        public const uint GL_MULT = 0x0103;
        public const uint GL_ADD = 0x0104;

        //  Alpha functions
        public const uint GL_NEVER = 0x0200;
        public const uint GL_LESS = 0x0201;
        public const uint GL_EQUAL = 0x0202;
        public const uint GL_LEQUAL = 0x0203;
        public const uint GL_GREATER = 0x0204;
        public const uint GL_NOTEQUAL = 0x0205;
        public const uint GL_GEQUAL = 0x0206;
        public const uint GL_ALWAYS = 0x0207;

        //  AttribMask
        public const uint GL_CURRENT_BIT = 0x00000001;
        public const uint GL_POINT_BIT = 0x00000002;
        public const uint GL_LINE_BIT = 0x00000004;
        public const uint GL_POLYGON_BIT = 0x00000008;
        public const uint GL_POLYGON_STIPPLE_BIT = 0x00000010;
        public const uint GL_PIXEL_MODE_BIT = 0x00000020;
        public const uint GL_LIGHTING_BIT = 0x00000040;
        public const uint GL_FOG_BIT = 0x00000080;
        public const uint GL_DEPTH_BUFFER_BIT = 0x00000100;
        public const uint GL_ACCUM_BUFFER_BIT = 0x00000200;
        public const uint GL_STENCIL_BUFFER_BIT = 0x00000400;
        public const uint GL_VIEWPORT_BIT = 0x00000800;
        public const uint GL_TRANSFORM_BIT = 0x00001000;
        public const uint GL_ENABLE_BIT = 0x00002000;
        public const uint GL_COLOR_BUFFER_BIT = 0x00004000;
        public const uint GL_HINT_BIT = 0x00008000;
        public const uint GL_EVAL_BIT = 0x00010000;
        public const uint GL_LIST_BIT = 0x00020000;
        public const uint GL_TEXTURE_BIT = 0x00040000;
        public const uint GL_SCISSOR_BIT = 0x00080000;
        public const uint GL_ALL_ATTRIB_BITS = 0x000fffff;

        //  BeginMode

        /// <summary>
        /// Treats each vertex as a single point. Vertex n defines point n. N points are drawn.
        /// </summary>
        public const uint GL_POINTS = 0x0000;

        /// <summary>
        /// Treats each pair of vertices as an independent line segment. Vertices 2n - 1 and 2n define line n. N/2 lines are drawn.
        /// </summary>
		public const uint GL_LINES = 0x0001;

        /// <summary>
        /// Draws a connected group of line segments from the first vertex to the last, then back to the first. Vertices n and n + 1 define line n. The last line, however, is defined by vertices N and 1. N lines are drawn.
        /// </summary>
		public const uint GL_LINE_LOOP = 0x0002;

        /// <summary>
        /// Draws a connected group of line segments from the first vertex to the last. Vertices n and n+1 define line n. N - 1 lines are drawn.
        /// </summary>
		public const uint GL_LINE_STRIP = 0x0003;

        /// <summary>
        /// Treats each triplet of vertices as an independent triangle. Vertices 3n - 2, 3n - 1, and 3n define triangle n. N/3 triangles are drawn.
        /// </summary>
		public const uint GL_TRIANGLES = 0x0004;

        /// <summary>
        /// Draws a connected group of triangles. One triangle is defined for each vertex presented after the first two vertices. For odd n, vertices n, n + 1, and n + 2 define triangle n. For even n, vertices n + 1, n, and n + 2 define triangle n. N - 2 triangles are drawn.
        /// </summary>
		public const uint GL_TRIANGLE_STRIP = 0x0005;

        /// <summary>
        /// Draws a connected group of triangles. one triangle is defined for each vertex presented after the first two vertices. Vertices 1, n + 1, n + 2 define triangle n. N - 2 triangles are drawn.
        /// </summary>
        public const uint GL_TRIANGLE_FAN = 0x0006;

        /// <summary>
        /// Treats each group of four vertices as an independent quadrilateral. Vertices 4n - 3, 4n - 2, 4n - 1, and 4n define quadrilateral n. N/4 quadrilaterals are drawn.
        /// </summary>
		public const uint GL_QUADS = 0x0007;

        /// <summary>
        /// Draws a connected group of quadrilaterals. One quadrilateral is defined for each pair of vertices presented after the first pair. Vertices 2n - 1, 2n, 2n + 2, and 2n + 1 define quadrilateral n. N/2 - 1 quadrilaterals are drawn. Note that the order in which vertices are used to construct a quadrilateral from strip data is different from that used with independent data.
        /// </summary>
		public const uint GL_QUAD_STRIP = 0x0008;

        /// <summary>
        /// Draws a single, convex polygon. Vertices 1 through N define this polygon.
        /// </summary>
		public const uint GL_POLYGON = 0x0009;

        //  BlendingFactorDest
        public const uint GL_ZERO = 0;
        public const uint GL_ONE = 1;
        public const uint GL_SRC_COLOR = 0x0300;
        public const uint GL_ONE_MINUS_SRC_COLOR = 0x0301;
        public const uint GL_SRC_ALPHA = 0x0302;
        public const uint GL_ONE_MINUS_SRC_ALPHA = 0x0303;
        public const uint GL_DST_ALPHA = 0x0304;
        public const uint GL_ONE_MINUS_DST_ALPHA = 0x0305;

        //  BlendingFactorSrc
        public const uint GL_DST_COLOR = 0x0306;
        public const uint GL_ONE_MINUS_DST_COLOR = 0x0307;
        public const uint GL_SRC_ALPHA_SATURATE = 0x0308;

        //   Boolean
        public const uint GL_TRUE = 1;
        public const uint GL_FALSE = 0;

        //   ClipPlaneName
        public const uint GL_CLIP_PLANE0 = 0x3000;
        public const uint GL_CLIP_PLANE1 = 0x3001;
        public const uint GL_CLIP_PLANE2 = 0x3002;
        public const uint GL_CLIP_PLANE3 = 0x3003;
        public const uint GL_CLIP_PLANE4 = 0x3004;
        public const uint GL_CLIP_PLANE5 = 0x3005;

        //   DataType
        public const uint GL_BYTE = 0x1400;
        public const uint GL_UNSIGNED_BYTE = 0x1401;
        public const uint GL_SHORT = 0x1402;
        public const uint GL_UNSIGNED_SHORT = 0x1403;
        public const uint GL_INT = 0x1404;
        public const uint GL_UNSIGNED_INT = 0x1405;
        public const uint GL_FLOAT = 0x1406;
        public const uint GL_2_BYTES = 0x1407;
        public const uint GL_3_BYTES = 0x1408;
        public const uint GL_4_BYTES = 0x1409;
        public const uint GL_DOUBLE = 0x140A;

        //   DrawBufferMode
        public const uint GL_NONE = 0;
        public const uint GL_FRONT_LEFT = 0x0400;
        public const uint GL_FRONT_RIGHT = 0x0401;
        public const uint GL_BACK_LEFT = 0x0402;
        public const uint GL_BACK_RIGHT = 0x0403;
        public const uint GL_FRONT = 0x0404;
        public const uint GL_BACK = 0x0405;
        public const uint GL_LEFT = 0x0406;
        public const uint GL_RIGHT = 0x0407;
        public const uint GL_FRONT_AND_BACK = 0x0408;
        public const uint GL_AUX0 = 0x0409;
        public const uint GL_AUX1 = 0x040A;
        public const uint GL_AUX2 = 0x040B;
        public const uint GL_AUX3 = 0x040C;

        //   ErrorCode
        public const uint GL_NO_ERROR = 0;
        public const uint GL_INVALID_ENUM = 0x0500;
        public const uint GL_INVALID_VALUE = 0x0501;
        public const uint GL_INVALID_OPERATION = 0x0502;
        public const uint GL_STACK_OVERFLOW = 0x0503;
        public const uint GL_STACK_UNDERFLOW = 0x0504;
        public const uint GL_OUT_OF_MEMORY = 0x0505;

        //   FeedBackMode
        public const uint GL_2D = 0x0600;
        public const uint GL_3D = 0x0601;
        public const uint GL_4D_COLOR = 0x0602;
        public const uint GL_3D_COLOR_TEXTURE = 0x0603;
        public const uint GL_4D_COLOR_TEXTURE = 0x0604;

        //   FeedBackToken
        public const uint GL_PASS_THROUGH_TOKEN = 0x0700;
        public const uint GL_POINT_TOKEN = 0x0701;
        public const uint GL_LINE_TOKEN = 0x0702;
        public const uint GL_POLYGON_TOKEN = 0x0703;
        public const uint GL_BITMAP_TOKEN = 0x0704;
        public const uint GL_DRAW_PIXEL_TOKEN = 0x0705;
        public const uint GL_COPY_PIXEL_TOKEN = 0x0706;
        public const uint GL_LINE_RESET_TOKEN = 0x0707;

        //   FogMode
        public const uint GL_EXP = 0x0800;
        public const uint GL_EXP2 = 0x0801;

        //   FrontFaceDirection
        public const uint GL_CW = 0x0900;
        public const uint GL_CCW = 0x0901;

        //    GetMapTarget 
        public const uint GL_COEFF = 0x0A00;
        public const uint GL_ORDER = 0x0A01;
        public const uint GL_DOMAIN = 0x0A02;

        //   GetTarget
        public const uint GL_CURRENT_COLOR = 0x0B00;
        public const uint GL_CURRENT_INDEX = 0x0B01;
        public const uint GL_CURRENT_NORMAL = 0x0B02;
        public const uint GL_CURRENT_TEXTURE_COORDS = 0x0B03;
        public const uint GL_CURRENT_RASTER_COLOR = 0x0B04;
        public const uint GL_CURRENT_RASTER_INDEX = 0x0B05;
        public const uint GL_CURRENT_RASTER_TEXTURE_COORDS = 0x0B06;
        public const uint GL_CURRENT_RASTER_POSITION = 0x0B07;
        public const uint GL_CURRENT_RASTER_POSITION_VALID = 0x0B08;
        public const uint GL_CURRENT_RASTER_DISTANCE = 0x0B09;
        public const uint GL_POINT_SMOOTH = 0x0B10;
        public const uint GL_POINT_SIZE = 0x0B11;
        public const uint GL_POINT_SIZE_RANGE = 0x0B12;
        public const uint GL_POINT_SIZE_GRANULARITY = 0x0B13;
        public const uint GL_LINE_SMOOTH = 0x0B20;
        public const uint GL_LINE_WIDTH = 0x0B21;
        public const uint GL_LINE_WIDTH_RANGE = 0x0B22;
        public const uint GL_LINE_WIDTH_GRANULARITY = 0x0B23;
        public const uint GL_LINE_STIPPLE = 0x0B24;
        public const uint GL_LINE_STIPPLE_PATTERN = 0x0B25;
        public const uint GL_LINE_STIPPLE_REPEAT = 0x0B26;
        public const uint GL_LIST_MODE = 0x0B30;
        public const uint GL_MAX_LIST_NESTING = 0x0B31;
        public const uint GL_LIST_BASE = 0x0B32;
        public const uint GL_LIST_INDEX = 0x0B33;
        public const uint GL_POLYGON_MODE = 0x0B40;
        public const uint GL_POLYGON_SMOOTH = 0x0B41;
        public const uint GL_POLYGON_STIPPLE = 0x0B42;
        public const uint GL_EDGE_FLAG = 0x0B43;
        public const uint GL_CULL_FACE = 0x0B44;
        public const uint GL_CULL_FACE_MODE = 0x0B45;
        public const uint GL_FRONT_FACE = 0x0B46;
        public const uint GL_LIGHTING = 0x0B50;
        public const uint GL_LIGHT_MODEL_LOCAL_VIEWER = 0x0B51;
        public const uint GL_LIGHT_MODEL_TWO_SIDE = 0x0B52;
        public const uint GL_LIGHT_MODEL_AMBIENT = 0x0B53;
        public const uint GL_SHADE_MODEL = 0x0B54;
        public const uint GL_COLOR_MATERIAL_FACE = 0x0B55;
        public const uint GL_COLOR_MATERIAL_PARAMETER = 0x0B56;
        public const uint GL_COLOR_MATERIAL = 0x0B57;
        public const uint GL_FOG = 0x0B60;
        public const uint GL_FOG_INDEX = 0x0B61;
        public const uint GL_FOG_DENSITY = 0x0B62;
        public const uint GL_FOG_START = 0x0B63;
        public const uint GL_FOG_END = 0x0B64;
        public const uint GL_FOG_MODE = 0x0B65;
        public const uint GL_FOG_COLOR = 0x0B66;
        public const uint GL_DEPTH_RANGE = 0x0B70;
        public const uint GL_DEPTH_TEST = 0x0B71;
        public const uint GL_DEPTH_WRITEMASK = 0x0B72;
        public const uint GL_DEPTH_CLEAR_VALUE = 0x0B73;
        public const uint GL_DEPTH_FUNC = 0x0B74;
        public const uint GL_ACCUM_CLEAR_VALUE = 0x0B80;
        public const uint GL_STENCIL_TEST = 0x0B90;
        public const uint GL_STENCIL_CLEAR_VALUE = 0x0B91;
        public const uint GL_STENCIL_FUNC = 0x0B92;
        public const uint GL_STENCIL_VALUE_MASK = 0x0B93;
        public const uint GL_STENCIL_FAIL = 0x0B94;
        public const uint GL_STENCIL_PASS_DEPTH_FAIL = 0x0B95;
        public const uint GL_STENCIL_PASS_DEPTH_PASS = 0x0B96;
        public const uint GL_STENCIL_REF = 0x0B97;
        public const uint GL_STENCIL_WRITEMASK = 0x0B98;
        public const uint GL_MATRIX_MODE = 0x0BA0;
        public const uint GL_NORMALIZE = 0x0BA1;
        public const uint GL_VIEWPORT = 0x0BA2;
        public const uint GL_MODELVIEW_STACK_DEPTH = 0x0BA3;
        public const uint GL_PROJECTION_STACK_DEPTH = 0x0BA4;
        public const uint GL_TEXTURE_STACK_DEPTH = 0x0BA5;
        public const uint GL_MODELVIEW_MATRIX = 0x0BA6;
        public const uint GL_PROJECTION_MATRIX = 0x0BA7;
        public const uint GL_TEXTURE_MATRIX = 0x0BA8;
        public const uint GL_ATTRIB_STACK_DEPTH = 0x0BB0;
        public const uint GL_CLIENT_ATTRIB_STACK_DEPTH = 0x0BB1;
        public const uint GL_ALPHA_TEST = 0x0BC0;
        public const uint GL_ALPHA_TEST_FUNC = 0x0BC1;
        public const uint GL_ALPHA_TEST_REF = 0x0BC2;
        public const uint GL_DITHER = 0x0BD0;
        public const uint GL_BLEND_DST = 0x0BE0;
        public const uint GL_BLEND_SRC = 0x0BE1;
        public const uint GL_BLEND = 0x0BE2;
        public const uint GL_LOGIC_OP_MODE = 0x0BF0;
        public const uint GL_INDEX_LOGIC_OP = 0x0BF1;
        public const uint GL_COLOR_LOGIC_OP = 0x0BF2;
        public const uint GL_AUX_BUFFERS = 0x0C00;
        public const uint GL_DRAW_BUFFER = 0x0C01;
        public const uint GL_READ_BUFFER = 0x0C02;
        public const uint GL_SCISSOR_BOX = 0x0C10;
        public const uint GL_SCISSOR_TEST = 0x0C11;
        public const uint GL_INDEX_CLEAR_VALUE = 0x0C20;
        public const uint GL_INDEX_WRITEMASK = 0x0C21;
        public const uint GL_COLOR_CLEAR_VALUE = 0x0C22;
        public const uint GL_COLOR_WRITEMASK = 0x0C23;
        public const uint GL_INDEX_MODE = 0x0C30;
        public const uint GL_RGBA_MODE = 0x0C31;
        public const uint GL_DOUBLEBUFFER = 0x0C32;
        public const uint GL_STEREO = 0x0C33;
        public const uint GL_RENDER_MODE = 0x0C40;
        public const uint GL_PERSPECTIVE_CORRECTION_HINT = 0x0C50;
        public const uint GL_POINT_SMOOTH_HINT = 0x0C51;
        public const uint GL_LINE_SMOOTH_HINT = 0x0C52;
        public const uint GL_POLYGON_SMOOTH_HINT = 0x0C53;
        public const uint GL_FOG_HINT = 0x0C54;
        public const uint GL_TEXTURE_GEN_S = 0x0C60;
        public const uint GL_TEXTURE_GEN_T = 0x0C61;
        public const uint GL_TEXTURE_GEN_R = 0x0C62;
        public const uint GL_TEXTURE_GEN_Q = 0x0C63;
        public const uint GL_PIXEL_MAP_I_TO_I = 0x0C70;
        public const uint GL_PIXEL_MAP_S_TO_S = 0x0C71;
        public const uint GL_PIXEL_MAP_I_TO_R = 0x0C72;
        public const uint GL_PIXEL_MAP_I_TO_G = 0x0C73;
        public const uint GL_PIXEL_MAP_I_TO_B = 0x0C74;
        public const uint GL_PIXEL_MAP_I_TO_A = 0x0C75;
        public const uint GL_PIXEL_MAP_R_TO_R = 0x0C76;
        public const uint GL_PIXEL_MAP_G_TO_G = 0x0C77;
        public const uint GL_PIXEL_MAP_B_TO_B = 0x0C78;
        public const uint GL_PIXEL_MAP_A_TO_A = 0x0C79;
        public const uint GL_PIXEL_MAP_I_TO_I_SIZE = 0x0CB0;
        public const uint GL_PIXEL_MAP_S_TO_S_SIZE = 0x0CB1;
        public const uint GL_PIXEL_MAP_I_TO_R_SIZE = 0x0CB2;
        public const uint GL_PIXEL_MAP_I_TO_G_SIZE = 0x0CB3;
        public const uint GL_PIXEL_MAP_I_TO_B_SIZE = 0x0CB4;
        public const uint GL_PIXEL_MAP_I_TO_A_SIZE = 0x0CB5;
        public const uint GL_PIXEL_MAP_R_TO_R_SIZE = 0x0CB6;
        public const uint GL_PIXEL_MAP_G_TO_G_SIZE = 0x0CB7;
        public const uint GL_PIXEL_MAP_B_TO_B_SIZE = 0x0CB8;
        public const uint GL_PIXEL_MAP_A_TO_A_SIZE = 0x0CB9;
        public const uint GL_UNPACK_SWAP_BYTES = 0x0CF0;
        public const uint GL_UNPACK_LSB_FIRST = 0x0CF1;
        public const uint GL_UNPACK_ROW_LENGTH = 0x0CF2;
        public const uint GL_UNPACK_SKIP_ROWS = 0x0CF3;
        public const uint GL_UNPACK_SKIP_PIXELS = 0x0CF4;
        public const uint GL_UNPACK_ALIGNMENT = 0x0CF5;
        public const uint GL_PACK_SWAP_BYTES = 0x0D00;
        public const uint GL_PACK_LSB_FIRST = 0x0D01;
        public const uint GL_PACK_ROW_LENGTH = 0x0D02;
        public const uint GL_PACK_SKIP_ROWS = 0x0D03;
        public const uint GL_PACK_SKIP_PIXELS = 0x0D04;
        public const uint GL_PACK_ALIGNMENT = 0x0D05;
        public const uint GL_MAP_COLOR = 0x0D10;
        public const uint GL_MAP_STENCIL = 0x0D11;
        public const uint GL_INDEX_SHIFT = 0x0D12;
        public const uint GL_INDEX_OFFSET = 0x0D13;
        public const uint GL_RED_SCALE = 0x0D14;
        public const uint GL_RED_BIAS = 0x0D15;
        public const uint GL_ZOOM_X = 0x0D16;
        public const uint GL_ZOOM_Y = 0x0D17;
        public const uint GL_GREEN_SCALE = 0x0D18;
        public const uint GL_GREEN_BIAS = 0x0D19;
        public const uint GL_BLUE_SCALE = 0x0D1A;
        public const uint GL_BLUE_BIAS = 0x0D1B;
        public const uint GL_ALPHA_SCALE = 0x0D1C;
        public const uint GL_ALPHA_BIAS = 0x0D1D;
        public const uint GL_DEPTH_SCALE = 0x0D1E;
        public const uint GL_DEPTH_BIAS = 0x0D1F;
        public const uint GL_MAX_EVAL_ORDER = 0x0D30;
        public const uint GL_MAX_LIGHTS = 0x0D31;
        public const uint GL_MAX_CLIP_PLANES = 0x0D32;
        public const uint GL_MAX_TEXTURE_SIZE = 0x0D33;
        public const uint GL_MAX_PIXEL_MAP_TABLE = 0x0D34;
        public const uint GL_MAX_ATTRIB_STACK_DEPTH = 0x0D35;
        public const uint GL_MAX_MODELVIEW_STACK_DEPTH = 0x0D36;
        public const uint GL_MAX_NAME_STACK_DEPTH = 0x0D37;
        public const uint GL_MAX_PROJECTION_STACK_DEPTH = 0x0D38;
        public const uint GL_MAX_TEXTURE_STACK_DEPTH = 0x0D39;
        public const uint GL_MAX_VIEWPORT_DIMS = 0x0D3A;
        public const uint GL_MAX_CLIENT_ATTRIB_STACK_DEPTH = 0x0D3B;
        public const uint GL_SUBPIXEL_BITS = 0x0D50;
        public const uint GL_INDEX_BITS = 0x0D51;
        public const uint GL_RED_BITS = 0x0D52;
        public const uint GL_GREEN_BITS = 0x0D53;
        public const uint GL_BLUE_BITS = 0x0D54;
        public const uint GL_ALPHA_BITS = 0x0D55;
        public const uint GL_DEPTH_BITS = 0x0D56;
        public const uint GL_STENCIL_BITS = 0x0D57;
        public const uint GL_ACCUM_RED_BITS = 0x0D58;
        public const uint GL_ACCUM_GREEN_BITS = 0x0D59;
        public const uint GL_ACCUM_BLUE_BITS = 0x0D5A;
        public const uint GL_ACCUM_ALPHA_BITS = 0x0D5B;
        public const uint GL_NAME_STACK_DEPTH = 0x0D70;
        public const uint GL_AUTO_NORMAL = 0x0D80;
        public const uint GL_MAP1_COLOR_4 = 0x0D90;
        public const uint GL_MAP1_INDEX = 0x0D91;
        public const uint GL_MAP1_NORMAL = 0x0D92;
        public const uint GL_MAP1_TEXTURE_COORD_1 = 0x0D93;
        public const uint GL_MAP1_TEXTURE_COORD_2 = 0x0D94;
        public const uint GL_MAP1_TEXTURE_COORD_3 = 0x0D95;
        public const uint GL_MAP1_TEXTURE_COORD_4 = 0x0D96;
        public const uint GL_MAP1_VERTEX_3 = 0x0D97;
        public const uint GL_MAP1_VERTEX_4 = 0x0D98;
        public const uint GL_MAP2_COLOR_4 = 0x0DB0;
        public const uint GL_MAP2_INDEX = 0x0DB1;
        public const uint GL_MAP2_NORMAL = 0x0DB2;
        public const uint GL_MAP2_TEXTURE_COORD_1 = 0x0DB3;
        public const uint GL_MAP2_TEXTURE_COORD_2 = 0x0DB4;
        public const uint GL_MAP2_TEXTURE_COORD_3 = 0x0DB5;
        public const uint GL_MAP2_TEXTURE_COORD_4 = 0x0DB6;
        public const uint GL_MAP2_VERTEX_3 = 0x0DB7;
        public const uint GL_MAP2_VERTEX_4 = 0x0DB8;
        public const uint GL_MAP1_GRID_DOMAIN = 0x0DD0;
        public const uint GL_MAP1_GRID_SEGMENTS = 0x0DD1;
        public const uint GL_MAP2_GRID_DOMAIN = 0x0DD2;
        public const uint GL_MAP2_GRID_SEGMENTS = 0x0DD3;
        public const uint GL_TEXTURE_1D = 0x0DE0;
        public const uint GL_TEXTURE_2D = 0x0DE1;
        public const uint GL_FEEDBACK_BUFFER_POINTER = 0x0DF0;
        public const uint GL_FEEDBACK_BUFFER_SIZE = 0x0DF1;
        public const uint GL_FEEDBACK_BUFFER_TYPE = 0x0DF2;
        public const uint GL_SELECTION_BUFFER_POINTER = 0x0DF3;
        public const uint GL_SELECTION_BUFFER_SIZE = 0x0DF4;

        //   GetTextureParameter
        public const uint GL_TEXTURE_WIDTH = 0x1000;
        public const uint GL_TEXTURE_HEIGHT = 0x1001;
        public const uint GL_TEXTURE_INTERNAL_FORMAT = 0x1003;
        public const uint GL_TEXTURE_BORDER_COLOR = 0x1004;
        public const uint GL_TEXTURE_BORDER = 0x1005;

        //   HintMode
        public const uint GL_DONT_CARE = 0x1100;
        public const uint GL_FASTEST = 0x1101;
        public const uint GL_NICEST = 0x1102;

        //   LightName
        public const uint GL_LIGHT0 = 0x4000;
        public const uint GL_LIGHT1 = 0x4001;
        public const uint GL_LIGHT2 = 0x4002;
        public const uint GL_LIGHT3 = 0x4003;
        public const uint GL_LIGHT4 = 0x4004;
        public const uint GL_LIGHT5 = 0x4005;
        public const uint GL_LIGHT6 = 0x4006;
        public const uint GL_LIGHT7 = 0x4007;

        //   LightParameter
        public const uint GL_AMBIENT = 0x1200;
        public const uint GL_DIFFUSE = 0x1201;
        public const uint GL_SPECULAR = 0x1202;
        public const uint GL_POSITION = 0x1203;
        public const uint GL_SPOT_DIRECTION = 0x1204;
        public const uint GL_SPOT_EXPONENT = 0x1205;
        public const uint GL_SPOT_CUTOFF = 0x1206;
        public const uint GL_CONSTANT_ATTENUATION = 0x1207;
        public const uint GL_LINEAR_ATTENUATION = 0x1208;
        public const uint GL_QUADRATIC_ATTENUATION = 0x1209;

        //   ListMode
        public const uint GL_COMPILE = 0x1300;
        public const uint GL_COMPILE_AND_EXECUTE = 0x1301;

        //   LogicOp
        public const uint GL_CLEAR = 0x1500;
        public const uint GL_AND = 0x1501;
        public const uint GL_AND_REVERSE = 0x1502;
        public const uint GL_COPY = 0x1503;
        public const uint GL_AND_INVERTED = 0x1504;
        public const uint GL_NOOP = 0x1505;
        public const uint GL_XOR = 0x1506;
        public const uint GL_OR = 0x1507;
        public const uint GL_NOR = 0x1508;
        public const uint GL_EQUIV = 0x1509;
        public const uint GL_INVERT = 0x150A;
        public const uint GL_OR_REVERSE = 0x150B;
        public const uint GL_COPY_INVERTED = 0x150C;
        public const uint GL_OR_INVERTED = 0x150D;
        public const uint GL_NAND = 0x150E;
        public const uint GL_SET = 0x150F;

        //   MaterialParameter
        public const uint GL_EMISSION = 0x1600;
        public const uint GL_SHININESS = 0x1601;
        public const uint GL_AMBIENT_AND_DIFFUSE = 0x1602;
        public const uint GL_COLOR_INDEXES = 0x1603;

        //   MatrixMode
        public const uint GL_MODELVIEW = 0x1700;
        public const uint GL_PROJECTION = 0x1701;
        public const uint GL_TEXTURE = 0x1702;

        //   PixelCopyType
        public const uint GL_COLOR = 0x1800;
        public const uint GL_DEPTH = 0x1801;
        public const uint GL_STENCIL = 0x1802;

        //   PixelFormat
        public const uint GL_COLOR_INDEX = 0x1900;
        public const uint GL_STENCIL_INDEX = 0x1901;
        public const uint GL_DEPTH_COMPONENT = 0x1902;
        public const uint GL_RED = 0x1903;
        public const uint GL_GREEN = 0x1904;
        public const uint GL_BLUE = 0x1905;
        public const uint GL_ALPHA = 0x1906;
        public const uint GL_RGB = 0x1907;
        public const uint GL_RGBA = 0x1908;
        public const uint GL_LUMINANCE = 0x1909;
        public const uint GL_LUMINANCE_ALPHA = 0x190A;

        //   PixelType
        public const uint GL_BITMAP = 0x1A00;

        //   PolygonMode
        public const uint GL_POINT = 0x1B00;
        public const uint GL_LINE = 0x1B01;
        public const uint GL_FILL = 0x1B02;

        //   RenderingMode 
        public const uint GL_RENDER = 0x1C00;
        public const uint GL_FEEDBACK = 0x1C01;
        public const uint GL_SELECT = 0x1C02;

        //   ShadingModel
        public const uint GL_FLAT = 0x1D00;
        public const uint GL_SMOOTH = 0x1D01;

        //   StencilOp	
        public const uint GL_KEEP = 0x1E00;
        public const uint GL_REPLACE = 0x1E01;
        public const uint GL_INCR = 0x1E02;
        public const uint GL_DECR = 0x1E03;

        //   StringName
        public const uint GL_VENDOR = 0x1F00;
        public const uint GL_RENDERER = 0x1F01;
        public const uint GL_VERSION = 0x1F02;
        public const uint GL_EXTENSIONS = 0x1F03;

        //   TextureCoordName
        public const uint GL_S = 0x2000;
        public const uint GL_T = 0x2001;
        public const uint GL_R = 0x2002;
        public const uint GL_Q = 0x2003;

        //   TextureEnvMode
        public const uint GL_MODULATE = 0x2100;
        public const uint GL_DECAL = 0x2101;

        //   TextureEnvParameter
        public const uint GL_TEXTURE_ENV_MODE = 0x2200;
        public const uint GL_TEXTURE_ENV_COLOR = 0x2201;

        //   TextureEnvTarget
        public const uint GL_TEXTURE_ENV = 0x2300;

        //   TextureGenMode 
        public const uint GL_EYE_LINEAR = 0x2400;
        public const uint GL_OBJECT_LINEAR = 0x2401;
        public const uint GL_SPHERE_MAP = 0x2402;

        //   TextureGenParameter
        public const uint GL_TEXTURE_GEN_MODE = 0x2500;
        public const uint GL_OBJECT_PLANE = 0x2501;
        public const uint GL_EYE_PLANE = 0x2502;

        //   TextureMagFilter
        public const uint GL_NEAREST = 0x2600;
        public const uint GL_LINEAR = 0x2601;

        //   TextureMinFilter 
        public const uint GL_NEAREST_MIPMAP_NEAREST = 0x2700;
        public const uint GL_LINEAR_MIPMAP_NEAREST = 0x2701;
        public const uint GL_NEAREST_MIPMAP_LINEAR = 0x2702;
        public const uint GL_LINEAR_MIPMAP_LINEAR = 0x2703;

        //   TextureParameterName
        public const uint GL_TEXTURE_MAG_FILTER = 0x2800;
        public const uint GL_TEXTURE_MIN_FILTER = 0x2801;
        public const uint GL_TEXTURE_WRAP_S = 0x2802;
        public const uint GL_TEXTURE_WRAP_T = 0x2803;

        //   TextureWrapMode
        public const uint GL_CLAMP = 0x2900;
        public const uint GL_REPEAT = 0x2901;

        //   ClientAttribMask
        public const uint GL_CLIENT_PIXEL_STORE_BIT = 0x00000001;
        public const uint GL_CLIENT_VERTEX_ARRAY_BIT = 0x00000002;
        public const uint GL_CLIENT_ALL_ATTRIB_BITS = 0xffffffff;

        //   Polygon Offset
        public const uint GL_POLYGON_OFFSET_FACTOR = 0x8038;
        public const uint GL_POLYGON_OFFSET_UNITS = 0x2A00;
        public const uint GL_POLYGON_OFFSET_POINT = 0x2A01;
        public const uint GL_POLYGON_OFFSET_LINE = 0x2A02;
        public const uint GL_POLYGON_OFFSET_FILL = 0x8037;

        //   Texture 
        public const uint GL_ALPHA4 = 0x803B;
        public const uint GL_ALPHA8 = 0x803C;
        public const uint GL_ALPHA12 = 0x803D;
        public const uint GL_ALPHA16 = 0x803E;
        public const uint GL_LUMINANCE4 = 0x803F;
        public const uint GL_LUMINANCE8 = 0x8040;
        public const uint GL_LUMINANCE12 = 0x8041;
        public const uint GL_LUMINANCE16 = 0x8042;
        public const uint GL_LUMINANCE4_ALPHA4 = 0x8043;
        public const uint GL_LUMINANCE6_ALPHA2 = 0x8044;
        public const uint GL_LUMINANCE8_ALPHA8 = 0x8045;
        public const uint GL_LUMINANCE12_ALPHA4 = 0x8046;
        public const uint GL_LUMINANCE12_ALPHA12 = 0x8047;
        public const uint GL_LUMINANCE16_ALPHA16 = 0x8048;
        public const uint GL_INTENSITY = 0x8049;
        public const uint GL_INTENSITY4 = 0x804A;
        public const uint GL_INTENSITY8 = 0x804B;
        public const uint GL_INTENSITY12 = 0x804C;
        public const uint GL_INTENSITY16 = 0x804D;
        public const uint GL_R3_G3_B2 = 0x2A10;
        public const uint GL_RGB4 = 0x804F;
        public const uint GL_RGB5 = 0x8050;
        public const uint GL_RGB8 = 0x8051;
        public const uint GL_RGB10 = 0x8052;
        public const uint GL_RGB12 = 0x8053;
        public const uint GL_RGB16 = 0x8054;
        public const uint GL_RGBA2 = 0x8055;
        public const uint GL_RGBA4 = 0x8056;
        public const uint GL_RGB5_A1 = 0x8057;
        public const uint GL_RGBA8 = 0x8058;
        public const uint GL_RGB10_A2 = 0x8059;
        public const uint GL_RGBA12 = 0x805A;
        public const uint GL_RGBA16 = 0x805B;
        public const uint GL_TEXTURE_RED_SIZE = 0x805C;
        public const uint GL_TEXTURE_GREEN_SIZE = 0x805D;
        public const uint GL_TEXTURE_BLUE_SIZE = 0x805E;
        public const uint GL_TEXTURE_ALPHA_SIZE = 0x805F;
        public const uint GL_TEXTURE_LUMINANCE_SIZE = 0x8060;
        public const uint GL_TEXTURE_INTENSITY_SIZE = 0x8061;
        public const uint GL_PROXY_TEXTURE_1D = 0x8063;
        public const uint GL_PROXY_TEXTURE_2D = 0x8064;

        //   Texture object
        public const uint GL_TEXTURE_PRIORITY = 0x8066;
        public const uint GL_TEXTURE_RESIDENT = 0x8067;
        public const uint GL_TEXTURE_BINDING_1D = 0x8068;
        public const uint GL_TEXTURE_BINDING_2D = 0x8069;

        //   Vertex array
        public const uint GL_VERTEX_ARRAY = 0x8074;
        public const uint GL_NORMAL_ARRAY = 0x8075;
        public const uint GL_COLOR_ARRAY = 0x8076;
        public const uint GL_INDEX_ARRAY = 0x8077;
        public const uint GL_TEXTURE_COORD_ARRAY = 0x8078;
        public const uint GL_EDGE_FLAG_ARRAY = 0x8079;
        public const uint GL_VERTEX_ARRAY_SIZE = 0x807A;
        public const uint GL_VERTEX_ARRAY_TYPE = 0x807B;
        public const uint GL_VERTEX_ARRAY_STRIDE = 0x807C;
        public const uint GL_NORMAL_ARRAY_TYPE = 0x807E;
        public const uint GL_NORMAL_ARRAY_STRIDE = 0x807F;
        public const uint GL_COLOR_ARRAY_SIZE = 0x8081;
        public const uint GL_COLOR_ARRAY_TYPE = 0x8082;
        public const uint GL_COLOR_ARRAY_STRIDE = 0x8083;
        public const uint GL_INDEX_ARRAY_TYPE = 0x8085;
        public const uint GL_INDEX_ARRAY_STRIDE = 0x8086;
        public const uint GL_TEXTURE_COORD_ARRAY_SIZE = 0x8088;
        public const uint GL_TEXTURE_COORD_ARRAY_TYPE = 0x8089;
        public const uint GL_TEXTURE_COORD_ARRAY_STRIDE = 0x808A;
        public const uint GL_EDGE_FLAG_ARRAY_STRIDE = 0x808C;
        public const uint GL_VERTEX_ARRAY_POINTER = 0x808E;
        public const uint GL_NORMAL_ARRAY_POINTER = 0x808F;
        public const uint GL_COLOR_ARRAY_POINTER = 0x8090;
        public const uint GL_INDEX_ARRAY_POINTER = 0x8091;
        public const uint GL_TEXTURE_COORD_ARRAY_POINTER = 0x8092;
        public const uint GL_EDGE_FLAG_ARRAY_POINTER = 0x8093;
        public const uint GL_V2F = 0x2A20;
        public const uint GL_V3F = 0x2A21;
        public const uint GL_C4UB_V2F = 0x2A22;
        public const uint GL_C4UB_V3F = 0x2A23;
        public const uint GL_C3F_V3F = 0x2A24;
        public const uint GL_N3F_V3F = 0x2A25;
        public const uint GL_C4F_N3F_V3F = 0x2A26;
        public const uint GL_T2F_V3F = 0x2A27;
        public const uint GL_T4F_V4F = 0x2A28;
        public const uint GL_T2F_C4UB_V3F = 0x2A29;
        public const uint GL_T2F_C3F_V3F = 0x2A2A;
        public const uint GL_T2F_N3F_V3F = 0x2A2B;
        public const uint GL_T2F_C4F_N3F_V3F = 0x2A2C;
        public const uint GL_T4F_C4F_N3F_V4F = 0x2A2D;

        //   Extensions
        public const uint GL_EXT_vertex_array = 1;
        public const uint GL_EXT_bgra = 1;
        public const uint GL_EXT_paletted_texture = 1;
        public const uint GL_WIN_swap_hint = 1;
        public const uint GL_WIN_draw_range_elements = 1;

        //   EXT_vertex_array 
        public const uint GL_VERTEX_ARRAY_EXT = 0x8074;
        public const uint GL_NORMAL_ARRAY_EXT = 0x8075;
        public const uint GL_COLOR_ARRAY_EXT = 0x8076;
        public const uint GL_INDEX_ARRAY_EXT = 0x8077;
        public const uint GL_TEXTURE_COORD_ARRAY_EXT = 0x8078;
        public const uint GL_EDGE_FLAG_ARRAY_EXT = 0x8079;
        public const uint GL_VERTEX_ARRAY_SIZE_EXT = 0x807A;
        public const uint GL_VERTEX_ARRAY_TYPE_EXT = 0x807B;
        public const uint GL_VERTEX_ARRAY_STRIDE_EXT = 0x807C;
        public const uint GL_VERTEX_ARRAY_COUNT_EXT = 0x807D;
        public const uint GL_NORMAL_ARRAY_TYPE_EXT = 0x807E;
        public const uint GL_NORMAL_ARRAY_STRIDE_EXT = 0x807F;
        public const uint GL_NORMAL_ARRAY_COUNT_EXT = 0x8080;
        public const uint GL_COLOR_ARRAY_SIZE_EXT = 0x8081;
        public const uint GL_COLOR_ARRAY_TYPE_EXT = 0x8082;
        public const uint GL_COLOR_ARRAY_STRIDE_EXT = 0x8083;
        public const uint GL_COLOR_ARRAY_COUNT_EXT = 0x8084;
        public const uint GL_INDEX_ARRAY_TYPE_EXT = 0x8085;
        public const uint GL_INDEX_ARRAY_STRIDE_EXT = 0x8086;
        public const uint GL_INDEX_ARRAY_COUNT_EXT = 0x8087;
        public const uint GL_TEXTURE_COORD_ARRAY_SIZE_EXT = 0x8088;
        public const uint GL_TEXTURE_COORD_ARRAY_TYPE_EXT = 0x8089;
        public const uint GL_TEXTURE_COORD_ARRAY_STRIDE_EXT = 0x808A;
        public const uint GL_TEXTURE_COORD_ARRAY_COUNT_EXT = 0x808B;
        public const uint GL_EDGE_FLAG_ARRAY_STRIDE_EXT = 0x808C;
        public const uint GL_EDGE_FLAG_ARRAY_COUNT_EXT = 0x808D;
        public const uint GL_VERTEX_ARRAY_POINTER_EXT = 0x808E;
        public const uint GL_NORMAL_ARRAY_POINTER_EXT = 0x808F;
        public const uint GL_COLOR_ARRAY_POINTER_EXT = 0x8090;
        public const uint GL_INDEX_ARRAY_POINTER_EXT = 0x8091;
        public const uint GL_TEXTURE_COORD_ARRAY_POINTER_EXT = 0x8092;
        public const uint GL_EDGE_FLAG_ARRAY_POINTER_EXT = 0x8093;
        public const uint GL_DOUBLE_EXT = 1;/*DOUBLE*/

        //   EXT_paletted_texture
        public const uint GL_COLOR_TABLE_FORMAT_EXT = 0x80D8;
        public const uint GL_COLOR_TABLE_WIDTH_EXT = 0x80D9;
        public const uint GL_COLOR_TABLE_RED_SIZE_EXT = 0x80DA;
        public const uint GL_COLOR_TABLE_GREEN_SIZE_EXT = 0x80DB;
        public const uint GL_COLOR_TABLE_BLUE_SIZE_EXT = 0x80DC;
        public const uint GL_COLOR_TABLE_ALPHA_SIZE_EXT = 0x80DD;
        public const uint GL_COLOR_TABLE_LUMINANCE_SIZE_EXT = 0x80DE;
        public const uint GL_COLOR_TABLE_INTENSITY_SIZE_EXT = 0x80DF;
        public const uint GL_COLOR_INDEX1_EXT = 0x80E2;
        public const uint GL_COLOR_INDEX2_EXT = 0x80E3;
        public const uint GL_COLOR_INDEX4_EXT = 0x80E4;
        public const uint GL_COLOR_INDEX8_EXT = 0x80E5;
        public const uint GL_COLOR_INDEX12_EXT = 0x80E6;
        public const uint GL_COLOR_INDEX16_EXT = 0x80E7;

        //   WIN_draw_range_elements
        public const uint GL_MAX_ELEMENTS_VERTICES_WIN = 0x80E8;
        public const uint GL_MAX_ELEMENTS_INDICES_WIN = 0x80E9;

        //   WIN_phong_shading
        public const uint GL_PHONG_WIN = 0x80EA;
        public const uint GL_PHONG_HINT_WIN = 0x80EB;


        //   WIN_specular_fog 
        public const uint FOG_SPECULAR_TEXTURE_WIN = 0x80EC;

        public const uint GL_DEBUG_OUTPUT_SYNCHRONOUS = 0x8242;



        //  Constants
        public const uint GL_BUFFER_SIZE = 0x8764;
        public const uint GL_BUFFER_USAGE = 0x8765;
        public const uint GL_QUERY_COUNTER_BITS = 0x8864;
        public const uint GL_CURRENT_QUERY = 0x8865;
        public const uint GL_QUERY_RESULT = 0x8866;
        public const uint GL_QUERY_RESULT_AVAILABLE = 0x8867;
        public const uint GL_ARRAY_BUFFER = 0x8892;
        public const uint GL_ELEMENT_ARRAY_BUFFER = 0x8893;
        public const uint GL_ARRAY_BUFFER_BINDING = 0x8894;
        public const uint GL_ELEMENT_ARRAY_BUFFER_BINDING = 0x8895;
        public const uint GL_VERTEX_ATTRIB_ARRAY_BUFFER_BINDING = 0x889F;
        public const uint GL_READ_ONLY = 0x88B8;
        public const uint GL_WRITE_ONLY = 0x88B9;
        public const uint GL_READ_WRITE = 0x88BA;
        public const uint GL_BUFFER_ACCESS = 0x88BB;
        public const uint GL_BUFFER_MAPPED = 0x88BC;
        public const uint GL_BUFFER_MAP_POINTER = 0x88BD;
        public const uint GL_STREAM_DRAW = 0x88E0;
        public const uint GL_STREAM_READ = 0x88E1;
        public const uint GL_STREAM_COPY = 0x88E2;
        public const uint GL_STATIC_DRAW = 0x88E4;
        public const uint GL_STATIC_READ = 0x88E5;
        public const uint GL_STATIC_COPY = 0x88E6;
        public const uint GL_DYNAMIC_DRAW = 0x88E8;
        public const uint GL_DYNAMIC_READ = 0x88E9;
        public const uint GL_DYNAMIC_COPY = 0x88EA;
        public const uint GL_SAMPLES_PASSED = 0x8914;

        public const uint GL_UNSIGNED_BYTE_3_3_2 = 0x8032;
        public const uint GL_UNSIGNED_SHORT_4_4_4_4 = 0x8033;
        public const uint GL_UNSIGNED_SHORT_5_5_5_1 = 0x8034;
        public const uint GL_UNSIGNED_INT_8_8_8_8 = 0x8035;
        public const uint GL_UNSIGNED_INT_10_10_10_2 = 0x8036;
        public const uint GL_TEXTURE_BINDING_3D = 0x806A;
        public const uint GL_PACK_SKIP_IMAGES = 0x806B;
        public const uint GL_PACK_IMAGE_HEIGHT = 0x806C;
        public const uint GL_UNPACK_SKIP_IMAGES = 0x806D;
        public const uint GL_UNPACK_IMAGE_HEIGHT = 0x806E;
        public const uint GL_TEXTURE_3D = 0x806F;
        public const uint GL_PROXY_TEXTURE_3D = 0x8070;
        public const uint GL_TEXTURE_DEPTH = 0x8071;
        public const uint GL_TEXTURE_WRAP_R = 0x8072;
        public const uint GL_MAX_3D_TEXTURE_SIZE = 0x8073;
        public const uint GL_UNSIGNED_BYTE_2_3_3_REV = 0x8362;
        public const uint GL_UNSIGNED_SHORT_5_6_5 = 0x8363;
        public const uint GL_UNSIGNED_SHORT_5_6_5_REV = 0x8364;
        public const uint GL_UNSIGNED_SHORT_4_4_4_4_REV = 0x8365;
        public const uint GL_UNSIGNED_SHORT_1_5_5_5_REV = 0x8366;
        public const uint GL_UNSIGNED_INT_8_8_8_8_REV = 0x8367;
        public const uint GL_UNSIGNED_INT_2_10_10_10_REV = 0x8368;
        public const uint GL_BGR = 0x80E0;
        public const uint GL_BGRA = 0x80E1;
        public const uint GL_MAX_ELEMENTS_VERTICES = 0x80E8;
        public const uint GL_MAX_ELEMENTS_INDICES = 0x80E9;
        public const uint GL_CLAMP_TO_EDGE = 0x812F;
        public const uint GL_TEXTURE_MIN_LOD = 0x813A;
        public const uint GL_TEXTURE_MAX_LOD = 0x813B;
        public const uint GL_TEXTURE_BASE_LEVEL = 0x813C;
        public const uint GL_TEXTURE_MAX_LEVEL = 0x813D;
        public const uint GL_SMOOTH_POINT_SIZE_RANGE = 0x0B12;
        public const uint GL_SMOOTH_POINT_SIZE_GRANULARITY = 0x0B13;
        public const uint GL_SMOOTH_LINE_WIDTH_RANGE = 0x0B22;
        public const uint GL_SMOOTH_LINE_WIDTH_GRANULARITY = 0x0B23;
        public const uint GL_ALIASED_LINE_WIDTH_RANGE = 0x846E;
        public const uint GL_TEXTURE0 = 0x84C0;
        public const uint GL_TEXTURE1 = 0x84C1;
        public const uint GL_TEXTURE2 = 0x84C2;
        public const uint GL_TEXTURE3 = 0x84C3;
        public const uint GL_TEXTURE4 = 0x84C4;
        public const uint GL_TEXTURE5 = 0x84C5;
        public const uint GL_TEXTURE6 = 0x84C6;
        public const uint GL_TEXTURE7 = 0x84C7;
        public const uint GL_TEXTURE8 = 0x84C8;
        public const uint GL_TEXTURE9 = 0x84C9;
        public const uint GL_TEXTURE10 = 0x84CA;
        public const uint GL_TEXTURE11 = 0x84CB;
        public const uint GL_TEXTURE12 = 0x84CC;
        public const uint GL_TEXTURE13 = 0x84CD;
        public const uint GL_TEXTURE14 = 0x84CE;
        public const uint GL_TEXTURE15 = 0x84CF;
        public const uint GL_TEXTURE16 = 0x84D0;
        public const uint GL_TEXTURE17 = 0x84D1;
        public const uint GL_TEXTURE18 = 0x84D2;
        public const uint GL_TEXTURE19 = 0x84D3;
        public const uint GL_TEXTURE20 = 0x84D4;
        public const uint GL_TEXTURE21 = 0x84D5;
        public const uint GL_TEXTURE22 = 0x84D6;
        public const uint GL_TEXTURE23 = 0x84D7;
        public const uint GL_TEXTURE24 = 0x84D8;
        public const uint GL_TEXTURE25 = 0x84D9;
        public const uint GL_TEXTURE26 = 0x84DA;
        public const uint GL_TEXTURE27 = 0x84DB;
        public const uint GL_TEXTURE28 = 0x84DC;
        public const uint GL_TEXTURE29 = 0x84DD;
        public const uint GL_TEXTURE30 = 0x84DE;
        public const uint GL_TEXTURE31 = 0x84DF;
        public const uint GL_ACTIVE_TEXTURE = 0x84E0;
        public const uint GL_MULTISAMPLE = 0x809D;
        public const uint GL_SAMPLE_ALPHA_TO_COVERAGE = 0x809E;
        public const uint GL_SAMPLE_ALPHA_TO_ONE = 0x809F;
        public const uint GL_SAMPLE_COVERAGE = 0x80A0;
        public const uint GL_SAMPLE_BUFFERS = 0x80A8;
        public const uint GL_SAMPLES = 0x80A9;
        public const uint GL_SAMPLE_COVERAGE_VALUE = 0x80AA;
        public const uint GL_SAMPLE_COVERAGE_INVERT = 0x80AB;
        public const uint GL_TEXTURE_CUBE_MAP = 0x8513;
        public const uint GL_TEXTURE_BINDING_CUBE_MAP = 0x8514;
        public const uint GL_TEXTURE_CUBE_MAP_POSITIVE_X = 0x8515;
        public const uint GL_TEXTURE_CUBE_MAP_NEGATIVE_X = 0x8516;
        public const uint GL_TEXTURE_CUBE_MAP_POSITIVE_Y = 0x8517;
        public const uint GL_TEXTURE_CUBE_MAP_NEGATIVE_Y = 0x8518;
        public const uint GL_TEXTURE_CUBE_MAP_POSITIVE_Z = 0x8519;
        public const uint GL_TEXTURE_CUBE_MAP_NEGATIVE_Z = 0x851A;
        public const uint GL_PROXY_TEXTURE_CUBE_MAP = 0x851B;
        public const uint GL_MAX_CUBE_MAP_TEXTURE_SIZE = 0x851C;
        public const uint GL_COMPRESSED_RGB = 0x84ED;
        public const uint GL_COMPRESSED_RGBA = 0x84EE;
        public const uint GL_TEXTURE_COMPRESSION_HINT = 0x84EF;
        public const uint GL_TEXTURE_COMPRESSED_IMAGE_SIZE = 0x86A0;
        public const uint GL_TEXTURE_COMPRESSED = 0x86A1;
        public const uint GL_NUM_COMPRESSED_TEXTURE_FORMATS = 0x86A2;
        public const uint GL_COMPRESSED_TEXTURE_FORMATS = 0x86A3;
        public const uint GL_CLAMP_TO_BORDER = 0x812D;
        public const uint GL_BLEND_DST_RGB = 0x80C8;
        public const uint GL_BLEND_SRC_RGB = 0x80C9;
        public const uint GL_BLEND_DST_ALPHA = 0x80CA;
        public const uint GL_BLEND_SRC_ALPHA = 0x80CB;
        public const uint GL_POINT_FADE_THRESHOLD_SIZE = 0x8128;
        public const uint GL_DEPTH_COMPONENT16 = 0x81A5;
        public const uint GL_DEPTH_COMPONENT24 = 0x81A6;
        public const uint GL_DEPTH_COMPONENT32 = 0x81A7;
        public const uint GL_MIRRORED_REPEAT = 0x8370;
        public const uint GL_MAX_TEXTURE_LOD_BIAS = 0x84FD;
        public const uint GL_TEXTURE_LOD_BIAS = 0x8501;
        public const uint GL_INCR_WRAP = 0x8507;
        public const uint GL_DECR_WRAP = 0x8508;
        public const uint GL_TEXTURE_DEPTH_SIZE = 0x884A;
        public const uint GL_TEXTURE_COMPARE_MODE = 0x884C;
        public const uint GL_TEXTURE_COMPARE_FUNC = 0x884D;
        public const uint GL_BLEND_COLOR = 0x8005;
        public const uint GL_BLEND_EQUATION = 0x8009;
        public const uint GL_CONSTANT_COLOR = 0x8001;
        public const uint GL_ONE_MINUS_CONSTANT_COLOR = 0x8002;
        public const uint GL_CONSTANT_ALPHA = 0x8003;
        public const uint GL_ONE_MINUS_CONSTANT_ALPHA = 0x8004;
        public const uint GL_FUNC_ADD = 0x8006;
        public const uint GL_FUNC_REVERSE_SUBTRACT = 0x800B;
        public const uint GL_FUNC_SUBTRACT = 0x800A;
        public const uint GL_MIN = 0x8007;
        public const uint GL_MAX = 0x8008;
        public const uint GL_SRC1_ALPHA = 0x8589;
        public const uint GL_BLEND_EQUATION_RGB = 0x8009;
        public const uint GL_VERTEX_ATTRIB_ARRAY_ENABLED = 0x8622;
        public const uint GL_VERTEX_ATTRIB_ARRAY_SIZE = 0x8623;
        public const uint GL_VERTEX_ATTRIB_ARRAY_STRIDE = 0x8624;
        public const uint GL_VERTEX_ATTRIB_ARRAY_TYPE = 0x8625;
        public const uint GL_CURRENT_VERTEX_ATTRIB = 0x8626;
        public const uint GL_VERTEX_PROGRAM_POINT_SIZE = 0x8642;
        public const uint GL_VERTEX_ATTRIB_ARRAY_POINTER = 0x8645;
        public const uint GL_STENCIL_BACK_FUNC = 0x8800;
        public const uint GL_STENCIL_BACK_FAIL = 0x8801;
        public const uint GL_STENCIL_BACK_PASS_DEPTH_FAIL = 0x8802;
        public const uint GL_STENCIL_BACK_PASS_DEPTH_PASS = 0x8803;
        public const uint GL_MAX_DRAW_BUFFERS = 0x8824;
        public const uint GL_DRAW_BUFFER0 = 0x8825;
        public const uint GL_DRAW_BUFFER1 = 0x8826;
        public const uint GL_DRAW_BUFFER2 = 0x8827;
        public const uint GL_DRAW_BUFFER3 = 0x8828;
        public const uint GL_DRAW_BUFFER4 = 0x8829;
        public const uint GL_DRAW_BUFFER5 = 0x882A;
        public const uint GL_DRAW_BUFFER6 = 0x882B;
        public const uint GL_DRAW_BUFFER7 = 0x882C;
        public const uint GL_DRAW_BUFFER8 = 0x882D;
        public const uint GL_DRAW_BUFFER9 = 0x882E;
        public const uint GL_DRAW_BUFFER10 = 0x882F;
        public const uint GL_DRAW_BUFFER11 = 0x8830;
        public const uint GL_DRAW_BUFFER12 = 0x8831;
        public const uint GL_DRAW_BUFFER13 = 0x8832;
        public const uint GL_DRAW_BUFFER14 = 0x8833;
        public const uint GL_DRAW_BUFFER15 = 0x8834;
        public const uint GL_BLEND_EQUATION_ALPHA = 0x883D;
        public const uint GL_MAX_VERTEX_ATTRIBS = 0x8869;
        public const uint GL_VERTEX_ATTRIB_ARRAY_NORMALIZED = 0x886A;
        public const uint GL_MAX_TEXTURE_IMAGE_UNITS = 0x8872;
        public const uint GL_FRAGMENT_SHADER = 0x8B30;
        public const uint GL_VERTEX_SHADER = 0x8B31;
        public const uint GL_MAX_FRAGMENT_UNIFORM_COMPONENTS = 0x8B49;
        public const uint GL_MAX_VERTEX_UNIFORM_COMPONENTS = 0x8B4A;
        public const uint GL_MAX_VARYING_FLOATS = 0x8B4B;
        public const uint GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS = 0x8B4C;
        public const uint GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS = 0x8B4D;
        public const uint GL_SHADER_TYPE = 0x8B4F;
        public const uint GL_FLOAT_VEC2 = 0x8B50;
        public const uint GL_FLOAT_VEC3 = 0x8B51;
        public const uint GL_FLOAT_VEC4 = 0x8B52;
        public const uint GL_INT_VEC2 = 0x8B53;
        public const uint GL_INT_VEC3 = 0x8B54;
        public const uint GL_INT_VEC4 = 0x8B55;
        public const uint GL_BOOL = 0x8B56;
        public const uint GL_BOOL_VEC2 = 0x8B57;
        public const uint GL_BOOL_VEC3 = 0x8B58;
        public const uint GL_BOOL_VEC4 = 0x8B59;
        public const uint GL_FLOAT_MAT2 = 0x8B5A;
        public const uint GL_FLOAT_MAT3 = 0x8B5B;
        public const uint GL_FLOAT_MAT4 = 0x8B5C;
        public const uint GL_SAMPLER_1D = 0x8B5D;
        public const uint GL_SAMPLER_2D = 0x8B5E;
        public const uint GL_SAMPLER_3D = 0x8B5F;
        public const uint GL_SAMPLER_CUBE = 0x8B60;
        public const uint GL_SAMPLER_1D_SHADOW = 0x8B61;
        public const uint GL_SAMPLER_2D_SHADOW = 0x8B62;
        public const uint GL_DELETE_STATUS = 0x8B80;
        public const uint GL_COMPILE_STATUS = 0x8B81;
        public const uint GL_LINK_STATUS = 0x8B82;
        public const uint GL_VALIDATE_STATUS = 0x8B83;
        public const uint GL_INFO_LOG_LENGTH = 0x8B84;
        public const uint GL_ATTACHED_SHADERS = 0x8B85;
        public const uint GL_ACTIVE_UNIFORMS = 0x8B86;
        public const uint GL_ACTIVE_UNIFORM_MAX_LENGTH = 0x8B87;
        public const uint GL_SHADER_SOURCE_LENGTH = 0x8B88;
        public const uint GL_ACTIVE_ATTRIBUTES = 0x8B89;
        public const uint GL_ACTIVE_ATTRIBUTE_MAX_LENGTH = 0x8B8A;
        public const uint GL_FRAGMENT_SHADER_DERIVATIVE_HINT = 0x8B8B;
        public const uint GL_SHADING_LANGUAGE_VERSION = 0x8B8C;
        public const uint GL_CURRENT_PROGRAM = 0x8B8D;
        public const uint GL_POINT_SPRITE_COORD_ORIGIN = 0x8CA0;
        public const uint GL_LOWER_LEFT = 0x8CA1;
        public const uint GL_UPPER_LEFT = 0x8CA2;
        public const uint GL_STENCIL_BACK_REF = 0x8CA3;
        public const uint GL_STENCIL_BACK_VALUE_MASK = 0x8CA4;
        public const uint GL_STENCIL_BACK_WRITEMASK = 0x8CA5;
        public const uint GL_PIXEL_PACK_BUFFER = 0x88EB;
        public const uint GL_PIXEL_UNPACK_BUFFER = 0x88EC;
        public const uint GL_PIXEL_PACK_BUFFER_BINDING = 0x88ED;
        public const uint GL_PIXEL_UNPACK_BUFFER_BINDING = 0x88EF;
        public const uint GL_FLOAT_MAT2x3 = 0x8B65;
        public const uint GL_FLOAT_MAT2x4 = 0x8B66;
        public const uint GL_FLOAT_MAT3x2 = 0x8B67;
        public const uint GL_FLOAT_MAT3x4 = 0x8B68;
        public const uint GL_FLOAT_MAT4x2 = 0x8B69;
        public const uint GL_FLOAT_MAT4x3 = 0x8B6A;
        public const uint GL_SRGB = 0x8C40;
        public const uint GL_SRGB8 = 0x8C41;
        public const uint GL_SRGB_ALPHA = 0x8C42;
        public const uint GL_SRGB8_ALPHA8 = 0x8C43;
        public const uint GL_COMPRESSED_SRGB = 0x8C48;
        public const uint GL_COMPRESSED_SRGB_ALPHA = 0x8C49;
        public const uint GL_COMPARE_REF_TO_TEXTURE = 0x884E;
        public const uint GL_CLIP_DISTANCE0 = 0x3000;
        public const uint GL_CLIP_DISTANCE1 = 0x3001;
        public const uint GL_CLIP_DISTANCE2 = 0x3002;
        public const uint GL_CLIP_DISTANCE3 = 0x3003;
        public const uint GL_CLIP_DISTANCE4 = 0x3004;
        public const uint GL_CLIP_DISTANCE5 = 0x3005;
        public const uint GL_CLIP_DISTANCE6 = 0x3006;
        public const uint GL_CLIP_DISTANCE7 = 0x3007;
        public const uint GL_MAX_CLIP_DISTANCES = 0x0D32;
        public const uint GL_MAJOR_VERSION = 0x821B;
        public const uint GL_MINOR_VERSION = 0x821C;
        public const uint GL_NUM_EXTENSIONS = 0x821D;
        public const uint GL_CONTEXT_FLAGS = 0x821E;
        public const uint GL_COMPRESSED_RED = 0x8225;
        public const uint GL_COMPRESSED_RG = 0x8226;
        public const uint GL_CONTEXT_FLAG_FORWARD_COMPATIBLE_BIT = 0x00000001;
        public const uint GL_RGBA32F = 0x8814;
        public const uint GL_RGB32F = 0x8815;
        public const uint GL_RGBA16F = 0x881A;
        public const uint GL_RGB16F = 0x881B;
        public const uint GL_VERTEX_ATTRIB_ARRAY_INTEGER = 0x88FD;
        public const uint GL_MAX_ARRAY_TEXTURE_LAYERS = 0x88FF;
        public const uint GL_MIN_PROGRAM_TEXEL_OFFSET = 0x8904;
        public const uint GL_MAX_PROGRAM_TEXEL_OFFSET = 0x8905;
        public const uint GL_CLAMP_READ_COLOR = 0x891C;
        public const uint GL_FIXED_ONLY = 0x891D;
        public const uint GL_MAX_VARYING_COMPONENTS = 0x8B4B;
        public const uint GL_TEXTURE_1D_ARRAY = 0x8C18;
        public const uint GL_PROXY_TEXTURE_1D_ARRAY = 0x8C19;
        public const uint GL_TEXTURE_2D_ARRAY = 0x8C1A;
        public const uint GL_PROXY_TEXTURE_2D_ARRAY = 0x8C1B;
        public const uint GL_TEXTURE_BINDING_1D_ARRAY = 0x8C1C;
        public const uint GL_TEXTURE_BINDING_2D_ARRAY = 0x8C1D;
        public const uint GL_R11F_G11F_B10F = 0x8C3A;
        public const uint GL_UNSIGNED_INT_10F_11F_11F_REV = 0x8C3B;
        public const uint GL_RGB9_E5 = 0x8C3D;
        public const uint GL_UNSIGNED_INT_5_9_9_9_REV = 0x8C3E;
        public const uint GL_TEXTURE_SHARED_SIZE = 0x8C3F;
        public const uint GL_TRANSFORM_FEEDBACK_VARYING_MAX_LENGTH = 0x8C76;
        public const uint GL_TRANSFORM_FEEDBACK_BUFFER_MODE = 0x8C7F;
        public const uint GL_MAX_TRANSFORM_FEEDBACK_SEPARATE_COMPONENTS = 0x8C80;
        public const uint GL_TRANSFORM_FEEDBACK_VARYINGS = 0x8C83;
        public const uint GL_TRANSFORM_FEEDBACK_BUFFER_START = 0x8C84;
        public const uint GL_TRANSFORM_FEEDBACK_BUFFER_SIZE = 0x8C85;
        public const uint GL_PRIMITIVES_GENERATED = 0x8C87;
        public const uint GL_TRANSFORM_FEEDBACK_PRIMITIVES_WRITTEN = 0x8C88;
        public const uint GL_RASTERIZER_DISCARD = 0x8C89;
        public const uint GL_MAX_TRANSFORM_FEEDBACK_INTERLEAVED_COMPONENTS = 0x8C8A;
        public const uint GL_MAX_TRANSFORM_FEEDBACK_SEPARATE_ATTRIBS = 0x8C8B;
        public const uint GL_INTERLEAVED_ATTRIBS = 0x8C8C;
        public const uint GL_SEPARATE_ATTRIBS = 0x8C8D;
        public const uint GL_TRANSFORM_FEEDBACK_BUFFER = 0x8C8E;
        public const uint GL_TRANSFORM_FEEDBACK_BUFFER_BINDING = 0x8C8F;
        public const uint GL_RGBA32UI = 0x8D70;
        public const uint GL_RGB32UI = 0x8D71;
        public const uint GL_RGBA16UI = 0x8D76;
        public const uint GL_RGB16UI = 0x8D77;
        public const uint GL_RGBA8UI = 0x8D7C;
        public const uint GL_RGB8UI = 0x8D7D;
        public const uint GL_RGBA32I = 0x8D82;
        public const uint GL_RGB32I = 0x8D83;
        public const uint GL_RGBA16I = 0x8D88;
        public const uint GL_RGB16I = 0x8D89;
        public const uint GL_RGBA8I = 0x8D8E;
        public const uint GL_RGB8I = 0x8D8F;
        public const uint GL_RED_INTEGER = 0x8D94;
        public const uint GL_GREEN_INTEGER = 0x8D95;
        public const uint GL_BLUE_INTEGER = 0x8D96;
        public const uint GL_RGB_INTEGER = 0x8D98;
        public const uint GL_RGBA_INTEGER = 0x8D99;
        public const uint GL_BGR_INTEGER = 0x8D9A;
        public const uint GL_BGRA_INTEGER = 0x8D9B;
        public const uint GL_SAMPLER_1D_ARRAY = 0x8DC0;
        public const uint GL_SAMPLER_2D_ARRAY = 0x8DC1;
        public const uint GL_SAMPLER_1D_ARRAY_SHADOW = 0x8DC3;
        public const uint GL_SAMPLER_2D_ARRAY_SHADOW = 0x8DC4;
        public const uint GL_SAMPLER_CUBE_SHADOW = 0x8DC5;
        public const uint GL_UNSIGNED_INT_VEC2 = 0x8DC6;
        public const uint GL_UNSIGNED_INT_VEC3 = 0x8DC7;
        public const uint GL_UNSIGNED_INT_VEC4 = 0x8DC8;
        public const uint GL_INT_SAMPLER_1D = 0x8DC9;
        public const uint GL_INT_SAMPLER_2D = 0x8DCA;
        public const uint GL_INT_SAMPLER_3D = 0x8DCB;
        public const uint GL_INT_SAMPLER_CUBE = 0x8DCC;
        public const uint GL_INT_SAMPLER_1D_ARRAY = 0x8DCE;
        public const uint GL_INT_SAMPLER_2D_ARRAY = 0x8DCF;
        public const uint GL_UNSIGNED_INT_SAMPLER_1D = 0x8DD1;
        public const uint GL_UNSIGNED_INT_SAMPLER_2D = 0x8DD2;
        public const uint GL_UNSIGNED_INT_SAMPLER_3D = 0x8DD3;
        public const uint GL_UNSIGNED_INT_SAMPLER_CUBE = 0x8DD4;
        public const uint GL_UNSIGNED_INT_SAMPLER_1D_ARRAY = 0x8DD6;
        public const uint GL_UNSIGNED_INT_SAMPLER_2D_ARRAY = 0x8DD7;
        public const uint GL_QUERY_WAIT = 0x8E13;
        public const uint GL_QUERY_NO_WAIT = 0x8E14;
        public const uint GL_QUERY_BY_REGION_WAIT = 0x8E15;
        public const uint GL_QUERY_BY_REGION_NO_WAIT = 0x8E16;
        public const uint GL_BUFFER_ACCESS_FLAGS = 0x911F;
        public const uint GL_BUFFER_MAP_LENGTH = 0x9120;
        public const uint GL_BUFFER_MAP_OFFSET = 0x9121;
        public const uint GL_DEPTH_COMPONENT32F = 0x8CAC;
        public const uint GL_DEPTH32F_STENCIL8 = 0x8CAD;
        public const uint GL_FLOAT_32_UNSIGNED_INT_24_8_REV = 0x8DAD;
        public const uint GL_INVALID_FRAMEBUFFER_OPERATION = 0x0506;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_COLOR_ENCODING = 0x8210;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_COMPONENT_TYPE = 0x8211;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_RED_SIZE = 0x8212;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_GREEN_SIZE = 0x8213;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_BLUE_SIZE = 0x8214;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_ALPHA_SIZE = 0x8215;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_DEPTH_SIZE = 0x8216;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_STENCIL_SIZE = 0x8217;
        public const uint GL_FRAMEBUFFER_DEFAULT = 0x8218;
        public const uint GL_FRAMEBUFFER_UNDEFINED = 0x8219;
        public const uint GL_DEPTH_STENCIL_ATTACHMENT = 0x821A;
        public const uint GL_MAX_RENDERBUFFER_SIZE = 0x84E8;
        public const uint GL_DEPTH_STENCIL = 0x84F9;
        public const uint GL_UNSIGNED_INT_24_8 = 0x84FA;
        public const uint GL_DEPTH24_STENCIL8 = 0x88F0;
        public const uint GL_TEXTURE_STENCIL_SIZE = 0x88F1;
        public const uint GL_TEXTURE_RED_TYPE = 0x8C10;
        public const uint GL_TEXTURE_GREEN_TYPE = 0x8C11;
        public const uint GL_TEXTURE_BLUE_TYPE = 0x8C12;
        public const uint GL_TEXTURE_ALPHA_TYPE = 0x8C13;
        public const uint GL_TEXTURE_DEPTH_TYPE = 0x8C16;
        public const uint GL_UNSIGNED_NORMALIZED = 0x8C17;
        public const uint GL_FRAMEBUFFER_BINDING = 0x8CA6;
        public const uint GL_DRAW_FRAMEBUFFER_BINDING = 0x8CA6;
        public const uint GL_RENDERBUFFER_BINDING = 0x8CA7;
        public const uint GL_READ_FRAMEBUFFER = 0x8CA8;
        public const uint GL_DRAW_FRAMEBUFFER = 0x8CA9;
        public const uint GL_READ_FRAMEBUFFER_BINDING = 0x8CAA;
        public const uint GL_RENDERBUFFER_SAMPLES = 0x8CAB;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_OBJECT_TYPE = 0x8CD0;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_OBJECT_NAME = 0x8CD1;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_LEVEL = 0x8CD2;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_CUBE_MAP_FACE = 0x8CD3;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_LAYER = 0x8CD4;
        public const uint GL_FRAMEBUFFER_COMPLETE = 0x8CD5;
        public const uint GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT = 0x8CD6;
        public const uint GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT = 0x8CD7;
        public const uint GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER = 0x8CDB;
        public const uint GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER = 0x8CDC;
        public const uint GL_FRAMEBUFFER_UNSUPPORTED = 0x8CDD;
        public const uint GL_MAX_COLOR_ATTACHMENTS = 0x8CDF;
        public const uint GL_COLOR_ATTACHMENT0 = 0x8CE0;
        public const uint GL_COLOR_ATTACHMENT1 = 0x8CE1;
        public const uint GL_COLOR_ATTACHMENT2 = 0x8CE2;
        public const uint GL_COLOR_ATTACHMENT3 = 0x8CE3;
        public const uint GL_COLOR_ATTACHMENT4 = 0x8CE4;
        public const uint GL_COLOR_ATTACHMENT5 = 0x8CE5;
        public const uint GL_COLOR_ATTACHMENT6 = 0x8CE6;
        public const uint GL_COLOR_ATTACHMENT7 = 0x8CE7;
        public const uint GL_COLOR_ATTACHMENT8 = 0x8CE8;
        public const uint GL_COLOR_ATTACHMENT9 = 0x8CE9;
        public const uint GL_COLOR_ATTACHMENT10 = 0x8CEA;
        public const uint GL_COLOR_ATTACHMENT11 = 0x8CEB;
        public const uint GL_COLOR_ATTACHMENT12 = 0x8CEC;
        public const uint GL_COLOR_ATTACHMENT13 = 0x8CED;
        public const uint GL_COLOR_ATTACHMENT14 = 0x8CEE;
        public const uint GL_COLOR_ATTACHMENT15 = 0x8CEF;
        public const uint GL_COLOR_ATTACHMENT16 = 0x8CF0;
        public const uint GL_COLOR_ATTACHMENT17 = 0x8CF1;
        public const uint GL_COLOR_ATTACHMENT18 = 0x8CF2;
        public const uint GL_COLOR_ATTACHMENT19 = 0x8CF3;
        public const uint GL_COLOR_ATTACHMENT20 = 0x8CF4;
        public const uint GL_COLOR_ATTACHMENT21 = 0x8CF5;
        public const uint GL_COLOR_ATTACHMENT22 = 0x8CF6;
        public const uint GL_COLOR_ATTACHMENT23 = 0x8CF7;
        public const uint GL_COLOR_ATTACHMENT24 = 0x8CF8;
        public const uint GL_COLOR_ATTACHMENT25 = 0x8CF9;
        public const uint GL_COLOR_ATTACHMENT26 = 0x8CFA;
        public const uint GL_COLOR_ATTACHMENT27 = 0x8CFB;
        public const uint GL_COLOR_ATTACHMENT28 = 0x8CFC;
        public const uint GL_COLOR_ATTACHMENT29 = 0x8CFD;
        public const uint GL_COLOR_ATTACHMENT30 = 0x8CFE;
        public const uint GL_COLOR_ATTACHMENT31 = 0x8CFF;
        public const uint GL_DEPTH_ATTACHMENT = 0x8D00;
        public const uint GL_STENCIL_ATTACHMENT = 0x8D20;
        public const uint GL_FRAMEBUFFER = 0x8D40;
        public const uint GL_RENDERBUFFER = 0x8D41;
        public const uint GL_RENDERBUFFER_WIDTH = 0x8D42;
        public const uint GL_RENDERBUFFER_HEIGHT = 0x8D43;
        public const uint GL_RENDERBUFFER_INTERNAL_FORMAT = 0x8D44;
        public const uint GL_STENCIL_INDEX1 = 0x8D46;
        public const uint GL_STENCIL_INDEX4 = 0x8D47;
        public const uint GL_STENCIL_INDEX8 = 0x8D48;
        public const uint GL_STENCIL_INDEX16 = 0x8D49;
        public const uint GL_RENDERBUFFER_RED_SIZE = 0x8D50;
        public const uint GL_RENDERBUFFER_GREEN_SIZE = 0x8D51;
        public const uint GL_RENDERBUFFER_BLUE_SIZE = 0x8D52;
        public const uint GL_RENDERBUFFER_ALPHA_SIZE = 0x8D53;
        public const uint GL_RENDERBUFFER_DEPTH_SIZE = 0x8D54;
        public const uint GL_RENDERBUFFER_STENCIL_SIZE = 0x8D55;
        public const uint GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE = 0x8D56;
        public const uint GL_MAX_SAMPLES = 0x8D57;
        public const uint GL_FRAMEBUFFER_SRGB = 0x8DB9;
        public const uint GL_HALF_FLOAT = 0x140B;
        public const uint GL_MAP_READ_BIT = 0x0001;
        public const uint GL_MAP_WRITE_BIT = 0x0002;
        public const uint GL_MAP_INVALIDATE_RANGE_BIT = 0x0004;
        public const uint GL_MAP_INVALIDATE_BUFFER_BIT = 0x0008;
        public const uint GL_MAP_FLUSH_EXPLICIT_BIT = 0x0010;
        public const uint GL_MAP_UNSYNCHRONIZED_BIT = 0x0020;
        public const uint GL_COMPRESSED_RED_RGTC1 = 0x8DBB;
        public const uint GL_COMPRESSED_SIGNED_RED_RGTC1 = 0x8DBC;
        public const uint GL_COMPRESSED_RG_RGTC2 = 0x8DBD;
        public const uint GL_COMPRESSED_SIGNED_RG_RGTC2 = 0x8DBE;
        public const uint GL_RG = 0x8227;
        public const uint GL_RG_INTEGER = 0x8228;
        public const uint GL_R8 = 0x8229;
        public const uint GL_R16 = 0x822A;
        public const uint GL_RG8 = 0x822B;
        public const uint GL_RG16 = 0x822C;
        public const uint GL_R16F = 0x822D;
        public const uint GL_R32F = 0x822E;
        public const uint GL_RG16F = 0x822F;
        public const uint GL_RG32F = 0x8230;
        public const uint GL_R8I = 0x8231;
        public const uint GL_R8UI = 0x8232;
        public const uint GL_R16I = 0x8233;
        public const uint GL_R16UI = 0x8234;
        public const uint GL_R32I = 0x8235;
        public const uint GL_R32UI = 0x8236;
        public const uint GL_RG8I = 0x8237;
        public const uint GL_RG8UI = 0x8238;
        public const uint GL_RG16I = 0x8239;
        public const uint GL_RG16UI = 0x823A;
        public const uint GL_RG32I = 0x823B;
        public const uint GL_RG32UI = 0x823C;
        public const uint GL_VERTEX_ARRAY_BINDING = 0x85B5;
        public const uint GL_SAMPLER_2D_RECT = 0x8B63;
        public const uint GL_SAMPLER_2D_RECT_SHADOW = 0x8B64;
        public const uint GL_SAMPLER_BUFFER = 0x8DC2;
        public const uint GL_INT_SAMPLER_2D_RECT = 0x8DCD;
        public const uint GL_INT_SAMPLER_BUFFER = 0x8DD0;
        public const uint GL_UNSIGNED_INT_SAMPLER_2D_RECT = 0x8DD5;
        public const uint GL_UNSIGNED_INT_SAMPLER_BUFFER = 0x8DD8;
        public const uint GL_TEXTURE_BUFFER = 0x8C2A;
        public const uint GL_MAX_TEXTURE_BUFFER_SIZE = 0x8C2B;
        public const uint GL_TEXTURE_BINDING_BUFFER = 0x8C2C;
        public const uint GL_TEXTURE_BUFFER_DATA_STORE_BINDING = 0x8C2D;
        public const uint GL_TEXTURE_RECTANGLE = 0x84F5;
        public const uint GL_TEXTURE_BINDING_RECTANGLE = 0x84F6;
        public const uint GL_PROXY_TEXTURE_RECTANGLE = 0x84F7;
        public const uint GL_MAX_RECTANGLE_TEXTURE_SIZE = 0x84F8;
        public const uint GL_R8_SNORM = 0x8F94;
        public const uint GL_RG8_SNORM = 0x8F95;
        public const uint GL_RGB8_SNORM = 0x8F96;
        public const uint GL_RGBA8_SNORM = 0x8F97;
        public const uint GL_R16_SNORM = 0x8F98;
        public const uint GL_RG16_SNORM = 0x8F99;
        public const uint GL_RGB16_SNORM = 0x8F9A;
        public const uint GL_RGBA16_SNORM = 0x8F9B;
        public const uint GL_SIGNED_NORMALIZED = 0x8F9C;
        public const uint GL_PRIMITIVE_RESTART = 0x8F9D;
        public const uint GL_PRIMITIVE_RESTART_INDEX = 0x8F9E;
        public const uint GL_COPY_READ_BUFFER = 0x8F36;
        public const uint GL_COPY_WRITE_BUFFER = 0x8F37;
        public const uint GL_UNIFORM_BUFFER = 0x8A11;
        public const uint GL_UNIFORM_BUFFER_BINDING = 0x8A28;
        public const uint GL_UNIFORM_BUFFER_START = 0x8A29;
        public const uint GL_UNIFORM_BUFFER_SIZE = 0x8A2A;
        public const uint GL_MAX_VERTEX_UNIFORM_BLOCKS = 0x8A2B;
        public const uint GL_MAX_GEOMETRY_UNIFORM_BLOCKS = 0x8A2C;
        public const uint GL_MAX_FRAGMENT_UNIFORM_BLOCKS = 0x8A2D;
        public const uint GL_MAX_COMBINED_UNIFORM_BLOCKS = 0x8A2E;
        public const uint GL_MAX_UNIFORM_BUFFER_BINDINGS = 0x8A2F;
        public const uint GL_MAX_UNIFORM_BLOCK_SIZE = 0x8A30;
        public const uint GL_MAX_COMBINED_VERTEX_UNIFORM_COMPONENTS = 0x8A31;
        public const uint GL_MAX_COMBINED_GEOMETRY_UNIFORM_COMPONENTS = 0x8A32;
        public const uint GL_MAX_COMBINED_FRAGMENT_UNIFORM_COMPONENTS = 0x8A33;
        public const uint GL_UNIFORM_BUFFER_OFFSET_ALIGNMENT = 0x8A34;
        public const uint GL_ACTIVE_UNIFORM_BLOCK_MAX_NAME_LENGTH = 0x8A35;
        public const uint GL_ACTIVE_UNIFORM_BLOCKS = 0x8A36;
        public const uint GL_UNIFORM_TYPE = 0x8A37;
        public const uint GL_UNIFORM_SIZE = 0x8A38;
        public const uint GL_UNIFORM_NAME_LENGTH = 0x8A39;
        public const uint GL_UNIFORM_BLOCK_INDEX = 0x8A3A;
        public const uint GL_UNIFORM_OFFSET = 0x8A3B;
        public const uint GL_UNIFORM_ARRAY_STRIDE = 0x8A3C;
        public const uint GL_UNIFORM_MATRIX_STRIDE = 0x8A3D;
        public const uint GL_UNIFORM_IS_ROW_MAJOR = 0x8A3E;
        public const uint GL_UNIFORM_BLOCK_BINDING = 0x8A3F;
        public const uint GL_UNIFORM_BLOCK_DATA_SIZE = 0x8A40;
        public const uint GL_UNIFORM_BLOCK_NAME_LENGTH = 0x8A41;
        public const uint GL_UNIFORM_BLOCK_ACTIVE_UNIFORMS = 0x8A42;
        public const uint GL_UNIFORM_BLOCK_ACTIVE_UNIFORM_INDICES = 0x8A43;
        public const uint GL_UNIFORM_BLOCK_REFERENCED_BY_VERTEX_SHADER = 0x8A44;
        public const uint GL_UNIFORM_BLOCK_REFERENCED_BY_GEOMETRY_SHADER = 0x8A45;
        public const uint GL_UNIFORM_BLOCK_REFERENCED_BY_FRAGMENT_SHADER = 0x8A46;
        public const uint GL_INVALID_INDEX = 0xFFFFFFFF;
        public const uint GL_CONTEXT_CORE_PROFILE_BIT = 0x00000001;
        public const uint GL_CONTEXT_COMPATIBILITY_PROFILE_BIT = 0x00000002;
        public const uint GL_LINES_ADJACENCY = 0x000A;
        public const uint GL_LINE_STRIP_ADJACENCY = 0x000B;
        public const uint GL_TRIANGLES_ADJACENCY = 0x000C;
        public const uint GL_TRIANGLE_STRIP_ADJACENCY = 0x000D;
        public const uint GL_PROGRAM_POINT_SIZE = 0x8642;
        public const uint GL_MAX_GEOMETRY_TEXTURE_IMAGE_UNITS = 0x8C29;
        public const uint GL_FRAMEBUFFER_ATTACHMENT_LAYERED = 0x8DA7;
        public const uint GL_FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS = 0x8DA8;
        public const uint GL_GEOMETRY_SHADER = 0x8DD9;
        public const uint GL_GEOMETRY_VERTICES_OUT = 0x8916;
        public const uint GL_GEOMETRY_INPUT_TYPE = 0x8917;
        public const uint GL_GEOMETRY_OUTPUT_TYPE = 0x8918;
        public const uint GL_MAX_GEOMETRY_UNIFORM_COMPONENTS = 0x8DDF;
        public const uint GL_MAX_GEOMETRY_OUTPUT_VERTICES = 0x8DE0;
        public const uint GL_MAX_GEOMETRY_TOTAL_OUTPUT_COMPONENTS = 0x8DE1;
        public const uint GL_MAX_VERTEX_OUTPUT_COMPONENTS = 0x9122;
        public const uint GL_MAX_GEOMETRY_INPUT_COMPONENTS = 0x9123;
        public const uint GL_MAX_GEOMETRY_OUTPUT_COMPONENTS = 0x9124;
        public const uint GL_MAX_FRAGMENT_INPUT_COMPONENTS = 0x9125;
        public const uint GL_CONTEXT_PROFILE_MASK = 0x9126;
        public const uint GL_DEPTH_CLAMP = 0x864F;
        public const uint GL_QUADS_FOLLOW_PROVOKING_VERTEX_CONVENTION = 0x8E4C;
        public const uint GL_FIRST_VERTEX_CONVENTION = 0x8E4D;
        public const uint GL_LAST_VERTEX_CONVENTION = 0x8E4E;
        public const uint GL_PROVOKING_VERTEX = 0x8E4F;
        public const uint GL_TEXTURE_CUBE_MAP_SEAMLESS = 0x884F;
        public const uint GL_MAX_SERVER_WAIT_TIMEOUT = 0x9111;
        public const uint GL_OBJECT_TYPE = 0x9112;
        public const uint GL_SYNC_CONDITION = 0x9113;
        public const uint GL_SYNC_STATUS = 0x9114;
        public const uint GL_SYNC_FLAGS = 0x9115;
        public const uint GL_SYNC_FENCE = 0x9116;
        public const uint GL_SYNC_GPU_COMMANDS_COMPLETE = 0x9117;
        public const uint GL_UNSIGNALED = 0x9118;
        public const uint GL_SIGNALED = 0x9119;
        public const uint GL_ALREADY_SIGNALED = 0x911A;
        public const uint GL_TIMEOUT_EXPIRED = 0x911B;
        public const uint GL_CONDITION_SATISFIED = 0x911C;
        public const uint GL_WAIT_FAILED = 0x911D;
        public const ulong GL_TIMEOUT_IGNORED = 0xFFFFFFFFFFFFFFFF;
        public const uint GL_SYNC_FLUSH_COMMANDS_BIT = 0x00000001;
        public const uint GL_SAMPLE_POSITION = 0x8E50;
        public const uint GL_SAMPLE_MASK = 0x8E51;
        public const uint GL_SAMPLE_MASK_VALUE = 0x8E52;
        public const uint GL_MAX_SAMPLE_MASK_WORDS = 0x8E59;
        public const uint GL_TEXTURE_2D_MULTISAMPLE = 0x9100;
        public const uint GL_PROXY_TEXTURE_2D_MULTISAMPLE = 0x9101;
        public const uint GL_TEXTURE_2D_MULTISAMPLE_ARRAY = 0x9102;
        public const uint GL_PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY = 0x9103;
        public const uint GL_TEXTURE_BINDING_2D_MULTISAMPLE = 0x9104;
        public const uint GL_TEXTURE_BINDING_2D_MULTISAMPLE_ARRAY = 0x9105;
        public const uint GL_TEXTURE_SAMPLES = 0x9106;
        public const uint GL_TEXTURE_FIXED_SAMPLE_LOCATIONS = 0x9107;
        public const uint GL_SAMPLER_2D_MULTISAMPLE = 0x9108;
        public const uint GL_INT_SAMPLER_2D_MULTISAMPLE = 0x9109;
        public const uint GL_UNSIGNED_INT_SAMPLER_2D_MULTISAMPLE = 0x910A;
        public const uint GL_SAMPLER_2D_MULTISAMPLE_ARRAY = 0x910B;
        public const uint GL_INT_SAMPLER_2D_MULTISAMPLE_ARRAY = 0x910C;
        public const uint GL_UNSIGNED_INT_SAMPLER_2D_MULTISAMPLE_ARRAY = 0x910D;
        public const uint GL_MAX_COLOR_TEXTURE_SAMPLES = 0x910E;
        public const uint GL_MAX_DEPTH_TEXTURE_SAMPLES = 0x910F;
        public const uint GL_MAX_INTEGER_SAMPLES = 0x9110;
        public const uint GL_VERTEX_ATTRIB_ARRAY_DIVISOR = 0x88FE;
        public const uint GL_SRC1_COLOR = 0x88F9;
        public const uint GL_ONE_MINUS_SRC1_COLOR = 0x88FA;
        public const uint GL_ONE_MINUS_SRC1_ALPHA = 0x88FB;
        public const uint GL_MAX_DUAL_SOURCE_DRAW_BUFFERS = 0x88FC;
        public const uint GL_ANY_SAMPLES_PASSED = 0x8C2F;
        public const uint GL_SAMPLER_BINDING = 0x8919;
        public const uint GL_RGB10_A2UI = 0x906F;
        public const uint GL_TEXTURE_SWIZZLE_R = 0x8E42;
        public const uint GL_TEXTURE_SWIZZLE_G = 0x8E43;
        public const uint GL_TEXTURE_SWIZZLE_B = 0x8E44;
        public const uint GL_TEXTURE_SWIZZLE_A = 0x8E45;
        public const uint GL_TEXTURE_SWIZZLE_RGBA = 0x8E46;
        public const uint GL_TIME_ELAPSED = 0x88BF;
        public const uint GL_TIMESTAMP = 0x8E28;
        public const uint GL_INT_2_10_10_10_REV = 0x8D9F;

        /// <summary>
        ///     The unsafe NULL pointer.
        ///     <para>Analog of IntPtr.Zero.</para>
        /// </summary>
        public static readonly void* NULL = (void*)0;

        #endregion



        #region The OpenGL DLL Functions (Exactly the same naming).

        public const string LIBRARY_OPENGL = "opengl32.dll";

        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glAccum(uint op, float value);

        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glAlphaFunc(uint func, float ref_notkeword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern byte glAreTexturesResident(int n, uint[] textures, byte[] residences);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glArrayElement(int i);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glBegin(uint mode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glBindTexture(uint target, uint texture);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glBitmap(int width, int height, float xorig, float yorig, float xmove, float ymove, byte[] bitmap);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glBlendFunc(uint sfactor, uint dfactor);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glCallList(uint list);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glCallLists(int n, uint type, IntPtr lists);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glCallLists(int n, uint type, uint[] lists);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glCallLists(int n, uint type, byte[] lists);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glClear(uint mask);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glClearAccum(float red, float green, float blue, float alpha);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glClearColor(float red, float green, float blue, float alpha);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glClearDepth(double depth);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glClearIndex(float c);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glClearStencil(int s);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glClipPlane(uint plane, double[] equation);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3b(byte red, byte green, byte blue);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3bv(byte[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3d(double red, double green, double blue);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3f(float red, float green, float blue);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3i(int red, int green, int blue);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3s(short red, short green, short blue);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3ub(byte red, byte green, byte blue);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3ubv(byte[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3ui(uint red, uint green, uint blue);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3uiv(uint[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3us(ushort red, ushort green, ushort blue);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor3usv(ushort[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4b(byte red, byte green, byte blue, byte alpha);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4bv(byte[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4d(double red, double green, double blue, double alpha);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4f(float red, float green, float blue, float alpha);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4i(int red, int green, int blue, int alpha);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4s(short red, short green, short blue, short alpha);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4ub(byte red, byte green, byte blue, byte alpha);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4ubv(byte[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4ui(uint red, uint green, uint blue, uint alpha);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4uiv(uint[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4us(ushort red, ushort green, ushort blue, ushort alpha);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColor4usv(ushort[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColorMask(byte red, byte green, byte blue, byte alpha);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColorMaterial(uint face, uint mode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColorPointer(int size, uint type, int stride, IntPtr pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColorPointer(int size, uint type, int stride, byte[] pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glColorPointer(int size, uint type, int stride, float[] pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glCopyPixels(int x, int y, int width, int height, uint type);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glCopyTexImage1D(uint target, int level, uint internalFormat, int x, int y, int width, int border);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glCopyTexImage2D(uint target, int level, uint internalFormat, int x, int y, int width, int height, int border);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glCopyTexSubImage1D(uint target, int level, int xoffset, int x, int y, int width);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glCopyTexSubImage2D(uint target, int level, int xoffset, int yoffset, int x, int y, int width, int height);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glCullFace(uint mode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDeleteLists(uint list, int range);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDeleteTextures(int n, uint[] textures);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDepthFunc(uint func);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDepthMask(byte flag);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDepthRange(double zNear, double zFar);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDisable(uint cap);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDisableClientState(uint array);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDrawArrays(uint mode, int first, int count);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDrawBuffer(uint mode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDrawElements(uint mode, int count, uint type, IntPtr indices);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDrawElements(uint mode, int count, uint type, uint[] indices);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDrawPixels(int width, int height, uint format, uint type, float[] pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDrawPixels(int width, int height, uint format, uint type, uint[] pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDrawPixels(int width, int height, uint format, uint type, ushort[] pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDrawPixels(int width, int height, uint format, uint type, byte[] pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glDrawPixels(int width, int height, uint format, uint type, IntPtr pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEdgeFlag(byte flag);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEdgeFlagPointer(int stride, int[] pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEdgeFlagv(byte[] flag);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEnable(uint cap);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEnableClientState(uint array);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEnd();
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEndList();
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEvalCoord1d(double u);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEvalCoord1dv(double[] u);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEvalCoord1f(float u);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEvalCoord1fv(float[] u);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEvalCoord2d(double u, double v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEvalCoord2dv(double[] u);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEvalCoord2f(float u, float v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEvalCoord2fv(float[] u);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEvalMesh1(uint mode, int i1, int i2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEvalMesh2(uint mode, int i1, int i2, int j1, int j2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEvalPoint1(int i);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glEvalPoint2(int i, int j);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glFeedbackBuffer(int size, uint type, float[] buffer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glFinish();
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glFlush();
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glFogf(uint pname, float param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glFogfv(uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glFogi(uint pname, int param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glFogiv(uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glFrontFace(uint mode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glFrustum(double left, double right, double bottom, double top, double zNear, double zFar);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern uint glGenLists(int range);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGenTextures(int n, uint* textures);

        /// <summary>
        /// Generate a buffer and return the index
        /// </summary>
        public static uint glGenTextures()
        {
            uint index = 0;
            glGenTextures(1, &index);
            CheckError();
            return index;
        }

        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetBooleanv(uint pname, byte[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetClipPlane(uint plane, double[] equation);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetDoublev(uint pname, double[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern uint glGetError();
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetFloatv(uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetIntegerv(uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetLightfv(uint light, uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetLightiv(uint light, uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetMapdv(uint target, uint query, double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetMapfv(uint target, uint query, float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetMapiv(uint target, uint query, int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetMaterialfv(uint face, uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetMaterialiv(uint face, uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetPixelMapfv(uint map, float[] values);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetPixelMapuiv(uint map, uint[] values);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetPixelMapusv(uint map, ushort[] values);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetPointerv(uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetPolygonStipple(byte[] mask);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public unsafe static extern sbyte* glGetString(uint name);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetTexEnvfv(uint target, uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetTexEnviv(uint target, uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetTexGendv(uint coord, uint pname, double[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetTexGenfv(uint coord, uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetTexGeniv(uint coord, uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetTexImage(uint target, int level, uint format, uint type, int[] pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetTexLevelParameterfv(uint target, int level, uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetTexLevelParameteriv(uint target, int level, uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetTexParameterfv(uint target, uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glGetTexParameteriv(uint target, uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glHint(uint target, uint mode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glIndexMask(uint mask);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glIndexPointer(uint type, int stride, int[] pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glIndexd(double c);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glIndexdv(double[] c);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glIndexf(float c);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glIndexfv(float[] c);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glIndexi(int c);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glIndexiv(int[] c);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glIndexs(short c);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glIndexsv(short[] c);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glIndexub(byte c);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glIndexubv(byte[] c);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glInitNames();
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glInterleavedArrays(uint format, int stride, int[] pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern byte glIsEnabled(uint cap);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern byte glIsList(uint list);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern byte glIsTexture(uint texture);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLightModelf(uint pname, float param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLightModelfv(uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLightModeli(uint pname, int param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLightModeliv(uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLightf(uint light, uint pname, float param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLightfv(uint light, uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLighti(uint light, uint pname, int param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLightiv(uint light, uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLineStipple(int factor, ushort pattern);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLineWidth(float width);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glListBase(uint base_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLoadIdentity();
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLoadMatrixd(double[] m);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLoadMatrixf(float[] m);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLoadName(uint name);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glLogicOp(uint opcode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMap1d(uint target, double u1, double u2, int stride, int order, double[] points);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMap1f(uint target, float u1, float u2, int stride, int order, float[] points);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMap2d(uint target, double u1, double u2, int ustride, int uorder, double v1, double v2, int vstride, int vorder, double[] points);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMap2f(uint target, float u1, float u2, int ustride, int uorder, float v1, float v2, int vstride, int vorder, float[] points);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMapGrid1d(int un, double u1, double u2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMapGrid1f(int un, float u1, float u2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMapGrid2d(int un, double u1, double u2, int vn, double v1, double v2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMapGrid2f(int un, float u1, float u2, int vn, float v1, float v2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMaterialf(uint face, uint pname, float param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMaterialfv(uint face, uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMateriali(uint face, uint pname, int param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMaterialiv(uint face, uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMatrixMode(uint mode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMultMatrixd(double[] m);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glMultMatrixf(float[] m);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNewList(uint list, uint mode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNormal3b(byte nx, byte ny, byte nz);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNormal3bv(byte[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNormal3d(double nx, double ny, double nz);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNormal3dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNormal3f(float nx, float ny, float nz);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNormal3fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNormal3i(int nx, int ny, int nz);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNormal3iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNormal3s(short nx, short ny, short nz);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNormal3sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNormalPointer(uint type, int stride, IntPtr pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glNormalPointer(uint type, int stride, float[] pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glOrtho(double left, double right, double bottom, double top, double zNear, double zFar);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPassThrough(float token);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPixelMapfv(uint map, int mapsize, float[] values);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPixelMapuiv(uint map, int mapsize, uint[] values);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPixelMapusv(uint map, int mapsize, ushort[] values);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPixelStoref(uint pname, float param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPixelStorei(uint pname, int param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPixelTransferf(uint pname, float param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPixelTransferi(uint pname, int param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPixelZoom(float xfactor, float yfactor);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPointSize(float size);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPolygonMode(uint face, uint mode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPolygonOffset(float factor, float units);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPolygonStipple(byte[] mask);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPopAttrib();
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPopClientAttrib();
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPopMatrix();
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPopName();
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPrioritizeTextures(int n, uint[] textures, float[] priorities);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPushAttrib(uint mask);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPushClientAttrib(uint mask);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPushMatrix();
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glPushName(uint name);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos2d(double x, double y);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos2dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos2f(float x, float y);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos2fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos2i(int x, int y);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos2iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos2s(short x, short y);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos2sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos3d(double x, double y, double z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos3dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos3f(float x, float y, float z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos3fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos3i(int x, int y, int z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos3iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos3s(short x, short y, short z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos3sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos4d(double x, double y, double z, double w);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos4dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos4f(float x, float y, float z, float w);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos4fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos4i(int x, int y, int z, int w);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos4iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos4s(short x, short y, short z, short w);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRasterPos4sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glReadBuffer(uint mode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, byte[] pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, IntPtr pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRectd(double x1, double y1, double x2, double y2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRectdv(double[] v1, double[] v2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRectf(float x1, float y1, float x2, float y2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRectfv(float[] v1, float[] v2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRecti(int x1, int y1, int x2, int y2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRectiv(int[] v1, int[] v2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRects(short x1, short y1, short x2, short y2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRectsv(short[] v1, short[] v2);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern int glRenderMode(uint mode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRotated(double angle, double x, double y, double z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glRotatef(float angle, float x, float y, float z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glScaled(double x, double y, double z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glScalef(float x, float y, float z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glScissor(int x, int y, int width, int height);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glSelectBuffer(int size, uint[] buffer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glShadeModel(uint mode);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glStencilFunc(uint func, int ref_notkeword, uint mask);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glStencilMask(uint mask);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glStencilOp(uint fail, uint zfail, uint zpass);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord1d(double s);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord1dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord1f(float s);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord1fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord1i(int s);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord1iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord1s(short s);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord1sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord2d(double s, double t);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord2dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord2f(float s, float t);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord2fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord2i(int s, int t);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord2iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord2s(short s, short t);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord2sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord3d(double s, double t, double r);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord3dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord3f(float s, float t, float r);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord3fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord3i(int s, int t, int r);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord3iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord3s(short s, short t, short r);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord3sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord4d(double s, double t, double r, double q);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord4dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord4f(float s, float t, float r, float q);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord4fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord4i(int s, int t, int r, int q);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord4iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord4s(short s, short t, short r, short q);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoord4sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoordPointer(int size, uint type, int stride, IntPtr pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexCoordPointer(int size, uint type, int stride, float[] pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexEnvf(uint target, uint pname, float param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexEnvfv(uint target, uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexEnvi(uint target, uint pname, int param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexEnviv(uint target, uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexGend(uint coord, uint pname, double param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexGendv(uint coord, uint pname, double[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexGenf(uint coord, uint pname, float param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexGenfv(uint coord, uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexGeni(uint coord, uint pname, int param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexGeniv(uint coord, uint pname, int[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage1D(uint target, int level, uint internalformat, int width, int border, uint format, uint type, byte[] pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage1D(uint target, int level, uint internalformat, int width, int border, uint format, uint type, sbyte[] pixels); //format=GL_BYTE
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage1D(uint target, int level, uint internalformat, int width, int border, uint format, uint type, ushort[] pixels); //format=GL_UNSIGNED_SHORT
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage1D(uint target, int level, uint internalformat, int width, int border, uint format, uint type, short[] pixels); //format=GL_SHORT
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage1D(uint target, int level, uint internalformat, int width, int border, uint format, uint type, uint[] pixels); //format=GL_UNSIGNED_INT
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage1D(uint target, int level, uint internalformat, int width, int border, uint format, uint type, int[] pixels); //format=GL_INT
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage1D(uint target, int level, uint internalformat, int width, int border, uint format, uint type, float[] pixels); //format=GL_FLOAT
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage1D(uint target, int level, uint internalformat, int width, int border, uint format, uint type, IntPtr pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, byte[] pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, sbyte[] pixels); //format=GL_BYTE
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, ushort[] pixels); //format=GL_UNSIGNED_SHORT
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, short[] pixels); //format=GL_SHORT
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, uint[] pixels); //format=GL_UNSIGNED_INT
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, int[] pixels); //format=GL_INT
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, float[] pixels); //format=GL_FLOAT
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, IntPtr pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexParameterf(uint target, uint pname, float param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexParameterfv(uint target, uint pname, float[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexParameteri(uint target, uint pname, uint param);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexParameteriv(uint target, uint pname, uint[] params_notkeyword);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexSubImage1D(uint target, int level, int xoffset, int width, uint format, uint type, int[] pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTexSubImage2D(uint target, int level, int xoffset, int yoffset, int width, int height, uint format, uint type, int[] pixels);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTranslated(double x, double y, double z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glTranslatef(float x, float y, float z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex2d(double x, double y);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex2dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex2f(float x, float y);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex2fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex2i(int x, int y);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex2iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex2s(short x, short y);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex2sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex3d(double x, double y, double z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex3dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex3f(float x, float y, float z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex3fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex3i(int x, int y, int z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex3iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex3s(short x, short y, short z);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex3sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex4d(double x, double y, double z, double w);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex4dv(double[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex4f(float x, float y, float z, float w);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex4fv(float[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex4i(int x, int y, int z, int w);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex4iv(int[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex4s(short x, short y, short z, short w);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertex4sv(short[] v);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertexPointer(int size, uint type, int stride, IntPtr pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertexPointer(int size, uint type, int stride, short[] pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertexPointer(int size, uint type, int stride, int[] pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertexPointer(int size, uint type, int stride, float[] pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glVertexPointer(int size, uint type, int stride, double[] pointer);
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern void glViewport(int x, int y, int width, int height);


        /// <summary>
        /// Gets a proc address.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <returns>The address of the function.</returns>
        [DllImport(LIBRARY_OPENGL, SetLastError = true)] public static extern IntPtr wglGetProcAddress(string name);


        public delegate void DebugProc(uint source, uint type, int id, uint severity, int length, char* message, nint userParam);


        #endregion


        #region Extensions methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glGenBuffers(int n, uint* buffers) => GLExtensions.glGenBuffers(n, buffers);

        /// <summary>
        /// Generate buffers for each elements in buffers
        /// </summary>
        public static void glGenBuffers(uint[] buffers)
        {
            if (buffers.Length == 0)
                throw new ArgumentException("Buffers array cannot be empty.");

            fixed (uint* bufferPtr = &buffers[0])
            {
                glGenBuffers(buffers.Length, bufferPtr);
                CheckError();
            }
        }

        /// <summary>
        /// Generate a buffer and return the index
        /// </summary>
        public static uint glGenBuffers()
        {
            uint index = 0;
            glGenBuffers(1, &index);
            CheckError();
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glBindBuffer(uint target, uint buffer) => GLExtensions.glBindBuffer(target, buffer);

        /// <summary>
        ///     Delete named buffer objects.
        /// </summary>
        /// <param name="n">Specifies the number of buffer objects to be deleted.</param>
        /// <param name="buffers">Specifies an array of buffer objects to be deleted.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glDeleteBuffers(int n, uint* buffers) => GLExtensions.glDeleteBuffers(n, buffers);

        /// <summary>
        ///     Delete named buffer objects in an array
        /// </summary>
        /// <param name="vbo">object to be deleted.</param>
        public static void glDeleteBuffers(uint[] buffers)
        {
            if (buffers.Length == 0)
                throw new ArgumentException("Buffers array cannot be empty.");

            fixed (uint* bufferPtr = &buffers[0])
            {
                glDeleteBuffers(buffers.Length, bufferPtr);
                CheckError();
            }
        }

        /// <summary>
        ///     Delete named buffer objects.
        /// </summary>
        /// <param name="vbo">object to be deleted.</param>
        public static void glDeleteBuffers(uint vbo)
        {
            glDeleteBuffers(1, &vbo);
            CheckError();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glBufferData(uint target, int size, void* data, uint usage) => GLExtensions.glBufferData(target, size, data, usage);

        /// <summary>
        /// Send data for a buffer of vector
        /// </summary>
        public static void glBufferData(uint target, Vector2[] data, uint usage)
        {
            fixed (Vector2* ptr = &data[0])
            {
                GLExtensions.glBufferData(target, sizeof(Vector2) * data.Length, ptr, usage);
            }
        }

        /// <summary>
        /// Send data for a buffer of vector
        /// </summary>
        public static void glBufferData(uint target, Vector3[] data, uint usage)
        {
            fixed (Vector3* ptr = &data[0])
            {
                GLExtensions.glBufferData(target, sizeof(Vector3) * data.Length, ptr, usage);
            }
        }

        /// <summary>
        /// Send data for a buffer of int
        /// </summary>
        public static void glBufferData(uint target, int[] data, uint usage)
        {
            fixed (int* ptr = &data[0])
            {
                GLExtensions.glBufferData(target, sizeof(int) * data.Length, ptr, usage);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glEnableVertexAttribArray(uint index) => GLExtensions.glEnableVertexAttribArray(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glVertexAttribPointer(uint index, int size, uint type, bool normalized, int stride, uint pointer) => GLExtensions.glVertexAttribPointer(index, size, type, normalized, stride, pointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glDisableVertexAttribArray(uint index) => GLExtensions.glDisableVertexAttribArray(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glDebugMessageCallback(DebugProc callback, void* userParam) => GLExtensions.glDebugMessageCallback(callback, userParam);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glGenVertexArrays(int n, uint* arrays) => GLExtensions.glGenVertexArrays(n, arrays);

        /// <summary>
        /// Generate an vertex array and return the index
        /// </summary>
        public static uint glGenVertexArrays()
        {
            uint index = 0;
            glGenVertexArrays(1, &index);
            CheckError();
            return index;
        }

        /// <summary>
        ///     Delete vertex array objects.
        /// </summary>
        /// <param name="n">Specifies the number of vertex array objects to be deleted.</param>
        /// <param name="arrays">Specifies the address of an array containing the n names of the objects to be deleted.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glDeleteVertexArrays(int n, uint* arrays) => GLExtensions.glDeleteVertexArrays(n, arrays);

        /// <summary>
        ///     Delete vertex array objects.
        /// </summary>
        /// <param name="vao">name of the objects to be deleted.</param>
        public static void glDeleteVertexArrays(uint vao)
        {
            glDeleteVertexArrays(1, &vao);
            CheckError();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glBindVertexArray(uint array) => GLExtensions.glBindVertexArray(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glCompileShader(uint shader) => GLExtensions.glCompileShader(shader);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint glCreateProgram() => GLExtensions.glCreateProgram();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint glCreateShader(uint type) => GLExtensions.glCreateShader(type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool glIsProgram(uint program) => GLExtensions.glIsProgram(program);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool glIsShader(uint shader) => GLExtensions.glIsShader(shader);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glDeleteProgram(uint program) => GLExtensions.glDeleteProgram(program);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glDeleteShader(uint shader) => GLExtensions.glDeleteShader(shader);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glDetachShader(uint program, uint shader) => GLExtensions.glDetachShader(program, shader);

        /// <summary>
        ///      Replaces the source code in a shader object.
        /// </summary>
        /// <param name="shader">Specifies the handle of the shader object whose source code is to be replaced.</param>
        /// <param name="source">The source code to be loaded into the shader.</param>
        public static void glShaderSource(uint shader, string source)
        {
            var buffer = Encoding.UTF8.GetBytes(source);
            fixed (byte* p = &buffer[0])
            {
                var sources = new[] { p };
                fixed (byte** s = &sources[0])
                {
                    var length = buffer.Length;
                    GLExtensions.glShaderSource(shader, 1, s, &length);
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glAttachShader(uint program, uint shader) => GLExtensions.glAttachShader(program, shader);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glLinkProgram(uint program) => GLExtensions.glLinkProgram(program);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glUseProgram(uint program) => GLExtensions.glUseProgram(program);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glValidateProgram(uint program) => GLExtensions.glValidateProgram(program);

        /// <summary>
        /// Return a parameter from a program object.
        /// </summary>
        /// <param name="program">Specifies the program object to be queried.</param>
        /// <param name="pname">Specifies the object parameter.</param>
        /// <param name="count">The number of parameters to return..</param>
        /// <returns>The requested parameters.</returns>
        public static bool glGetProgramiv(uint program, uint pname)
        {
            int args;

            GLExtensions.glGetProgramiv(program, pname, &args);

            return args == 1;
        }

        /// <summary>
        /// Return a parameter from a shader object.
        /// </summary>
        /// <param name="shader">Specifies the shader object to be queried.</param>
        /// <param name="pname">Specifies the object parameter.<para>Must be GL_SHADER_TYPE, GL_DELETE_STATUS, GL_COMPILE_STATUS, GL_INFO_LOG_LENGTH, or GL_SHADER_SOURCE_LENGTH.</para></param>
        /// <param name="count">The number of parameters to return..</param>
        /// <returns>The requested parameters.</returns>
        public static bool glGetShaderiv(uint shader, uint pname)
        {
            int args;

            GLExtensions.glGetShaderiv(shader, pname, &args);

            return args == 1;
        }

        /// <summary>
        ///     Returns the information log for a program object.
        /// </summary>
        /// <param name="program">Specifies the program object whose information log is to be queried.</param>
        /// <param name="bufSize">Specifies the size of the character buffer for storing the returned information log.</param>
        /// <returns>The info log, or <c>null</c> if an error occured.</returns>
        public static string glGetProgramInfoLog(uint program, int bufSize = 1024)
        {
            var buffer = Marshal.AllocHGlobal(bufSize);
            try
            {
                int length;
                var source = (byte*)buffer.ToPointer();
                GLExtensions.glGetProgramInfoLog(program, bufSize, &length, source);
                return NativeUtils.PtrToStringUtf8(buffer, length);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        ///     Returns the information log for a shader object.
        /// </summary>
        /// <param name="shader">Specifies the shader object whose information log is to be queried.</param>
        /// <param name="bufSize">Specifies the size of the character buffer for storing the returned information log.</param>
        /// <returns>The info log, or <c>null</c> if an error occured.</returns>
        public static string glGetShaderInfoLog(uint shader, int bufSize = 1024)
        {
            var buffer = Marshal.AllocHGlobal(bufSize);
            try
            {
                int length = bufSize;
                var source = (byte*)buffer.ToPointer();
                GLExtensions.glGetShaderInfoLog(shader, bufSize, &length, source);
                return NativeUtils.PtrToStringUtf8(buffer, length);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }


        /// <summary>
        ///      Returns the location of a uniform variable.
        /// </summary>
        /// <param name="program">Specifies the program object to be queried.</param>
        /// <param name="name">A array of bytes containing the name of the uniform variable whose location is to be queried.</param>
        /// <returns>An integer that represents the location of a specific uniform variable within a program object.</returns>
        public static int glGetUniformLocation(uint program, string name)
        {
            var bytes = Encoding.UTF8.GetBytes(name);
            fixed (byte* b = &bytes[0])
            {
                return GLExtensions.glGetUniformLocation(program, b);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glUniform1f(int location, float v0) => GLExtensions.glUniform1f(location, v0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glUniform1i(int location, int v0) => GLExtensions.glUniform1i(location, v0);


        /// <summary>
        /// Specify the value of a uniform variable for the current program object.
        /// </summary>
        /// <param name="location">Specifies the location of the uniform variable to be modified.</param>
        /// <param name="count">Specifies the number of matrices that are to be modified.</param>
        /// <param name="transpose">Specifies whether to transpose the matrix as the values are loaded into the uniform variable.</param>
        /// <param name="values">An array of count values that will be used to update the specified uniform variable.</param>
        public static void glUniformMatrix4fv(int location, int count, bool transpose, float[] values)
        {
            fixed (float* value = &values[0])
            {
                GLExtensions.glUniformMatrix4fv(location, count, transpose, value);
            }
        }

        /// <summary>
        /// Specify the value of a uniform variable for the current program object.
        /// </summary>
        /// <param name="location">Specifies the location of the uniform variable to be modified.</param>
        /// <param name="count">Specifies the number of matrices that are to be modified.</param>
        /// <param name="transpose">Specifies whether to transpose the matrix as the values are loaded into the uniform variable.</param>
        /// <param name="values">An array of count values that will be used to update the specified uniform variable.</param>
        public static void glUniformMatrix4fv(int location, Matrix4 matrix)
        {

            //fixed (float* value = &matrix.M11)
            //{
                GLExtensions.glUniformMatrix4fv(location, 1, true, &matrix.M11);
            //}
        }

        /// <summary>
        ///     Select active texture unit.
        /// </summary>
        /// <param name="texture">Specifies which texture unit to make active.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glActiveTexture(uint textureUnit) => GLExtensions.glActiveTexture(textureUnit);

        /// <summary>
        ///     Select active texture unit.
        /// </summary>
        /// <param name="texture">Specifies which texture unit to make active.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void glDrawElementsBaseVertex(uint mode, int count, uint type, int indices, int baseVertex) => GLExtensions.glDrawElementsBaseVertex(mode, count, type, indices, baseVertex);


        #endregion



        #region My public methods

        /// <summary>
        /// Check if we have an OpenGL error and throws an exception if we have
        /// </summary>
        public static void CheckError()
        {
            uint error = glGetError();

            if (error != GL_NO_ERROR)
            {
                switch (error)
                {
                    case GL_INVALID_ENUM:
                        throw new Exception("Open GL Error - Invalid enum: 0x" + error.ToString("x"));
                    case GL_INVALID_VALUE:
                        throw new Exception("Open GL Error - Invalid value: 0x" + error.ToString("x"));
                    case GL_INVALID_OPERATION:
                        throw new Exception("Open GL Error - Invalid enum: 0x" + error.ToString("x"));
                    case GL_STACK_OVERFLOW:
                        throw new Exception("Open GL Error - Stack overflow: 0x" + error.ToString("x"));
                    case GL_STACK_UNDERFLOW:
                        throw new Exception("Open GL Error - Stack underflow: 0x" + error.ToString("x"));
                    case GL_OUT_OF_MEMORY:
                        throw new Exception("Open GL Error - Out of memory: 0x" + error.ToString("x"));
                    default:
                        throw new Exception("Open GL Error - Unkown error: 0x" + error.ToString("x"));
                }

            }  
        }



        /// <summary>
        /// Creates a shader of the specified type from the given source string.
        /// </summary>
        /// <param name="type">An OpenGL enum for the shader type.</param>
        /// <param name="source">The source code of the shader.</param>
        /// <returns>The created shader. No error checking is performed for this basic example.</returns>
        public static uint CreateShader(uint type, string source)
        {
            var shader = glCreateShader(type);
            if (shader == 0)
                throw new Exception($"Error creating a shader type {type}");

            glShaderSource(shader, source);

            glCompileShader(shader);

            if (!glGetShaderiv(shader, GL_COMPILE_STATUS))
                throw new Exception($"Error compiling shader: {glGetShaderInfoLog(shader)}");

            return shader;
        }


        #endregion

        #region My privates methods

        

        #endregion
    }
}
