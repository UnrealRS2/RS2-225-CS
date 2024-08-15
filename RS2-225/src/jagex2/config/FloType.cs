using RS2_225.jagex2.io;

namespace RS2_225.jagex2.config;

public class FloType
{
    private static int count;
    private static FloType?[]? instances;
    private int rgb;
    private int texture = -1;
    private bool overlay = false;
    private bool occlude = true;
    private string name;
    private int hue;
    private int saturation;
    private int lightness;
    private int chroma;
    private int luminance;
    private int hsl;

    public static void unpack(Jagfile config)
    {
        var dat = new Packet(config.read("flo.dat", null));
        count = dat.g2();

        if (instances == null) {
            instances = new FloType?[count];
        }

        for (var id = 0; id < count; id++) {
            if (instances[id] == null) {
                instances[id] = new FloType();
            }

            instances[id].decode(dat);
        }
        Console.WriteLine("Decoded " + instances.Length + " FloType configs");
    }
    
    public void decode(Packet dat) {
        while (true) {
            var code = dat.g1();
            if (code == 0) {
                return;
            }

            if (code == 1) {
                rgb = dat.g3();
                setColor(rgb);
            } else if (code == 2) {
                texture = dat.g1();
            } else if (code == 3) {
                overlay = true;
            } else if (code == 5) {
                occlude = false;
            } else if (code == 6) {
                name = dat.gjstr();
            } else {
                Console.Error.WriteLine("Error unrecognised flo config code: " + code);
            }
        }
    }
    
	private void setColor(int rgb) {
		var red = (rgb >> 16 & 0xFF) / 256.0D;
		var green = (rgb >> 8 & 0xFF) / 256.0D;
		var blue = (rgb & 0xFF) / 256.0D;

		var min = red;
		if (green < red) {
			min = green;
		}
		if (blue < min) {
			min = blue;
		}

		var max = red;
		if (green > red) {
			max = green;
		}
		if (blue > max) {
			max = blue;
		}

		var h = 0.0D;
		var s = 0.0D;
		var l = (min + max) / 2.0D;

		if (min != max) {
			if (l < 0.5D) {
				s = (max - min) / (max + min);
			}
			if (l >= 0.5D) {
				s = (max - min) / (2.0D - max - min);
			}

			if (red == max) {
				h = (green - blue) / (max - min);
			} else if (green == max) {
				h = (blue - red) / (max - min) + 2.0D;
			} else if (blue == max) {
				h = (red - green) / (max - min) + 4.0D;
			}
		}

		h /= 6.0D;

		this.hue = (int) (h * 256.0D);
		this.saturation = (int) (s * 256.0D);
		this.lightness = (int) (l * 256.0D);

		if (this.saturation < 0) {
			this.saturation = 0;
		} else if (this.saturation > 255) {
			this.saturation = 255;
		}

		if (this.lightness < 0) {
			this.lightness = 0;
		} else if (this.lightness > 255) {
			this.lightness = 255;
		}

		if (l > 0.5D) {
			luminance = (int) ((1.0D - l) * s * 512.0D);
		} else {
			luminance = (int) (l * s * 512.0D);
		}

		if (luminance < 1) {
			luminance = 1;
		}

		chroma = (int) (h * luminance);

		var hue = this.hue + (int) (java.Math.random() * 16.0D) - 8;
		switch (hue)
		{
			case < 0:
				hue = 0;
				break;
			case > 255:
				hue = 255;
				break;
		}

		var saturation = this.saturation + (int) (java.Math.random() * 48.0D) - 24;
		switch (saturation)
		{
			case < 0:
				saturation = 0;
				break;
			case > 255:
				saturation = 255;
				break;
		}

		var lightness = this.lightness + (int) (java.Math.random() * 48.0D) - 24;
		switch (lightness)
		{
			case < 0:
				lightness = 0;
				break;
			case > 255:
				lightness = 255;
				break;
		}

		hsl = hsl24to16(hue, saturation, lightness);
	}
	
	private int hsl24to16(int hue, int saturation, int lightness) {
		if (lightness > 179) {
			saturation /= 2;
		}

		if (lightness > 192) {
			saturation /= 2;
		}

		if (lightness > 217) {
			saturation /= 2;
		}

		if (lightness > 243) {
			saturation /= 2;
		}

		return (hue / 4 << 10) + (saturation / 32 << 7) + lightness / 2;
	}
}