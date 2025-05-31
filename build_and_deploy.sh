#!/bin/bash

# Set paths
UNITY_PATH="/path/to/unity/executable"  # Update with your Unity path
PROJECT_PATH="/workspaces/firstperson"
BUILD_PATH="$PROJECT_PATH/Builds/Android"
APK_NAME="FPSGame.apk"

# Create build directory
mkdir -p "$BUILD_PATH"

# Build the Unity project
echo "Building Unity project for Android..."
"$UNITY_PATH" -batchmode -quit -projectPath "$PROJECT_PATH" \
  -executeMethod BuildScript.BuildAndroid \
  -logFile "$PROJECT_PATH/unity_build.log"

# Check if build was successful
if [ -f "$BUILD_PATH/$APK_NAME" ]; then
  echo "Build successful: $BUILD_PATH/$APK_NAME"
  echo "APK is ready for deployment to a physical device or emulator."
  echo "You can find it at: $BUILD_PATH/$APK_NAME"
  
  echo "To install on a connected device manually:"
  echo "1. Connect your Android device via USB"
  echo "2. Enable USB debugging on your device"
  echo "3. Run: adb install -r $BUILD_PATH/$APK_NAME"
else
  echo "Build failed. Check the log file: $PROJECT_PATH/unity_build.log"
fi
