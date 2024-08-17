﻿using RS2_225.jagex2.graphics;
using RS2_225.jagex2.io;

namespace RS2_225.jagex2.config;

public class IdkType
{
    public static int count;

    public static IdkType[] instances;

    public bool disable;

    private readonly int[] heads = [-1, -1, -1, -1, -1];

    private int[] models;

    private readonly int[] recol_d = new int[6];

    private readonly int[] recol_s = new int[6];

    public int type = -1;

    public static void unpack(Jagfile config)
    {
        var dat = new Packet(config.read("idk.dat", null));
        count = dat.g2();

        if (instances == null) instances = new IdkType[count];

        for (var id = 0; id < count; id++)
        {
            if (instances[id] == null) instances[id] = new IdkType();

            instances[id].decode(dat);
        }

        Console.WriteLine("Decoded " + count + " IdkType configs");
    }

    public void decode(Packet dat)
    {
        while (true)
        {
            var code = dat.g1();
            if (code == 0) break;

            if (code == 1)
            {
                type = dat.g1();
            }
            else if (code == 2)
            {
                var count = dat.g1();
                models = new int[count];

                for (var i = 0; i < count; i++) models[i] = dat.g2();
            }
            else if (code == 3)
            {
                disable = true;
            }
            else if (code >= 40 && code < 50)
            {
                recol_s[code - 40] = dat.g2();
            }
            else if (code >= 50 && code < 60)
            {
                recol_d[code - 50] = dat.g2();
            }
            else if (code >= 60 && code < 70)
            {
                heads[code - 60] = dat.g2();
            }
            else
            {
                Console.WriteLine("Error unrecognised idk config code: " + code);
            }
        }
    }

    public Model getModel()
    {
        if (this.models == null) return null;

        var models = new Model[this.models.Length];
        for (var i = 0; i < this.models.Length; i++) models[i] = new Model(this.models[i]);

        Model model;
        if (models.Length == 1)
            model = models[0];
        else
            model = new Model(models, models.Length);

        for (var i = 0; i < 6 && recol_s[i] != 0; i++) model.recolor(recol_s[i], recol_d[i]);

        return model;
    }

    public Model getHeadModel()
    {
        var models = new Model[5];

        var count = 0;
        for (var i = 0; i < 5; i++)
            if (heads[i] != -1)
                models[count++] = new Model(heads[i]);

        var model = new Model(models, count);
        for (var i = 0; i < 6 && recol_s[i] != 0; i++) model.recolor(recol_s[i], recol_d[i]);

        return model;
    }
}