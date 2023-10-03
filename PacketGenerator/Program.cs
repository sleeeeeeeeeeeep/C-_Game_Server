using System.Data;
using System.Xml;

namespace PacketGenerator
{
    internal class Program
    {
        static ushort packetId;
        static string packetEnums; // 패킷 종류 정의하는 코드
        static string genPacket; // 패킷 읽고 쓰는 코드

        static string clientManagerRegister;
        static string serverManagerRegister;

        static void Main(string[] args)
        {
            string pdlPath = "PDL.xml";

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            if(args.Length >= 1 ) 
            {
                pdlPath = args[0];
            }

            // using 범위 벗어나면 객체 소멸
            using(XmlReader reader = XmlReader.Create(pdlPath, settings))
            {
                reader.MoveToContent();

                while(reader.Read())
                {
                    // 깊이가 1 and 엘리먼트 여는 거면 실행
                    if(reader.Depth == 1 && reader.NodeType == XmlNodeType.Element)
                    {
                        ParsePacket(reader);
                    }

                    Console.WriteLine(reader.Name + " " + reader["name"]);
                }


                string fileText = String.Format(PacketFormat.fileFormat, packetEnums, genPacket);
                File.WriteAllText("GeneratePacket.cs", fileText);

                string clientManagerText = String.Format(PacketFormat.managerFormat, clientManagerRegister);
                File.WriteAllText("ClientPacketManager.cs", clientManagerText);

                string serverManagerText = String.Format(PacketFormat.managerFormat, serverManagerRegister);
                File.WriteAllText("ServerPacketManager.cs", serverManagerText);
            }

        }

        public static void ParsePacket(XmlReader reader)
        {
            if(reader.NodeType == XmlNodeType.EndElement)
            {
                Console.WriteLine("패킷 노드 끝남");
                return;
            }

            if(reader.Name.ToLower() != "packet")
            {
                Console.WriteLine("패킷 노드가 아님");
                return;
            }

            string packetName = reader["name"];

            if(string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("패킷 이름 없음");
                return;
            }

            Tuple<string, string, string> t = ParseMembers(reader);
            genPacket += String.Format(
                PacketFormat.packetFormat,
                packetName,
                t.Item1,
                t.Item2,
                t.Item3
            );

            packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId) + Environment.NewLine + "\t";

            if(packetName.StartsWith("S_") || packetName.StartsWith("s_"))
            {
                clientManagerRegister += string.Format(PacketFormat.managerRegisterFormat, packetName) + Environment.NewLine;
            }
            else
            {
                serverManagerRegister += string.Format(PacketFormat.managerRegisterFormat, packetName) + Environment.NewLine;
            }
        }

        // {1} 멤버 변수
        // {2} 멤버 변수 read
        // {3} 멤버 변수 write
        public static Tuple<string, string, string> ParseMembers(XmlReader reader)
        {
            string packetName = reader["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";


            int depth = reader.Depth + 1; // 패킷 속성 깊이
            while (reader.Read())
            {
                if(reader.Depth != depth)
                {
                    break;
                }

                string memberName = reader["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("멤버 이름 없음");
                    return null;
                }

                if (string.IsNullOrEmpty(memberCode))
                {
                    memberCode += Environment.NewLine; // 개행
                }

                if (string.IsNullOrEmpty(readCode))
                {
                    readCode += Environment.NewLine; // 개행
                }

                if (string.IsNullOrEmpty(writeCode))
                {
                    writeCode += Environment.NewLine; // 개행
                }

                string memberType = reader.Name.ToLower();
                switch (memberType)
                {
                    case "byte":
                    case "sbyte":
                        // {0} 변수 자료형
                        // {1} 변수 이름
                        memberCode += String.Format(PacketFormat.memberFormat, memberType, memberName);

                        // {0} 변수 이름
                        // {1} 변수 자료형(바이트 크기)
                        readCode += String.Format(PacketFormat.readByteFormat, memberName, memberType);

                        // {0} 변수 이름
                        // {1} 변수 자료형(바이트 크기)
                        writeCode += String.Format(PacketFormat.writeByteFormat, memberName, memberType);

                        break;

                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        // {0} 변수 자료형
                        // {1} 변수 이름
                        memberCode += String.Format(PacketFormat.memberFormat, memberType, memberName);

                        // {0} 변수 이름
                        // {1} 바이트 크기로 변하는 BitConverter.To~()
                        // {2} 변수 자료형(바이트 크기)
                        readCode += String.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);

                        // {0} 변수 이름
                        // {1} 변수 자료형
                        writeCode += String.Format(PacketFormat.writeFormat, memberName, memberType);

                        break;

                    case "string":
                        // {0} 변수 자료형
                        // {1} 변수 이름
                        memberCode += String.Format(PacketFormat.memberFormat, memberType, memberName);

                        // {0} 변수 이름
                        readCode += String.Format(PacketFormat.readStringFormat, memberName);

                        // {0} 변수 이름
                        writeCode += String.Format(PacketFormat.writeStringFormat, memberName);

                        break;

                    case "list":
                        Tuple<string, string, string> t = ParseList(reader);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;

                        break;

                    default:
                        break;
                }
            }
            // 들여쓰기
            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseList(XmlReader reader)
        {
            string listName = reader["name"];

            if(string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("리스트 이름 없음");
                return null;
            }

            Tuple<string, string, string> t = ParseMembers(reader);

            string memberCode = string.Format(
                PacketFormat.memberListFormat, 
                FirstCharToUpper(listName),
                FirstCharToLower(listName),
                t.Item1,
                t.Item2,
                t.Item3
            );

            string readCode = string.Format(
               PacketFormat.readListFormat,
               FirstCharToUpper(listName),
               FirstCharToLower(listName)
           );

            string writeCode = string.Format(
               PacketFormat.writeListFormat,
               FirstCharToUpper(listName),
               FirstCharToLower(listName)
           );

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static string FirstCharToUpper(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            return text[0].ToString().ToUpper() + text.Substring(1);
        }

        public static string FirstCharToLower(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            return text[0].ToString().ToLower() + text.Substring(1);
        }
    }
}