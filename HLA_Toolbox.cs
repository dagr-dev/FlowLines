using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace HLA_Toolbox
{
    public class HLA_Toolbox : GH_AssemblyInfo
    {
        public override string Name => "HLA__Toolbox";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("9256748d-a642-461a-90df-375363ddf0b9");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}
