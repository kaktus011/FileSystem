﻿using System.Collections;

namespace Kursova
{
    internal static class Bitmap
    {
        //TODO: don't read whole BitArray, read only what I need
        //Its wrong to load the whole BitArray in memory.
        //Only load the byte i need to edit the bits in!!
        //but this works for now...

        public static void UpdateBitmap(BinaryReader br, int sectorSize, int bitmapSectors, int sectorCount)
        {
            BitArray bitArr = new(sectorCount);
            for (var i = 0; i < bitmapSectors; i++)
                bitArr[i] = true;

            br.BaseStream.Position = bitmapSectors * sectorSize + 1;
            for (var i = bitmapSectors; i < sectorCount; i++)
            {
                var tmp = br.ReadChar();//first char of file/dir name
                if (tmp != 0)
                    bitArr[i] = true;//taken sector
                else
                    bitArr[i] = false;//free sector
                br.BaseStream.Position += sectorSize - 1;
            }

            WriteBitmap(bitArr, new BinaryWriter(FileSystem.GetFileStream()));
        }

        public static long FindFreeSector(BinaryReader br, int requiredSectors, int bitmapSectors, int sectorSize)
        {
            //TODO not even using the bitmap ?
            br.BaseStream.Position = 0;
            var freeBitNum = -1;
            for (var i = 0; i < bitmapSectors * sectorSize; i++)
            {
                var currByte = br.ReadByte();
                if (currByte == 255)
                    continue;
                var bits = new BitArray(new[] { currByte });
                
                for (int j = 0; j < 8; j++)
                {
                    if(bits[j])
                        continue;
                    freeBitNum = j;
                    break;
                }

                if (freeBitNum != -1)
                    break;
                //scan byte for free bits
                //number of free bit is number of free sector
            }
            if (freeBitNum == -1)
                return -1;

            long offset = (freeBitNum * sectorSize);// + (sectorSize * bitmapSectors);
            return offset;
        }

        private static bool FindSectorChain(BinaryReader br, long offset,int bitmapSectors, int sectorCount, int sectorSize)
        {
            //try to find free sectors next to one another
            br.BaseStream.Position = offset;
            for (var i = bitmapSectors; i < sectorCount; i++)
            {
                var tmp = br.ReadChars(2);
                if(tmp[1] != 0)
                    continue;
                br.BaseStream.Position += sectorSize - 2;
            }
            //if there isn't any -> need to have a custom linked list >:(
            return false;
        }

        private static void WriteBitmap(BitArray bitArr, BinaryWriter bw)
        {
            bw.Seek(0, SeekOrigin.Begin);
            for (var i = 0; i < bitArr.Length;)
            {
                byte tmp = 0;
                for (var j = i; j < i + 8; j++)//get a byte from 8 bool values
                    if(bitArr[j])
                        tmp |= (byte)(1 << j);

                bw.Write(tmp);
                i += 8;
            }
        }
    }
}
