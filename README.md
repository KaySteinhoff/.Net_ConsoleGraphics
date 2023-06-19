# .Net_ConsoleGraphics
This repo contains a single class that changes the console window to be usable like a blank canvas.
It also comes with some functions like DrawLine, DrawTriangle and DrawImage.
It's double buffered so when it updates there isn't any flickering.

That's about it idk why I made it but here it is.

## How to use

To use it simply create an instance of WindowHandler and it will automatically setup the Console window to be used.

```cs
using ConsoleGraphics;

namespace Example
{
    public class Program
    {
        static bool running = true;

        static void Main()
        {
            WindowHandler handler = new WindowHandler(800, 600, Color.Gray);
            
            handler.DrawTriangle(new Point(50, 50), new Point(750, 50), new Point(750, 550), Color.White);
            handler.Render();
        }
    }
}
```

Now you have it all set-up to be used!

## Constructor

The WindowHandler constructor takes in three arguments: Width, Height and ClearColor of the managed context.

```cs
public WindowHandler(int width, int height, System.Drawing.Color clearColor)
{...
```

## Functions

The WindowHandler has 7 native functions.

- DrawImage
- DrawLine
- DrawTriangle
- DrawEllipse
- DrawCircle
- Render
- Destroy

These can be seperated into two three very creative categories: Drawing, Rendering, Destroing. (Yes, I know nobody saw that coming)

As you might expect the Render() function belongs to the Rendering category, Destroy() to Destroing and all the others to Drawing.

They all do pretty much what they say however be aware that as soon as you call Render() all progress will be wiped so you can redraw you next image/frame.

### Parameters

|Function|Desciption|Parameters (in order)|
|---|---|---|
|DrawImage|Draws an Image to the given location with the given size. All values are in pixel coordinates.|Image img, Point location, Size dimensions|
|DrawLine|Draws a line from p1 to p2 using the specified color|Point p1, Point p2, Color color|
