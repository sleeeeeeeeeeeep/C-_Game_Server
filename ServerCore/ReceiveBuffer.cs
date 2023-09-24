using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class ReceiveBuffer
    {
        ArraySegment<byte> _buffer;

        int _readPos;
        int _writePos;

        public ReceiveBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        // 패킷 크기
        public int DataSize
        {
            get
            {
                return _writePos - _readPos;
            }
        }

        // 빈 공간
        public int FreeSize
        {
            get
            {
                return _buffer.Count - _writePos;
            }
        }

        public ArraySegment<byte> ReadSegment
        {
            get { 
                return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize);
            }
        }

        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSize;

            // 남은 데이터가 없으면 오프셋 0으로(rp와 wp가 겹치면)
            if(dataSize == 0)
            {
                _readPos = 0;
                _writePos = 0;
                return;
            }
            // 데이터 있으면 데이터/오프셋을 시작 위치로 옮겨줌
            else
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes)
        {
            if(numOfBytes > DataSize)
            {
                return false;
            }

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
            {
                return false;
            }

            _writePos += numOfBytes;
            return true;
        }
    }
}
