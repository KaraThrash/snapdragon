// /******************************************************************************
//  * File: QCHTSDKAndroidDebug.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace QCHT.Core
{
#if DEBUG
    public class QCHTSDKAndroidDebug
    {
        public enum CAMERA_NAME
        {
            CAMERA_NAME_TOP_LEFT = 0,
            CAMERA_NAME_TOP_RIGHT = 1,
            CAMERA_NAME_BOTTOM_LEFT = 2,
            CAMERA_NAME_BOTTOM_RIGHT = 3,
        }
        
        private static QCHTSDKAndroidDebug debugSdk;
        private int _debugImageWidth = 0, _debugImageHeight = 0;
        private IntPtr _pixelPointer = IntPtr.Zero;
        private Texture2D[] _imageTextures = new Texture2D[4];
        
        private IntPtr _debugPointPointer = IntPtr.Zero;
        private IntPtr _handSidesPointer = IntPtr.Zero;
        
        private readonly float[] _rawDebugPoints = new float[256];
        private readonly Vector2[] _debugPoints = new Vector2[256];
        
        public static QCHTSDKAndroidDebug Instance
        {
            get
            {
                if (debugSdk == null)
                {
                    debugSdk = new QCHTSDKAndroidDebug();
                }

                return debugSdk;
            }
        }

        public void Destroy()
        {
            DestroyImagesTextures();

            if (_debugPointPointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal((_debugPointPointer));
            }
            if (_handSidesPointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal((_handSidesPointer));
            }
        }

        private void ApplyTextureFromPtr(ref Texture2D imageTexture, ref IntPtr pixelPtr)
        {
            imageTexture.LoadRawTextureData(pixelPtr, 4 * _debugImageWidth * _debugImageHeight);
            imageTexture.Apply();
        }

        private bool _pixelPointerLoaded;
        private void CreateImagesTextures()
        {
            for(int i = 0; i < 4; i++)
            {
                var tex = _imageTextures[i];
                if (tex == null || tex.width != _debugImageWidth || tex.height != _debugImageHeight)
                {
                    if (!_pixelPointerLoaded)
                    {
                        _pixelPointer = Marshal.AllocHGlobal(_debugImageHeight * _debugImageWidth * 4);
                        _pixelPointerLoaded = true;
                    }
                    
                    _imageTextures[i] = new Texture2D(_debugImageWidth, _debugImageHeight, TextureFormat.RGBA32, false);
                }
            }
        }
        
        private void DestroyImagesTextures()
        {
            if (_pixelPointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_pixelPointer);
                for(int i = 0; i < 4; i++)
                {
                    _imageTextures[i] = null;
                }
            }
        }

        public Texture2D GetDebugImage(CAMERA_NAME cameraId)
        {
            if (!QCHTManager.Instance.IsRunning) return null;
#if !UNITY_EDITOR
            if (_debugImageHeight == 0 || _debugImageWidth == 0)
            {
                ClaySDKGetDebugImage(IntPtr.Zero, ref _debugImageWidth, ref _debugImageHeight, (int)cameraId);
            }

            CreateImagesTextures();
            ClaySDKGetDebugImage(_pixelPointer, ref _debugImageWidth, ref _debugImageHeight, (int)cameraId);
            
            if (_pixelPointer != IntPtr.Zero)
            {
                ApplyTextureFromPtr(ref _imageTextures[(int)cameraId], ref _pixelPointer);
            }
#endif
            return _imageTextures[(int) cameraId];
        }

        public Vector2[] Get2DHandPoints(bool isLeft, CAMERA_NAME cameraId)
        {
#if !UNITY_EDITOR
            var pointsCount = 0;
            if (_debugPointPointer == IntPtr.Zero)
            {
                _debugPointPointer = Marshal.AllocHGlobal(256);
            }

            if (_handSidesPointer == IntPtr.Zero)
            {
                _handSidesPointer = Marshal.AllocHGlobal(256);
            }

            ClaySDKGetDebugPoints(ref _debugPointPointer, ref pointsCount, ref _handSidesPointer, (int) cameraId);
            Marshal.Copy(_debugPointPointer, _rawDebugPoints, 0, pointsCount * 2);
            
            var valueIndex = 0;
            for (var i = 0; i < pointsCount; i++)
            {
                var p = new Vector2(.0f, .0f)
                {
                    x = _rawDebugPoints[valueIndex],
                    y = _rawDebugPoints[valueIndex + 1]
                };
                valueIndex += 2;
                _debugPoints[i] = p;
            }
#endif
            return _debugPoints;
        }

        public static void SetDebugValue(string paramName, float value)
        {
#if !UNITY_EDITOR
           ClaySDKSetDebugValue(paramName, value);
#endif
        }

        public static void StartVideoCapture(string folderName)
        {
            DateTime now = DateTime.Now;
            string fullFolderName = "/sdcard/Pictures/QchtDTG/" + now.ToString("yyyyMMdd") + "/" +
                                    now.ToString("HHmmss") + "_" + folderName;
#if !UNITY_EDITOR
            ClaySDKStartRecordVideo(fullFolderName, "");
#endif
        }

        public static void StopVideoCapture()
        {
#if !UNITY_EDITOR
            ClaySDKStopRecordVideo();
#endif
        }
        
        public static void StartHandDataCapture(string folderName)
        {
            DateTime now = DateTime.Now;
            string fullFolderName = "/sdcard/Pictures/QchtDTG/" + now.ToString("yyyyMMdd") + "/" +
                                    now.ToString("HHmmss") + "_" + folderName;
#if !UNITY_EDITOR
            ClaySDKStartRecordData(fullFolderName);
#endif
        }

        public static void StopHandDataCapture()
        {
#if !UNITY_EDITOR
            ClaySDKStopRecordData();
#endif
        }

        public static string GetSDKDebugLogs()
        {
#if !UNITY_EDITOR
            IntPtr ptrToLog = ClaySDKGetDebugLogs();
            string logString = Marshal.PtrToStringAnsi(ptrToLog);
            return logString;
#else
            return "Helloooooo...\nIs it me you're looking foooooor...";
#endif
        }

        [DllImport("claysdk")]
        private static extern void ClaySDKGetDebugImage(IntPtr pixels, ref int width, ref int height, int cam = 1);

        [DllImport("claysdk")]
        private static extern void ClaySDKGetDebugPoints(ref IntPtr pointsData, ref int pointsCount, ref IntPtr handSides, int cam = 1);

        [DllImport("claysdk")]
        private static extern void ClaySDKStartRecordVideo(string pathName, string fileName);

        [DllImport("claysdk")]
        private static extern void ClaySDKStopRecordVideo();
        
        [DllImport("claysdk")]
        private static extern void ClaySDKStartRecordData(string pathName);
        
        [DllImport("claysdk")]
        private static extern void ClaySDKStopRecordData();

        [DllImport("claysdk")]
        private static extern void ClaySDKSetDebugValue(string key, float value);

        [DllImport("claysdk")]
        private static extern IntPtr ClaySDKGetDebugLogs();
    }
#endif
}