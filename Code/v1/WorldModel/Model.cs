using Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldModel
{
    class Model
    {
        private InputStream input;
        private Polygon bounds;
        private List<Robot> robots;
        private List<Object> objects { get; private set; }

        public Model(InputStream input)
        {
            this.input = input;
        }
    }
}
