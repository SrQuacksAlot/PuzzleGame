using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pussel
{
    internal class classPicbox
    {
        public PictureBox pictureBox = new PictureBox();
        public Point orgPoint = new Point();
        public int nr;

        public classPicbox(int _nr, Point _orgPoint, PictureBox _pictureBox ) {
            nr = _nr;
            orgPoint = _orgPoint;
            pictureBox = _pictureBox;
        }

        //public Point Org_point
        //{
        //    get { return orgPoint; }
        //    set { orgPoint = value; }
        //}

        //public PictureBox PictureBox()
        //{
             
        //}
    }
}
