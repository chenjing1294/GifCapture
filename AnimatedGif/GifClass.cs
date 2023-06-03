﻿#region License Information (GPL v3)

/*
   Source code provocatively stolen from ShareX: https://github.com/ShareX/ShareX.
   (Seriously, awesome work over there, I used some of the parts to create an easy
   to use .NET package for everyone.)
   Their License:

   ShareX - A program that allows you to take screenshots and share any file type
   Copyright (c) 2007-2017 ShareX Team
   This program is free software; you can redistribute it and/or
   modify it under the terms of the GNU General Public License
   as published by the Free Software Foundation; either version 2
   of the License, or (at your option) any later version.
   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.
   You should have received a copy of the GNU General Public License
   along with this program; if not, write to the Free Software
   Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
   Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)


using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace AnimatedGif
{
    public class GifClass
    {
        public enum GifBlockType
        {
            ImageDescriptor = 0x2C,
            Extension = 0x21,
            Trailer = 0x3B
        }

        public enum GifVersion
        {
            GIF87a,
            GIF89a
        }

        public List<byte> ColorTable = new List<byte>();
        public List<byte> GifSignature = new List<byte>();
        public List<byte> ImageData = new List<byte>();
        public List<byte> ImageDescriptor = new List<byte>();
        public List<byte> ScreenDescriptor = new List<byte>();

        public GifVersion Version = GifVersion.GIF87a;

        public void LoadGifPicture(Image img, GifQuality quality)
        {
            List<byte> dataList;

            using (var ms = new MemoryStream())
            {
                img.SaveGif(ms, quality);
                dataList = new List<byte>(ms.ToArray());
            }

            if (!AnalyzeGifSignature(dataList)) throw new Exception("File is not a gif!");

            AnalyzeScreenDescriptor(dataList);

            var blockType = GetTypeOfNextBlock(dataList);

            while (blockType != GifBlockType.Trailer)
            {
                switch (blockType)
                {
                    case GifBlockType.ImageDescriptor:
                        AnalyzeImageDescriptor(dataList);
                        break;
                    case GifBlockType.Extension:
                        ThrowAwayExtensionBlock(dataList);
                        break;
                }

                blockType = GetTypeOfNextBlock(dataList);
            }
        }

        private bool AnalyzeGifSignature(List<byte> gifData)
        {
            for (int i = 0; i < 6; i++) GifSignature.Add(gifData[i]);

            gifData.RemoveRange(0, 6);

            List<char> chars = GifSignature.ConvertAll(ByteToChar);

            string s = new string(chars.ToArray());

            if (s == GifVersion.GIF89a.ToString()) Version = GifVersion.GIF89a;
            else if (s == GifVersion.GIF87a.ToString()) Version = GifVersion.GIF87a;
            else return false;

            return true;
        }

        private char ByteToChar(byte b)
        {
            return (char) b;
        }

        private void AnalyzeScreenDescriptor(List<byte> gifData)
        {
            for (int i = 0; i < 7; i++) ScreenDescriptor.Add(gifData[i]);

            gifData.RemoveRange(0, 7);

            // if the first bit of the fifth byte is set the GlobelColorTable follows this block

            bool globalColorTableFollows = (ScreenDescriptor[4] & 0x80) != 0;

            if (globalColorTableFollows)
            {
                int pixel = ScreenDescriptor[4] & 0x07;

                int lengthOfColorTableInByte = 3 * (int) Math.Pow(2, pixel + 1);

                for (int i = 0; i < lengthOfColorTableInByte; i++) ColorTable.Add(gifData[i]);

                gifData.RemoveRange(0, lengthOfColorTableInByte);
            }

            ScreenDescriptor[4] = (byte) (ScreenDescriptor[4] & 0x7F);
        }

        private GifBlockType GetTypeOfNextBlock(List<byte> gifData)
        {
            var blockType = (GifBlockType) gifData[0];

            return blockType;
        }

        private void AnalyzeImageDescriptor(List<byte> gifData)
        {
            for (int i = 0; i < 10; i++) ImageDescriptor.Add(gifData[i]);

            gifData.RemoveRange(0, 10);

            // get ColorTable if exists

            bool localColorMapFollows = (ImageDescriptor[9] & 0x80) != 0;

            if (localColorMapFollows)
            {
                int pixel = ImageDescriptor[9] & 0x07;

                int lengthOfColorTableInByte = 3 * (int) Math.Pow(2, pixel + 1);

                ColorTable.Clear();

                for (int i = 0; i < lengthOfColorTableInByte; i++) ColorTable.Add(gifData[i]);

                gifData.RemoveRange(0, lengthOfColorTableInByte);
            }
            else
            {
                int lastThreeBitsOfGlobalTableDescription = ScreenDescriptor[4] & 0x07;

                ImageDescriptor[9] = (byte) (ImageDescriptor[9] & 0xF8);

                ImageDescriptor[9] = (byte) (ImageDescriptor[9] | lastThreeBitsOfGlobalTableDescription);
            }

            ImageDescriptor[9] = (byte) (ImageDescriptor[9] | 0x80);

            GetImageData(gifData);
        }

        private void GetImageData(List<byte> gifData)
        {
            ImageData.Add(gifData[0]);

            gifData.RemoveAt(0);

            while (gifData[0] != 0x00)
            {
                int countOfFollowingDataBytes = gifData[0];

                for (int i = 0; i <= countOfFollowingDataBytes; i++) ImageData.Add(gifData[i]);

                gifData.RemoveRange(0, countOfFollowingDataBytes + 1);
            }

            ImageData.Add(gifData[0]);

            gifData.RemoveAt(0);
        }

        private void ThrowAwayExtensionBlock(List<byte> gifData)
        {
            gifData.RemoveRange(0, 2); // Delete ExtensionBlockIndicator and ExtensionDetermination

            while (gifData[0] != 0) gifData.RemoveRange(0, gifData[0] + 1);

            gifData.RemoveAt(0);
        }
    }
}