# Android Deployment Troubleshooting Guide

## Common Build Errors

### SDK Not Found
```
Unable to locate Android SDK
```
**Solution:**
- Ensure Android SDK is installed
- Set correct SDK path in Unity preferences
- Verify environment variables are set correctly:
  ```
  export ANDROID_HOME=~/android-sdk
  export PATH=$PATH:$ANDROID_HOME/cmdline-tools/bin
  export PATH=$PATH:$ANDROID_HOME/platform-tools
  ```

### JDK Issues
```
Unable to locate JDK
```
**Solution:**
- Install OpenJDK 8:
  ```
  sudo apt-get install -y openjdk-8-jdk
  ```
- Set JDK path in Unity preferences
- Ensure JAVA_HOME environment variable is set:
  ```
  export JAVA_HOME=/usr/lib/jvm/java-8-openjdk-amd64
  ```

### Gradle Build Failed
```
Gradle build failed with unknown error
```
**Solution:**
- Update Gradle version in Unity preferences
- Check custom Gradle template if enabled
- Try disabling "Custom Gradle Template" in Player Settings
- Delete Library/Gradle folder and rebuild

### Missing SDK Components
```
Failed to find target with hash string 'android-30'
```
**Solution:**
- Install the required SDK platform:
  ```
  sdkmanager "platforms;android-30"
  ```
- Accept all licenses:
  ```
  sdkmanager --licenses
  ```

### IL2CPP Build Errors
```
IL2CPP build failed
```
**Solution:**
- Install build-tools:
  ```
  sdkmanager "build-tools;30.0.3"
  ```
- Ensure NDK is installed
- Try setting scripting backend to Mono temporarily for testing

## Common Runtime Issues

### Performance Problems
- Check PerformanceOptimizer settings
- Reduce draw distance
- Decrease texture quality
- Disable post-processing effects
- Ensure object pooling is working correctly
- Profile the game to find bottlenecks

### Input Issues
- Verify MobileInputHandler.cs is properly initialized
- Check UI canvas scaling mode
- Test on multiple device resolutions
- Verify touch input events are being captured

### Crash on Startup
- Check logcat for error messages:
  ```
  adb logcat -s Unity ActivityManager
  ```
- Verify minimum SDK version compatibility
- Check for missing plugin dependencies
- Try increasing IL2CPP stack size

### Device Compatibility
- Ensure manifest has proper compatibility settings
- Test on multiple device types if possible
- Check if required hardware features are properly marked as optional

## Testing Tools
- Use Android Debug Bridge (adb) for installation and debugging:
  ```
  adb install -r /path/to/FPSGame.apk
  adb logcat -s Unity
  ```
- Use Android Studio's profiler tools
- Use Unity Remote for quick testing during development
