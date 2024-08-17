using System.Drawing.Imaging;

namespace RS2_225.jagex2.graphics;

public class PixMap
{
    private readonly ColorPalette colorPalette;

    private int height;

    private BitmapData image;
    public int[] pixels;

    private int width;

    //TODO: Finish
    public PixMap(int width, int height)
    {
        this.width = width;
        this.height = height;
        pixels = new int[width * height];
        /*this.colorModel = new DirectColorModel(32, 0xff0000, 0xff00, 0xff);
        this.image = c.createImage(this);

        this.setPixels();
        c.prepareImage(this.image, this);

        this.setPixels();
        c.prepareImage(this.image, this);

        this.setPixels();
        c.prepareImage(this.image, this);

        this.bind();*/
    }
}