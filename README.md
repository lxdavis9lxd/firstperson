# First Person Shooter for Android

A simple first-person shooter game built with Unity for Android devices.

## Overview
This project is a basic FPS game designed to run on Android devices. It includes:
- First-person player controller with touch controls
- Enemy AI with pathfinding and dynamic spawning
- Weapon system with multiple weapons
- Procedurally generated levels
- Object pooling for performance
- Mobile-optimized graphics and performance
- Minimap system for navigation
- Audio management with 3D sound effects

## Setup Instructions

### Prerequisites
1. Unity 2022.3 LTS or newer
2. Android SDK and NDK (can be installed through Unity Hub)
3. Java Development Kit (JDK) 8 or newer
4. Android device or emulator for testing

### Setting Up the Development Environment
1. Install OpenJDK 8:
   ```
   sudo apt-get install -y openjdk-8-jdk
   ```

2. Set up Android SDK:
   ```
   # Download Android SDK command line tools
   wget https://dl.google.com/android/repository/commandlinetools-linux-9477386_latest.zip
   mkdir -p ~/android-sdk
   unzip commandlinetools-linux-9477386_latest.zip -d ~/android-sdk
   
   # Set environment variables
   export ANDROID_HOME=~/android-sdk
   export PATH=$PATH:$ANDROID_HOME/cmdline-tools/bin
   export PATH=$PATH:$ANDROID_HOME/platform-tools
   
   # Install required SDK components
   sdkmanager --update
   sdkmanager "platform-tools" "platforms;android-30" "build-tools;30.0.3"
   sdkmanager --licenses
   ```

### Building the Android Project in Unity
1. Open the project in Unity (File > Open Project)
2. Configure Android build settings:
   - Go to File > Build Settings
   - Select Android as the platform
   - Click "Switch Platform" if it's not already selected
   - Set Texture Compression to "ASTC"
   - Enable "Custom Gradle Template" if you need special configurations

3. Player Settings for Android:
   - Go to Edit > Project Settings > Player
   - Set Company Name and Product Name
   - Set Version number
   - In the Android tab:
     - Set minimum API level (Android 7.0/API 24 recommended)
     - Configure graphics API (OpenGL ES 3.0 recommended)
     - Set scripting backend to IL2CPP for better performance
     - Enable ARM64 architecture

4. Build the APK:
   - In Build Settings, click "Build"
   - Select a folder to save the APK
   - Wait for the build to complete

### Alternatively, Build using Command Line
You can use the provided build script:
```
# Make the script executable
chmod +x build_and_deploy.sh

# Edit the script to set your Unity path
nano build_and_deploy.sh

# Run the script
./build_and_deploy.sh
```

### Testing on a Physical Device
1. Enable Developer options on your Android device:
   - Go to Settings > About phone
   - Tap "Build number" seven times
   - Go back to Settings > Developer options
   - Enable USB debugging

2. Connect your device to your computer
3. Install the APK:
   ```
   adb install -r /path/to/FPSGame.apk
   ```

## Project Structure
- `Assets/Scripts` - C# scripts for game logic
- `Assets/Prefabs` - Reusable game objects
- `Assets/Scenes` - Game scenes
- `Assets/Materials` - Materials for game objects
- `Assets/Models` - 3D models for the game
- `Assets/Audio` - Sound effects and music
- `Assets/Animations` - Character and weapon animations
- `Assets/Textures` - Textures and UI elements
- `Assets/Plugins/Android` - Android-specific files

## Controls
- Swipe on right side of screen to look around
- Virtual joystick on left side for movement
- Touch buttons for shooting, reloading, and jumping
- Additional buttons for weapon switching and pausing

## Performance Optimization
The game includes automatic performance optimization for different Android devices:
- Dynamic resolution scaling
- Automatic quality adjustment based on frame rate
- Level of Detail (LOD) management
- Draw distance adjustment
- Object pooling for bullets and effects

## Troubleshooting
- If you encounter build errors related to SDK, ensure you've accepted all SDK licenses
- For low performance on older devices, the game will automatically adjust quality settings
- If touch controls are not responsive, ensure the UI canvas is properly configured for the device resolution