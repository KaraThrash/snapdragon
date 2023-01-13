// /******************************************************************************
//  * File: Texture2DExtensions.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.IO;
using UnityEngine;

namespace QCHT.Core.Extensions
{
    public static class Texture2DExtensions
    {
        public static void SaveTextureAsPNG(this Texture2D _texture, string _fullPath, string fileName)
        {
            byte[] _bytes = _texture.EncodeToPNG();
            if (!Directory.Exists(_fullPath))
            {
                Directory.CreateDirectory(_fullPath);
            }

            File.WriteAllBytes(_fullPath + fileName + ".png", _bytes);
            VRDEBUG.LogMessage(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
        }

        public static void DrawPoint(this Texture2D tex, Vector2 p, Color col, int radius = 3)
        {
            int x = (int) p.x;
            int y = (int) p.y;

            float rSquared = radius * radius;

            for (int u = x - radius; u < x + radius + 1; u++)
            for (int v = y - radius; v < y + radius + 1; v++)
                if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
                    tex.SetPixel(u, v, col);
        }

        public static void DrawHand(this Texture2D tex, QCHTHand hand, Color col)
        {
            DrawHandPalm(tex, hand, col);
            for (int i = 0; i < 5; i++)
            {
                var joints = QCHTFinger.GetFingerJointsId((FingerId) i);
                DrawLine(tex, hand.GetWristPosition(), hand.GetPoint(joints[0]), col);
                DrawLine(tex, hand.GetPoint(joints[0]), hand.GetPoint(joints[1]), col);
                DrawLine(tex, hand.GetPoint(joints[1]), hand.GetPoint(joints[2]), col);
                if (i != 0)
                {
                    DrawLine(tex, hand.GetPoint(joints[2]), hand.GetPoint(joints[3]), col);
                }
            }
        }

        public static void DrawHandPalm(this Texture2D tex, QCHTHand hand, Color col)
        {
            for (int i = 0; i < 5; i++)
            {
                var joints1 = QCHTFinger.GetFingerJointsId((FingerId) i);

                if (i < 4)
                {
                    var joints2 = QCHTFinger.GetFingerJointsId((FingerId) i + 1);
                    DrawLine(tex, hand.GetPoint(joints1[0]), hand.GetPoint(joints2[0]), col);
                    if (i == 0)
                    {
                        DrawLine(tex, hand.GetPoint(QCHTPointId.WRIST_END), hand.GetPoint(joints1[0]), col);
                    }
                }
                else if (i == 4)
                {
                    DrawLine(tex, hand.GetPoint(joints1[0]), hand.GetPoint(QCHTPointId.WRIST_START), col);
                }
            }

            DrawLine(tex, hand.GetPoint(QCHTPointId.WRIST_START), hand.GetPoint(QCHTPointId.WRIST_END), col);
        }

        public static void DrawLine(this Texture2D tex, Vector2 start, Vector2 end, Color col)
        {
            DrawLine(tex, (int) start.x, tex.height - (int) start.y, (int) end.x, tex.height - (int) end.y, col);
        }

        public static void DrawLine(this Texture2D tex, int x0, int y0, int x1, int y1, Color col)
        {
            int dy = (int) (y1 - y0);
            int dx = (int) (x1 - x0);
            int stepx, stepy;

            if (dy < 0)
            {
                dy = -dy;
                stepy = -1;
            }
            else
            {
                stepy = 1;
            }

            if (dx < 0)
            {
                dx = -dx;
                stepx = -1;
            }
            else
            {
                stepx = 1;
            }

            dy <<= 1;
            dx <<= 1;

            float fraction = 0;

            tex.SetPixel(x0, y0, col);
            if (dx > dy)
            {
                fraction = dy - (dx >> 1);
                while (Mathf.Abs(x0 - x1) > 1)
                {
                    if (fraction >= 0)
                    {
                        y0 += stepy;
                        fraction -= dx;
                    }

                    x0 += stepx;
                    fraction += dy;
                    tex.SetPixel(x0, y0, col);
                }
            }
            else
            {
                fraction = dx - (dy >> 1);
                while (Mathf.Abs(y0 - y1) > 1)
                {
                    if (fraction >= 0)
                    {
                        x0 += stepx;
                        fraction -= dy;
                    }

                    y0 += stepy;
                    fraction += dx;
                    tex.SetPixel(x0, y0, col);
                }
            }
        }
    }
}