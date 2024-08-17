namespace RS2_225.jagex2.io;

public class BZip2
{
    private static readonly BZip2State state = new();

    public static int read(byte[] decompressed, int length, byte[] stream, int avail_in, int next_in)
    {
        lock (state)
        {
            state.stream = stream;
            state.next_in = next_in;
            state.decompressed = decompressed;
            state.next_out = 0;
            state.avail_in = avail_in;
            state.avail_out = length;
            state.bsLive = 0;
            state.bsBuff = 0;
            state.total_in_lo32 = 0;
            state.total_in_hi32 = 0;
            state.total_out_lo32 = 0;
            state.total_out_hi32 = 0;
            state.currBlockNo = 0;
            decompress(state);
            return length - state.avail_out;
        }
    }

    public static void decompress(BZip2State s)
    {
        // libbzip2 uses these variables in a save area
        /*@Pc(3) boolean save_i = false;
        @Pc(5) boolean save_j = false;
        @Pc(7) boolean save_t = false;
        @Pc(9) boolean save_alphaSize = false;
        @Pc(11) boolean save_nGroups = false;
        @Pc(13) boolean save_nSelectors = false;
        @Pc(15) boolean save_EOB = false;
        @Pc(17) boolean save_groupNo = false;
        @Pc(19) boolean save_groupPos = false;
        @Pc(21) boolean save_nextSym = false;
        @Pc(23) boolean save_nblockMAX = false;
        @Pc(25) boolean save_nblock = false;
        @Pc(27) boolean save_es = false;
        @Pc(29) boolean save_N = false;
        @Pc(31) boolean save_curr = false;
        @Pc(33) boolean save_zt = false;
        @Pc(35) boolean save_zn = false;
        @Pc(37) boolean save_zvec = false;
        @Pc(39) boolean save_zj = false;*/

        var gMinlen = 0;
        int[] gLimit = null;
        int[] gBase = null;
        int[] gPerm = null;

        s.blockSize100k = 1;
        if (BZip2State.tt == null) BZip2State.tt = new int[s.blockSize100k * 100000];

        var reading = true;
        while (reading)
        {
            var uc = getUnsignedChar(s);
            if (uc == 0x17) return;

            // uc checks originally broke the loop and returned an error in libbzip2
            uc = getUnsignedChar(s);
            uc = getUnsignedChar(s);
            uc = getUnsignedChar(s);
            uc = getUnsignedChar(s);
            uc = getUnsignedChar(s);

            s.currBlockNo++;

            uc = getUnsignedChar(s);
            uc = getUnsignedChar(s);
            uc = getUnsignedChar(s);
            uc = getUnsignedChar(s);

            uc = getBit(s);
            s.blockRandomized = uc != 0;
            if (s.blockRandomized) Console.Error.WriteLine("PANIC! RANDOMISED BLOCK!");

            s.origPtr = 0;
            uc = getUnsignedChar(s);
            s.origPtr = (s.origPtr << 8) | (uc & 0xFF);
            uc = getUnsignedChar(s);
            s.origPtr = (s.origPtr << 8) | (uc & 0xFF);
            uc = getUnsignedChar(s);
            s.origPtr = (s.origPtr << 8) | (uc & 0xFF);

            // Receive the mapping table
            int i;
            for (i = 0; i < 16; i++)
            {
                uc = getBit(s);
                s.inUse16[i] = uc == 1;
            }

            for (i = 0; i < 256; i++) s.inUse[i] = false;

            int j;
            for (i = 0; i < 16; i++)
                if (s.inUse16[i])
                    for (j = 0; j < 16; j++)
                    {
                        uc = getBit(s);
                        if (uc == 1) s.inUse[i * 16 + j] = true;
                    }

            makeMaps(s);
            var alphaSize = s.nInUse + 2;

            var nGroups = getBits(3, s);
            var nSelectors = getBits(15, s);
            for (i = 0; i < nSelectors; i++)
            {
                j = 0;

                while (true)
                {
                    uc = getBit(s);
                    if (uc == 0) break;
                    j++;
                }

                s.selectorMtf[i] = (sbyte)j;
            }

            // Undo the MTF values for the selectors
            var pos = new sbyte[BZip2State.BZ_N_GROUPS];
            sbyte v;
            for (v = 0; v < nGroups; v++) pos[v] = v;

            for (i = 0; i < nSelectors; i++)
            {
                v = s.selectorMtf[i];
                var tmp = pos[v];
                while (v > 0)
                {
                    pos[v] = pos[v - 1];
                    v--;
                }

                pos[0] = tmp;
                s.selector[i] = tmp;
            }

            // Now the coding tables
            int t;
            for (t = 0; t < nGroups; t++)
            {
                var curr = getBits(5, s);

                for (i = 0; i < alphaSize; i++)
                {
                    while (true)
                    {
                        uc = getBit(s);
                        if (uc == 0) break;

                        uc = getBit(s);
                        if (uc == 0)
                            curr++;
                        else
                            curr--;
                    }

                    s.len[t][i] = (sbyte)curr;
                }
            }

            // Create the Huffman decoding tables
            for (t = 0; t < nGroups; t++)
            {
                sbyte minLen = 32;
                sbyte maxLen = 0;

                for (i = 0; i < alphaSize; i++)
                {
                    if (s.len[t][i] > maxLen) maxLen = s.len[t][i];

                    if (s.len[t][i] < minLen) minLen = s.len[t][i];
                }

                createDecodeTables(s.limit[t], s._base[t], s.perm[t], s.len[t], minLen, maxLen, alphaSize);
                s.minLens[t] = minLen;
            }

            // Now the MTF values
            var EOB = s.nInUse + 1;
            var nblockMAX = s.blockSize100k * 100000;
            var groupNo = -1;
            sbyte groupPos = 0;

            for (i = 0; i <= 255; i++) s.unzftab[i] = 0;

            // MTF init
            var kk = BZip2State.MTFA_SIZE - 1;
            for (var ii = 256 / BZip2State.MTFL_SIZE - 1; ii >= 0; ii--)
            {
                for (var jj = BZip2State.MTFL_SIZE - 1; jj >= 0; jj--)
                {
                    s.mtfa[kk] = (sbyte)(ii * BZip2State.MTFL_SIZE + jj);
                    kk--;
                }

                s.mtfbase[ii] = kk + 1;
            }
            // end MTF init

            var nblock = 0;

            // macro: GET_MTF_VAL
            sbyte gSel;
            if (groupPos == 0)
            {
                groupNo++;
                groupPos = 50;
                gSel = s.selector[groupNo];
                gMinlen = s.minLens[gSel];
                gLimit = s.limit[gSel];
                gPerm = s.perm[gSel];
                gBase = s._base[gSel];
            }

            var gPos = groupPos - 1;
            var zn = gMinlen;
            int zvec;
            sbyte zj;
            for (zvec = getBits(gMinlen, s); zvec > gLimit[zn]; zvec = (zvec << 1) | zj)
            {
                zn++;
                zj = getBit(s);
            }

            var nextSym = gPerm[zvec - gBase[zn]];
            while (nextSym != EOB)
                if (nextSym == BZip2State.BZ_RUNA || nextSym == BZip2State.BZ_RUNB)
                {
                    var es = -1;
                    var N = 1;

                    do
                    {
                        if (nextSym == BZip2State.BZ_RUNA)
                            es += N;
                        else if (nextSym == BZip2State.BZ_RUNB) es += N * 2;

                        N *= 2;
                        if (gPos == 0)
                        {
                            groupNo++;
                            gPos = 50;
                            gSel = s.selector[groupNo];
                            gMinlen = s.minLens[gSel];
                            gLimit = s.limit[gSel];
                            gPerm = s.perm[gSel];
                            gBase = s._base[gSel];
                        }

                        gPos--;
                        zn = gMinlen;
                        for (zvec = getBits(gMinlen, s); zvec > gLimit[zn]; zvec = (zvec << 1) | zj)
                        {
                            zn++;
                            zj = getBit(s);
                        }

                        nextSym = gPerm[zvec - gBase[zn]];
                    } while (nextSym == BZip2State.BZ_RUNA || nextSym == BZip2State.BZ_RUNB);

                    es++;
                    uc = s.seqToUnseq[s.mtfa[s.mtfbase[0]] & 0xFF];
                    s.unzftab[uc & 0xFF] += es;

                    while (es > 0)
                    {
                        BZip2State.tt[nblock] = uc & 0xFF;
                        nblock++;
                        es--;
                    }
                }
                else
                {
                    // uc = MTF ( nextSym-1 )
                    var nn = nextSym - 1;
                    int pp;

                    if (nn < BZip2State.MTFL_SIZE)
                    {
                        // avoid general-case expense
                        pp = s.mtfbase[0];
                        uc = s.mtfa[pp + nn];

                        while (nn > 3)
                        {
                            var z = pp + nn;
                            s.mtfa[z] = s.mtfa[z - 1];
                            s.mtfa[z - 1] = s.mtfa[z - 2];
                            s.mtfa[z - 2] = s.mtfa[z - 3];
                            s.mtfa[z - 3] = s.mtfa[z - 4];
                            nn -= 4;
                        }

                        while (nn > 0)
                        {
                            s.mtfa[pp + nn] = s.mtfa[pp + nn - 1];
                            nn--;
                        }

                        s.mtfa[pp] = uc;
                    }
                    else
                    {
                        // general case
                        var lno = nn / BZip2State.MTFL_SIZE;
                        var off = nn % BZip2State.MTFL_SIZE;

                        pp = s.mtfbase[lno] + off;
                        uc = s.mtfa[pp];

                        while (pp > s.mtfbase[lno])
                        {
                            s.mtfa[pp] = s.mtfa[pp - 1];
                            pp--;
                        }

                        s.mtfbase[lno]++;

                        while (lno > 0)
                        {
                            s.mtfbase[lno]--;
                            s.mtfa[s.mtfbase[lno]] = s.mtfa[s.mtfbase[lno - 1] + 16 - 1];
                            lno--;
                        }

                        s.mtfbase[0]--;
                        s.mtfa[s.mtfbase[0]] = uc;

                        if (s.mtfbase[0] == 0)
                        {
                            kk = BZip2State.MTFA_SIZE - 1;
                            for (var ii = 256 / BZip2State.MTFL_SIZE - 1; ii >= 0; ii--)
                            {
                                for (var jj = BZip2State.MTFL_SIZE - 1; jj >= 0; jj--)
                                {
                                    s.mtfa[kk] = s.mtfa[s.mtfbase[ii] + jj];
                                    kk--;
                                }

                                s.mtfbase[ii] = kk + 1;
                            }
                        }
                    }

                    // end uc = MTF ( nextSym-1 )
                    s.unzftab[s.seqToUnseq[uc & 0xFF] & 0xFF]++;
                    BZip2State.tt[nblock] = s.seqToUnseq[uc & 0xFF] & 0xFF;
                    nblock++;

                    // macro: GET_MTF_VAL
                    if (gPos == 0)
                    {
                        groupNo++;
                        gPos = 50;
                        gSel = s.selector[groupNo];
                        gMinlen = s.minLens[gSel];
                        gLimit = s.limit[gSel];
                        gPerm = s.perm[gSel];
                        gBase = s._base[gSel];
                    }

                    gPos--;
                    zn = gMinlen;
                    for (zvec = getBits(gMinlen, s); zvec > gLimit[zn]; zvec = (zvec << 1) | zj)
                    {
                        zn++;
                        zj = getBit(s);
                    }

                    nextSym = gPerm[zvec - gBase[zn]];
                }

            // Set up cftab to facilitate generation of T^(-1)

            // Actually generate cftab
            s.cftab[0] = 0;

            for (i = 1; i <= 256; i++) s.cftab[i] = s.unzftab[i - 1];

            for (i = 1; i <= 256; i++) s.cftab[i] += s.cftab[i - 1];

            s.state_out_len = 0;
            s.state_out_ch = 0;

            // compute the T^(-1) vector
            for (i = 0; i < nblock; i++)
            {
                uc = (sbyte)(BZip2State.tt[i] & 0xFF);
                BZip2State.tt[s.cftab[uc & 0xFF]] |= i << 8;
                s.cftab[uc & 0xFF]++;
            }

            s.tPos = BZip2State.tt[s.origPtr] >> 8;
            s.c_nblock_used = 0;

            // macro: BZ_GET_FAST
            s.tPos = BZip2State.tt[s.tPos];
            s.k0 = (sbyte)(s.tPos & 0xFF);
            s.tPos >>= 8;
            s.c_nblock_used++;

            s.save_nblock = nblock;
            finish(s);

            if (s.c_nblock_used == s.save_nblock + 1 && s.state_out_len == 0)
                reading = true;
            else
                reading = false;
        }
    }

    private static void finish(BZip2State s)
    {
        var c_state_out_ch = s.state_out_ch;
        var c_state_out_len = s.state_out_len;
        var c_nblock_used = s.c_nblock_used;
        var c_k0 = s.k0;
        var c_tt = BZip2State.tt;
        var c_tPos = s.tPos;
        var cs_decompressed = s.decompressed;
        var cs_next_out = s.next_out;
        var cs_avail_out = s.avail_out;
        var avail_out_INIT = cs_avail_out;
        var s_save_nblockPP = s.save_nblock + 1;


        while (true)
        {
            if (c_state_out_len > 0)
                while (true)
                {
                    if (cs_avail_out == 0) goto label67;

                    if (c_state_out_len == 1)
                    {
                        if (cs_avail_out == 0)
                        {
                            c_state_out_len = 1;
                            goto label67;
                        }

                        cs_decompressed[cs_next_out] = c_state_out_ch;
                        cs_next_out++;
                        cs_avail_out--;
                        break;
                    }

                    cs_decompressed[cs_next_out] = c_state_out_ch;
                    c_state_out_len--;
                    cs_next_out++;
                    cs_avail_out--;
                }

            var next = true;
            sbyte k1;
            while (next)
            {
                next = false;
                if (c_nblock_used == s_save_nblockPP)
                {
                    c_state_out_len = 0;
                    goto label67;
                }

                // macro: BZ_GET_FAST_C
                c_state_out_ch = (byte)c_k0;
                c_tPos = c_tt[c_tPos];
                k1 = (sbyte)(c_tPos & 0xFF);
                c_tPos >>= 0x8;
                c_nblock_used++;

                if (k1 != c_k0)
                {
                    c_k0 = k1;
                    if (cs_avail_out == 0)
                    {
                        c_state_out_len = 1;
                        goto label67;
                    }

                    cs_decompressed[cs_next_out] = c_state_out_ch;
                    cs_next_out++;
                    cs_avail_out--;
                    next = true;
                }
                else if (c_nblock_used == s_save_nblockPP)
                {
                    if (cs_avail_out == 0)
                    {
                        c_state_out_len = 1;
                        goto label67;
                    }

                    cs_decompressed[cs_next_out] = c_state_out_ch;
                    cs_next_out++;
                    cs_avail_out--;
                    next = true;
                }
            }

            // macro: BZ_GET_FAST_C
            c_state_out_len = 2;
            c_tPos = c_tt[c_tPos];
            k1 = (sbyte)(c_tPos & 0xFF);
            c_tPos >>= 0x8;
            c_nblock_used++;

            if (c_nblock_used != s_save_nblockPP)
            {
                if (k1 == c_k0)
                {
                    // macro: BZ_GET_FAST_C
                    c_state_out_len = 3;
                    c_tPos = c_tt[c_tPos];
                    k1 = (sbyte)(c_tPos & 0xFF);
                    c_tPos >>= 0x8;
                    c_nblock_used++;

                    if (c_nblock_used != s_save_nblockPP)
                    {
                        if (k1 == c_k0)
                        {
                            // macro: BZ_GET_FAST_C
                            c_tPos = c_tt[c_tPos];
                            k1 = (sbyte)(c_tPos & 0xFF);
                            c_tPos >>= 0x8;
                            c_nblock_used++;

                            // macro: BZ_GET_FAST_C
                            c_state_out_len = (k1 & 0xFF) + 4;
                            c_tPos = c_tt[c_tPos];
                            c_k0 = (sbyte)(c_tPos & 0xFF);
                            c_tPos >>= 0x8;
                            c_nblock_used++;
                        }
                        else
                        {
                            c_k0 = k1;
                        }
                    }
                }
                else
                {
                    c_k0 = k1;
                }
            }
        }

        label67:
        var total_out_lo32_old = s.total_out_lo32;
        s.total_out_lo32 += avail_out_INIT - cs_avail_out;
        if (s.total_out_lo32 < total_out_lo32_old) s.total_out_hi32++;

        // save
        s.state_out_ch = c_state_out_ch;
        s.state_out_len = c_state_out_len;
        s.c_nblock_used = c_nblock_used;
        s.k0 = c_k0;
        BZip2State.tt = c_tt;
        s.tPos = c_tPos;
        // s.decompressed = cs_decompressed;
        s.next_out = cs_next_out;
        s.avail_out = cs_avail_out;
        // end save
    }

    private static sbyte getUnsignedChar(BZip2State s)
    {
        return (sbyte)getBits(8, s);
    }

    private static sbyte getBit(BZip2State s)
    {
        return (sbyte)getBits(1, s);
    }

    private static int getBits(int n, BZip2State s)
    {
        while (s.bsLive < n)
        {
            s.bsBuff = (s.bsBuff << 8) | (s.stream[s.next_in] & 0xFF);
            s.bsLive += 8;
            s.next_in++;
            s.avail_in--;
            s.total_in_lo32++;
            if (s.total_in_lo32 == 0) s.total_in_hi32++;
        }

        var value = (s.bsBuff >> (s.bsLive - n)) & ((1 << n) - 1);
        s.bsLive -= n;
        return value;
    }

    private static void makeMaps(BZip2State s)
    {
        s.nInUse = 0;

        for (var i = 0; i < 256; i++)
            if (s.inUse[i])
            {
                s.seqToUnseq[s.nInUse] = (sbyte)i;
                s.nInUse++;
            }
    }

    private static void createDecodeTables(int[] limit, int[] _base, int[] perm, sbyte[] length, int minLen, int maxLen,
        int alphaSize)
    {
        var pp = 0;
        int i;

        for (i = minLen; i <= maxLen; i++)
        for (var j = 0; j < alphaSize; j++)
            if (length[j] == i)
            {
                perm[pp] = j;
                pp++;
            }

        for (i = 0; i < BZip2State.BZ_MAX_CODE_LEN; i++) _base[i] = 0;

        for (i = 0; i < alphaSize; i++) _base[length[i] + 1]++;

        for (i = 1; i < BZip2State.BZ_MAX_CODE_LEN; i++) _base[i] += _base[i - 1];

        for (i = 0; i < BZip2State.BZ_MAX_CODE_LEN; i++) limit[i] = 0;

        var vec = 0;
        for (i = minLen; i <= maxLen; i++)
        {
            vec += _base[i + 1] - _base[i];
            limit[i] = vec - 1;
            vec <<= 1;
        }

        for (i = minLen + 1; i <= maxLen; i++) _base[i] = ((limit[i - 1] + 1) << 1) - _base[i];
    }
}