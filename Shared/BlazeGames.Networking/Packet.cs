// This work is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported
// License. To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// Copyright 2012 Blaze Games

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace BlazeGames.Networking
{
    /// <summary>
    /// Packet System Version 2.3.1
    /// Ashton Storks, Blaze Games
    /// Last Updated: 11/10/2012
    /// </summary>
    class Packet : IDisposable
    {
        /// <summary>
        /// Gets or Sets the maximum packet length
        /// </summary>
        public static int MaxLength = 2048;

        /// <summary>
        /// Creates a packet with the following data
        /// </summary>
        /// <param name="Data">Byte, Byte[], Int16, Int32, Int64, UInt32, String, Boolean, or Double</param>
        /// <returns>New packet created with the data</returns>
        public static Packet New(params object[] Data)
        {
            Packet pak = new Packet(null);

            foreach (object data in Data)
            {
                switch (data.GetType().Name)
                {
                    case "Byte": pak.Write((Byte)data); break;
                    case "Byte[]": pak.Write((Byte[])data); break;
                    case "Int16": pak.Write((Int16)data); break;
                    case "Int32": pak.Write((Int32)data); break;
                    case "Int64": pak.Write((Int64)data); break;
                    case "UInt32": pak.Write((UInt32)data); break;
                    case "String": pak.Write((String)data); break;
                    case "Boolean": pak.Write((Boolean)data); break;
                    case "Double": pak.Write((Double)data); break;
                    default: throw new ArgumentException("Unknown Type: " + data.GetType().Name);
                }
            }

            return pak;
        }

        protected MemoryStream MemoryBuffer;
        private BinaryReader MemoryReader;
        private BinaryWriter MemoryWriter;
        private Crc32 crc32;

        /// <summary>
        /// Splits the buffer into all the nagled packets
        /// </summary>
        /// <param name="Buffer">Buffer to create the packets with</param>
        /// <returns>Nagle split packets</returns>
        public static Packet[] SplitPackets(byte[] Buffer)
        {
            List<Packet> Packets = new List<Packet>();
            MemoryStream MemoryBuffer = new MemoryStream();
            BinaryReader MemoryReader = new BinaryReader(MemoryBuffer);

            MemoryBuffer.Seek(0, SeekOrigin.Begin);
            MemoryBuffer.Write(Buffer, 0, Buffer.Length);
            MemoryBuffer.Position = 0;

            int CurrentPosition = 0;

            while (true)
            {
                try
                {
                    if(!MemoryBuffer.CanRead)
                        break;

                    int PacketInnerLength = MemoryReader.ReadInt32();

                    if (PacketInnerLength == 0 || (MemoryBuffer.Length - MemoryBuffer.Position) < PacketInnerLength)
                        break;

                    MemoryBuffer.Position = CurrentPosition;
                    byte[] PacketData = MemoryReader.ReadBytes(PacketInnerLength + 8);
                    CurrentPosition += PacketInnerLength + 8;
                    Packet pak = new Packet(PacketData);
                    if(pak.IsValid())
                        Packets.Add(pak);
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); break; }
            }

            MemoryReader.Close();
            MemoryBuffer.Close();
            MemoryReader.Dispose();
            MemoryBuffer.Dispose();

            return Packets.ToArray();
        }
        
        /// <summary>
        /// Creates a new packet instance with the specified buffer
        /// </summary>
        /// <param name="Buffer">Buffer to create the packet with or null to create a new packet</param>
        public Packet(byte[] Buffer)
        {
            MemoryBuffer = new MemoryStream();
            MemoryWriter = new BinaryWriter(MemoryBuffer);
            MemoryReader = new BinaryReader(MemoryBuffer);
            crc32 = new Crc32();

            if (Buffer != null)
            {
                MemoryBuffer.Seek(0, SeekOrigin.Begin);
                MemoryBuffer.Write(Buffer, 0, Buffer.Length);
                MemoryBuffer.Position = 0;
                int InnerLength = Readint();
                Readuint();

                //Close and Null all the MemoryStreams
                MemoryWriter.Close();
                MemoryReader.Close();
                MemoryBuffer.Close();
                MemoryWriter = null;
                MemoryReader = null;
                MemoryBuffer = null;

                MemoryBuffer = new MemoryStream();
                MemoryWriter = new BinaryWriter(MemoryBuffer);
                MemoryReader = new BinaryReader(MemoryBuffer);
                MemoryBuffer.Seek(0, SeekOrigin.Begin);
                MemoryBuffer.Write(Buffer, 0, InnerLength + 8);
                MemoryBuffer.Position = 0;

                Readint();
                Readuint();
            }
            else
            {
                MemoryBuffer.Position = 0;
                MemoryWriter.Write(0); //Packet Length
                MemoryWriter.Write(0); //Packet Hash
            }
        }

        /// <summary>
        /// Gets the total length of the packet including the header
        /// </summary>
        public int Length
        {
            get
            {
                return (int)MemoryBuffer.Length;
            }
        }

        /// <summary>
        /// Gets the inner packet length
        /// </summary>
        public int InnerLength
        {
            get
            {
                return (int)MemoryBuffer.Length - 8;
            }
        }

        /// <summary>
        /// Gets or sets the current pointer position
        /// </summary>
        public int Pointer
        {
            get
            {
                return (int)MemoryBuffer.Position;
            }
            set
            {
                MemoryBuffer.Position = value;
            }
        }

        /// <summary>
        /// Checks is the packet is valid and non-modified
        /// </summary>
        /// <returns>Returns true if the packet is valid and non-modified</returns>
        public bool IsValid()
        {
            if (Length > 8)
            {
                int tmpPointer = Pointer;
                Pointer = 0;

                int PacketLength = Readint();
                uint Checksum = Readuint();
                byte[] InnerData = MemoryReader.ReadBytes(Length - 8);

                Pointer = tmpPointer;

                if (PacketLength != Length - 8)
                    return false;
                else if (Crc32.Compute(InnerData) != Checksum)
                    return false;
                else 
                    return true;
            }
            else
                return true;
        }

        public uint GetCheckSum()
        {
            int tmpPointer = Pointer;
            Pointer = 0;

            int PacketLength = Readint();
            uint Checksum = Readuint();
            byte[] InnerData = MemoryReader.ReadBytes(Length - 8);

            Pointer = tmpPointer;

            return Crc32.Compute(InnerData);
        }

        /// <summary>
        /// Converts the packet to a byte array that can be sent over a network or written to a file
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return MemoryBuffer.ToArray();
        }

        #region Write
        private void UpdateChecksum()
        {
            if (Length > 8)
            {
                int tmpPointer = Pointer;
                Pointer = 0;

                Readint();
                Readuint();
                byte[] InnerData = MemoryReader.ReadBytes(Length - 8);

                Pointer = 0;

                MemoryWriter.Write(Length - 8);
                MemoryWriter.Write(Crc32.Compute(InnerData));

                Pointer = tmpPointer;
            }
        }

        /// <summary>
        /// Writes a Byte to the packet
        /// </summary>
        /// <param name="Data">The Byte to write to the packet</param>
        public void Write(byte Data)
        {
            MemoryWriter.Write(Data);
            UpdateChecksum();
        }

        /// <summary>
        /// Writes a Boolean to the packet
        /// </summary>
        /// <param name="Data">The Boolean to write to the packet</param>
        public void Write(bool Data)
        {
            MemoryWriter.Write(Data);
            UpdateChecksum();
        }

        /// <summary>
        /// Writes an Int32 to the packet
        /// </summary>
        /// <param name="Data">The Int32 to write to the packet</param>
        public void Write(int Data)
        {
            MemoryWriter.Write(Data);
            UpdateChecksum();
        }

        /// <summary>
        /// Writes an Unsigned Int32 to the packet
        /// </summary>
        /// <param name="Data">The Unsigned Int32 to write to the packet</param>
        public void Write(uint Data)
        {
            MemoryWriter.Write(Data);
            UpdateChecksum();
        }

        /// <summary>
        /// Writes a Float to the packet
        /// </summary>
        /// <param name="Data">The Float to write to the packet</param>
        public void Write(float Data)
        {
            MemoryWriter.Write(Data);
            UpdateChecksum();
        }

        /// <summary>
        /// Writes a Double to the packet
        /// </summary>
        /// <param name="Data">The Double to write to the packet</param>
        public void Write(double Data)
        {
            MemoryWriter.Write(Data);
            UpdateChecksum();
        }

        /// <summary>
        /// Writes an Int16 to the packet
        /// </summary>
        /// <param name="Data">The Int16 to write to the packet</param>
        public void Write(short Data)
        {
            MemoryWriter.Write(Data);
            UpdateChecksum();
        }

        /// <summary>
        /// Writes an Int64 to the packet
        /// </summary>
        /// <param name="Data">The Int64 to write to the packet</param>
        public void Write(long Data)
        {
            MemoryWriter.Write(Data);
            UpdateChecksum();
        }

        /// <summary>
        /// Writes a string to the packet
        /// </summary>
        /// <param name="Data">The string to write to the packet</param>
        public void Write(string Data)
        {
            MemoryWriter.Write(Data.Length);
            for (int i = 0; i < Data.Length; i++)
                MemoryWriter.Write(Data[i]);
            UpdateChecksum();
        }

        /// <summary>
        /// Writes a Byte Array to the packet
        /// </summary>
        /// <param name="Data">The Byte array to write to the packet</param>
        public void Write(params byte[] Data)
        {
            MemoryWriter.Write(Data);
            UpdateChecksum();
        }
        #endregion
        #region Read
        /// <summary>
        /// Reads an Int32 from the packet
        /// </summary>
        /// <returns></returns>
        public int Readint()
        {
            return MemoryReader.ReadInt32();
        }

        /// <summary>
        /// Reads an Unsigned Int32 from the packet
        /// </summary>
        /// <returns></returns>
        public uint Readuint()
        {
            return MemoryReader.ReadUInt32();
        }

        /// <summary>
        /// Reads a String from the packet
        /// </summary>
        /// <returns></returns>
        public string Readstring()
        {
            int Length = MemoryReader.ReadInt32();
            byte[] stringBytes = MemoryReader.ReadBytes(Length);
            string Data = "";
            for (int i = 0; i < Length; i++)
                Data += (char)stringBytes[i];
            return Data;
        }

        /// <summary>
        /// Reads a Float from the packet
        /// </summary>
        /// <returns></returns>
        public float Readfloat()
        {
            return MemoryReader.ReadSingle();
        }

        /// <summary>
        /// Reads an Int16 from the packet
        /// </summary>
        /// <returns></returns>
        public short Readshort()
        {
            return MemoryReader.ReadInt16();
        }

        /// <summary>
        /// Reads a Byte from the packet
        /// </summary>
        /// <returns></returns>
        public byte Readbyte()
        {
            return MemoryReader.ReadByte();
        }

        /// <summary>
        /// Reads a Boolean from the packet
        /// </summary>
        /// <returns></returns>
        public bool Readbool()
        {
            return MemoryReader.ReadBoolean();
        }

        /// <summary>
        /// Reads an Int64 from the packet
        /// </summary>
        /// <returns></returns>
        public long Readlong()
        {
            return MemoryReader.ReadInt64();
        }
        #endregion

        public string ToHexString()
        {
            return BitConverter.ToString(ToArray());
        }

        /*private unsafe uint GetChecksum(byte[] array)
        {
            unchecked
            {
                uint checksum = 0;
                fixed (byte* arrayBase = array)
                {
                    byte* arrayPointer = arrayBase;
                    for (int i = array.Length - 1; i >= 0; i--)
                    {
                        checksum += *arrayPointer;
                        arrayPointer++;
                    }
                }
                return checksum;
            }
        }*/

        /* GC */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                MemoryReader.Dispose();
                MemoryWriter.Dispose();
                MemoryBuffer.Dispose();
            }
        }
    }

    class Crc32 : HashAlgorithm
    {
        public const UInt32 DefaultPolynomial = 0xedb88320;
        public const UInt32 DefaultSeed = 0xffffffff;

        private UInt32 hash;
        private UInt32 seed;
        private UInt32[] table;
        private static UInt32[] defaultTable;

        public Crc32()
        {
            table = InitializeTable(DefaultPolynomial);
            seed = DefaultSeed;
            Initialize();
        }

        public Crc32(UInt32 polynomial, UInt32 seed)
        {
            table = InitializeTable(polynomial);
            this.seed = seed;
            Initialize();
        }

        public override void Initialize()
        {
            hash = seed;
        }

        protected override void HashCore(byte[] buffer, int start, int length)
        {
            hash = CalculateHash(table, hash, buffer, start, length);
        }

        protected override byte[] HashFinal()
        {
            byte[] hashBuffer = UInt32ToBigEndianBytes(~hash);
            this.HashValue = hashBuffer;
            return hashBuffer;
        }

        public override int HashSize
        {
            get { return 32; }
        }

        public static UInt32 Compute(byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
        }

        public static UInt32 Compute(UInt32 seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
        }

        public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        private static UInt32[] InitializeTable(UInt32 polynomial)
        {
            if (polynomial == DefaultPolynomial && defaultTable != null)
                return defaultTable;

            UInt32[] createTable = new UInt32[256];
            for (int i = 0; i < 256; i++)
            {
                UInt32 entry = (UInt32)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                defaultTable = createTable;

            return createTable;
        }

        private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, byte[] buffer, int start, int size)
        {
            UInt32 crc = seed;
            for (int i = start; i < size; i++)
                unchecked
                {
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                }
            return crc;
        }

        private byte[] UInt32ToBigEndianBytes(UInt32 x)
        {
            return new byte[] {
			(byte)((x >> 24) & 0xff),
			(byte)((x >> 16) & 0xff),
			(byte)((x >> 8) & 0xff),
			(byte)(x & 0xff)
		};
        }
    }
}
