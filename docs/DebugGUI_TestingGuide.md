# Death Screen Effect - Debug GUI & Testing Guide

## Debug GUI Overview

The death screen effect includes a comprehensive debug GUI that appears in the top-right corner when `Show Progress GUI` is enabled in the Inspector.

### Debug Information Displayed

**Main Status:**
- Effect Type (Vignette or Standard Fade)
- Is Fading (current state)
- Fade Complete (completion status)
- Fade Progress (percentage)

**Vignette-Specific Info:**
- Vignette Progress (0.0 to 1.0)
- Vignette Intensity
- Vignette Speed
- Max Darkness Level
- Darkness Multiplier
- Vignette Center coordinates

**Real-time Info (during fade):**
- Elapsed Time
- Audio Volume
- Time Scale

## Test Buttons Available

### 1. **Test Death Fade**
- Triggers the standard death fade effect
- Uses current Inspector settings
- Perfect for testing your configuration

### 2. **Reset Effect**
- Immediately stops any ongoing fade
- Resets all values to initial state
- Restores normal gameplay conditions

### 3. **Recreate Vignette** *(Vignette mode only)*
- Regenerates the vignette texture
- Useful when changing vignette settings
- Updates the procedural texture with new parameters

### 4. **Ultra Dark Test** *(Vignette mode only)*
- Tests extreme darkness (98% dark)
- Uses 1.5x darkness multiplier
- Perfect for Little Nightmares-style intensity
- Auto-resets to original settings after 5 seconds

### 5. **Medium Dark Test** *(Vignette mode only)*
- Tests moderate darkness (75% dark)
- Uses 1.0x darkness multiplier
- Good for comparison with ultra dark
- Auto-resets to original settings after 5 seconds

## How to Use for Testing

### Setup:
1. Enable `Show Progress GUI` in the Inspector
2. Set `Use Vignette Effect` to true for Little Nightmares-style effects
3. Adjust settings in Inspector as needed

### Testing Workflow:
1. **Start with Test Death Fade** - Test your current settings
2. **Use Reset Effect** - Clear any ongoing effects
3. **Try Ultra Dark Test** - Experience maximum darkness
4. **Compare with Medium Dark Test** - See the difference
5. **Use Recreate Vignette** - If you changed vignette settings

### Inspector Settings to Experiment With:

**For Little Nightmares Effect:**
- Vignette Intensity: 2.0-3.0
- Max Darkness Level: 0.85-0.95
- Darkness Multiplier: 1.2-1.8
- Vignette Speed: 1.5-2.5

**For Subtle Effects:**
- Vignette Intensity: 1.0-1.5
- Max Darkness Level: 0.6-0.8
- Darkness Multiplier: 1.0-1.2
- Vignette Speed: 0.8-1.2

## Context Menu Options

Right-click the component in Inspector for these options:
- **Trigger Death Fade**
- **Reset Death Effect**
- **Test Ultra Dark Fade**
- **Test Medium Dark Fade**
- **Recreate Vignette Texture**

## Troubleshooting

**Debug GUI not showing:**
- Check `Show Progress GUI` is enabled
- Verify the script is active
- Check screen resolution (GUI is positioned top-right)

**Buttons not working:**
- Ensure the component is properly set up
- Check that fadeOverlay Image is assigned
- Verify no compilation errors

**Effect not visible:**
- Check Canvas/Image setup
- Verify Image color is set to white
- Ensure Canvas is set to "Screen Space - Overlay"
- Check sorting order

## Performance Notes

- Debug GUI only renders when `showProgressGUI` is true
- Test methods automatically reset after 5 seconds
- No performance impact when debug GUI is disabled
- Safe to use in builds (just disable Show Progress GUI)
