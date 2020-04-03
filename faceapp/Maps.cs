using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace faceapp
{
    public partial class Maps : Form
    {
        public Maps(string latitude, string longitude)
        {
            InitializeComponent();
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;

            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            //gMapControl1.SetPositionByKeywords("Paris, France");
            gMapControl1.Position = new GMap.NET.PointLatLng(Convert.ToDouble(latitude), Convert.ToDouble(longitude));

            //  webBrowser1.Url = new System.Uri("https://www.google.com/maps/@+"+ latitude + ","+ longitude + "z", System.UriKind.Absolute);

        }
    }
}
