using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary
{
    public class Message
    {
        private byte _sec; //8 bits
        private byte[] _data;
        private String _packet;

        public byte[] data
        {
            get
            {
                return _data;
            }
        }

        public String packet
        {
            get
            {
                return _packet;
            }
        }

        public Message(int sec)
        {
            sec = Convert.ToByte(sec);
            _packet = _sec.ToString();
        }

        public Message(int sec, byte[] data)
        {
            sec = Convert.ToByte(sec);
            _packet = _sec.ToString() + " | " + data.ToString();
        }
    }
}
