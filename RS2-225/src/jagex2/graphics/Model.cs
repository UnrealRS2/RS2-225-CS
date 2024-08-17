using RS2_225.jagex2.datastruct;
using RS2_225.jagex2.graphics.model;
using RS2_225.jagex2.io;

namespace RS2_225.jagex2.graphics;

public class Model : DoublyLinkable
{
    public static Metadata[]? metadata;

    private static Packet head;

    public static Packet face1;

    public static Packet face2;

    public static Packet face3;

    public static Packet face4;

    public static Packet face5;

    public static Packet point1;

    public static Packet point2;

    public static Packet point3;

    public static Packet point4;

    public static Packet point5;

    public static Packet vertex1;

    public static Packet vertex2;

    public static Packet axis;

    public static bool[] faceClippedX = new bool[4096];

    public static bool[] faceNearClipped = new bool[4096];

    public static int[] vertexScreenX = new int[4096];

    public static int[] vertexScreenY = new int[4096];

    public static int[] vertexScreenZ = new int[4096];

    public static int[] vertexViewSpaceX = new int[4096];

    public static int[] vertexViewSpaceY = new int[4096];

    public static int[] vertexViewSpaceZ = new int[4096];

    public static int[] tmpDepthFaceCount = new int[1500];

    public static int[][] tmpDepthFaces = new int[1500][];

    public static int[] tmpPriorityFaceCount = new int[12];

    public static int[][] tmpPriorityFaces = new int[12][];

    public static int[] tmpPriority10FaceDepth = new int[2000];

    public static int[] tmpPriority11FaceDepth = new int[2000];

    public static int[] tmpPriorityDepthSum = new int[12];

    public static int[] clippedX = new int[10];

    public static int[] clippedY = new int[10];

    public static int[] clippedColor = new int[10];

    public static int baseX;

    public static int baseY;

    public static int baseZ;

    public static bool checkHover;

    public static int mouseX;

    public static int mouseZ;

    public static int pickedCount;

    public static int[] pickedBitsets = new int[1000];

    public static int[] sin = Pix3D.sinTable;

    public static int[] cos = Pix3D.cosTable;

    public static int[] palette = Pix3D.colourTable;

    public static int[] reciprical16 = Pix3D.divTable2;

    private readonly int[] faceAlpha;

    public int[] faceColor;

    private int[] faceColorA;

    private int[] faceColorB;

    private int[] faceColorC;

    public int faceCount;

    public int[] faceInfo;

    private int[] faceLabel;

    private readonly int[] facePriority;

    public int[] faceVertexA;

    public int[] faceVertexB;

    public int[] faceVertexC;

    public int[][] labelFaces;

    public int[][] labelVertices;

    private int maxDepth;

    public int maxX;

    public int maxY;

    public int maxZ;

    private int minDepth;

    public int minX;

    public int minY;

    public int minZ;

    public int objRaise;

    public bool pickable = false;

    private readonly int priority;

    public int radius;

    private readonly int texturedFaceCount;

    private readonly int[] texturedVertexA;

    private readonly int[] texturedVertexB;

    private readonly int[] texturedVertexC;

    public int vertexCount;

    private int[] vertexLabel;

    public VertexNormal[] vertexNormal;

    public VertexNormal[] vertexNormalOriginal;

    public int[] vertexX;

    public int[] vertexY;

    public int[] vertexZ;

    static Model()
    {
        for (var i = 0; i < 1500; i++) tmpDepthFaces[i] = new int[512];
        for (var i = 0; i < 12; i++) tmpPriorityFaces[i] = new int[2000];
    }

    public Model(int id)
    {
        if (metadata == null) return;

        var meta = metadata[id];
        if (meta == null)
        {
            Console.WriteLine("Error model:" + id + " not found!");
        }
        else
        {
            vertexCount = meta.vertexCount;
            faceCount = meta.faceCount;
            texturedFaceCount = meta.texturedFaceCount;
            vertexX = new int[vertexCount];
            vertexY = new int[vertexCount];
            vertexZ = new int[vertexCount];
            faceVertexA = new int[faceCount];
            faceVertexB = new int[faceCount];
            faceVertexC = new int[faceCount];
            texturedVertexA = new int[texturedFaceCount];
            texturedVertexB = new int[texturedFaceCount];
            texturedVertexC = new int[texturedFaceCount];

            if (meta.vertexLabelsOffset >= 0) vertexLabel = new int[vertexCount];

            if (meta.faceInfosOffset >= 0) faceInfo = new int[faceCount];

            if (meta.facePrioritiesOffset >= 0)
                facePriority = new int[faceCount];
            else
                priority = -meta.facePrioritiesOffset - 1;

            if (meta.faceAlphasOffset >= 0) faceAlpha = new int[faceCount];

            if (meta.faceLabelsOffset >= 0) faceLabel = new int[faceCount];

            faceColor = new int[faceCount];

            point1.pos = meta.vertexFlagsOffset;
            point2.pos = meta.vertexXOffset;
            point3.pos = meta.vertexYOffset;
            point4.pos = meta.vertexZOffset;
            point5.pos = meta.vertexLabelsOffset;

            var dx = 0;
            var db = 0;
            var dc = 0;
            int a;
            int b;
            int c;

            for (var v = 0; v < vertexCount; v++)
            {
                var flags = point1.g1();
                a = 0;
                if ((flags & 0x1) != 0) a = point2.gsmart();

                b = 0;
                if ((flags & 0x2) != 0) b = point3.gsmart();

                c = 0;
                if ((flags & 0x4) != 0) c = point4.gsmart();

                vertexX[v] = dx + a;
                vertexY[v] = db + b;
                vertexZ[v] = dc + c;
                dx = vertexX[v];
                db = vertexY[v];
                dc = vertexZ[v];

                if (vertexLabel != null) vertexLabel[v] = point5.g1();
            }

            face1.pos = meta.faceColorsOffset;
            face2.pos = meta.faceInfosOffset;
            face3.pos = meta.facePrioritiesOffset;
            face4.pos = meta.faceAlphasOffset;
            face5.pos = meta.faceLabelsOffset;
            for (var f = 0; f < faceCount; f++)
            {
                faceColor[f] = face1.g2();
                if (faceInfo != null) faceInfo[f] = face2.g1();

                if (facePriority != null) facePriority[f] = face3.g1();

                if (faceAlpha != null) faceAlpha[f] = face4.g1();

                if (faceLabel != null) faceLabel[f] = face5.g1();
            }

            vertex1.pos = meta.faceVerticesOffset;
            vertex2.pos = meta.faceOrientationsOffset;

            a = 0;
            b = 0;
            c = 0;
            var last = 0;

            for (var f = 0; f < faceCount; f++)
            {
                var orientation = vertex2.g1();

                if (orientation == 1)
                {
                    a = vertex1.gsmart() + last;
                    b = vertex1.gsmart() + a;
                    c = vertex1.gsmart() + b;
                    last = c;
                    faceVertexA[f] = a;
                    faceVertexB[f] = b;
                    faceVertexC[f] = c;
                }
                else if (orientation == 2)
                {
                    a = a;
                    b = c;
                    c = vertex1.gsmart() + last;
                    last = c;
                    faceVertexA[f] = a;
                    faceVertexB[f] = b;
                    faceVertexC[f] = c;
                }
                else if (orientation == 3)
                {
                    a = c;
                    b = b;
                    c = vertex1.gsmart() + last;
                    last = c;
                    faceVertexA[f] = a;
                    faceVertexB[f] = b;
                    faceVertexC[f] = c;
                }
                else if (orientation == 4)
                {
                    var tmp = a;
                    a = b;
                    b = tmp;
                    c = vertex1.gsmart() + last;
                    last = c;
                    faceVertexA[f] = a;
                    faceVertexB[f] = tmp;
                    faceVertexC[f] = c;
                }
            }

            axis.pos = meta.faceTextureAxisOffset * 6;
            for (var f = 0; f < texturedFaceCount; f++)
            {
                texturedVertexA[f] = axis.g2();
                texturedVertexB[f] = axis.g2();
                texturedVertexC[f] = axis.g2();
            }
        }
    }

    public Model(Model[] models, int count)
    {
        var copyInfo = false;
        var copyPriorities = false;
        var copyAlpha = false;
        var copyLabels = false;

        vertexCount = 0;
        faceCount = 0;
        texturedFaceCount = 0;
        priority = -1;

        for (var i = 0; i < count; i++)
        {
            var model = models[i];
            if (model != null)
            {
                vertexCount += model.vertexCount;
                faceCount += model.faceCount;
                texturedFaceCount += model.texturedFaceCount;
                copyInfo |= model.faceInfo != null;

                if (model.facePriority == null)
                {
                    if (priority == -1) priority = model.priority;

                    if (priority != model.priority) copyPriorities = true;
                }
                else
                {
                    copyPriorities = true;
                }

                copyAlpha |= model.faceAlpha != null;
                copyLabels |= model.faceLabel != null;
            }
        }

        vertexX = new int[vertexCount];
        vertexY = new int[vertexCount];
        vertexZ = new int[vertexCount];
        vertexLabel = new int[vertexCount];
        faceVertexA = new int[faceCount];
        faceVertexB = new int[faceCount];
        faceVertexC = new int[faceCount];
        texturedVertexA = new int[texturedFaceCount];
        texturedVertexB = new int[texturedFaceCount];
        texturedVertexC = new int[texturedFaceCount];

        if (copyInfo) faceInfo = new int[faceCount];

        if (copyPriorities) facePriority = new int[faceCount];

        if (copyAlpha) faceAlpha = new int[faceCount];

        if (copyLabels) faceLabel = new int[faceCount];

        faceColor = new int[faceCount];
        vertexCount = 0;
        faceCount = 0;
        texturedFaceCount = 0;

        for (var i = 0; i < count; i++)
        {
            var model = models[i];

            if (model != null)
            {
                for (var face = 0; face < model.faceCount; face++)
                {
                    if (copyInfo)
                    {
                        if (model.faceInfo == null)
                            faceInfo[faceCount] = 0;
                        else
                            faceInfo[faceCount] = model.faceInfo[face];
                    }

                    if (copyPriorities)
                    {
                        if (model.facePriority == null)
                            facePriority[faceCount] = model.priority;
                        else
                            facePriority[faceCount] = model.facePriority[face];
                    }

                    if (copyAlpha)
                    {
                        if (model.faceAlpha == null)
                            faceAlpha[faceCount] = 0;
                        else
                            faceAlpha[faceCount] = model.faceAlpha[face];
                    }

                    if (copyLabels && model.faceLabel != null) faceLabel[faceCount] = model.faceLabel[face];

                    faceColor[faceCount] = model.faceColor[face];
                    faceVertexA[faceCount] = addVertex(model, model.faceVertexA[face]);
                    faceVertexB[faceCount] = addVertex(model, model.faceVertexB[face]);
                    faceVertexC[faceCount] = addVertex(model, model.faceVertexC[face]);
                    faceCount++;
                }

                for (var f = 0; f < model.texturedFaceCount; f++)
                {
                    texturedVertexA[texturedFaceCount] = addVertex(model, model.texturedVertexA[f]);
                    texturedVertexB[texturedFaceCount] = addVertex(model, model.texturedVertexB[f]);
                    texturedVertexC[texturedFaceCount] = addVertex(model, model.texturedVertexC[f]);
                    texturedFaceCount++;
                }
            }
        }
    }

    public Model(Model[] models, int count, bool dummy)
    {
        var copyInfo = false;
        var copyPriority = false;
        var copyAlpha = false;
        var copyColor = false;

        this.vertexCount = 0;
        faceCount = 0;
        texturedFaceCount = 0;
        priority = -1;

        for (var i = 0; i < count; i++)
        {
            var model = models[i];
            if (model != null)
            {
                vertexCount += model.vertexCount;
                faceCount += model.faceCount;
                texturedFaceCount += model.texturedFaceCount;

                copyInfo |= model.faceInfo != null;

                if (model.facePriority == null)
                {
                    if (priority == -1) priority = model.priority;
                    if (priority != model.priority) copyPriority = true;
                }
                else
                {
                    copyPriority = true;
                }

                copyAlpha |= model.faceAlpha != null;
                copyColor |= model.faceColor != null;
            }
        }

        vertexX = new int[this.vertexCount];
        vertexY = new int[this.vertexCount];
        vertexZ = new int[this.vertexCount];
        faceVertexA = new int[faceCount];
        faceVertexB = new int[faceCount];
        faceVertexC = new int[faceCount];
        faceColorA = new int[faceCount];
        faceColorB = new int[faceCount];
        faceColorC = new int[faceCount];
        texturedVertexA = new int[texturedFaceCount];
        texturedVertexB = new int[texturedFaceCount];
        texturedVertexC = new int[texturedFaceCount];

        if (copyInfo) faceInfo = new int[faceCount];

        if (copyPriority) facePriority = new int[faceCount];

        if (copyAlpha) faceAlpha = new int[faceCount];

        if (copyColor) faceColor = new int[faceCount];

        this.vertexCount = 0;
        faceCount = 0;
        texturedFaceCount = 0;

        for (var i = 0; i < count; i++)
        {
            var model = models[i];
            if (model != null)
            {
                var vertexCount = this.vertexCount;

                for (var v = 0; v < model.vertexCount; v++)
                {
                    vertexX[this.vertexCount] = model.vertexX[v];
                    vertexY[this.vertexCount] = model.vertexY[v];
                    vertexZ[this.vertexCount] = model.vertexZ[v];
                    this.vertexCount++;
                }

                for (var f = 0; f < model.faceCount; f++)
                {
                    faceVertexA[faceCount] = model.faceVertexA[f] + vertexCount;
                    faceVertexB[faceCount] = model.faceVertexB[f] + vertexCount;
                    faceVertexC[faceCount] = model.faceVertexC[f] + vertexCount;
                    faceColorA[faceCount] = model.faceColorA[f];
                    faceColorB[faceCount] = model.faceColorB[f];
                    faceColorC[faceCount] = model.faceColorC[f];

                    if (copyInfo)
                    {
                        if (model.faceInfo == null)
                            faceInfo[faceCount] = 0;
                        else
                            faceInfo[faceCount] = model.faceInfo[f];
                    }

                    if (copyPriority)
                    {
                        if (model.facePriority == null)
                            facePriority[faceCount] = model.priority;
                        else
                            facePriority[faceCount] = model.facePriority[f];
                    }

                    if (copyAlpha)
                    {
                        if (model.faceAlpha == null)
                            faceAlpha[faceCount] = 0;
                        else
                            faceAlpha[faceCount] = model.faceAlpha[f];
                    }

                    if (copyColor && model.faceColor != null) faceColor[faceCount] = model.faceColor[f];

                    faceCount++;
                }

                for (var f = 0; f < model.texturedFaceCount; f++)
                {
                    texturedVertexA[texturedFaceCount] = model.texturedVertexA[f] + vertexCount;
                    texturedVertexB[texturedFaceCount] = model.texturedVertexB[f] + vertexCount;
                    texturedVertexC[texturedFaceCount] = model.texturedVertexC[f] + vertexCount;
                    texturedFaceCount++;
                }
            }
        }

        calculateBoundsCylinder();
    }

    public Model(Model src, bool shareColors, bool shareAlpha, bool shareVertices)
    {
        vertexCount = src.vertexCount;
        faceCount = src.faceCount;
        texturedFaceCount = src.texturedFaceCount;

        if (shareVertices)
        {
            vertexX = src.vertexX;
            vertexY = src.vertexY;
            vertexZ = src.vertexZ;
        }
        else
        {
            vertexX = new int[vertexCount];
            vertexY = new int[vertexCount];
            vertexZ = new int[vertexCount];

            for (var v = 0; v < vertexCount; v++)
            {
                vertexX[v] = src.vertexX[v];
                vertexY[v] = src.vertexY[v];
                vertexZ[v] = src.vertexZ[v];
            }
        }

        if (shareColors)
        {
            faceColor = src.faceColor;
        }
        else
        {
            faceColor = new int[faceCount];
            Array.Copy(src.faceColor, 0, faceColor, 0, faceCount);
        }

        if (shareAlpha)
        {
            faceAlpha = src.faceAlpha;
        }
        else
        {
            faceAlpha = new int[faceCount];
            if (src.faceAlpha == null)
                for (var f = 0; f < faceCount; f++)
                    faceAlpha[f] = 0;
            else
                Array.Copy(src.faceAlpha, 0, faceAlpha, 0, faceCount);
        }

        vertexLabel = src.vertexLabel;
        faceLabel = src.faceLabel;
        faceInfo = src.faceInfo;
        faceVertexA = src.faceVertexA;
        faceVertexB = src.faceVertexB;
        faceVertexC = src.faceVertexC;
        facePriority = src.facePriority;
        priority = src.priority;
        texturedVertexA = src.texturedVertexA;
        texturedVertexB = src.texturedVertexB;
        texturedVertexC = src.texturedVertexC;
    }

    public Model(Model src, bool copyVertexY, bool copyFaces)
    {
        vertexCount = src.vertexCount;
        faceCount = src.faceCount;
        texturedFaceCount = src.texturedFaceCount;

        if (copyVertexY)
        {
            vertexY = new int[vertexCount];
            Array.Copy(src.vertexY, 0, vertexY, 0, vertexCount);
        }
        else
        {
            vertexY = src.vertexY;
        }

        if (copyFaces)
        {
            faceColorA = new int[faceCount];
            faceColorB = new int[faceCount];
            faceColorC = new int[faceCount];
            for (var f = 0; f < faceCount; f++)
            {
                faceColorA[f] = src.faceColorA[f];
                faceColorB[f] = src.faceColorB[f];
                faceColorC[f] = src.faceColorC[f];
            }

            faceInfo = new int[faceCount];
            if (src.faceInfo == null)
                for (var f = 0; f < faceCount; f++)
                    faceInfo[f] = 0;
            else
                Array.Copy(src.faceInfo, 0, faceInfo, 0, faceCount);

            vertexNormal = new VertexNormal[vertexCount];
            for (var v = 0; v < vertexCount; v++)
            {
                var copy = vertexNormal[v] = new VertexNormal();
                var original = src.vertexNormal[v];
                copy.x = original.x;
                copy.y = original.y;
                copy.z = original.z;
                copy.w = original.w;
            }

            vertexNormalOriginal = src.vertexNormalOriginal;
        }
        else
        {
            faceColorA = src.faceColorA;
            faceColorB = src.faceColorB;
            faceColorC = src.faceColorC;
            faceInfo = src.faceInfo;
        }

        vertexX = src.vertexX;
        vertexZ = src.vertexZ;
        faceColor = src.faceColor;
        faceAlpha = src.faceAlpha;
        facePriority = src.facePriority;
        priority = src.priority;
        faceVertexA = src.faceVertexA;
        faceVertexB = src.faceVertexB;
        faceVertexC = src.faceVertexC;
        texturedVertexA = src.texturedVertexA;
        texturedVertexB = src.texturedVertexB;
        texturedVertexC = src.texturedVertexC;
        maxY = src.maxY;
        minY = src.minY;
        radius = src.radius;
        minDepth = src.minDepth;
        maxDepth = src.maxDepth;
        minX = src.minX;
        maxZ = src.maxZ;
        minZ = src.minZ;
        maxX = src.maxX;
    }

    public Model(Model src, bool shareAlpha)
    {
        vertexCount = src.vertexCount;
        faceCount = src.faceCount;
        texturedFaceCount = src.texturedFaceCount;

        vertexX = new int[vertexCount];
        vertexY = new int[vertexCount];
        vertexZ = new int[vertexCount];

        for (var v = 0; v < vertexCount; v++)
        {
            vertexX[v] = src.vertexX[v];
            vertexY[v] = src.vertexY[v];
            vertexZ[v] = src.vertexZ[v];
        }

        if (shareAlpha)
        {
            faceAlpha = src.faceAlpha;
        }
        else
        {
            faceAlpha = new int[faceCount];
            if (src.faceAlpha == null)
                for (var f = 0; f < faceCount; f++)
                    faceAlpha[f] = 0;
            else
                Array.Copy(src.faceAlpha, 0, faceAlpha, 0, faceCount);
        }

        faceInfo = src.faceInfo;
        faceColor = src.faceColor;
        facePriority = src.facePriority;
        priority = src.priority;
        labelFaces = src.labelFaces;
        labelVertices = src.labelVertices;
        faceVertexA = src.faceVertexA;
        faceVertexB = src.faceVertexB;
        faceVertexC = src.faceVertexC;
        faceColorA = src.faceColorA;
        faceColorB = src.faceColorB;
        faceColorC = src.faceColorC;
        texturedVertexA = src.texturedVertexA;
        texturedVertexB = src.texturedVertexB;
        texturedVertexC = src.texturedVertexC;
    }

    public static void unload()
    {
        metadata = null;
        head = null;
        face1 = null;
        face2 = null;
        face3 = null;
        face4 = null;
        face5 = null;
        point1 = null;
        point2 = null;
        point3 = null;
        point4 = null;
        point5 = null;
        vertex1 = null;
        vertex2 = null;
        axis = null;
        faceClippedX = null;
        faceNearClipped = null;
        vertexScreenX = null;
        vertexScreenY = null;
        vertexScreenZ = null;
        vertexViewSpaceX = null;
        vertexViewSpaceY = null;
        vertexViewSpaceZ = null;
        tmpDepthFaceCount = null;
        tmpDepthFaces = null;
        tmpPriorityFaceCount = null;
        tmpPriorityFaces = null;
        tmpPriority10FaceDepth = null;
        tmpPriority11FaceDepth = null;
        tmpPriorityDepthSum = null;
        sin = null;
        cos = null;
        palette = null;
        reciprical16 = null;
    }

    public static void unpack(Jagfile models)
    {
        try
        {
            head = new Packet(models.read("ob_head.dat", null));
            face1 = new Packet(models.read("ob_face1.dat", null));
            face2 = new Packet(models.read("ob_face2.dat", null));
            face3 = new Packet(models.read("ob_face3.dat", null));
            face4 = new Packet(models.read("ob_face4.dat", null));
            face5 = new Packet(models.read("ob_face5.dat", null));
            point1 = new Packet(models.read("ob_point1.dat", null));
            point2 = new Packet(models.read("ob_point2.dat", null));
            point3 = new Packet(models.read("ob_point3.dat", null));
            point4 = new Packet(models.read("ob_point4.dat", null));
            point5 = new Packet(models.read("ob_point5.dat", null));
            vertex1 = new Packet(models.read("ob_vertex1.dat", null));
            vertex2 = new Packet(models.read("ob_vertex2.dat", null));
            axis = new Packet(models.read("ob_axis.dat", null));
            head.pos = 0;
            point1.pos = 0;
            point2.pos = 0;
            point3.pos = 0;
            point4.pos = 0;
            vertex1.pos = 0;
            vertex2.pos = 0;
            var count = head.g2();
            metadata = new Metadata[count + 100];
            var vertexTextureDataOffset = 0;
            var labelDataOffset = 0;
            var triangleColorDataOffset = 0;
            var triangleInfoDataOffset = 0;
            var trianglePriorityDataOffset = 0;
            var triangleAlphaDataOffset = 0;
            var triangleSkinDataOffset = 0;
            for (var i = 0; i < count; i++)
            {
                var index = head.g2();
                var meta = metadata[index] = new Metadata();
                meta.vertexCount = head.g2();
                meta.faceCount = head.g2();
                meta.texturedFaceCount = head.g1();
                meta.vertexFlagsOffset = point1.pos;
                meta.vertexXOffset = point2.pos;
                meta.vertexYOffset = point3.pos;
                meta.vertexZOffset = point4.pos;
                meta.faceVerticesOffset = vertex1.pos;
                meta.faceOrientationsOffset = vertex2.pos;
                var hasInfo = head.g1();
                var hasPriorities = head.g1();
                var hasAlpha = head.g1();
                var hasSkins = head.g1();
                var hasLabels = head.g1();
                for (var v = 0; v < meta.vertexCount; v++)
                {
                    var flags = point1.g1();
                    if ((flags & 0x1) != 0) point2.gsmart();
                    if ((flags & 0x2) != 0) point3.gsmart();
                    if ((flags & 0x4) != 0) point4.gsmart();
                }

                for (var f = 0; f < meta.faceCount; f++)
                {
                    var type = vertex2.g1();
                    if (type == 1)
                    {
                        vertex1.gsmart();
                        vertex1.gsmart();
                    }

                    vertex1.gsmart();
                }

                meta.faceColorsOffset = triangleColorDataOffset;
                triangleColorDataOffset += meta.faceCount * 2;
                if (hasInfo == 1)
                {
                    meta.faceInfosOffset = triangleInfoDataOffset;
                    triangleInfoDataOffset += meta.faceCount;
                }
                else
                {
                    meta.faceInfosOffset = -1;
                }

                if (hasPriorities == 255)
                {
                    meta.facePrioritiesOffset = trianglePriorityDataOffset;
                    trianglePriorityDataOffset += meta.faceCount;
                }
                else
                {
                    meta.facePrioritiesOffset = -hasPriorities - 1;
                }

                if (hasAlpha == 1)
                {
                    meta.faceAlphasOffset = triangleAlphaDataOffset;
                    triangleAlphaDataOffset += meta.faceCount;
                }
                else
                {
                    meta.faceAlphasOffset = -1;
                }

                if (hasSkins == 1)
                {
                    meta.faceLabelsOffset = triangleSkinDataOffset;
                    triangleSkinDataOffset += meta.faceCount;
                }
                else
                {
                    meta.faceLabelsOffset = -1;
                }

                if (hasLabels == 1)
                {
                    meta.vertexLabelsOffset = labelDataOffset;
                    labelDataOffset += meta.vertexCount;
                }
                else
                {
                    meta.vertexLabelsOffset = -1;
                }

                meta.faceTextureAxisOffset = vertexTextureDataOffset;
                vertexTextureDataOffset += meta.texturedFaceCount;
            }

            Console.WriteLine("Unpacked " + count + " Models");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading model index");
            Console.WriteLine(ex.Message);
        }
    }

    public static int mulColorLightness(int hsl, int scalar, int faceInfo)
    {
        if ((faceInfo & 0x2) == 2)
        {
            if (scalar < 0)
                scalar = 0;
            else if (scalar > 127) scalar = 127;
            return 127 - scalar;
        }

        scalar = (scalar * (hsl & 0x7F)) >> 7;
        if (scalar < 2)
            scalar = 2;
        else if (scalar > 126) scalar = 126;
        return (hsl & 0xFF80) + scalar;
    }

    private int addVertex(Model src, int vertexId)
    {
        var identical = -1;
        var x = src.vertexX[vertexId];
        var y = src.vertexY[vertexId];
        var z = src.vertexZ[vertexId];
        for (var v = 0; v < vertexCount; v++)
            if (x == vertexX[v] && y == vertexY[v] && z == vertexZ[v])
            {
                identical = v;
                break;
            }

        if (identical == -1)
        {
            vertexX[vertexCount] = x;
            vertexY[vertexCount] = y;
            vertexZ[vertexCount] = z;
            if (src.vertexLabel != null) vertexLabel[vertexCount] = src.vertexLabel[vertexId];
            identical = vertexCount++;
        }

        return identical;
    }

    public void calculateBoundsCylinder()
    {
        maxY = 0;
        radius = 0;
        minY = 0;

        for (var i = 0; i < vertexCount; i++)
        {
            var x = vertexX[i];
            var y = vertexY[i];
            var z = vertexZ[i];

            if (-y > maxY) maxY = -y;
            if (y > minY) minY = y;

            var radiusSqr = x * x + z * z;
            if (radiusSqr > radius) radius = radiusSqr;
        }

        radius = (int)(Math.Sqrt(radius) + 0.99D);
        minDepth = (int)(Math.Sqrt(radius * radius + maxY * maxY) + 0.99D);
        maxDepth = minDepth + (int)(Math.Sqrt(radius * radius + minY * minY) + 0.99D);
    }

    public void calculateBoundsY()
    {
        maxY = 0;
        minY = 0;

        for (var v = 0; v < vertexCount; v++)
        {
            var y = vertexY[v];
            if (-y > maxY) maxY = -y;
            if (y > minY) minY = y;
        }

        minDepth = (int)(Math.Sqrt(radius * radius + maxY * maxY) + 0.99D);
        maxDepth = minDepth + (int)(Math.Sqrt(radius * radius + minY * minY) + 0.99D);
    }

    private void calculateBoundsAABB()
    {
        maxY = 0;
        radius = 0;
        minY = 0;
        minX = 999999;
        maxX = -999999;
        maxZ = -99999;
        minZ = 99999;

        for (var v = 0; v < vertexCount; v++)
        {
            var x = vertexX[v];
            var y = vertexY[v];
            var z = vertexZ[v];

            if (x < minX) minX = x;
            if (x > maxX) maxX = x;

            if (z < minZ) minZ = z;
            if (z > maxZ) maxZ = z;

            if (-y > maxY) maxY = -y;
            if (y > minY) minY = y;

            var radiusSqr = x * x + z * z;
            if (radiusSqr > radius) radius = radiusSqr;
        }

        radius = (int)Math.Sqrt(radius);
        minDepth = (int)Math.Sqrt(radius * radius + maxY * maxY);
        maxDepth = minDepth + (int)Math.Sqrt(radius * radius + minY * minY);
    }

    public void createLabelReferences()
    {
        if (vertexLabel != null)
        {
            var labelVertexCount = new int[256];

            var count = 0;
            for (var _v = 0; _v < vertexCount; _v++)
            {
                var label = vertexLabel[_v];
                var countDebug = labelVertexCount[label]++;

                if (label > count) count = label;
            }

            labelVertices = new int[count + 1][];
            for (var label = 0; label <= count; label++)
            {
                labelVertices[label] = new int[labelVertexCount[label]];
                labelVertexCount[label] = 0;
            }

            var v = 0;
            while (v < vertexCount)
            {
                var label = vertexLabel[v];
                labelVertices[label][labelVertexCount[label]++] = v++;
            }

            vertexLabel = null;
        }

        if (faceLabel != null)
        {
            var labelFaceCount = new int[256];

            var count = 0;
            for (var f = 0; f < faceCount; f++)
            {
                var label = faceLabel[f];
                var countDebug = labelFaceCount[label]++;
                if (label > count) count = label;
            }

            labelFaces = new int[count + 1][];
            for (var label = 0; label <= count; label++)
            {
                labelFaces[label] = new int[labelFaceCount[label]];
                labelFaceCount[label] = 0;
            }

            var face = 0;
            while (face < faceCount)
            {
                var label = faceLabel[face];
                labelFaces[label][labelFaceCount[label]++] = face++;
            }

            faceLabel = null;
        }
    }

    public void applyTransform(int id)
    {
        if (labelVertices != null && id != -1)
        {
            var frame = AnimFrame.instances[id];
            var _base = frame._base;

            baseX = 0;
            baseY = 0;
            baseZ = 0;

            for (var i = 0; i < frame.length; i++)
            {
                var group = frame.groups[i];
                applyTransform(frame.x[i], frame.y[i], frame.z[i], _base.labels[group], _base.types[group]);
            }
        }
    }

    public void applyTransforms(int id, int id2, int[] walkmerge)
    {
        if (id == -1) return;

        if (walkmerge == null || id2 == -1)
        {
            applyTransform(id);
        }
        else
        {
            var frame = AnimFrame.instances[id];
            var frame2 = AnimFrame.instances[id2];
            var _base = frame._base;

            baseX = 0;
            baseY = 0;
            baseZ = 0;

            var length = 0;
            var merge = walkmerge[length++];

            for (var i = 0; i < frame.length; i++)
            {
                var group = frame.groups[i];
                while (group > merge) merge = walkmerge[length++];

                if (group != merge || _base.types[group] == AnimBase.OP_BASE)
                    applyTransform(frame.x[i], frame.y[i], frame.z[i], _base.labels[group], _base.types[group]);
            }

            baseX = 0;
            baseY = 0;
            baseZ = 0;

            length = 0;
            merge = walkmerge[length++];

            for (var i = 0; i < frame2.length; i++)
            {
                var group = frame2.groups[i];
                while (group > merge) merge = walkmerge[length++];

                if (group == merge || _base.types[group] == AnimBase.OP_BASE)
                    applyTransform(frame2.x[i], frame2.y[i], frame2.z[i], _base.labels[group], _base.types[group]);
            }
        }
    }

    private void applyTransform(int x, int y, int z, int[] labels, int type)
    {
        var labelCount = labels.Length;

        if (type == AnimBase.OP_BASE)
        {
            var count = 0;
            baseX = 0;
            baseY = 0;
            baseZ = 0;

            for (var g = 0; g < labelCount; g++)
            {
                var label = labels[g];
                if (label < labelVertices.Length)
                {
                    var vertices = labelVertices[label];
                    for (var i = 0; i < vertices.Length; i++)
                    {
                        var v = vertices[i];
                        baseX += vertexX[v];
                        baseY += vertexY[v];
                        baseZ += vertexZ[v];
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                baseX = baseX / count + x;
                baseY = baseY / count + y;
                baseZ = baseZ / count + z;
            }
            else
            {
                baseX = x;
                baseY = y;
                baseZ = z;
            }
        }
        else if (type == AnimBase.OP_TRANSLATE)
        {
            for (var g = 0; g < labelCount; g++)
            {
                var label = labels[g];
                if (label >= labelVertices.Length) continue;

                var vertices = labelVertices[label];
                for (var i = 0; i < vertices.Length; i++)
                {
                    var v = vertices[i];
                    vertexX[v] += x;
                    vertexY[v] += y;
                    vertexZ[v] += z;
                }
            }
        }
        else if (type == AnimBase.OP_ROTATE)
        {
            for (var g = 0; g < labelCount; g++)
            {
                var label = labels[g];
                if (label >= labelVertices.Length) continue;

                var vertices = labelVertices[label];
                for (var i = 0; i < vertices.Length; i++)
                {
                    var v = vertices[i];
                    vertexX[v] -= baseX;
                    vertexY[v] -= baseY;
                    vertexZ[v] -= baseZ;

                    var pitch = (x & 0xFF) * 8;
                    var yaw = (y & 0xFF) * 8;
                    var roll = (z & 0xFF) * 8;

                    int sin;
                    int cos;

                    if (roll != 0)
                    {
                        sin = Model.sin[roll];
                        cos = Model.cos[roll];
                        var x_ = (vertexY[v] * sin + vertexX[v] * cos) >> 16;
                        vertexY[v] = (vertexY[v] * cos - vertexX[v] * sin) >> 16;
                        vertexX[v] = x_;
                    }

                    if (pitch != 0)
                    {
                        sin = Model.sin[pitch];
                        cos = Model.cos[pitch];
                        var y_ = (vertexY[v] * cos - vertexZ[v] * sin) >> 16;
                        vertexZ[v] = (vertexY[v] * sin + vertexZ[v] * cos) >> 16;
                        vertexY[v] = y_;
                    }

                    if (yaw != 0)
                    {
                        sin = Model.sin[yaw];
                        cos = Model.cos[yaw];
                        var x_ = (vertexZ[v] * sin + vertexX[v] * cos) >> 16;
                        vertexZ[v] = (vertexZ[v] * cos - vertexX[v] * sin) >> 16;
                        vertexX[v] = x_;
                    }

                    vertexX[v] += baseX;
                    vertexY[v] += baseY;
                    vertexZ[v] += baseZ;
                }
            }
        }
        else if (type == AnimBase.OP_SCALE)
        {
            for (var g = 0; g < labelCount; g++)
            {
                var label = labels[g];
                if (label >= labelVertices.Length) continue;

                var vertices = labelVertices[label];
                for (var i = 0; i < vertices.Length; i++)
                {
                    var v = vertices[i];

                    vertexX[v] -= baseX;
                    vertexY[v] -= baseY;
                    vertexZ[v] -= baseZ;

                    vertexX[v] = vertexX[v] * x / 128;
                    vertexY[v] = vertexY[v] * y / 128;
                    vertexZ[v] = vertexZ[v] * z / 128;

                    vertexX[v] += baseX;
                    vertexY[v] += baseY;
                    vertexZ[v] += baseZ;
                }
            }
        }
        else if (type == AnimBase.OP_ALPHA && labelFaces != null && faceAlpha != null)
        {
            for (var g = 0; g < labelCount; g++)
            {
                var label = labels[g];
                if (label >= labelFaces.Length) continue;

                var triangles = labelFaces[label];
                for (var i = 0; i < triangles.Length; i++)
                {
                    var t = triangles[i];

                    faceAlpha[t] += x * 8;
                    if (faceAlpha[t] < 0) faceAlpha[t] = 0;

                    if (faceAlpha[t] > 255) faceAlpha[t] = 255;
                }
            }
        }
    }

    public void rotateY90()
    {
        for (var v = 0; v < vertexCount; v++)
        {
            var tmp = vertexX[v];
            vertexX[v] = vertexZ[v];
            vertexZ[v] = -tmp;
        }
    }

    public void rotateX(int angle)
    {
        var sin = Model.sin[angle];
        var cos = Model.cos[angle];
        for (var v = 0; v < vertexCount; v++)
        {
            var tmp = (vertexY[v] * cos - vertexZ[v] * sin) >> 16;
            vertexZ[v] = (vertexY[v] * sin + vertexZ[v] * cos) >> 16;
            vertexY[v] = tmp;
        }
    }

    public void translate(int y, int x, int z)
    {
        for (var v = 0; v < vertexCount; v++)
        {
            vertexX[v] += x;
            vertexY[v] += y;
            vertexZ[v] += z;
        }
    }

    public void recolor(int src, int dst)
    {
        for (var f = 0; f < faceCount; f++)
            if (faceColor[f] == src)
                faceColor[f] = dst;
    }

    public void rotateY180()
    {
        for (var v = 0; v < vertexCount; v++) vertexZ[v] = -vertexZ[v];

        for (var f = 0; f < faceCount; f++)
        {
            var temp = faceVertexA[f];
            faceVertexA[f] = faceVertexC[f];
            faceVertexC[f] = temp;
        }
    }

    public void scale(int x, int y, int z)
    {
        for (var v = 0; v < vertexCount; v++)
        {
            vertexX[v] = vertexX[v] * x / 128;
            vertexY[v] = vertexY[v] * y / 128;
            vertexZ[v] = vertexZ[v] * z / 128;
        }
    }

    public void calculateNormals(int lightAmbient, int lightAttenuation, int lightSrcX, int lightSrcY, int lightSrcZ,
        bool applyLighting)
    {
        var lightMagnitude = (int)Math.Sqrt(lightSrcX * lightSrcX + lightSrcY * lightSrcY + lightSrcZ * lightSrcZ);
        var attenuation = (lightAttenuation * lightMagnitude) >> 8;

        if (faceColorA == null)
        {
            faceColorA = new int[faceCount];
            faceColorB = new int[faceCount];
            faceColorC = new int[faceCount];
        }

        if (vertexNormal == null)
        {
            vertexNormal = new VertexNormal[vertexCount];
            for (var v = 0; v < vertexCount; v++) vertexNormal[v] = new VertexNormal();
        }

        for (var f = 0; f < faceCount; f++)
        {
            var a = faceVertexA[f];
            var b = faceVertexB[f];
            var c = faceVertexC[f];

            var dxAB = vertexX[b] - vertexX[a];
            var dyAB = vertexY[b] - vertexY[a];
            var dzAB = vertexZ[b] - vertexZ[a];

            var dxAC = vertexX[c] - vertexX[a];
            var dyAC = vertexY[c] - vertexY[a];
            var dzAC = vertexZ[c] - vertexZ[a];

            var nx = dyAB * dzAC - dyAC * dzAB;
            var ny = dzAB * dxAC - dzAC * dxAB;
            int nz;
            for (nz = dxAB * dyAC - dxAC * dyAB;
                 nx > 8192 || ny > 8192 || nz > 8192 || nx < -8192 || ny < -8192 || nz < -8192;
                 nz >>= 0x1)
            {
                nx >>= 0x1;
                ny >>= 0x1;
            }

            var length = (int)Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (length <= 0) length = 1;

            nx = nx * 256 / length;
            ny = ny * 256 / length;
            nz = nz * 256 / length;

            if (faceInfo == null || (faceInfo[f] & 0x1) == 0)
            {
                var n = vertexNormal[a];
                n.x += nx;
                n.y += ny;
                n.z += nz;
                n.w++;

                n = vertexNormal[b];
                n.x += nx;
                n.y += ny;
                n.z += nz;
                n.w++;

                n = vertexNormal[c];
                n.x += nx;
                n.y += ny;
                n.z += nz;
                n.w++;
            }
            else
            {
                var lightness = lightAmbient + (lightSrcX * nx + lightSrcY * ny + lightSrcZ * nz) /
                    (attenuation + attenuation / 2);
                faceColorA[f] = mulColorLightness(faceColor[f], lightness, faceInfo[f]);
            }
        }

        if (applyLighting)
        {
            this.applyLighting(lightAmbient, attenuation, lightSrcX, lightSrcY, lightSrcZ);
        }
        else
        {
            vertexNormalOriginal = new VertexNormal[vertexCount];
            for (var v = 0; v < vertexCount; v++)
            {
                var normal = vertexNormal[v];
                var copy = vertexNormalOriginal[v] = new VertexNormal();
                copy.x = normal.x;
                copy.y = normal.y;
                copy.z = normal.z;
                copy.w = normal.w;
            }
        }

        if (applyLighting)
            calculateBoundsCylinder();
        else
            calculateBoundsAABB();
    }

    public void applyLighting(int lightAmbient, int lightAttenuation, int lightSrcX, int lightSrcY, int lightSrcZ)
    {
        for (var f = 0; f < faceCount; f++)
        {
            var a = faceVertexA[f];
            var b = faceVertexB[f];
            var c = faceVertexC[f];

            if (faceInfo == null)
            {
                var color = faceColor[f];

                var n = vertexNormal[a];
                var lightness = lightAmbient +
                                (lightSrcX * n.x + lightSrcY * n.y + lightSrcZ * n.z) / (lightAttenuation * n.w);
                faceColorA[f] = mulColorLightness(color, lightness, 0);

                n = vertexNormal[b];
                lightness = lightAmbient +
                            (lightSrcX * n.x + lightSrcY * n.y + lightSrcZ * n.z) / (lightAttenuation * n.w);
                faceColorB[f] = mulColorLightness(color, lightness, 0);

                n = vertexNormal[c];
                lightness = lightAmbient +
                            (lightSrcX * n.x + lightSrcY * n.y + lightSrcZ * n.z) / (lightAttenuation * n.w);
                faceColorC[f] = mulColorLightness(color, lightness, 0);
            }
            else if ((faceInfo[f] & 0x1) == 0)
            {
                var color = faceColor[f];
                var info = faceInfo[f];

                var n = vertexNormal[a];
                var lightness = lightAmbient +
                                (lightSrcX * n.x + lightSrcY * n.y + lightSrcZ * n.z) / (lightAttenuation * n.w);
                faceColorA[f] = mulColorLightness(color, lightness, info);

                n = vertexNormal[b];
                lightness = lightAmbient +
                            (lightSrcX * n.x + lightSrcY * n.y + lightSrcZ * n.z) / (lightAttenuation * n.w);
                faceColorB[f] = mulColorLightness(color, lightness, info);

                n = vertexNormal[c];
                lightness = lightAmbient +
                            (lightSrcX * n.x + lightSrcY * n.y + lightSrcZ * n.z) / (lightAttenuation * n.w);
                faceColorC[f] = mulColorLightness(color, lightness, info);
            }
        }

        vertexNormal = null;
        vertexNormalOriginal = null;
        vertexLabel = null;
        faceLabel = null;

        if (faceInfo != null)
            for (var f = 0; f < faceCount; f++)
                if ((faceInfo[f] & 0x2) == 2)
                    return;

        faceColor = null;
    }

    public void drawSimple(int pitch, int yaw, int roll, int eyePitch, int eyeX, int eyeY, int eyeZ)
    {
        var centerX = Pix3D.centerW3D;
        var centerY = Pix3D.centerH3D;
        var sinPitch = sin[pitch];
        var cosPitch = cos[pitch];
        var sinYaw = sin[yaw];
        var cosYaw = cos[yaw];
        var sinRoll = sin[roll];
        var cosRoll = cos[roll];
        var sinEyePitch = sin[eyePitch];
        var cosEyePitch = cos[eyePitch];
        var midZ = (eyeY * sinEyePitch + eyeZ * cosEyePitch) >> 16;

        for (var v = 0; v < vertexCount; v++)
        {
            var x = vertexX[v];
            var y = vertexY[v];
            var z = vertexZ[v];

            int temp;
            if (roll != 0)
            {
                temp = (y * sinRoll + x * cosRoll) >> 16;
                y = (y * cosRoll - x * sinRoll) >> 16;
                x = temp;
            }

            if (pitch != 0)
            {
                temp = (y * cosPitch - z * sinPitch) >> 16;
                z = (y * sinPitch + z * cosPitch) >> 16;
                y = temp;
            }

            if (yaw != 0)
            {
                temp = (z * sinYaw + x * cosYaw) >> 16;
                z = (z * cosYaw - x * sinYaw) >> 16;
                x = temp;
            }

            x += eyeX;
            y += eyeY;
            z += eyeZ;

            temp = (y * cosEyePitch - z * sinEyePitch) >> 16;
            z = (y * sinEyePitch + z * cosEyePitch) >> 16;

            vertexScreenZ[v] = z - midZ;
            vertexScreenX[v] = centerX + (x << 9) / z;
            vertexScreenY[v] = centerY + (temp << 9) / z;

            if (texturedFaceCount > 0)
            {
                vertexViewSpaceX[v] = x;
                vertexViewSpaceY[v] = temp;
                vertexViewSpaceZ[v] = z;
            }
        }

        try
        {
            draw(false, false, 0);
        }
        catch (Exception ex)
        {
        }
    }

    public void draw(int yaw, int sinEyePitch, int cosEyePitch, int sinEyeYaw, int cosEyeYaw, int relativeX,
        int relativeY, int relativeZ, int bitset)
    {
        var zPrime = (relativeZ * cosEyeYaw - relativeX * sinEyeYaw) >> 16;
        var midZ = (relativeY * sinEyePitch + zPrime * cosEyePitch) >> 16;
        var radiusCosEyePitch = (radius * cosEyePitch) >> 16;

        var maxZ = midZ + radiusCosEyePitch;
        if (maxZ <= 50 || midZ >= 3500) return;

        var midX = (relativeZ * sinEyeYaw + relativeX * cosEyeYaw) >> 16;
        var leftX = (midX - radius) << 9;
        if (leftX / maxZ >= Pix2D.centerW2D) return;

        var rightX = (midX + radius) << 9;
        if (rightX / maxZ <= -Pix2D.centerW2D) return;

        var midY = (relativeY * cosEyePitch - zPrime * sinEyePitch) >> 16;
        var radiusSinEyePitch = (radius * sinEyePitch) >> 16;

        var bottomY = (midY + radiusSinEyePitch) << 9;
        if (bottomY / maxZ <= -Pix2D.centerH2D) return;

        var yPrime = radiusSinEyePitch + ((maxY * cosEyePitch) >> 16);
        var topY = (midY - yPrime) << 9;
        if (topY / maxZ >= Pix2D.centerH2D) return;

        var radiusZ = radiusCosEyePitch + ((maxY * sinEyePitch) >> 16);

        var clipped = midZ - radiusZ <= 50;
        var picking = false;

        if (bitset > 0 && checkHover)
        {
            var z = midZ - radiusCosEyePitch;
            if (z <= 50) z = 50;

            if (midX > 0)
            {
                leftX /= maxZ;
                rightX /= z;
            }
            else
            {
                rightX /= maxZ;
                leftX /= z;
            }

            if (midY > 0)
            {
                topY /= maxZ;
                bottomY /= z;
            }
            else
            {
                bottomY /= maxZ;
                topY /= z;
            }

            var mouseX = Model.mouseX - Pix3D.centerW3D;
            var mouseY = mouseZ - Pix3D.centerH3D;
            if (mouseX > leftX && mouseX < rightX && mouseY > topY && mouseY < bottomY)
            {
                if (pickable)
                    pickedBitsets[pickedCount++] = bitset;
                else
                    picking = true;
            }
        }

        var centerX = Pix3D.centerW3D;
        var centerY = Pix3D.centerH3D;

        var sinYaw = 0;
        var cosYaw = 0;
        if (yaw != 0)
        {
            sinYaw = sin[yaw];
            cosYaw = cos[yaw];
        }

        for (var v = 0; v < vertexCount; v++)
        {
            var x = vertexX[v];
            var y = vertexY[v];
            var z = vertexZ[v];

            int temp;
            if (yaw != 0)
            {
                temp = (z * sinYaw + x * cosYaw) >> 16;
                z = (z * cosYaw - x * sinYaw) >> 16;
                x = temp;
            }

            x += relativeX;
            y += relativeY;
            z += relativeZ;

            temp = (z * sinEyeYaw + x * cosEyeYaw) >> 16;
            z = (z * cosEyeYaw - x * sinEyeYaw) >> 16;
            x = temp;

            temp = (y * cosEyePitch - z * sinEyePitch) >> 16;
            z = (y * sinEyePitch + z * cosEyePitch) >> 16;

            vertexScreenZ[v] = z - midZ;

            if (z >= 50)
            {
                vertexScreenX[v] = centerX + (x << 9) / z;
                vertexScreenY[v] = centerY + (temp << 9) / z;
            }
            else
            {
                vertexScreenX[v] = -5000;
                clipped = true;
            }

            if (clipped || texturedFaceCount > 0)
            {
                vertexViewSpaceX[v] = x;
                vertexViewSpaceY[v] = temp;
                vertexViewSpaceZ[v] = z;
            }
        }

        try
        {
            draw(clipped, picking, bitset);
        }
        catch (Exception ex)
        {
        }
    }

    private void draw(bool clipped, bool picking, int bitset)
    {
        for (var depth = 0; depth < maxDepth; depth++) tmpDepthFaceCount[depth] = 0;

        for (var f = 0; f < this.faceCount; f++)
            if (faceInfo == null || faceInfo[f] != -1)
            {
                var a = faceVertexA[f];
                var b = faceVertexB[f];
                var c = faceVertexC[f];

                var xA = vertexScreenX[a];
                var xB = vertexScreenX[b];
                var xC = vertexScreenX[c];

                if (clipped && (xA == -5000 || xB == -5000 || xC == -5000))
                {
                    faceNearClipped[f] = true;
                    var depthAverage = (vertexScreenZ[a] + vertexScreenZ[b] + vertexScreenZ[c]) / 3 + minDepth;
                    tmpDepthFaces[depthAverage][tmpDepthFaceCount[depthAverage]++] = f;
                }
                else
                {
                    if (picking && pointWithinTriangle(mouseX, mouseZ, vertexScreenY[a], vertexScreenY[b],
                            vertexScreenY[c], xA, xB, xC))
                    {
                        pickedBitsets[pickedCount++] = bitset;
                        picking = false;
                    }

                    if ((xA - xB) * (vertexScreenY[c] - vertexScreenY[b]) -
                        (vertexScreenY[a] - vertexScreenY[b]) * (xC - xB) > 0)
                    {
                        faceNearClipped[f] = false;
                        faceClippedX[f] = xA < 0 || xB < 0 || xC < 0 || xA > Pix2D.safeWidth || xB > Pix2D.safeWidth ||
                                          xC > Pix2D.safeWidth;
                        var depthAverage = (vertexScreenZ[a] + vertexScreenZ[b] + vertexScreenZ[c]) / 3 + minDepth;
                        tmpDepthFaces[depthAverage][tmpDepthFaceCount[depthAverage]++] = f;
                    }
                }
            }

        if (facePriority == null)
        {
            for (var depth = maxDepth - 1; depth >= 0; depth--)
            {
                var count = tmpDepthFaceCount[depth];
                if (count > 0)
                {
                    var faces = tmpDepthFaces[depth];
                    for (var f = 0; f < count; f++) drawFace(faces[f]);
                }
            }

            return;
        }

        for (var priority = 0; priority < 12; priority++)
        {
            tmpPriorityFaceCount[priority] = 0;
            tmpPriorityDepthSum[priority] = 0;
        }

        for (var depth = maxDepth - 1; depth >= 0; depth--)
        {
            var faceCount = tmpDepthFaceCount[depth];
            if (faceCount > 0)
            {
                var faces = tmpDepthFaces[depth];
                for (var i = 0; i < faceCount; i++)
                {
                    var _priorityDepth = faces[i];
                    var _priorityFace = facePriority[_priorityDepth];
                    var _priorityFaceCount = tmpPriorityFaceCount[_priorityFace]++;
                    tmpPriorityFaces[_priorityFace][_priorityFaceCount] = _priorityDepth;
                    if (_priorityFace < 10)
                        tmpPriorityDepthSum[_priorityFace] += depth;
                    else if (_priorityFace == 10)
                        tmpPriority10FaceDepth[_priorityFaceCount] = depth;
                    else
                        tmpPriority11FaceDepth[_priorityFaceCount] = depth;
                }
            }
        }

        var averagePriorityDepthSum1_2 = 0;
        if (tmpPriorityFaceCount[1] > 0 || tmpPriorityFaceCount[2] > 0)
            averagePriorityDepthSum1_2 = (tmpPriorityDepthSum[1] + tmpPriorityDepthSum[2]) /
                                         (tmpPriorityFaceCount[1] + tmpPriorityFaceCount[2]);
        var averagePriorityDepthSum3_4 = 0;
        if (tmpPriorityFaceCount[3] > 0 || tmpPriorityFaceCount[4] > 0)
            averagePriorityDepthSum3_4 = (tmpPriorityDepthSum[3] + tmpPriorityDepthSum[4]) /
                                         (tmpPriorityFaceCount[3] + tmpPriorityFaceCount[4]);
        var averagePriorityDepthSum6_8 = 0;
        if (tmpPriorityFaceCount[6] > 0 || tmpPriorityFaceCount[8] > 0)
            averagePriorityDepthSum6_8 = (tmpPriorityDepthSum[6] + tmpPriorityDepthSum[8]) /
                                         (tmpPriorityFaceCount[6] + tmpPriorityFaceCount[8]);

        var priorityFace = 0;
        var priorityFaceCount = tmpPriorityFaceCount[10];

        var priorityFaces = tmpPriorityFaces[10];
        var priorithFaceDepths = tmpPriority10FaceDepth;
        if (priorityFace == priorityFaceCount)
        {
            priorityFace = 0;
            priorityFaceCount = tmpPriorityFaceCount[11];
            priorityFaces = tmpPriorityFaces[11];
            priorithFaceDepths = tmpPriority11FaceDepth;
        }

        int priorityDepth;
        if (priorityFace < priorityFaceCount)
            priorityDepth = priorithFaceDepths[priorityFace];
        else
            priorityDepth = -1000;

        for (var priority = 0; priority < 10; priority++)
        {
            while (priority == 0 && priorityDepth > averagePriorityDepthSum1_2)
            {
                drawFace(priorityFaces[priorityFace++]);

                if (priorityFace == priorityFaceCount && priorityFaces != tmpPriorityFaces[11])
                {
                    priorityFace = 0;
                    priorityFaceCount = tmpPriorityFaceCount[11];
                    priorityFaces = tmpPriorityFaces[11];
                    priorithFaceDepths = tmpPriority11FaceDepth;
                }

                if (priorityFace < priorityFaceCount)
                    priorityDepth = priorithFaceDepths[priorityFace];
                else
                    priorityDepth = -1000;
            }

            while (priority == 3 && priorityDepth > averagePriorityDepthSum3_4)
            {
                drawFace(priorityFaces[priorityFace++]);

                if (priorityFace == priorityFaceCount && priorityFaces != tmpPriorityFaces[11])
                {
                    priorityFace = 0;
                    priorityFaceCount = tmpPriorityFaceCount[11];
                    priorityFaces = tmpPriorityFaces[11];
                    priorithFaceDepths = tmpPriority11FaceDepth;
                }

                if (priorityFace < priorityFaceCount)
                    priorityDepth = priorithFaceDepths[priorityFace];
                else
                    priorityDepth = -1000;
            }

            while (priority == 5 && priorityDepth > averagePriorityDepthSum6_8)
            {
                drawFace(priorityFaces[priorityFace++]);

                if (priorityFace == priorityFaceCount && priorityFaces != tmpPriorityFaces[11])
                {
                    priorityFace = 0;
                    priorityFaceCount = tmpPriorityFaceCount[11];
                    priorityFaces = tmpPriorityFaces[11];
                    priorithFaceDepths = tmpPriority11FaceDepth;
                }

                if (priorityFace < priorityFaceCount)
                    priorityDepth = priorithFaceDepths[priorityFace];
                else
                    priorityDepth = -1000;
            }

            var count = tmpPriorityFaceCount[priority];
            var faces = tmpPriorityFaces[priority];
            for (var i = 0; i < count; i++) drawFace(faces[i]);
        }

        while (priorityDepth != -1000)
        {
            drawFace(priorityFaces[priorityFace++]);

            if (priorityFace == priorityFaceCount && priorityFaces != tmpPriorityFaces[11])
            {
                priorityFace = 0;
                priorityFaces = tmpPriorityFaces[11];
                priorityFaceCount = tmpPriorityFaceCount[11];
                priorithFaceDepths = tmpPriority11FaceDepth;
            }

            if (priorityFace < priorityFaceCount)
                priorityDepth = priorithFaceDepths[priorityFace];
            else
                priorityDepth = -1000;
        }
    }

    private void drawFace(int face)
    {
        if (faceNearClipped[face])
        {
            drawNearClippedFace(face);
            return;
        }

        var a = faceVertexA[face];
        var b = faceVertexB[face];
        var c = faceVertexC[face];

        Pix3D.hclip = faceClippedX[face];

        if (faceAlpha == null)
            Pix3D.trans = 0;
        else
            Pix3D.trans = faceAlpha[face];

        int type;
        if (faceInfo == null)
            type = 0;
        else
            type = faceInfo[face] & 0x3;

        if (type == 0)
        {
            Pix3D.gouraudTriangle(vertexScreenX[a], vertexScreenX[b], vertexScreenX[c], vertexScreenY[a],
                vertexScreenY[b], vertexScreenY[c], faceColorA[face], faceColorB[face], faceColorC[face]);
        }
        else if (type == 1)
        {
            Pix3D.flatTriangle(vertexScreenX[a], vertexScreenX[b], vertexScreenX[c], vertexScreenY[a], vertexScreenY[b],
                vertexScreenY[c], palette[faceColorA[face]]);
        }
        else if (type == 2)
        {
            var texturedFace = faceInfo[face] >> 2;
            var tA = texturedVertexA[texturedFace];
            var tB = texturedVertexB[texturedFace];
            var tC = texturedVertexC[texturedFace];
            Pix3D.textureTriangle(vertexScreenX[a], vertexScreenX[b], vertexScreenX[c], vertexScreenY[a],
                vertexScreenY[b], vertexScreenY[c], faceColorA[face], faceColorB[face], faceColorC[face],
                vertexViewSpaceX[tA], vertexViewSpaceY[tA], vertexViewSpaceZ[tA], vertexViewSpaceX[tB],
                vertexViewSpaceX[tC], vertexViewSpaceY[tB], vertexViewSpaceY[tC], vertexViewSpaceZ[tB],
                vertexViewSpaceZ[tC], faceColor[face]);
        }
        else if (type == 3)
        {
            var texturedFace = faceInfo[face] >> 2;
            var tA = texturedVertexA[texturedFace];
            var tB = texturedVertexB[texturedFace];
            var tC = texturedVertexC[texturedFace];
            Pix3D.textureTriangle(vertexScreenX[a], vertexScreenX[b], vertexScreenX[c], vertexScreenY[a],
                vertexScreenY[b], vertexScreenY[c], faceColorA[face], faceColorA[face], faceColorA[face],
                vertexViewSpaceX[tA], vertexViewSpaceY[tA], vertexViewSpaceZ[tA], vertexViewSpaceX[tB],
                vertexViewSpaceX[tC], vertexViewSpaceY[tB], vertexViewSpaceY[tC], vertexViewSpaceZ[tB],
                vertexViewSpaceZ[tC], faceColor[face]);
        }
    }

    private void drawNearClippedFace(int face)
    {
        var centerX = Pix3D.centerW3D;
        var centerY = Pix3D.centerH3D;
        var elements = 0;

        var a = faceVertexA[face];
        var b = faceVertexB[face];
        var c = faceVertexC[face];

        var zA = vertexViewSpaceZ[a];
        var zB = vertexViewSpaceZ[b];
        var zC = vertexViewSpaceZ[c];

        if (zA >= 50)
        {
            clippedX[elements] = vertexScreenX[a];
            clippedY[elements] = vertexScreenY[a];
            clippedColor[elements++] = faceColorA[face];
        }
        else
        {
            var xA = vertexViewSpaceX[a];
            var yA = vertexViewSpaceY[a];
            var colorA = faceColorA[face];

            if (zC >= 50)
            {
                var scalar = (50 - zA) * reciprical16[zC - zA];
                clippedX[elements] = centerX + ((xA + (((vertexViewSpaceX[c] - xA) * scalar) >> 16)) << 9) / 50;
                clippedY[elements] = centerY + ((yA + (((vertexViewSpaceY[c] - yA) * scalar) >> 16)) << 9) / 50;
                clippedColor[elements++] = colorA + (((faceColorC[face] - colorA) * scalar) >> 16);
            }

            if (zB >= 50)
            {
                var scalar = (50 - zA) * reciprical16[zB - zA];
                clippedX[elements] = centerX + ((xA + (((vertexViewSpaceX[b] - xA) * scalar) >> 16)) << 9) / 50;
                clippedY[elements] = centerY + ((yA + (((vertexViewSpaceY[b] - yA) * scalar) >> 16)) << 9) / 50;
                clippedColor[elements++] = colorA + (((faceColorB[face] - colorA) * scalar) >> 16);
            }
        }

        if (zB >= 50)
        {
            clippedX[elements] = vertexScreenX[b];
            clippedY[elements] = vertexScreenY[b];
            clippedColor[elements++] = faceColorB[face];
        }
        else
        {
            var xB = vertexViewSpaceX[b];
            var yB = vertexViewSpaceY[b];
            var colorB = faceColorB[face];

            if (zA >= 50)
            {
                var scalar = (50 - zB) * reciprical16[zA - zB];
                clippedX[elements] = centerX + ((xB + (((vertexViewSpaceX[a] - xB) * scalar) >> 16)) << 9) / 50;
                clippedY[elements] = centerY + ((yB + (((vertexViewSpaceY[a] - yB) * scalar) >> 16)) << 9) / 50;
                clippedColor[elements++] = colorB + (((faceColorA[face] - colorB) * scalar) >> 16);
            }

            if (zC >= 50)
            {
                var scalar = (50 - zB) * reciprical16[zC - zB];
                clippedX[elements] = centerX + ((xB + (((vertexViewSpaceX[c] - xB) * scalar) >> 16)) << 9) / 50;
                clippedY[elements] = centerY + ((yB + (((vertexViewSpaceY[c] - yB) * scalar) >> 16)) << 9) / 50;
                clippedColor[elements++] = colorB + (((faceColorC[face] - colorB) * scalar) >> 16);
            }
        }

        if (zC >= 50)
        {
            clippedX[elements] = vertexScreenX[c];
            clippedY[elements] = vertexScreenY[c];
            clippedColor[elements++] = faceColorC[face];
        }
        else
        {
            var xC = vertexViewSpaceX[c];
            var yC = vertexViewSpaceY[c];
            var colorC = faceColorC[face];

            if (zB >= 50)
            {
                var scalar = (50 - zC) * reciprical16[zB - zC];
                clippedX[elements] = centerX + ((xC + (((vertexViewSpaceX[b] - xC) * scalar) >> 16)) << 9) / 50;
                clippedY[elements] = centerY + ((yC + (((vertexViewSpaceY[b] - yC) * scalar) >> 16)) << 9) / 50;
                clippedColor[elements++] = colorC + (((faceColorB[face] - colorC) * scalar) >> 16);
            }

            if (zA >= 50)
            {
                var scalar = (50 - zC) * reciprical16[zA - zC];
                clippedX[elements] = centerX + ((xC + (((vertexViewSpaceX[a] - xC) * scalar) >> 16)) << 9) / 50;
                clippedY[elements] = centerY + ((yC + (((vertexViewSpaceY[a] - yC) * scalar) >> 16)) << 9) / 50;
                clippedColor[elements++] = colorC + (((faceColorA[face] - colorC) * scalar) >> 16);
            }
        }

        var x0 = clippedX[0];
        var x1 = clippedX[1];
        var x2 = clippedX[2];
        var y0 = clippedY[0];
        var y1 = clippedY[1];
        var y2 = clippedY[2];

        if ((x0 - x1) * (y2 - y1) - (y0 - y1) * (x2 - x1) <= 0) return;

        Pix3D.hclip = false;

        if (elements == 3)
        {
            if (x0 < 0 || x1 < 0 || x2 < 0 || x0 > Pix2D.safeWidth || x1 > Pix2D.safeWidth || x2 > Pix2D.safeWidth)
                Pix3D.hclip = true;

            int type;
            if (faceInfo == null)
                type = 0;
            else
                type = faceInfo[face] & 0x3;

            if (type == 0)
            {
                Pix3D.gouraudTriangle(x0, x1, x2, y0, y1, y2, clippedColor[0], clippedColor[1], clippedColor[2]);
            }
            else if (type == 1)
            {
                Pix3D.flatTriangle(x0, x1, x2, y0, y1, y2, palette[faceColorA[face]]);
            }
            else if (type == 2)
            {
                var texturedFace = faceInfo[face] >> 2;
                var tA = texturedVertexA[texturedFace];
                var tB = texturedVertexB[texturedFace];
                var tC = texturedVertexC[texturedFace];
                Pix3D.textureTriangle(x0, x1, x2, y0, y1, y2, clippedColor[0], clippedColor[1], clippedColor[2],
                    vertexViewSpaceX[tA], vertexViewSpaceY[tA], vertexViewSpaceZ[tA], vertexViewSpaceX[tB],
                    vertexViewSpaceX[tC], vertexViewSpaceY[tB], vertexViewSpaceY[tC], vertexViewSpaceZ[tB],
                    vertexViewSpaceZ[tC], faceColor[face]);
            }
            else if (type == 3)
            {
                var texturedFace = faceInfo[face] >> 2;
                var tA = texturedVertexA[texturedFace];
                var tB = texturedVertexB[texturedFace];
                var tC = texturedVertexC[texturedFace];
                Pix3D.textureTriangle(x0, x1, x2, y0, y1, y2, faceColorA[face], faceColorA[face], faceColorA[face],
                    vertexViewSpaceX[tA], vertexViewSpaceY[tA], vertexViewSpaceZ[tA], vertexViewSpaceX[tB],
                    vertexViewSpaceX[tC], vertexViewSpaceY[tB], vertexViewSpaceY[tC], vertexViewSpaceZ[tB],
                    vertexViewSpaceZ[tC], faceColor[face]);
            }
        }
        else if (elements == 4)
        {
            if (x0 < 0 || x1 < 0 || x2 < 0 || x0 > Pix2D.safeWidth || x1 > Pix2D.safeWidth || x2 > Pix2D.safeWidth ||
                clippedX[3] < 0 || clippedX[3] > Pix2D.safeWidth) Pix3D.hclip = true;

            int type;
            if (faceInfo == null)
                type = 0;
            else
                type = faceInfo[face] & 0x3;

            if (type == 0)
            {
                Pix3D.gouraudTriangle(x0, x1, x2, y0, y1, y2, clippedColor[0], clippedColor[1], clippedColor[2]);
                Pix3D.gouraudTriangle(x0, x2, clippedX[3], y0, y2, clippedY[3], clippedColor[0], clippedColor[2],
                    clippedColor[3]);
            }
            else if (type == 1)
            {
                var colorA = palette[faceColorA[face]];
                Pix3D.flatTriangle(x0, x1, x2, y0, y1, y2, colorA);
                Pix3D.flatTriangle(x0, x2, clippedX[3], y0, y2, clippedY[3], colorA);
            }
            else if (type == 2)
            {
                var texturedFace = faceInfo[face] >> 2;
                var tA = texturedVertexA[texturedFace];
                var tB = texturedVertexB[texturedFace];
                var tC = texturedVertexC[texturedFace];
                Pix3D.textureTriangle(x0, x1, x2, y0, y1, y2, clippedColor[0], clippedColor[1], clippedColor[2],
                    vertexViewSpaceX[tA], vertexViewSpaceY[tA], vertexViewSpaceZ[tA], vertexViewSpaceX[tB],
                    vertexViewSpaceX[tC], vertexViewSpaceY[tB], vertexViewSpaceY[tC], vertexViewSpaceZ[tB],
                    vertexViewSpaceZ[tC], faceColor[face]);
                Pix3D.textureTriangle(x0, x2, clippedX[3], y0, y2, clippedY[3], clippedColor[0], clippedColor[2],
                    clippedColor[3], vertexViewSpaceX[tA], vertexViewSpaceY[tA], vertexViewSpaceZ[tA],
                    vertexViewSpaceX[tB], vertexViewSpaceX[tC], vertexViewSpaceY[tB], vertexViewSpaceY[tC],
                    vertexViewSpaceZ[tB], vertexViewSpaceZ[tC], faceColor[face]);
            }
            else if (type == 3)
            {
                var texturedFace = faceInfo[face] >> 2;
                var tA = texturedVertexA[texturedFace];
                var tB = texturedVertexB[texturedFace];
                var tC = texturedVertexC[texturedFace];
                Pix3D.textureTriangle(x0, x1, x2, y0, y1, y2, faceColorA[face], faceColorA[face], faceColorA[face],
                    vertexViewSpaceX[tA], vertexViewSpaceY[tA], vertexViewSpaceZ[tA], vertexViewSpaceX[tB],
                    vertexViewSpaceX[tC], vertexViewSpaceY[tB], vertexViewSpaceY[tC], vertexViewSpaceZ[tB],
                    vertexViewSpaceZ[tC], faceColor[face]);
                Pix3D.textureTriangle(x0, x2, clippedX[3], y0, y2, clippedY[3], faceColorA[face], faceColorA[face],
                    faceColorA[face], vertexViewSpaceX[tA], vertexViewSpaceY[tA], vertexViewSpaceZ[tA],
                    vertexViewSpaceX[tB], vertexViewSpaceX[tC], vertexViewSpaceY[tB], vertexViewSpaceY[tC],
                    vertexViewSpaceZ[tB], vertexViewSpaceZ[tC], faceColor[face]);
            }
        }
    }

    private bool pointWithinTriangle(int x, int y, int yA, int yB, int yC, int xA, int xB, int xC)
    {
        if (y < yA && y < yB && y < yC)
            return false;
        if (y > yA && y > yB && y > yC)
            return false;
        if (x < xA && x < xB && x < xC)
            return false;
        return x <= xA || x <= xB || x <= xC;
    }
}