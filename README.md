# RetroTVFX
A small collection of shaders for a range of authentic old TV effects (Composite, S-Video, RF, etc) in Unity.

## Installation
### From Git URL
Open the Package Manager in Unity, click the Plus icon in the upper left corner, and select "Add package from Git URL". Enter this repository's URL (`git@github.com:GlaireDaggers/RetroTVFX.git`) into the field.

### From disk
Clone this repository to your computer. Open the Package Manager in Unity, click the Plus icon in the upper left corner, and select "Add package from disk".
Browse to where you cloned this repository to and select the "package.json" file.

After the package has installed, you may optionally install the test and example assets via the Package Manager.

## Examples
See the example scenes in `Samples/RetroTVFX/[version]/Examples/Scenes`
* Basic shows off a minimal example with a rotating 3D cube
* PSX goes beyond basic, adding a low-res effect and wobbly PSX-esque model
* 2D shows a demo scene of a sprite running down a hallway with onscreen controls for the CRT effect

## Quick Start
Just add the CRT Effect component to your camera. It should work out of the box as a post effect script.

## CRT Effect parameters

### Filter Quality
Sets the quality of the luma/chroma signal filters. 8 taps is lower quality but likely faster, 24 taps is higher quality but likely slower. Use 24 if you need more accurate NTSC behavior, otherwise 8 should work just fine.

### Video Mode
Sets the mode of the video signal. Think of this like switching what kind of cable hookup you're using to display the image.

| Video Mode       | Description                                                                                              |
| ---------------- | -------------------------------------------------------------------------------------------------------- |
| RF               | Looks like an RF coax cable. Blurriest option and adds signal noise                                      |
| Composite        | Acts like a composite RCA video cable. A little clearer than RF and no noise                             |
| S-Video          | Acts like an S-Video cable. Splits luma and chroma signals for a much cleaner look                       |
| Component        | Acts like Y/I/Q component cables. No color bleeding at all.                                              |
| VGA/SCART        | Acts like a VGA or SCART cable. Should be nearly indistinguishable from Component                        |
| VGA/SCART (Fast) | Much like VGA/SCART, but with fewer blit operations. Performs no downscaling, so it may look artificial. |

### Quantize RGB
Allows incoming RGB values to be quantized to a lower bit depth. Set the bits per R, G, and B channels.

### Display size X/Y
Sets the internal render texture size. The screen will be downscaled to this size before being passed through the video filter.

In general, size Y can be left at 480 and size X can be used to control blurriness - 640 leads to a much more fuzzy image, and 1280 is a much sharper image.

### Stretch to display
Causes the final image to be stretched to the bounds of the game window.

### Aspect Ratio
If Stretch to display is disabled, you can use this to force an aspect ratio for the image. The image will be fit to the bounds of the game window and surrounded with a black border. Note that this may produce a squashed image if the camera renders at a different aspect ratio, attach the Override Aspect Ratio script to correct for this.

### Enable TV Curvature
If enabled, warps the image with a barrel lens effect.

### Curvature
Sets the amount of distortion for TV curvature

### TV Overlay
Sets an image to be overlaid on top of the image (to add a border mask like effect)

### Enable pixel mask
Enables a pixel mask to be overlaid on top of the image to emulate shadow masks or dot grills.

### Pixel mask texture
The texture to be overlaid for pixel mask effect

### Mask repeat X/Y
Amount to repeat pixel mask texture on X/Y axes

### Pixel mask brightness
Allows to compensate for pixel mask darkening by overbrightening image. Use values >1 to over-brighten.

### Rolling sync flicker
Enables a "rolling flicker" type effect you'd see when recording a CRT television with a camera.

### Rolling flicker factor
Sets the opacity of the rolling flicker effect.

### Rolling V-Sync time
Sets the phase duration of the V-Sync. 1.0 is perfectly synchronized and will not display any "rolling". Any value other than 1.0 will simulate the "desynchronized" effect
and cause it to roll faster or slower

### RF Noise
Sets the amount of RF noise added to the image (RF only)

### YIQ Filter
Allows the YIQ signal to be distorted. The chroma plane can be scaled and offset, and the luma signal can also be sharpened here

### Enable burst count animation
Enable animating the NTSC burst phase, which causes the scanline flickering. Disabling this will freeze the animation and make the scanline artifacts more apparent, but some
old consoles did work this way.

### Anti flicker
When burst count animation is enabled, this adds an option to reduce the amount of flickering by blending the current and previous frames together. This adds a "motion blur"
like effect, but reduces the amount of flickering which may be desirable.

## Low resolution effect
The PSX demo scene includes an example of adding a low-res effect. This is done by splitting your camera setup into two cameras: One camera will render the game world into a render texture, and the other will display that render texture to the screen and apply the CRT effect. I'll call these the RenderCamera and the DisplayCamera, respectively.

Your RenderCamera should have a target texture assigned. Several are included, but if you want to make your own you can just create a RenderTexture asset set to the desired resolution and assign it to the camera's Target Texture slot.

Meanwhile, your DisplayCamera should be set to a Culling Mask of Nothing (so you don't render the scene twice), preferably a Clear Flags of Solid Color, and you should attach the Blit Render Texture script above the CRT Effect, assigning it the same render texture your RenderCamera is rendering into.

## Unity UI Input
The 2D demo scene includes an example of rendering Unity UI into the low resolution target texture and then displaying that on the screen. However, this complicates input a bit, as changing the aspect ratio, resolution, and even the TV curvature distortion can all throw off the cursor position. To fix this, use the included LoResStandaloneInputModule script.

When you create a new Canvas in Unity, a new EventSystem game object is created as well. On this object, remove the built-in StandaloneInputModule script and add the LoResStandaloneInputModule to replace it. The parameters are as follows:

### Desired res x/y
The resolution of the render texture being rendered into

### Fisheye x/y
Should be set to the same value as "Curvature" on the CRT Effect script

### Stretch to display
Should be set to the same setting as "Stretch to display" on the CRT Effect script

### Display Aspect
Should be set to the same setting as "Aspect ratio" on the CRT Effect script, if Stretch to display is disabled.