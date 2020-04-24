using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Larpx.ResourceSpider.CommonHelper
{
    public class MD5
    {
        public string GetBufferHash(string sBuffer)
        {
            UTF8Encoding uTF8Encoding = new UTF8Encoding();
            return this.GetBufferHash(uTF8Encoding.GetBytes(sBuffer));
        }

        public string GetBufferHash(byte[] byBuffer)
        {
            System.Security.Cryptography.MD5 mD = new MD5CryptoServiceProvider();
            byte[] value = mD.ComputeHash(byBuffer);
            return BitConverter.ToString(value).Replace("-", "");
        }

        public string GetFileHash(string sFileName)
        {
            if (!File.Exists(sFileName))
            {
                return string.Empty;
            }
            string result = string.Empty;
            System.Security.Cryptography.MD5 mD = new MD5CryptoServiceProvider();
            try
            {
                FileStream fileStream = new FileStream(sFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] value = mD.ComputeHash(fileStream);
                fileStream.Close();
                result = BitConverter.ToString(value).Replace("-", "");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        public bool Verify(string sBuffer, string sHash)
        {
            string bufferHash = this.GetBufferHash(sBuffer);
            StringComparer ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
            return ordinalIgnoreCase.Compare(bufferHash, sHash) == 0;
        }
    }

    public class DataEncoder
    {
        CRC32 m_oCRC = new CRC32();
        Random m_oRandom = new Random();
        Base32Ex m_oBase32Ex = new Base32Ex();

        public string Encrypt(string sString)
        {
            string text = m_oBase32Ex.Encode(Encoding.UTF8.GetBytes(sString));
            string text2 = m_oCRC.GetBufferHash(text).ToLower();
            if (text.Length > "amgbnhcoidpjeqkfrl".Length)
            {
                int num = m_oRandom.Next(0, "amgbnhcoidpjeqkfrl".Length);
                text = text.Insert(num, text2);
                text = "amgbnhcoidpjeqkfrl"[num] + text;
            }
            else
            {
                text = text2 + text;
            }
            return text;
        }

        public string Decrypt(string sString)
        {
            if (String.IsNullOrEmpty(sString) || sString.Length < 8)
            {
                throw new Exception("Invalid Parameter.");
            }
            string text;
            string a;
            if (sString.Length > "amgbnhcoidpjeqkfrl".Length + 8)
            {
                int startIndex = "amgbnhcoidpjeqkfrl".IndexOf(sString.Substring(0, 1));
                text = sString.Remove(0, 1);
                a = text.Substring(startIndex, 8);
                text = text.Remove(startIndex, 8);
            }
            else
            {
                a = sString.Substring(0, 8);
                text = sString.Remove(0, 8);
            }
            if (a == m_oCRC.GetBufferHash(text).ToLower())
            {
                return Encoding.UTF8.GetString(m_oBase32Ex.Decode(text));
            }
            throw new Exception("CRC verify check fail.");
        }

    }

    public class Base32
    {
        public Base32() : this("abcdefghijklmnopqrstuvwxyz234567", '=')
        {
        }

        public Base32(char cPadding) : this("abcdefghijklmnopqrstuvwxyz234567", cPadding)
        {
        }

        public Base32(string sEncodingTable) : this(sEncodingTable, '=')
        {
        }

        public Base32(string sEncodingTable, char padding)
        {
            eTable = sEncodingTable;
            cPadding = padding;
            dTable = new byte[128];
            InitialiseDecodingTable();
        }

        public virtual byte[] Decode(string sData)
        {
            List<byte> list = new List<byte>();
            int num = sData.Length;
            while (num > 0 && Ignore(sData[num - 1]))
            {
                num--;
            }
            int i = 0;
            int num2 = num - 8;
            for (i = NextI(sData, i, num2); i < num2; i = NextI(sData, i, num2))
            {
                byte b = dTable[(int)sData[i++]];
                i = NextI(sData, i, num2);
                byte b2 = dTable[(int)sData[i++]];
                i = NextI(sData, i, num2);
                byte b3 = dTable[(int)sData[i++]];
                i = NextI(sData, i, num2);
                byte b4 = dTable[(int)sData[i++]];
                i = NextI(sData, i, num2);
                byte b5 = dTable[(int)sData[i++]];
                i = NextI(sData, i, num2);
                byte b6 = dTable[(int)sData[i++]];
                i = NextI(sData, i, num2);
                byte b7 = dTable[(int)sData[i++]];
                i = NextI(sData, i, num2);
                byte b8 = dTable[(int)sData[i++]];
                list.Add((byte)((int)b << 3 | b2 >> 2));
                list.Add((byte)((int)b2 << 6 | (int)b3 << 1 | b4 >> 4));
                list.Add((byte)((int)b4 << 4 | b5 >> 1));
                list.Add((byte)((int)b5 << 7 | (int)b6 << 2 | b7 >> 3));
                list.Add((byte)((int)b7 << 5 | (int)b8));
            }
            DecodeLastBlock(list, sData[num - 8], sData[num - 7], sData[num - 6], sData[num - 5], sData[num - 4], sData[num - 3], sData[num - 2], sData[num - 1]);
            return list.ToArray();
        }

        protected virtual int DecodeLastBlock(ICollection<byte> byOutStream, char c1, char c2, char c3, char c4, char c5, char c6, char c7, char c8)
        {
            if (c3 == cPadding)
            {
                byte b = dTable[(int)c1];
                byte b2 = dTable[(int)c2];
                byOutStream.Add((byte)((int)b << 3 | b2 >> 2));
                return 1;
            }
            if (c5 == cPadding)
            {
                byte b3 = dTable[(int)c1];
                byte b4 = dTable[(int)c2];
                byte b5 = dTable[(int)c3];
                byte b6 = dTable[(int)c4];
                byOutStream.Add((byte)((int)b3 << 3 | b4 >> 2));
                byOutStream.Add((byte)((int)b4 << 6 | (int)b5 << 1 | b6 >> 4));
                return 2;
            }
            if (c6 == cPadding)
            {
                byte b7 = dTable[(int)c1];
                byte b8 = dTable[(int)c2];
                byte b9 = dTable[(int)c3];
                byte b10 = dTable[(int)c4];
                byte b11 = dTable[(int)c5];
                byOutStream.Add((byte)((int)b7 << 3 | b8 >> 2));
                byOutStream.Add((byte)((int)b8 << 6 | (int)b9 << 1 | b10 >> 4));
                byOutStream.Add((byte)((int)b10 << 4 | b11 >> 1));
                return 3;
            }
            if (c8 == cPadding)
            {
                byte b12 = dTable[(int)c1];
                byte b13 = dTable[(int)c2];
                byte b14 = dTable[(int)c3];
                byte b15 = dTable[(int)c4];
                byte b16 = dTable[(int)c5];
                byte b17 = dTable[(int)c6];
                byte b18 = dTable[(int)c7];
                byOutStream.Add((byte)((int)b12 << 3 | b13 >> 2));
                byOutStream.Add((byte)((int)b13 << 6 | (int)b14 << 1 | b15 >> 4));
                byOutStream.Add((byte)((int)b15 << 4 | b16 >> 1));
                byOutStream.Add((byte)((int)b16 << 7 | (int)b17 << 2 | b18 >> 3));
                return 4;
            }
            byte b19 = dTable[(int)c1];
            byte b20 = dTable[(int)c2];
            byte b21 = dTable[(int)c3];
            byte b22 = dTable[(int)c4];
            byte b23 = dTable[(int)c5];
            byte b24 = dTable[(int)c6];
            byte b25 = dTable[(int)c7];
            byte b26 = dTable[(int)c8];
            byOutStream.Add((byte)((int)b19 << 3 | b20 >> 2));
            byOutStream.Add((byte)((int)b20 << 6 | (int)b21 << 1 | b22 >> 4));
            byOutStream.Add((byte)((int)b22 << 4 | b23 >> 1));
            byOutStream.Add((byte)((int)b23 << 7 | (int)b24 << 2 | b25 >> 3));
            byOutStream.Add((byte)((int)b25 << 5 | (int)b26));
            return 5;
        }

        public virtual string Encode(byte[] byInput)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int num = byInput.Length % 5;
            int num2 = byInput.Length - num;
            for (int i = 0; i < num2; i += 5)
            {
                int num3 = (int)(byInput[i] & 255);
                int num4 = (int)(byInput[i + 1] & 255);
                int num5 = (int)(byInput[i + 2] & 255);
                int num6 = (int)(byInput[i + 3] & 255);
                int num7 = (int)(byInput[i + 4] & 255);
                stringBuilder.Append(eTable[num3 >> 3 & 31]);
                stringBuilder.Append(eTable[(num3 << 2 | num4 >> 6) & 31]);
                stringBuilder.Append(eTable[num4 >> 1 & 31]);
                stringBuilder.Append(eTable[(num4 << 4 | num5 >> 4) & 31]);
                stringBuilder.Append(eTable[(num5 << 1 | num6 >> 7) & 31]);
                stringBuilder.Append(eTable[num6 >> 2 & 31]);
                stringBuilder.Append(eTable[(num6 << 3 | num7 >> 5) & 31]);
                stringBuilder.Append(eTable[num7 & 31]);
            }
            switch (num)
            {
                case 1:
                    {
                        int num8 = (int)(byInput[num2] & 255);
                        stringBuilder.Append(eTable[num8 >> 3 & 31]);
                        stringBuilder.Append(eTable[num8 << 2 & 31]);
                        stringBuilder.Append(cPadding).Append(cPadding).Append(cPadding).Append(cPadding).Append(cPadding).Append(cPadding);
                        break;
                    }
                case 2:
                    {
                        int num9 = (int)(byInput[num2] & 255);
                        int num10 = (int)(byInput[num2 + 1] & 255);
                        stringBuilder.Append(eTable[num9 >> 3 & 31]);
                        stringBuilder.Append(eTable[(num9 << 2 | num10 >> 6) & 31]);
                        stringBuilder.Append(eTable[num10 >> 1 & 31]);
                        stringBuilder.Append(eTable[num10 << 4 & 31]);
                        stringBuilder.Append(cPadding).Append(cPadding).Append(cPadding).Append(cPadding);
                        break;
                    }
                case 3:
                    {
                        int num11 = (int)(byInput[num2] & 255);
                        int num12 = (int)(byInput[num2 + 1] & 255);
                        int num13 = (int)(byInput[num2 + 2] & 255);
                        stringBuilder.Append(eTable[num11 >> 3 & 31]);
                        stringBuilder.Append(eTable[(num11 << 2 | num12 >> 6) & 31]);
                        stringBuilder.Append(eTable[num12 >> 1 & 31]);
                        stringBuilder.Append(eTable[(num12 << 4 | num13 >> 4) & 31]);
                        stringBuilder.Append(eTable[num13 << 1 & 31]);
                        stringBuilder.Append(cPadding).Append(cPadding).Append(cPadding);
                        break;
                    }
                case 4:
                    {
                        int num14 = (int)(byInput[num2] & 255);
                        int num15 = (int)(byInput[num2 + 1] & 255);
                        int num16 = (int)(byInput[num2 + 2] & 255);
                        int num17 = (int)(byInput[num2 + 3] & 255);
                        stringBuilder.Append(eTable[num14 >> 3 & 31]);
                        stringBuilder.Append(eTable[(num14 << 2 | num15 >> 6) & 31]);
                        stringBuilder.Append(eTable[num15 >> 1 & 31]);
                        stringBuilder.Append(eTable[(num15 << 4 | num16 >> 4) & 31]);
                        stringBuilder.Append(eTable[(num16 << 1 | num17 >> 7) & 31]);
                        stringBuilder.Append(eTable[num17 >> 2 & 31]);
                        stringBuilder.Append(eTable[num17 << 3 & 31]);
                        stringBuilder.Append(cPadding);
                        break;
                    }
            }
            return stringBuilder.ToString();
        }

        protected bool Ignore(char cChar)
        {
            return cChar == '\n' || cChar == '\r' || cChar == '\t' || cChar == ' ' || cChar == '-';
        }

        protected void InitialiseDecodingTable()
        {
            for (int i = 0; i < eTable.Length; i++)
            {
                dTable[(int)eTable[i]] = (byte)i;
            }
        }

        protected int NextI(string sData, int nIdx, int nFinish)
        {
            while (nIdx < nFinish && Ignore(sData[nIdx]))
            {
                nIdx++;
            }
            return nIdx;
        }

        private readonly char cPadding;

        private const string DEF_ENCODING_TABLE = "abcdefghijklmnopqrstuvwxyz234567";

        private const char DEF_PADDING = '=';

        private readonly byte[] dTable;

        private readonly string eTable;
    }

    public class Base32Ex : Base32
    {
        public Base32Ex() : base("ybn6dro3tfg8ejk4mcp1qzx7uwi5sah9", '=')
        {
        }

        public override byte[] Decode(string sData)
        {
            int num = Convert.ToInt32(Math.Floor((double)sData.Length / 1.6));
            int totalWidth = 8 * Convert.ToInt32(Math.Ceiling((double)num / 5.0));
            string sData2 = sData.PadRight(totalWidth, '=').ToLower();
            return base.Decode(sData2);
        }

        public override string Encode(byte[] byInput)
        {
            string text = base.Encode(byInput);
            return text.TrimEnd(new char[]
            {
                '='
            });
        }

        private const string DEF_ENCODING_TABLE = "ybn6dro3tfg8ejk4mcp1qzx7uwi5sah9";

        private const char DEF_PADDING = '=';
    }

    public class CRC32
    {
        public string GetBufferHash(string sBuffer)
        {
            UTF8Encoding uTF8Encoding = new UTF8Encoding();
            return GetBufferHash(uTF8Encoding.GetBytes(sBuffer));
        }

        public string GetBufferHash(byte[] byBuffer)
        {
            uint num = 4294967295u;
            for (int i = 0; i < byBuffer.Length; i++)
            {
                num = ((num >> 8 & 16777215u) ^ CRCTable[(int)((UIntPtr)((num ^ (uint)byBuffer[i]) & 255u))]);
            }
            num ^= 4294967295u;
            return BitConverter.ToString(BitConverter.GetBytes(num)).Replace("-", "");
        }

        public string GetFileHash(string sFileName)
        {
            if (!System.IO.File.Exists(sFileName))
            {
                return string.Empty;
            }
            byte[] array = new byte[1024];
            uint num = 4294967295u;
            using (System.IO.FileStream fileStream = System.IO.File.OpenRead(sFileName))
            {
                while (true)
                {
                    int num2 = fileStream.Read(array, 0, 1024);
                    if (num2 == 0)
                    {
                        break;
                    }
                    for (int i = 0; i < num2; i++)
                    {
                        num = ((num >> 8 & 16777215u) ^ CRCTable[(int)((UIntPtr)((num ^ (uint)array[i]) & 255u))]);
                    }
                }
            }
            num ^= 4294967295u;
            return BitConverter.ToString(BitConverter.GetBytes(num)).Replace("-", "");
        }

        private uint[] CRCTable = new uint[]
        {
            0u,
            1996959894u,
            3993919788u,
            2567524794u,
            124634137u,
            1886057615u,
            3915621685u,
            2657392035u,
            249268274u,
            2044508324u,
            3772115230u,
            2547177864u,
            162941995u,
            2125561021u,
            3887607047u,
            2428444049u,
            498536548u,
            1789927666u,
            4089016648u,
            2227061214u,
            450548861u,
            1843258603u,
            4107580753u,
            2211677639u,
            325883990u,
            1684777152u,
            4251122042u,
            2321926636u,
            335633487u,
            1661365465u,
            4195302755u,
            2366115317u,
            997073096u,
            1281953886u,
            3579855332u,
            2724688242u,
            1006888145u,
            1258607687u,
            3524101629u,
            2768942443u,
            901097722u,
            1119000684u,
            3686517206u,
            2898065728u,
            853044451u,
            1172266101u,
            3705015759u,
            2882616665u,
            651767980u,
            1373503546u,
            3369554304u,
            3218104598u,
            565507253u,
            1454621731u,
            3485111705u,
            3099436303u,
            671266974u,
            1594198024u,
            3322730930u,
            2970347812u,
            795835527u,
            1483230225u,
            3244367275u,
            3060149565u,
            1994146192u,
            31158534u,
            2563907772u,
            4023717930u,
            1907459465u,
            112637215u,
            2680153253u,
            3904427059u,
            2013776290u,
            251722036u,
            2517215374u,
            3775830040u,
            2137656763u,
            141376813u,
            2439277719u,
            3865271297u,
            1802195444u,
            476864866u,
            2238001368u,
            4066508878u,
            1812370925u,
            453092731u,
            2181625025u,
            4111451223u,
            1706088902u,
            314042704u,
            2344532202u,
            4240017532u,
            1658658271u,
            366619977u,
            2362670323u,
            4224994405u,
            1303535960u,
            984961486u,
            2747007092u,
            3569037538u,
            1256170817u,
            1037604311u,
            2765210733u,
            3554079995u,
            1131014506u,
            879679996u,
            2909243462u,
            3663771856u,
            1141124467u,
            855842277u,
            2852801631u,
            3708648649u,
            1342533948u,
            654459306u,
            3188396048u,
            3373015174u,
            1466479909u,
            544179635u,
            3110523913u,
            3462522015u,
            1591671054u,
            702138776u,
            2966460450u,
            3352799412u,
            1504918807u,
            783551873u,
            3082640443u,
            3233442989u,
            3988292384u,
            2596254646u,
            62317068u,
            1957810842u,
            3939845945u,
            2647816111u,
            81470997u,
            1943803523u,
            3814918930u,
            2489596804u,
            225274430u,
            2053790376u,
            3826175755u,
            2466906013u,
            167816743u,
            2097651377u,
            4027552580u,
            2265490386u,
            503444072u,
            1762050814u,
            4150417245u,
            2154129355u,
            426522225u,
            1852507879u,
            4275313526u,
            2312317920u,
            282753626u,
            1742555852u,
            4189708143u,
            2394877945u,
            397917763u,
            1622183637u,
            3604390888u,
            2714866558u,
            953729732u,
            1340076626u,
            3518719985u,
            2797360999u,
            1068828381u,
            1219638859u,
            3624741850u,
            2936675148u,
            906185462u,
            1090812512u,
            3747672003u,
            2825379669u,
            829329135u,
            1181335161u,
            3412177804u,
            3160834842u,
            628085408u,
            1382605366u,
            3423369109u,
            3138078467u,
            570562233u,
            1426400815u,
            3317316542u,
            2998733608u,
            733239954u,
            1555261956u,
            3268935591u,
            3050360625u,
            752459403u,
            1541320221u,
            2607071920u,
            3965973030u,
            1969922972u,
            40735498u,
            2617837225u,
            3943577151u,
            1913087877u,
            83908371u,
            2512341634u,
            3803740692u,
            2075208622u,
            213261112u,
            2463272603u,
            3855990285u,
            2094854071u,
            198958881u,
            2262029012u,
            4057260610u,
            1759359992u,
            534414190u,
            2176718541u,
            4139329115u,
            1873836001u,
            414664567u,
            2282248934u,
            4279200368u,
            1711684554u,
            285281116u,
            2405801727u,
            4167216745u,
            1634467795u,
            376229701u,
            2685067896u,
            3608007406u,
            1308918612u,
            956543938u,
            2808555105u,
            3495958263u,
            1231636301u,
            1047427035u,
            2932959818u,
            3654703836u,
            1088359270u,
            936918000u,
            2847714899u,
            3736837829u,
            1202900863u,
            817233897u,
            3183342108u,
            3401237130u,
            1404277552u,
            615818150u,
            3134207493u,
            3453421203u,
            1423857449u,
            601450431u,
            3009837614u,
            3294710456u,
            1567103746u,
            711928724u,
            3020668471u,
            3272380065u,
            1510334235u,
            755167117u
        };
    }
}
