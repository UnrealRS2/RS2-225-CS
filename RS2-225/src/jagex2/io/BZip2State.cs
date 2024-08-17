namespace RS2_225.jagex2.io;

public class BZip2State
{
    public static int MTFA_SIZE = 4096;
    public static int MTFL_SIZE = 16;
    public static int BZ_MAX_ALPHA_SIZE = 258;
    public static int BZ_MAX_CODE_LEN = 23;
    private static int anInt732 = 1; // TODO
    public static int BZ_N_GROUPS = 6;
    private static readonly int BZ_G_SIZE = 50;
    private static int anInt735 = 4; // TODO
    private static readonly int BZ_MAX_SELECTORS = 2 + 900000 / BZ_G_SIZE; // 18002
    public static int BZ_RUNA = 0;
    public static int BZ_RUNB = 1;
    public static int[] tt;
    public int[][] _base;
    public int avail_in;
    public int avail_out;
    public bool blockRandomized;
    public int blockSize100k;
    public int bsBuff;
    public int bsLive;
    public int c_nblock_used;
    public int[] cftab = new int[257];
    private int[] cftabCopy = new int[257];
    public int currBlockNo;
    public byte[] decompressed;
    public bool[] inUse = new bool[256];
    public bool[] inUse16 = new bool[16];
    public int k0;
    public sbyte[][] len;
    public int[][] limit;
    public int[] minLens = new int[BZ_N_GROUPS];
    public sbyte[] mtfa = new sbyte[MTFA_SIZE];
    public int[] mtfbase = new int[256 / MTFL_SIZE];
    public int next_in;
    public int next_out;
    public int nInUse;
    public int origPtr;
    public int[][] perm;
    public int save_nblock;
    public sbyte[] selector = new sbyte[BZ_MAX_SELECTORS];
    public sbyte[] selectorMtf = new sbyte[BZ_MAX_SELECTORS];
    public sbyte[] seqToUnseq = new sbyte[256];
    public byte state_out_ch;
    public int state_out_len;
    public byte[] stream;
    public int total_in_hi32;
    public int total_in_lo32;
    public int total_out_hi32;
    public int total_out_lo32;
    public int tPos;
    public int[] unzftab = new int[256];

    public BZip2State()
    {
        len = new sbyte[BZ_N_GROUPS][];
        for (var i = 0; i < BZ_N_GROUPS; i++) len[i] = new sbyte[BZ_MAX_ALPHA_SIZE];

        limit = new int[BZ_N_GROUPS][];
        _base = new int[BZ_N_GROUPS][];
        perm = new int[BZ_N_GROUPS][];
        for (var i = 0; i < BZ_N_GROUPS; i++)
        {
            limit[i] = new int[BZ_MAX_ALPHA_SIZE];
            _base[i] = new int[BZ_MAX_ALPHA_SIZE];
            perm[i] = new int[BZ_MAX_ALPHA_SIZE];
        }
    }
}