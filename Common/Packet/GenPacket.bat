start ../../PacketGenerator/bin/Debug/net7.0/PacketGenerator.exe ../../PacketGenerator/bin/Debug/net7.0/PDL.xml
xcopy /Y GeneratePacket.cs "../../DummyClient/Packet"
xcopy /Y GeneratePacket.cs "../../Server/Packet"