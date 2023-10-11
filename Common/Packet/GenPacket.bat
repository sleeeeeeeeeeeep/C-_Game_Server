start ../../PacketGenerator/bin/Debug/net7.0/PacketGenerator.exe ../../PacketGenerator/bin/Debug/net7.0/PDL.xml
xcopy /Y GeneratePacket.cs "../../DummyClient/Packet"
xcopy /Y GeneratePacket.cs "../../Client/Assets/Scripts/Packet"
xcopy /Y GeneratePacket.cs "../../Server/Packet"

xcopy /Y ClientPacketManager.cs "../../DummyClient/Packet"
xcopy /Y ClientPacketManager.cs "../../Client/Assets/Scripts/Packet"
xcopy /Y ServerPacketManager.cs "../../Server/Packet"