using System;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace NugetPack
{
   
    public class Solution : ICloneable
    {
        public List<Rectangle> rectangles { get; set; }
        private int n { get; set; }
        private Rectangle res{get; set;}
     
        public int Metric { get; set; }
  
        public bool isValid;

        public object Clone()
        {
            return new Solution(rectangles);
        }


        public Solution(List<Rectangle> rectgs)
        {
            this.rectangles = new List<Rectangle>();
            for (int i = 0; i < rectgs.Count(); i++)
            {
                this.rectangles.Add((Rectangle) rectgs[i].Clone());
            }
            this.IsValid();
        }

        public bool IsValid()
        {
            for (int i = 0; i < rectangles.Count; i++)
            {
                for (int j = i + 1; j < rectangles.Count; j++) {
                
                    if (!rectangles[i].IsNotCrossing(rectangles[j]))
                    {
                        
                        return false;
                    }
                }
            }

            return true;
            
        }

        public int count_metric()
        {
            if (!IsValid()) return 1000000;
            int X_l = rectangles[0].x_l;
            int X_r = rectangles[0].x_r;
            int Y_t = rectangles[0].y_t;
            int Y_b = rectangles[0].y_b;
            foreach (Rectangle rec in rectangles)
            {
                if (rec.x_l < X_l)
                {
                    X_l = rec.x_l;
                }
                if (rec.x_r > X_r)
                {
                    X_r = rec.x_r;
                }
                if (rec.y_b < Y_b)
                {
                    Y_b = rec.y_b;
                }
                if (rec.y_t > Y_t)
                {
                    Y_t = rec.y_t;
                }

            }

            var res = new Rectangle(X_l, Y_b, X_r - X_l, Y_t - Y_b);
            Metric = res.GetArea();
            return res.GetArea();
        }

        public override string ToString()
        {
            var res = new StringBuilder();
            int k = 3;
            foreach (Rectangle rec in rectangles)
            {
                --k;
                res.Append($"[size= {rec.height}, ({rec.x_l}, {rec.y_b})  ]");
                res.Append("\n");
                
            }

            return res.ToString();
        }


        public void normalaze()
        {
            var min_y_b = 10000;
            var min_x_l = 10000;
            
            foreach (var rec in rectangles)
            {
                min_x_l = Math.Min(min_x_l, rec.x_l);
                min_y_b = Math.Min(min_y_b, rec.y_b);
            }

            for (int i = 0; i < rectangles.Count; ++i)
            {
                rectangles[i].y_b -= min_y_b;
                rectangles[i].y_t -= min_y_b;
                rectangles[i].x_l -= min_x_l;
                rectangles[i].x_r -= min_x_l;
            }
        }
    }
}
