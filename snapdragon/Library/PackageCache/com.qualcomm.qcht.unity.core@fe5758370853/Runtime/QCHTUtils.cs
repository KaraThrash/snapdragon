// /******************************************************************************
//  * File: QCHTUtils.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace QCHT.Core
{
    public static class QCHTColor
    {
        public static Color DebugColorFromFingerID(FingerId fingerId)
        {
            return fingerId switch
            {
                FingerId.THUMB => new Color(232f / 255f, 0f / 255f, 11f / 255f),
                FingerId.INDEX => new Color(255f / 255f, 196f / 255f, 0f / 255f),
                FingerId.MIDDLE => new Color(26f / 255f, 201f / 255f, 56f / 255f),
                FingerId.RING => new Color(0f / 255f, 215f / 255f, 255f / 255f),
                FingerId.PINKY => new Color(139f / 255f, 43f / 255f, 226f / 255f),
                _ => Color.black
            };
        }

        public static Color DebugColorFromPointID(QCHTPointId pointId)
        {
            return pointId switch
            {
                QCHTPointId.PALM_CENTER => new Color(159f / 255f, 72f / 255f, 0f),
                QCHTPointId.WRIST_START => new Color(255f / 255f, 124f / 255f, 0f),
                QCHTPointId.WRIST_END => new Color(241f / 255f, 76f / 255f, 193f / 255f),
                QCHTPointId.WRIST_CENTER => Color.white,
                _ => DebugColorFromFingerID(QCHTFinger.FingerIdFromPointId((int) pointId))
            };
        }

        public static Color DebugColorFromPointID(int pointId)
        {
            return DebugColorFromPointID((QCHTPointId) pointId);
        }
    }

    public static class QCHTFinger
    {
        private static readonly QCHTPointId[][] s_fingerJoints =
        {
            new[]
            {
                QCHTPointId.THUMB_PIP, QCHTPointId.THUMB_DIP, QCHTPointId.THUMB_TIP
            },
            new[]
            {
                QCHTPointId.INDEX_MCP, QCHTPointId.INDEX_PIP, QCHTPointId.INDEX_DIP, QCHTPointId.INDEX_TIP
            },
            new[]
            {
                QCHTPointId.MIDDLE_MCP, QCHTPointId.MIDDLE_PIP, QCHTPointId.MIDDLE_DIP, QCHTPointId.MIDDLE_TIP
            },
            new[]
            {
                QCHTPointId.RING_MCP, QCHTPointId.RING_PIP, QCHTPointId.RING_DIP, QCHTPointId.RING_TIP
            },
            new[]
            {
                QCHTPointId.PINKY_MCP, QCHTPointId.PINKY_PIP, QCHTPointId.PINKY_DIP, QCHTPointId.PINKY_TIP
            }
        };

        public static string NameFromFingerID(FingerId fingerId)
        {
            return fingerId switch
            {
                FingerId.THUMB => "THUMB",
                FingerId.INDEX => "INDEX",
                FingerId.MIDDLE => "MIDDLE",
                FingerId.RING => "RING",
                FingerId.PINKY => "PINKY",
                _ => ""
            };
        }

        public static FingerId FingerIdFromPointId(int pointId)
        {
            return pointId switch
            {
                var p when (p >= 0 && p <= 2) => FingerId.THUMB,
                var p when (p >= 3 && p <= 6) => FingerId.INDEX,
                var p when (p >= 7 && p <= 10) => FingerId.MIDDLE,
                var p when (p >= 11 && p <= 14) => FingerId.RING,
                var p when (p >= 15 && p <= 18) => FingerId.PINKY,
                _ => FingerId.UNKNOWN
            };
        }

        public static FingerId FingerIdFromPointId(QCHTPointId pointId)
        {
            return FingerIdFromPointId((int) pointId);
        }

        /// <summary>
        /// Returns the array of joints ids for a given finger id  
        /// </summary>
        /// <param name="fingerId">Id of the finger</param>
        /// <returns>The array of joints ids for the finger id given in parameters</returns>
        public static QCHTPointId[] GetFingerJointsId(FingerId fingerId)
        {
            return s_fingerJoints[(int) fingerId];
        }
    }

    public static class QCHTTools
    {
        public static void AssertNotNull(System.Object o)
        {
            if (o == null)
            {
                throw new System.ArgumentException("Parameter cannot be null", "o");
            }
        }

        public static bool DoesTagExist(string tag)
        {
            try
            {
                GameObject.FindGameObjectsWithTag(tag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetAndroidExternalFilesDir()
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var externalFilesDirectories = context.Call<AndroidJavaObject[]>("getExternalFilesDirs", (object) null);

            AndroidJavaObject emulated = null;
            AndroidJavaObject sdCard = null;

            foreach (var directory in externalFilesDirectories)
            {
                using var environment = new AndroidJavaClass("android.os.Environment");
                var isRemovable = environment.CallStatic<bool>("isExternalStorageRemovable", directory);
                var isEmulated = environment.CallStatic<bool>("isExternalStorageEmulated", directory);

                if (isEmulated)
                    emulated = directory;
                else if (isRemovable && isEmulated == false)
                    sdCard = directory;
            }

            return sdCard != null ? sdCard.Call<string>("getAbsolutePath") : emulated.Call<string>("getAbsolutePath");
        }

        public static DeviceType GetDeviceType()
        {
            var deviceName = GetDevice();
            switch (deviceName)
            {
                case DeviceName.DeviceNameLenovoA3:
                case DeviceName.DeviceNameMerlin:
                case DeviceName.DeviceNameOracle:
                    return DeviceType.DeviceAR;
                case DeviceName.DeviceNameSheldon:
                    case DeviceName.DeviceNameTrinity:
                    case DeviceName.DeviceNameSAMSUNG_I120:
                    return DeviceType.DeviceMR;
                case DeviceName.DeviceNameSKU1:
                case DeviceName.DeviceNameSKU2:
                case DeviceName.DeviceNameSKU3:
                case DeviceName.DeviceNameSKU4:
                case DeviceName.DeviceNameOculus:
                    return DeviceType.DeviceVR;
                
            }

            return deviceName == DeviceName.DeviceNameUnityEditor ? DeviceType.DeviceEditor : DeviceType.DeviceUnknown;
        }

        public static DeviceName GetDevice()
        {
            var name = GetDeviceName();
            return name switch
            {
                "A3" => DeviceName.DeviceNameLenovoA3,
                "SM-I120" => DeviceName.DeviceNameSAMSUNG_I120,
                "Sheldon" => DeviceName.DeviceNameSheldon,
                "Merlin" => DeviceName.DeviceNameMerlin,
                "Trinity" => DeviceName.DeviceNameTrinity,
                "Morpheus SKU1" => DeviceName.DeviceNameSKU1,
                "Morpheus SKU2" => DeviceName.DeviceNameSKU2,
                "Morpheus SKU3" => DeviceName.DeviceNameSKU3,
                "Morpheus SKU4" => DeviceName.DeviceNameSKU4,
                "Oracle" => DeviceName.DeviceNameOracle,
                "Oculus" => DeviceName.DeviceNameOculus,
                "Unity Editor" => DeviceName.DeviceNameUnityEditor,
                _ => DeviceName.DeviceNameUnknown
            };
        }

        public static string GetDeviceName()
        {
#if !UNITY_EDITOR
            try
            {
                IntPtr ptrToDeviceName = getDeviceName();
                string deviceNameString = Marshal.PtrToStringAnsi(ptrToDeviceName);
                return deviceNameString;
            }
            catch (DllNotFoundException)
            {
                Debug.LogError("QCHT - Unable to find qxrdevice.so native library. If you are running on a Qualcomm device check for your Unity plugin folder");
                return "";
            }
            
#else
            return "Unity Editor";
#endif
        }

        [DllImport("qxrdevice")]
        private static extern IntPtr getDeviceName();
    }

    public enum UpdateMode
    {
        UpdateInUpdate,
        UpdateInFixedUpdate,
        UpdateInLateUpdate
    }

    public enum DeviceName
    {
        DeviceNameUnknown,
        DeviceNameLenovoA3,
        DeviceNameSAMSUNG_I120,
        DeviceNameSheldon,
        DeviceNameMerlin,
        DeviceNameTrinity,
        DeviceNameSKU1,
        DeviceNameSKU2,
        DeviceNameSKU3,
        DeviceNameSKU4,
        DeviceNameOracle,
        DeviceNameOculus,
        DeviceNameUnityEditor
    }

    public enum DeviceType
    {
        DeviceAR,
        DeviceVR,
        DeviceMR,
        DeviceEditor,
        DeviceUnknown
    }

    public static class VRDEBUG
    {
        public static void LogFrameStart(string message, bool logInEditor = true)
        {
#if VR_SDK_HTC
WaveVRDebug.LogMessage("-------------FRAME START-------------", logInEditor);
#else
            QCHTDebug.LogMessage("-------------FRAME START-- " + message + " -------------");
#endif
        }

        public static void LogFrameEnd(string message, bool logInEditor = true)
        {
#if VR_SDK_HTC
WaveVRDebug.LogMessage("-------------FRAME END-------------", logInEditor);
#else
            QCHTDebug.LogMessage("-------------FRAME END-- " + message + " -------------");
#endif
        }

        public static void LogMessage(string message, bool logInEditor = true)
        {
#if VR_SDK_HTC
WaveVRDebug.LogMessage(message, logInEditor);
#else
            QCHTDebug.LogMessage(message);
#endif
        }

        public static void LogVector2(string title, Vector2 v, bool logInEditor = true)
        {
#if VR_SDK_HTC
WaveVRDebug.LogVector2(title, v, logInEditor);
#else
            QCHTDebug.LogVector2(title, v);
#endif
        }

        public static void LogVector3(string title, Vector3 v, bool logInEditor = true)
        {
#if VR_SDK_HTC
WaveVRDebug.LogVector3(title, v, logInEditor);
#else
            QCHTDebug.LogVector3(title, v);
#endif
        }

        public static void LogQuaternion(string title, Quaternion q, bool logInEditor = true)
        {
#if VR_SDK_HTC
WaveVRDebug.LogQuaternion(title, q, logInEditor);
#else
            QCHTDebug.LogQuaternion(title, q);
#endif
        }

        public static void LogAngle(string title, Quaternion q, bool logInEditor = true)
        {
#if VR_SDK_HTC
WaveVRDebug.LogAngle(title, q, logInEditor);
#else
            QCHTDebug.LogAngle(title, q);
#endif
        }

        public static void LogColor(string title, Color c, bool logInEditor = true)
        {
#if VR_SDK_HTC
WaveVRDebug.LogColor(title, c, logInEditor);
#else
            QCHTDebug.LogColor(title, c);
#endif
        }
    }

    public static class QCHTDebug
    {
        public static void LogVector2(string title, Vector2 v)
        {
            Debug.Log(string.Format("qchtUnity: {0} - X:{1} | Y:{2}", title, v.x, v.y));
        }

        public static void LogVector3(string title, Vector3 v)
        {
            Debug.Log(string.Format("qchtUnity: {0} - X:{1} | Y:{2} | Z:{3}", title, v.x, v.y, v.z));
        }

        public static void LogQuaternion(string title, Quaternion q)
        {
            Debug.Log(string.Format("qchtUnity: {0} - X:{1:N5} | Y:{2:N5} | Z:{3:N5} | W:{4:N5}", title, q.x, q.y, q.z,
                q.w));
        }

        public static void LogAngle(string title, Quaternion q)
        {
            Debug.Log(string.Format("qchtUnity: {0} - X°:{1} | Y°:{2} | Z°:{3}",
                title,
                q.eulerAngles.x,
                q.eulerAngles.y,
                q.eulerAngles.z));
        }

        public static void LogColor(string title, Color c)
        {
            Debug.Log(string.Format("qchtUnity: {0} - R:{1} | G:{2} | B:{3} | A:{4}",
                title,
                c.r * 255,
                c.g * 255,
                c.b * 255,
                c.a * 255));
        }

        public static void LogMessage(string message)
        {
            Debug.Log("qchtUnity: " + message);
        }
    }
#if VR_SDK_HTC
    public static class WaveVRDebug
    {
        public static void LogMessage (string message, bool logInEditor = true)
        {
            Log.d(Application.identifier, message, logInEditor);
        }

        public static void LogVector2(string title, Vector2 v, bool logInEditor = true)
        {
            Log.d(Application.identifier, string.Format("qchtUnity: {0} - X:{1} | Y:{2}", title, v.x, v.y),logInEditor);
        }

        public static void LogVector3(string title, Vector3 v, bool logInEditor = true)
        {
            Log.d(Application.identifier, string.Format("qchtUnity: {0} - X:{1} | Y:{2} | Z:{3}", title, v.x, v.y, v.z), logInEditor);
        }

        public static void LogQuaternion(string title, Quaternion q, bool logInEditor = true)
        {
            Log.d(Application.identifier, string.Format("qchtUnity: {0} - X:{1:N5} | Y:{2:N5} | Z:{3:N5} | W:{4:N5}", title, q.x, q.y, q.z, q.w), logInEditor);
        }

        public static void LogAngle(string title, Quaternion q, bool logInEditor = true)
        {
            Log.d(Application.identifier,
                                    string.Format("qchtUnity: {0} - X°:{1} | Y°:{2} | Z°:{3}",
                                    title,
                                    q.eulerAngles.x,
                                    q.eulerAngles.y,
                                    q.eulerAngles.z), logInEditor);
        }

        public static void LogColor(string title, Color c, bool logInEditor = true)
        {
            Log.d(Application.identifier, string.Format("qchtUnity: {0} - R:{1} | G:{2} | B:{3} | A:{4}",
                                    title,
                                    c.r * 255,
                                    c.g * 255,
                                    c.b * 255,
                                    c.a * 255),logInEditor);
        }
    }
#endif
}