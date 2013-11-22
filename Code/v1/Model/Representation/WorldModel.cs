using Model.Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Representation
{
    /// <summary>
    /// WorldModel creates and manages a modelled representation of images 
    /// from an InputStream. 
    /// </summary>
    class WorldModel
    {
        private InputStream input;
        private Polygon bounds;
        private List<Robot> robots;
        private List<Object> objects { get; private set; }

        public WorldModel(InputStream input)
        {
            this.input = input;
        }
    }
}
