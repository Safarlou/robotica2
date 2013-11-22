using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Vision
{
    class MockInputStream : InputStream
    {
        public Bitmap Frame { get
            {
                if (Frame != null) return Frame;
                else throw new NullReferenceException();
            } 
            private set; }

        public MockInputStream(string file)
        {
            this.Frame = (Bitmap)Image.FromFile(file);
        }
        
        public System.Drawing.Bitmap GetFrame()
        {
            throw new NotImplementedException();
        }
    }
}
