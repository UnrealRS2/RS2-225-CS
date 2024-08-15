using RS2_225.jagex2.io;

namespace RS2_225.jagex2.config;

public class ConfigType
{
    int id;
    string? debugname;

    void decode(int code, Packet dat) { }

    public ConfigType decodeType(Packet dat)
    {
        while (true)
        {
            var opcode = dat.g1();
            if (opcode == 0)
            {
                break;
            }
            decode(opcode, dat);
        }
        return this;
    }
}