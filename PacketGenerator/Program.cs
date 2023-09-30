using System.Xml;

namespace PacketGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            // using 범위 벗어나면 객체 소멸
            using(XmlReader reader = XmlReader.Create("PDL.xml", settings))
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

            ParseMembers(reader);
        }

        public static void ParseMembers(XmlReader reader)
        {
            string packetName = reader["name"];

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
                    return;
                }

                string memberType = reader.Name.ToLower();
                switch (memberType)
                {
                    case "bool":
                        break;

                    case "byte":
                        break;

                    case "short":
                        break;

                    case "ushort":
                        break;

                    case "int":
                        break;

                    case "long":
                        break;

                    case "float":
                        break;

                    case "double":
                        break;

                    case "string":
                        break;

                    case "list":
                        break;

                    default:
                        break;
                }
            }
        }
    }
}