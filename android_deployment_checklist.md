# Android Deployment Checklist

Use this checklist to verify your Unity FPS game is ready for Android deployment:

## Pre-Build Verification
- [ ] Unity version is 2022.3 LTS or newer
- [ ] Android SDK and NDK are installed
- [ ] JDK 8 or newer is installed
- [ ] All required Android SDK licenses are accepted
- [ ] Android platform is selected in Build Settings
- [ ] Company name and product name are set in Player Settings
- [ ] Bundle version and version code are set
- [ ] Minimum API level is set to at least Android 7.0 (API 24)
- [ ] Target API level is set to a recent version (API 30+ recommended)
- [ ] IL2CPP scripting backend is selected for better performance
- [ ] ARM64 architecture is enabled
- [ ] AndroidManifest.xml has all required permissions:
  - [ ] INTERNET
  - [ ] ACCESS_NETWORK_STATE
  - [ ] VIBRATE
  - [ ] OpenGL ES 3.0 feature requirement

## Performance Optimization Checks
- [ ] PerformanceOptimizer.cs is included in a startup scene
- [ ] MobileInputHandler.cs is properly connected to player controller
- [ ] Object pooling is implemented for frequently spawned objects
- [ ] Texture compression is set to ASTC
- [ ] Graphics quality settings are configured for mobile
- [ ] Draw distance is optimized for mobile
- [ ] Lighting is optimized (prefer baked lighting)
- [ ] No unnecessary post-processing effects

## UI and Controls Checks
- [ ] All UI elements are properly scaled for mobile screens
- [ ] Touch controls are implemented and responsive:
  - [ ] Virtual joystick for movement
  - [ ] Look control area
  - [ ] Action buttons (shoot, jump, reload)
  - [ ] Weapon selection UI
  - [ ] Pause menu
- [ ] UI is readable on different screen sizes
- [ ] UI elements don't obscure important gameplay elements

## Final Checks
- [ ] Build the APK using the BuildScript.cs or via Unity Editor
- [ ] Verify the APK file is created in the Builds/Android directory
- [ ] Test the APK on a physical device or emulator
- [ ] Check for performance issues on target devices
- [ ] Verify that mobile input controls work correctly
- [ ] Test on different screen sizes if possible
